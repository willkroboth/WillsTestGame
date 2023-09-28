using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

namespace PinballClub.TestGame.GUI
{

    public class AudioClipGroup : MonoBehaviour
    {

        public AudioMixerGroup outputAudioMixerGroup = null;
        public bool playClipsSequentially = true;

        private int clipIndex = 0;
        private List<VariableAudioClip> variableClips = new List<VariableAudioClip>();  // known clips
        private List<VariableAudioClip> orderedClips = new List<VariableAudioClip>();  // ordered/shuffled/random clips

        // Use this for initialization
        void Start()
        {
            variableClips.Clear();

            variableClips.AddRange(gameObject.GetComponents<VariableAudioClip>());
            variableClips.AddRange(gameObject.GetComponentsInChildren<VariableAudioClip>());
            CalculateWeightedOrder();
        }

        void CalculateWeightedOrder()
        {
            orderedClips.Clear();
            foreach(VariableAudioClip clip in variableClips)
            {
                for (int i = 0; i < clip.weight; i++)
                    orderedClips.Add(clip);
            }

            if (!playClipsSequentially)
            {
                // Random shuffle
                int count = orderedClips.Count;
                for (int i=0; i < count; i++)
                {
                    int j = Random.Range(0, count * 10) % count;
                    int k = Random.Range(0, count * 10) % count;
                    VariableAudioClip temp = orderedClips[j];
                    orderedClips[j] = orderedClips[k];
                    orderedClips[k] = temp;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public VariableAudioClip NextClip()
        {
            VariableAudioClip result = null;

            if ((variableClips.Count > 0) && (orderedClips.Count > 0))
            {
                if (clipIndex < orderedClips.Count)
                    result = orderedClips[clipIndex];

                clipIndex = (clipIndex + 1) % orderedClips.Count;
                if ((clipIndex == 0) && !playClipsSequentially)
                    CalculateWeightedOrder();   // reshuffle
            }

            return (result);
        }
    }
}