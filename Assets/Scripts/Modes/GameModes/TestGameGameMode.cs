using Multimorphic.NetProcMachine.Machine;
using Multimorphic.NetProcMachine.LEDs;
using System.Collections.Generic;
using Multimorphic.P3App.Modes;
using PinballClub.TestGame.Modes;
using Multimorphic.P3;
using PinballClub.TestGame.GUI;

namespace PinballClub.TestGame.Modes
{
	public class TestGameGameMode : GameMode
	{

		public TestGameGameMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
		}
		
		public override void mode_started ()
		{
			base.mode_started ();
		}

		public override void mode_stopped ()
		{
			base.mode_stopped();
		}

		public virtual ModeSummary getModeSummary()
		{
			ModeSummary modeSummary = new ModeSummary();
			modeSummary.SetItemAndValue(0, "abc", "123");
			modeSummary.SetItemAndValue(1, "def", "456");
			modeSummary.SetItemAndValue(2, "ghi", "789");
			return modeSummary;
		}


        /// <summary>
        /// Request a playlist to be played.
        /// </summary>
        /// <param name="playlistName"></param>
        /// <param name="allowCurrentClipToEndBeforeChange">If true, the current playlist clip will play to its conclusion before the requested cliip is played (queueing).  If false, the requested clip will play immediately, interrupting the current playlist.</param>
        protected void RequestPlaylist(string playlistName, bool allowCurrentClipToEndBeforeChange = false)
        {
            PostModeEventToGUI("Evt_RequestPlaylist", new PlaylistRequest(this, playlistName, allowCurrentClipToEndBeforeChange));
        }


        /// <summary>
        /// Remove all playlist requests that this mode has requested.
        /// </summary>
        protected void RemovePlaylistRequests()
        {
            PostModeEventToGUI("Evt_RemovePlaylistRequests", this);
        }

        /// <summary>
        /// Remove all playlist requests for the given playlist name (regardless of which mode requested it)
        /// </summary>
        /// <param name="playlistName"></param>
        protected void RemovePlaylistRequests(string playlistName)
        {
            PostModeEventToGUI("Evt_RemovePlaylistRequests", playlistName);
        }

        /// <summary>
        /// Play a sound.
        /// </summary>
        /// <param name="soundName">Can be the name of a VariableClipGroup or the path of a sound clip relative to Assets/Resources/Sound. </param>
        protected void PlaySound(string soundName)
        {
            PostModeEventToGUI("Evt_PlaySound", soundName);
        }

        /// <summary>
        /// Abort a sound and any followup sounds.  If the sound clip or followup is already playing, it will not be interrupted and will play to the clip's conclusion, 
        /// but any followups will not be played.
        /// </summary>
        /// <param name="soundName">Can be the name of a VariableClipGroup or the path of a sound clip relative to Assets/Resources/Sound. </param>
        protected void AbortSound(string soundName)
        {
            PostModeEventToGUI("Evt_AbortSound", soundName);
        }

        /// <summary>
        /// Abort all sounds on mixer group, with the option to hard or soft abort any sound that is currently playing.
        /// </summary>
        /// <param name="mixerGroupName">The name of the mixer group to abort sounds on.</param>
        /// <param name="interruptiveAbort">If true, sound clips will be hard interrupted.  If the sound clip is already playing, it will cut off in process.
        /// If false, any clip that is already playing will not be interrupted and will play to the clip's conclusion.
        /// Either way, any subsequent followups will not be played.</param>
        protected void AbortSoundsOnMixerGroup(string mixerGroupName, bool interruptiveAbort = false)
        {
            if (interruptiveAbort)
                PostModeEventToGUI("Evt_InterruptiveAbortSoundsOnMixerGroup", mixerGroupName);
            else
                PostModeEventToGUI("Evt_AbortSoundsOnMixerGroup", mixerGroupName);
        }


    }
}


