using System;
using System.Collections.Generic;
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.Modes
{

    public class HomeIntroMode : GameMode
	{
		public HomeIntroMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			//AddGUIEventHandler ("Evt_SceneOutroComplete", SceneOutroCompleteEventHandler);
			AddGUIEventHandler ("Evt_IntroVideoStopped", FinishIntroEventHandler);
		}

 		public override void mode_started ()
		{
			base.mode_started();
			PostModeEventToGUI("Evt_PlayIntroVideo", 0);
		}

 		public override void mode_stopped ()
		{
			base.mode_stopped();
		}

		/*
		private void SceneOutroCompleteEventHandler(string evtName, object evtData)
		{
			Multimorphic.P3App.Logging.Logger.Log ("HomeIntroMode: Finished Zooming");
		}
		*/

		private void Abort()
		{
			PostModeEventToGUI("Evt_StopIntroVideo", 0);
		}

		private void FinishIntroEventHandler(string evtName, object evtData)
		{
			PostModeEventToModes ("Evt_HomeIntroComplete", 0);
		}

		public virtual bool sw_buttonRight0_active(Switch sw)
		{
			if (p3.Switches["buttonLeft0"].IsActive())
				Abort();
			return SWITCH_CONTINUE;
		}

		public virtual bool sw_buttonLeft0_active(Switch sw)
		{
			if (p3.Switches["buttonRight0"].IsActive())
				Abort();
			return SWITCH_CONTINUE;
		}
	}
}

