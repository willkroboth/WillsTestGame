using Multimorphic.NetProcMachine.Machine;
using Multimorphic.NetProcMachine.LEDs;
using System;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;

using System.Collections.Generic;

namespace PinballClub.TestGame.Modes
{

    public class IntroVideoMode : SceneMode
	{
		public IntroVideoMode (P3Controller controller, int priority, string SceneName)
			: base(controller, priority, SceneName)
		{
			AddGUIEventHandler ("Evt_IntroVideoStopped", FinishIntroEventHandler);
		}

		public override void mode_started ()
		{
			base.mode_started();
		}

		public override void mode_stopped ()
		{
			base.mode_stopped();
		}

		public override void SceneLiveEventHandler( string evtName, object evtData )
		{
			//base.SceneLiveEventHandler(evtName, evtData);
			PostModeEventToGUI("Evt_PlayIntroVideo", 0);
		}

		private void Abort()
		{
			PostModeEventToGUI("Evt_StopIntroVideo", 0);
		}

		private void FinishIntroEventHandler(string evtName, object evtData)
		{
			PostModeEventToModes ("Evt_GameIntroComplete", 0);
			Abort ();
		}

		public override bool sw_buttonRight0_active(Switch sw)
		{
			Abort();
			return SWITCH_CONTINUE;
		}

		public override bool sw_buttonLeft0_active(Switch sw)
		{
			Abort();
			return SWITCH_CONTINUE;
		}

	}
}

