using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.Modes;
using System.IO;
using System.Text.RegularExpressions;

namespace PinballClub.TestGame.GUI
{

    public class PlaylistRequest
    {
        public P3Mode requestingMode;
        public string playlistName;
        public int resumePlaylistClipIndex;
        public float resumePlaylistClipPosition;
        public bool allowCurrentClipToEndBeforeChange;

        public PlaylistRequest(P3Mode _requestingMode, string _playlistName, bool _allowCurrentClipToEndBeforeChange)
        {
            requestingMode = _requestingMode;
            playlistName = _playlistName;
            resumePlaylistClipIndex = -1;
            resumePlaylistClipPosition = 0;
            allowCurrentClipToEndBeforeChange = _allowCurrentClipToEndBeforeChange;
        }
    }

    /// <summary>
    /// Represents a collection of sounds chained by followups.  The result of a sound being requested to play and all of
    /// its followups calculated at that time.  This enables chains of sound to be aborted to avoid dangling followups.
    /// </summary>
    public class SoundChain
    {
        public string initialSoundName;
        public Transform parent;
        public Vector3 position;

        public bool doneAfterCurrentClipEnds = false;
        public bool done = false;

        // The following lists are corelated.
        public List<AudioClip> clips = new List<AudioClip>();
        public List<VariableAudioClip> variableClips = new List<VariableAudioClip>();
        public List<AudioClipGroup> clipGroups = new List<AudioClipGroup>();
        public List<GameObject> soundObjects = new List<GameObject>();
        public List<FollowUpAudioClipGroup> followUps = new List<FollowUpAudioClipGroup>();
        public int index = -1;  // the index into all of the above lists simultaneously.

        public AudioClipGroup currentClipGroup { get { return ((index >= 0) && (index < clipGroups.Count) && (clipGroups[index] != null)) ? clipGroups[index] : null; } }

        public void Add(AudioClip clip, VariableAudioClip variableClip, AudioClipGroup clipGroup)
        {
            clips.Add(clip);
            variableClips.Add(variableClip);
            clipGroups.Add(clipGroup);
        }
    }


    /// <summary>
    /// The implementation of audio for this project.  Implements the Multimorphic.P3App.GUI.IAudio interface.
    /// Allows individual clips to be played directly or in 3D locations.
    /// Allows playlists (several sequential clips) to be specified and triggered.  Playlists are added by adding a Playlist component to this object's gameObject (in the inspector) and adding clips to that Playlist.
    /// Allows volume control of the individial sound clips (as a group) and the playlists (as a group).
    /// </summary>
    public class TestGameAudio : P3Aware, IAudio
    {
        public static TestGameAudio Instance = null;

        GameObject mainCamera;
        public bool moveWithCamera = true;

        private Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioClipGroup> clipGroups = new Dictionary<string, AudioClipGroup>();
        private List<AudioSource> playlistSources = new List<AudioSource>();
        private List<PlaylistRequest> playlistRequests = new List<PlaylistRequest>();
        /// <summary>
        /// Chains of sounds that are in progress of being played.  
        /// Note that a request for a single sound gets turned into a sound chain of one.
        /// A sound with followups results in a sound chain with many clips.
        /// </summary>
        private List<SoundChain> soundChains = new List<SoundChain>();

        /// <summary>
        /// What are the priorities of channels?  Specifically, if a sound is to be played on mixer group M, what 
        /// other mixer groups need to have their clips stopped?
        /// </summary>
        private Dictionary<AudioMixerGroup, AudioMixerGroup[]> mixerGroupOverrides = new Dictionary<AudioMixerGroup, AudioMixerGroup[]>();

        /// <summary>
        /// What's the maximum number of clips that can be played simultaneously on a channel?
        /// If no entry is found in this dictionary, there is no restriction (the default).
        /// </summary>
        private Dictionary<AudioMixerGroup, int> mixerGroupMaximumClipCount = new Dictionary<AudioMixerGroup, int>();


        private List<AudioSource> fadingSources = new List<AudioSource>();

        float masterVolumeLevel = 1f;
        float playlistMasterVolumeLevel = 0.8f;
        const float voiceVolumeLevel = 0f;  // db, when unmuted

        private int sourceIndex;
        private int cleanupIndex;

        private int playlistSourceIndex;
        private int playlistClipIndex = -1;
        private Playlist currentPlaylist;
        private PlaylistRequest currentPlaylistRequest;

        private float watchdogCountdown;
        private float watchdogInterval = 2.5f;

        private float cleanupCountdown;
        private float cleanupInterval = 1.1f;

        private List<string> soundChain;
        private int soundChainIndex;
        private Transform soundChainTransform;

        private AudioMixer mixer = null;
        /// <summary>
        /// The master mixer group, for master volume.  All audio mixers lead to this mixer group.
        /// </summary>
        public AudioMixerGroup masterMixerGroup;
        /// <summary>
        /// The mixer group that that playlists get routed to.
        /// </summary>
        public AudioMixerGroup playlistMixerGroup;
        /// <summary>
        /// The mixer group used for sound effects.
        /// </summary>
        public AudioMixerGroup effectsMixerGroup;
        /// <summary>
        /// The collective mixer group for the various voice mixer groups.  The Necessary, ShotFeedback, Instruction and Chatter voice mixer groups route into this one.
        /// </summary>
        public AudioMixerGroup voiceMixerGroup;
        /// <summary>
        /// The mixer group used for voice callouts that must not be missed (e.g. "ball saved").  
        /// This group overrides the ShotFeedback, Instruction and Chatter mixer groups. 
        /// </summary>
        public AudioMixerGroup necessaryVoiceMixerGroup;
        /// <summary>
        /// The mixer group used for voice callouts that indicate a successful shot by the player (e.g. "Great shot" or "Jackpot").  
        /// This group overrides the Instruction and Chatter mixer groups.
        /// This group is overridden by the Necessary mixer group.
        /// </summary>
        public AudioMixerGroup shotFeedbackVoiceMixerGroup;
        /// <summary>
        /// The mixer group used for voice callouts that tell the player what to do (e.g. "Shoot the scoop" or "One more for multiball").
        /// This group overrides the Chatter mixer group.
        /// This group is overridden by the Necessary and ShotFeedback mixer groups.
        /// </summary>
        public AudioMixerGroup instructionVoiceMixerGroup;
        /// <summary>
        /// The mixer group used for voice callouts that are low priority and intended to add character to the game (e.g. "Great day for pinball" or in-game character conversations).
        /// This group is overridden by the Necessary and ShotFeedback and Instruction mixer groups.
        /// </summary>
        public AudioMixerGroup chatterVoiceMixerGroup;

        /// <summary>
        /// This dictionary is used to map sound requests to the correct audio mixer group.
        /// It will contain "Effects", "Necessary", "ShotFeedback", etc., paired up with their appropriate mixer group.
        /// This dictionary is populated in FindGroups().
        /// </summary>
        private Dictionary<string, AudioMixerGroup> mixerGroups = new Dictionary<string, AudioMixerGroup>();

        /// <summary>
        /// Calls to PlaySound and PlaySound3D which do not specify a mixer group name will cause the audio to be sent to this mixer group by default.
        /// </summary>
        public AudioMixerGroup defaultMixerGroup;

        void Awake()
        {
            Instance = this;
            Initialize();

            SceneManager.sceneLoaded += OnSceneFinishedLoading;
        }

        protected override void CreateEventHandlers()
        {
            base.CreateEventHandlers();
            AddModeEventHandler("Evt_SetSoundtrackVolume", SetSoundtrackVolumeEventHandler);
            AddModeEventHandler("Evt_SetBassGain", SetBassGainEventHandler);
            AddModeEventHandler("Evt_ChangePlaylist", ChangePlaylistEventHandler);
            AddModeEventHandler("Evt_RequestPlaylist", RequestPlaylistEventHandler);
            AddModeEventHandler("Evt_RemovePlaylistRequests", RemovePlaylistRequestsEventHandler);
            //          AddModeEventHandler("Evt_PlaySound", PlaySoundEventHandler);
            AddModeEventHandler("Evt_AbortSound", AbortSoundEventHandler);
            AddModeEventHandler("Evt_AbortSoundsOnMixerGroup", AbortSoundsOnMixerGroupEventHandler);
            AddModeEventHandler("Evt_InterruptiveAbortSoundsOnMixerGroup", InterruptiveAbortSoundsOnMixerGroupEventHandler);
        }

        private void Initialize()
        {
            Instance = this;

            if (!mixer && masterMixerGroup)
                mixer = masterMixerGroup.audioMixer;
            if (!mixer && playlistMixerGroup)
                mixer = playlistMixerGroup.audioMixer;
            if (!mixer && effectsMixerGroup)
                mixer = effectsMixerGroup.audioMixer;

            if (!mixer)
                Multimorphic.P3App.Logging.Logger.LogWarning("Unable to find Audio Mixer.");

            FindGroups();

            AudioMixerGroup[] overrides = { chatterVoiceMixerGroup };
            mixerGroupOverrides.Add(instructionVoiceMixerGroup, overrides);

            AudioMixerGroup[] overrides2 = { instructionVoiceMixerGroup, chatterVoiceMixerGroup };
            mixerGroupOverrides.Add(shotFeedbackVoiceMixerGroup, overrides2);

            AudioMixerGroup[] overrides3 = { shotFeedbackVoiceMixerGroup, instructionVoiceMixerGroup, chatterVoiceMixerGroup };
            mixerGroupOverrides.Add(necessaryVoiceMixerGroup, overrides3);

            // One clip per voice channel
            mixerGroupMaximumClipCount.Add(necessaryVoiceMixerGroup, 1);
            mixerGroupMaximumClipCount.Add(shotFeedbackVoiceMixerGroup, 1);
            mixerGroupMaximumClipCount.Add(instructionVoiceMixerGroup, 1);
            mixerGroupMaximumClipCount.Add(chatterVoiceMixerGroup, 1);


            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = playlistMixerGroup;
            playlistSources.Add(source);

            source = gameObject.AddComponent<AudioSource>();
            source.loop = false;
            source.outputAudioMixerGroup = playlistMixerGroup;
            playlistSources.Add(source);

            SetPlaylistMasterVolumeLevel(GetPlaylistMasterVolumeLevel());

        }

        // Use this for initialization
        public override void Start()
        {
            base.Start();

            Instance = this;
            Audio.audioInterface = this;
            FindCamera();
            FindGroups();
        }

        private void FindGroups()
        {
            // Find all the audio mixer groups
            mixerGroups.Clear();
            AudioMixerGroup[] _mixerGroups = mixer.FindMatchingGroups("");
            foreach (AudioMixerGroup mixerGroup in _mixerGroups)
                mixerGroups.Add(mixerGroup.name, mixerGroup);

            // Find all the audio clip groups
            clipGroups.Clear();
            AudioClipGroup[] _audioGroups = GameObject.FindObjectsOfType<AudioClipGroup>();
            foreach (AudioClipGroup group in _audioGroups)
                clipGroups.Add(group.name, group);
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();

            if (mainCamera == null)
                FindCamera();

            //           if ((clipGroups.Count == 0) && (Time.timeSinceLevelLoad < 0.3f))
            //               FindGroups();

            if (moveWithCamera)
            {
                if (gameObject.transform.position != mainCamera.transform.position)
                    gameObject.transform.position = mainCamera.transform.position;
                if (gameObject.transform.rotation != mainCamera.transform.rotation)
                    gameObject.transform.rotation = mainCamera.transform.rotation;
            }

            // Handle playlists
            if (currentPlaylist != null)
            {
                if (!playlistSources[playlistSourceIndex].isPlaying)
                    TriggerNextPlaylistClip();
            }

            // Watchdog to make sure the right playlist is playing
            watchdogCountdown -= Time.deltaTime;
            if (watchdogCountdown < 0)
            {
                watchdogCountdown = watchdogInterval;

                UpdatePlaylist();
            }

            if (fadingSources.Count > 0)
            {
                foreach (var source in fadingSources)
                {
                    if (source != null)
                        source.volume -= Time.deltaTime * 120;  // fade rate of 120 decibels per second
                }
            }

            // Clean up expired clips
            cleanupCountdown -= Time.deltaTime;
            if (cleanupCountdown < 0)
            {
                cleanupCountdown = cleanupInterval;
                if (soundChains.Count > 0)
                {
                    cleanupIndex = ++cleanupIndex % soundChains.Count;
                    // Only check (and possibly clean up) one per frame for performance reasons.
                    if (soundChains[cleanupIndex].done)
                    {
                        foreach (var soundObject in soundChains[cleanupIndex].soundObjects)
                        {
                            if (fadingSources.Count > 0)
                                fadingSources.Remove(soundObject.GetComponent<AudioSource>());
                            DestroyImmediate(soundObject);
                        }
                        soundChains.RemoveAt(cleanupIndex);
                        fadingSources.Remove(null);   // Just in case

                        cleanupCountdown = 0.2f;  // Come back soon - if there's one there might be many
                    }
                }
            }
        }

        private void FindCamera()
        {
            Camera cam = Camera.main;
            if (!cam)
                cam = GameObject.FindObjectOfType<Camera>();

            if (cam)
            {
                mainCamera = cam.gameObject;
                gameObject.transform.position = mainCamera.transform.position;
                gameObject.transform.rotation = mainCamera.transform.rotation;

                // We have our own listener, so turn off the camera's listener (if it exists).
                AudioListener camListener = mainCamera.GetComponent<AudioListener>();
                if (camListener)
                    camListener.enabled = false;
            }
        }

        private AudioMixerGroup AudioMixerGroupByName(string name)
        {
            AudioMixerGroup group = null;
            mixerGroups.TryGetValue(name, out group);
            return (group);
        }

        void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            Instance = this;
            mainCamera = null;
            FindCamera();
            FindGroups();
        }


        private AudioClip GetClip(string name, out AudioClipGroup clipGroup, out VariableAudioClip variableClip)
        {
            string clipName = name;
            AudioClip clip = null;

            variableClip = null;
            clipGroup = null;
            if (clipGroups.TryGetValue(name, out clipGroup))
            {
                variableClip = clipGroup.NextClip();
                clipName = variableClip.clipName;
            }

            if (clips.ContainsKey(clipName))
                clip = clips[clipName];

            if (clip == null)
            {
                clip = Resources.Load<AudioClip>("Sound/" + clipName);
                if (clip)
                    clips.Add(clipName, clip);
                else
                    Multimorphic.P3App.Logging.Logger.Log("Could not find the clip Assets/Resources/Sound/" + name + " or an AudioClipGroup named " + name);
            }

            return (clip);
        }

        private void SetAudioSourceParameters(AudioSource source, AudioClipGroup clipGroup, VariableAudioClip variableClip)
        {
            if (variableClip)
            {
                source.pitch = Random.Range(variableClip.minPitch, variableClip.maxPitch);
                source.volume = Random.Range(variableClip.minVolume, variableClip.maxVolume);

                if (clipGroup.outputAudioMixerGroup)
                    source.outputAudioMixerGroup = clipGroup.outputAudioMixerGroup;
                else
                    source.outputAudioMixerGroup = defaultMixerGroup;
            }
            else
            {
                source.pitch = 1f;
                source.volume = 1f;
                source.outputAudioMixerGroup = defaultMixerGroup;
            }
        }

        /// <summary>
        /// Creates a game object with an AudioSource so an Audio clip can play.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="clipGroup"></param>
        /// <param name="variableClip"></param>
        /// <returns></returns>
        private GameObject CreateSoundObject(AudioClip clip, AudioClipGroup clipGroup = null, VariableAudioClip variableClip = null)
        {
            GameObject soundObject = new GameObject(clip.name);
            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.clip = clip;
            // Multimorphic.P3App.Logging.Logger.LogWarning("Playing " + clip.name);
            SetAudioSourceParameters(source, clipGroup, variableClip);
            // P3App.Logging.Logger.LogError("Created sound object for clip" + clip.name + " on " + source.outputAudioMixerGroup.name);
            source.Play();
            DontDestroyOnLoad(soundObject);  // We'll handle the deletion in Update
            return (soundObject);
        }


        /// <summary>
        /// Play a clip as if it were right next to the listener.  
        /// </summary>
		/// <param name="name">The name of the audio clip (which must reside in /Assets/Resources/Sound) or audio clip group (which must be the name of an AudioClipGroup in the scene or on the audio prefab.</param>
        public void PlaySound(string name)
        {
            PlaySound3D(name, gameObject.transform, gameObject.transform);
        }

        /// <summary>
        /// Play a clip as if it were right next to the listener.
        /// </summary>
		/// <param name="name">The name of the audio clip (which must reside in /Assets/Resources/Sound) or audio clip group (which must be the name of an AudioClipGroup in the scene or on the audio prefab.</param>
        public void PlaySound3D(string name)
        {
            PlaySound(name);
        }

        /// <summary>
        /// Play a clip as if it were at the position in the scene given by a transform.
        /// </summary>
        /// <param name="name">The name of the audio clip (which must reside in /Assets/Resources/Sound) or audio clip group (which must be the name of an AudioClipGroup in the scene or on the audio prefab.</param>
        /// <param name="transform">The transform whose position will be the point in the scene from which the sound will occur.  Used for positioning within the scene only, not within the hierarchy.</param>
        public void PlaySound3D(string name, Transform transform)
        {
            PlaySound3D(name, transform.position, null);
        }

        /// <summary>
        /// Play a clip as if it were at the given position in the scene.
        /// </summary>
        /// <param name="name">The name of the audio clip (which must reside in /Assets/Resources/Sound) or audio clip group (which must be the name of an AudioClipGroup in the scene or on the audio prefab.</param>
        /// <param name="position">The position in the scene from which the sound will occur.</param>
        public void PlaySound3D(string name, Vector3 position)
        {
            PlaySound3D(name, position, null);
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="index">The index of the clip.  Ignored.</param>
        /// <param name="transform">The transform whose position will be the point in the scene from which the sound will occur.  Ignored.</param>
        public void PlaySound3D(int index, Transform transform)
        {
            // not implemented			
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="index">The index of the clip.  Ignored.</param>
        /// <param name="position">The position in the scene from which the sound will occur.  Ignored.</param>
        public void PlaySound3D(int index, Vector3 position)
        {
            // not implemented			
        }

        /// <summary>
        /// Play a clip as if it were at the position given by a transform.  Optionally, attach the AudioSource to the transform's game object.
        /// </summary>
        /// <param name="name">The name of the audio clip (which must reside in /Assets/Resources/Sound) or audio clip group (which must be the name of an AudioClipGroup in the scene or on the audio prefab.</param>
        /// <param name="transform">The transform whose position will be the point in the scene from which the sound will occur.</param>
        /// <param name="attachToSource">If <c>true</c>, the clip will be played in an AudioSource attached to the transform's game object.  Regardless, the lifetime of the AudioSource for the clip (and its gameObject) will be handled by this class.</param>
        /// <returns>An AudioSource which plays the clip.  It may be a component of this object or it may be a component of an empty game object as a child of the given transform, depending on the value of attachToSource.</returns>
        public object PlaySound3D(string name, Transform transform, bool attachToSource)
        {
            if (attachToSource)
                return (PlaySound3D(name, transform.position, transform));
            else
                return (PlaySound3D(name, transform.position, null));
        }

        public object PlaySound3D(string name, Vector3 position, Transform parent)
        {
            return (PlaySound3D(name, position, parent, null));
        }

        /// <summary>
		/// Play a clip as if it were at the position given.  Optionally, attach the resulting sound game object to a transform.
		/// </summary>
		/// <param name="name">The name of the audio clip (which must reside in /Assets/Resources/Sound) or audio clip group (which must be the name of an AudioClipGroup in the scene or on the audio prefab.</param>
		/// <param name="position">The position in the scene from which the sound will occur.</param>
		/// <param name="parent">The transform which will be the parent of the sound object that is created.</param>
		/// <returns>An AudioSource which plays the clip.  It may be a component of this object or it may be a component of an empty game object as a child of the given transform, depending on the value of attachToSource.</returns>
        public object PlaySound3D(string name, Vector3 position, Transform parent, AudioMixerGroup audioMixerGroup)
        {
            // This is the method that all the other variants of PlaySound and PlaySound3D call.

            AudioSource source = null;
            AudioClipGroup clipGroup = null;
            VariableAudioClip variableClip = null;
            AudioClip clip = GetClip(name, out clipGroup, out variableClip);

            //P3App.Logging.Logger.LogWarning("Could not find clip for " + name);

            bool freeToPlayClip = true;

            string existingClipName = "";

            if (clipGroup != null)
                audioMixerGroup = clipGroup.outputAudioMixerGroup;

            // Check mixer group clip count limitations
            if ((clipGroup != null) && (audioMixerGroup != null))
            {
                int maxClipCount = 0;
                if (mixerGroupMaximumClipCount.TryGetValue(audioMixerGroup, out maxClipCount))
                {
                    // The target mixer group has a limit on the number of clips it will simultaneously support.  Are we free to play?

                    int existingClipCount = 0;
                    foreach (var existingSoundChain in soundChains)
                    {
                        var existingClipGroup = existingSoundChain.currentClipGroup;
                        if ((existingClipGroup != null) && !existingSoundChain.done && (existingClipGroup.outputAudioMixerGroup == audioMixerGroup))
                        {
                            // An existing clip is on the target mixer group
                            existingClipCount++;
                            existingClipName = existingClipGroup.name;
                        }
                    }

                    // Is this a clip too far?
                    freeToPlayClip = (existingClipCount < maxClipCount);
                }
            }

            if (!freeToPlayClip)
            {
                Multimorphic.P3App.Logging.Logger.LogWarning("Could not play sound " + name + " due to mixer group clip limit and existing clip " + existingClipName);
                return null;
            }

            SoundChain newSoundChain = new SoundChain();
            newSoundChain.initialSoundName = name;

            while (clip != null)
            {
                newSoundChain.Add(clip, variableClip, clipGroup);

                FollowUpAudioClipGroup followUp = null;
                if (variableClip)
                    followUp = variableClip.gameObject.GetComponent<FollowUpAudioClipGroup>();

                if (clipGroup && !followUp)
                    followUp = clipGroup.gameObject.GetComponent<FollowUpAudioClipGroup>();

                newSoundChain.followUps.Add(followUp);

                if (followUp)
                    clip = GetClip(followUp.nextClipGroup.name, out clipGroup, out variableClip);
                else
                    clip = null;
            }

            // To enabled aborting sounds, everything is a sound chain.  Even single sounds are considered a sound chain of length 1.
            soundChains.Add(newSoundChain);
            StartCoroutine(AdvanceSoundChain_Coroutine(newSoundChain));

            return (source);
        }

        public IEnumerator AdvanceSoundChain_Coroutine(SoundChain soundChain)
        {
            //Multimorphic.P3App.Logging.Logger.LogError("Soundchain coroutine for index " + soundChain.index + " of chain with primary " + soundChain.initialSoundName);
            if (soundChain.doneAfterCurrentClipEnds)
                soundChain.done = true;
            else
            {
                if (soundChain.index >= 0)
                {
                    // We're currently playing a sound in the chain.  Wait for it to finish.
                    var followUp = soundChain.followUps[soundChain.index];
                    var parentClip = soundChain.clips[soundChain.index];
                    var delay = parentClip.length;
                    if (followUp)
                    {
                        delay = followUp.delayFrom == FollowAudioClipGroupDelayFrom.WhenParentClipStarts ? followUp.delay : followUp.delay + parentClip.length;

                        //if (parentClip)
                        //Multimorphic.P3App.Logging.Logger.LogError("delaying " + delay + " seconds for " + followUp.nextClipGroup.name + ", the followup to " + parentClip.name);
                    }

                    // Always delay so that we can come back and either play the next clip in the sound chain or mark the sound chain as done.
                    // During this yield, the soundchain may be told it's done, or doneAfterCurrentClipEnds
                    yield return new WaitForSeconds(delay);
                    //Multimorphic.P3App.Logging.Logger.LogError("Returned from delay of " + delay + " for " + soundChain.initialSoundName + soundChain.done + soundChain.doneAfterCurrentClipEnds);
                }

                // We're back from the delay for the last clip (i.e. index > 0), or we've yet to play the first clip of the chain (i.e. index == -1).  

                if (soundChain.done)
                {
                    // Nothing to do.  Do nothing.
                }
                else if (soundChain.doneAfterCurrentClipEnds)
                    soundChain.done = true;
                else
                {
                    // Play the next sound in the chain
                    soundChain.index++;
                    //Multimorphic.P3App.Logging.Logger.LogError("Processing next clip, index " + soundChain.index + " of chain with primary " + soundChain.initialSoundName + " " + soundChain.done + soundChain.doneAfterCurrentClipEnds + soundChain.clips.Count);


                    if (!soundChain.done && (soundChain.index < soundChain.clips.Count))
                    {
                        var clipGroup = soundChain.clipGroups[soundChain.index];
                        GameObject soundObject = CreateSoundObject(soundChain.clips[soundChain.index], soundChain.clipGroups[soundChain.index], soundChain.variableClips[soundChain.index]);
                        soundObject.transform.position = soundChain.position;
                        if (soundChain.parent)
                            soundObject.transform.parent = soundChain.parent;
                        soundChain.soundObjects.Add(soundObject);

                        //Multimorphic.P3App.Logging.Logger.LogError("Launching coroutine to advance sound chain, index " + (soundChain.index+1) + " of chain with primary " + soundChain.initialSoundName);
                        // Do this routine again, to await the end of the sound we just created.
                        StartCoroutine(AdvanceSoundChain_Coroutine(soundChain));

                        // If there are mixer group overrides, abort the sounds in those mixer groups.
                        if ((clipGroup != null) && (clipGroup.outputAudioMixerGroup != null))
                        {
                            AudioMixerGroup[] mixerGroupsToAbort;
                            mixerGroupOverrides.TryGetValue(clipGroup.outputAudioMixerGroup, out mixerGroupsToAbort);

                            if ((mixerGroupsToAbort != null) && (mixerGroupsToAbort.Length > 0))
                                StartCoroutine(AbortMixerGroupCoroutine(mixerGroupsToAbort, soundChain));
                        }
                    }
                    else
                        soundChain.done = true;
                }
            }
        }

        public IEnumerator AbortMixerGroupCoroutine(AudioMixerGroup[] mixerGroupsToAbort, SoundChain exception)
        {
            //Multimorphic.P3App.Logging.Logger.LogError("Abort mixer group " + mixerGroupsToAbort[0].name + " +" + (mixerGroupsToAbort.Length - 1));

            // Allow the ducking to fade out the sounds before we abort them.
            yield return new WaitForSeconds(0.1f);   // because the ducking attack time is 0.75, so clip should be inaudible by 0.8

            foreach (var mixer in mixerGroupsToAbort)
                AbortSoundsOnMixerGroup(mixer, exception);
        }



        /// <summary>
        /// Is there a limitation on playing the given clipGroup or variableClip
        /// due to having reached the maximum number of simultaneous clips?
        /// Searches for an AudioClipLimiter in the parents in order to determine
        /// the simultaneous clip limit.
        /// </summary>
        /// <param name="clipGroup"></param>
        /// <param name="variableClip"></param>
        /// <returns>
        /// True if the limit has been reached, otherwise False
        /// </returns>
        private AudioClipLimiter GetLimiter(AudioClipGroup clipGroup, VariableAudioClip variableClip)
        {
            AudioClipLimiter limiter = null;

            if (variableClip)
            {
                limiter = NearestLimiter(variableClip.gameObject);
            }

            if ((clipGroup != null) && (limiter == null))
            {
                limiter = NearestLimiter(clipGroup.gameObject);
            }

            return (limiter);
        }

        /// <summary>
        /// Searches upward in the hierarchy to find the nearest AudioClipLimiter
        /// </summary>
        /// <param name="go">A GameObject.</param>
        /// <returns>
        /// The nearest AudioClipLimiter in the parents in the hierarchy
        /// </returns>
        private AudioClipLimiter NearestLimiter(GameObject go)
        {
            AudioClipLimiter limiter = null;
            Transform t = go.transform;

            while ((t != null) && (limiter == null))
            {
                limiter = t.gameObject.GetComponent<AudioClipLimiter>();
                t = t.parent;
            }

            return (limiter);
        }

        public IEnumerator FollowUpCoroutine(string priorName, string followupName, Vector3 position, Transform parent, float delay)
        {
            // During this yield, the prior clip may have been aborted, in which case the value (which was priorName above) will get cleared, indicating the followup shouldn't play.
            yield return new WaitForSeconds(delay);

            // Is this followup still pending?
            string existingPriorName = "";

            if (existingPriorName != "")
            {
                // Multimorphic.P3App.Logging.Logger.LogError("====== Playing " + followupName);
                PlaySound3D(followupName, position, parent);   // Yep, still pending.  Play!
            }
        }


        /// <summary>
        /// Abort a sound clip (by name) if it is playing.  Aborts all associated followups.
        /// </summary>
        /// <param name="clipName"></param>
        public void AbortSound(string clipName, bool stopClipIfPlaying)
        {
            string shortenedClipName = Path.GetFileName(clipName);

            //Multimorphic.P3App.Logging.Logger.LogError("Aborting clip " + clipName + "...");

            foreach (var soundChain in soundChains)
            {
                foreach (var clip in soundChain.clips)
                {
                    if ((clip.name == clipName) || (clip.name == shortenedClipName) || (soundChain.initialSoundName == clipName))
                    {
                        // Soundchain contains the abortive clip.  Abort the sound chain.
                        soundChain.doneAfterCurrentClipEnds = true;
                        if (stopClipIfPlaying)
                        {
                            //Multimorphic.P3App.Logging.Logger.LogError("      ... hard aborting " + soundChain.initialSoundName);
                            if (soundChain.index >= 0 && soundChain.index < soundChain.soundObjects.Count)
                                fadingSources.Add(soundChain.soundObjects[soundChain.index].GetComponent<AudioSource>());
                        }
                        else
                        {
                            //Multimorphic.P3App.Logging.Logger.LogError("      ... soft aborting " + soundChain.initialSoundName);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Abort all sound clips and followups that are associated with a particular mixer by name.  Aborts all associated followups.
        /// </summary>
        /// <param name="mixerGroupName"></param>
        public void AbortSoundsOnMixerGroup(string mixerGroupName, bool interruptiveAbort = false)
        {
            var mixerGroup = mixerGroups[mixerGroupName];
            AbortSoundsOnMixerGroup(mixerGroup, null, interruptiveAbort);
        }

        /// <summary>
        /// Abort all sound clips and followups that are associated with a particular mixer.  Aborts all associated followups.
        /// </summary>
        /// <param name="mixerGroup"></param>
        /// <param name="exception">Which sound chain can be ignored during this action?</param>
        /// <param name="interruptiveAbort">Should the aborted clips be stop playing immediately, or after the current clip completes?</param>
        public void AbortSoundsOnMixerGroup(AudioMixerGroup mixerGroup, SoundChain exception = null, bool interruptiveAbort = false)
        {
            foreach (var soundChain in soundChains)
            {
                if ((soundChain != exception) && (soundChain.index >= 0) && (soundChain.index < soundChain.soundObjects.Count))
                {
                    var soundObject = soundChain.soundObjects[soundChain.index];

                    if (soundObject)
                    {
                        var source = soundObject.GetComponent<AudioSource>();
                        if (source && (source.outputAudioMixerGroup == mixerGroup))
                        {
                            // Multimorphic.P3App.Logging.Logger.LogError("BJTEST Aborting soundchain with primary " + soundChain.initialSoundName);
                            foreach (var clip in soundChain.clips)
                                AbortSound(clip.name, interruptiveAbort);
                        }
                    }
                }
            }
        }


        public void AbortSoundEventHandler(string eventName, object eventData)
        {
            this.AbortSound((string)eventData, true);
        }

        public void AbortFollowupSoundsEventHandler(string eventName, object eventData)
        {
            this.AbortSound((string)eventData, false);
        }


        public void AbortSoundsOnMixerGroupEventHandler(string eventName, object eventData)
        {
            string mixerGroupName = (string)eventData;
            this.AbortSoundsOnMixerGroup(mixerGroupName, false);
        }

        public void InterruptiveAbortSoundsOnMixerGroupEventHandler(string eventName, object eventData)
        {
            string mixerGroupName = (string)eventData;
            this.AbortSoundsOnMixerGroup(mixerGroupName, true);
        }




        public void PlaySound3DChain(List<string> names, Transform transform)
        {
            StartCoroutine(PlaySoundChainCoroutine(names, transform));
        }

        public IEnumerator PlaySoundChainCoroutine(List<string> names, Transform transform)
        {
            foreach (string name in names)
            {
                AudioClipGroup clipGroup = null;
                VariableAudioClip variableClip = null;
                AudioClip clip = GetClip(name, out clipGroup, out variableClip);
                GameObject soundObject = new GameObject(clip.name);
                soundObject.transform.position = transform.position;
                AudioSource source = soundObject.AddComponent<AudioSource>();
                if (clip)
                {
                    source.clip = clip;
                    SetAudioSourceParameters(source, clipGroup, variableClip);
                    source.Play();
                    yield return new WaitForSeconds(source.clip.length);
                }
                Destroy(soundObject);
            }
        }

        /// <summary>
        /// Stops all playlists.
        /// </summary>
        public void StopAllPlaylists()
        {
            foreach (AudioSource source in playlistSources)
            {
                source.Stop();
            }
            currentPlaylist = null;
            currentPlaylistRequest = null;
        }


        /// <summary>
        /// Changes the name of the playlist by.
        /// </summary>
        /// <param name="name">Name of the playlist to change to.</param>
        public void ChangePlaylistByName(string name)
        {
            ChangePlaylistByName(name, -1, 0);
        }

        public void ChangePlaylistByNameAfterCurrentClipEnds(string name)
        {
            ChangePlaylistByName(name, -1, 0, true);
        }

        public void ChangePlaylistByName(string name, int resumePlaylistClipIndex = -1, float resumeClipPosition = 0, bool allowCurrentClipToEndBeforeChange = false)
        {
            bool found = false;
            Multimorphic.P3App.Logging.Logger.Log("Starting playlist " + name);

            if ((currentPlaylist == null) || (currentPlaylist.PlaylistName != name))
            {    // no need to change to a playlist that's already playing.
                Playlist[] playlists = gameObject.GetComponents<Playlist>();
                //  currentPlaylist = null;         // don't clear the current playlist unless we really have the one requested
                foreach (Playlist playlist in playlists)
                {
                    if (playlist.PlaylistName == name)
                    {
                        // Set the new playlist
                        currentPlaylist = playlist;
                        playlistClipIndex = -1;
                        if (!allowCurrentClipToEndBeforeChange)
                            TriggerNextPlaylistClip(resumePlaylistClipIndex, resumeClipPosition);
                        found = true;
                    }
                }

                if (!found)
                    Multimorphic.P3App.Logging.Logger.Log("Could not find playlist " + name);
            }
        }


        public void TriggerNextPlaylistClip()
        {
            TriggerNextPlaylistClip(-1, 0);
        }

        /// <summary>
        /// Start playing the next clip in a playlist.
        /// </summary>
        public void TriggerNextPlaylistClip(int resumePlaylistClipIndex = -1, float resumeClipPosition = 0)
        {
            if (currentPlaylist != null)
            {
                playlistSources[playlistSourceIndex].Stop();
                playlistSourceIndex = (playlistSourceIndex + 1) % playlistSources.Count;
                AudioSource source = playlistSources[playlistSourceIndex];

                if (currentPlaylist.clips.Count > 0)
                {
                    if (resumePlaylistClipIndex == -1)
                        playlistClipIndex++;
                    else
                        playlistClipIndex = resumePlaylistClipIndex;

                    if (playlistClipIndex >= currentPlaylist.clips.Count)
                    {
                        playlistClipIndex = 0;
                        if (currentPlaylist.playFirstClipOnlyOnce)
                            playlistClipIndex = 1;
                    }

                    if (playlistClipIndex < currentPlaylist.clips.Count)
                    {    // this condition is necessary in case there's one or zero clips and playFirstClipOnlyOnce==true.
                        source.clip = currentPlaylist.clips[playlistClipIndex];
                        source.time = resumeClipPosition;
                        source.loop = false;
                        source.Play();

                        if (currentPlaylistRequest != null)
                            currentPlaylistRequest.allowCurrentClipToEndBeforeChange = false; // We only delay starting when first satisfying the request.
                    }
                }
            }
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="name">Name.  Ignored.</param>
        public void RefillSoundGroupPool(string name)
        {
            // not implemented			
        }

        /// <summary>
        /// Gets the master volume level.
        /// </summary>
        /// <returns>The master volume level (0f - 1f).</returns>
        public float GetMasterVolumeLevel()
        {
            return (masterVolumeLevel);
        }

        /// <summary>
        /// Sets the master volume level.
        /// </summary>
        /// <param name="value">Volume (0f - 1f).</param>
        public void SetMasterVolumeLevel(float value)
        {
            // Multimorphic.P3App.Logging.Logger.Log ("Setting Master Volume level to " + value.ToString ());
            masterVolumeLevel = Mathf.Clamp01(value);

            if (mixer)
                mixer.SetFloat("MasterVolume", LogarithmicPercentToDb(masterVolumeLevel));

        }

        /// <summary>
        /// Gets the playlist volume level.
        /// </summary>
        /// <returns>The playlist volume level (0f - 1f).</returns>
        public float GetPlaylistMasterVolumeLevel()
        {
            return (playlistMasterVolumeLevel);
        }

        /// <summary>
        /// Sets the playlist volume level.
        /// </summary>
        /// <param name="value">Volume (0f - 1f).</param>
        public void SetPlaylistMasterVolumeLevel(float value)
        {
            // Multimorphic.P3App.Logging.Logger.Log ("Setting Playlist Master Volume level to " + value.ToString ());
            playlistMasterVolumeLevel = Mathf.Clamp01(value);
            if (mixer)
                mixer.SetFloat("PlaylistVolume", LinearPercentToDb(playlistMasterVolumeLevel));
        }

        public void SetSoundtrackVolumeEventHandler(string eventName, object eventData)
        {
            SetPlaylistMasterVolumeLevel((float)eventData);
        }

        public void SetBassGainEventHandler(string eventName, object eventData)
        {
            mixer.SetFloat("MasterBassGain", (float)eventData);
        }

        public void ChangePlaylistEventHandler(string eventName, object eventData)
        {
            ChangePlaylistByName((string)eventData);
        }

        public void RequestPlaylistEventHandler(string eventName, object eventData)
        {
            playlistRequests.Add((PlaylistRequest)eventData);
            UpdatePlaylist();
        }

        /// <summary>
        /// Remove all playlist requests either (a) from a particular mode or (b) for a particular playlist name
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventData">Can be a mode or a string (playlist name).  All requests related to either will be removed.</param>
        public void RemovePlaylistRequestsEventHandler(string eventName, object eventData)
        {
            var mode = eventData as P3Mode;
            string playlistName = "";
            if (mode == null)
                playlistName = (string)eventData;

            playlistRequests.RemoveAll(request => (request.requestingMode == mode) || (request.playlistName == playlistName));
            UpdatePlaylist();
        }

        private void UpdatePlaylist()
        {
            PlaylistRequest best = null;

            for (int i = 0; i < playlistRequests.Count; i++)     // Why did a foreach here occasionally not iterate?
            {
                PlaylistRequest request = playlistRequests[i];

                if (best == null)
                    best = request;
                else if (request.requestingMode.Priority >= best.requestingMode.Priority) // With equal priority, later requests win
                    best = request;
            }

            if (best == null)
                StopAllPlaylists();
            else if ((currentPlaylistRequest != best) || (currentPlaylistRequest.allowCurrentClipToEndBeforeChange))
            {
                if ((currentPlaylist == null) || (best.playlistName != currentPlaylist.PlaylistName))
                {
                    var currentSource = playlistSources[playlistSourceIndex];
                    // Remember the resumption point for later return to the current playlist
                    if ((currentPlaylistRequest != null) && (currentSource != null))
                    {
                        currentPlaylistRequest.resumePlaylistClipIndex = playlistClipIndex;
                        currentPlaylistRequest.resumePlaylistClipPosition = currentSource.time;
                    }

                    // Change to the new playlist
                    ChangePlaylistByName(best.playlistName, best.resumePlaylistClipIndex, best.resumePlaylistClipPosition, best.allowCurrentClipToEndBeforeChange);
                    currentPlaylistRequest = best;
                }
            }
        }

        private float LogarithmicPercentToDb(float percent)
        {
            // convert 0..1 to -80..20 dB
            percent = Mathf.Clamp01(percent);
            return Mathf.Pow(percent, 0.24f) * 80f - 60f;
        }

        private float LinearPercentToDb(float percent)
        {
            // convert 0..1 to -80..20 dB
            percent = Mathf.Clamp01(percent);
            return (percent * 100f - 80f);
        }
    }
}

