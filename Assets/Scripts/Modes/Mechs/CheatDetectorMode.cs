using System;
using System.Collections.Generic;
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;

namespace PinballClub.TestGame.Modes
{

	/// <summary>
	/// A mode that detects grid innapopriate switch activity.
	/// </summary>
    public class CheatDetectorMode : P3Mode
	{
		private double lastGridTime;
		private const double FAST_SWITCH_TIME = 2;
		private const double SLOW_SWITCH_TIME = 4;

		public CheatDetectorMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			AddModeEventHandler("Grid Event", GridEventHandler, Priority);

			List<string> switchNames = new List<string>();
			switchNames.Add ("leftTarget");
			switchNames.Add ("rightTargetLow");
			switchNames.Add ("rightTargetHigh");
			switchNames.Add ("modeHole");
			switchNames.Add ("rightRamp");

			foreach (string name in switchNames)
				add_switch_handler(name, "active", 0, FastSwitchEventHandler);

			switchNames.Clear();
			switchNames.Add ("leftRamp");

			foreach (string name in switchNames)
				add_switch_handler(name, "active", 0, SlowSwitchEventHandler);
		}

 		public override void mode_started ()
		{
			base.mode_started ();
			lastGridTime = 0;
		}

		public bool GridEventHandler(string evtName, object evtData)
		{
			lastGridTime = Multimorphic.P3.Tools.Time.GetTime();
			return SWITCH_CONTINUE;
		}

		public bool FastSwitchEventHandler(Switch sw)
		{
			if (Multimorphic.P3.Tools.Time.GetTime() - lastGridTime > FAST_SWITCH_TIME)
				ReactToCheat();
			return SWITCH_CONTINUE;
		}

		public bool SlowSwitchEventHandler(Switch sw)
		{
			if (Multimorphic.P3.Tools.Time.GetTime() - lastGridTime > SLOW_SWITCH_TIME)
				ReactToCheat();
			return SWITCH_CONTINUE;
		}

		/// <summary>
		/// React to a detected cheat.
		/// </summary>
		private void ReactToCheat()
		{
			if (data.GetGameAttributeValue("CheatDetection").ToInt() == 2 || 
			    (!p3.simulated && data.GetGameAttributeValue("CheatDetection").ToInt() == 1))
			{
				data.currentPlayer.SaveData("Cheater", true);
				PostModeEventToModes ("Evt_ShowPopup", "Cheater!!!");
				PostModeEventToGUI("Evt_Cheater", 0);
			}
		}
	
	} 

}

