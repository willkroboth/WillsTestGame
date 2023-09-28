
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.NetProcMachine.LEDs;
using System;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Logging;

using System.Collections.Generic;

namespace PinballClub.TestGame.Modes
{

    public class TripleTargetsMode : TestGameGameMode
	{
		private List<bool> targetStates;
		private bool allTargetsHit;

		public TripleTargetsMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
		}

		public override void mode_started ()
		{
			base.mode_started ();
			Reset ();
		}

		public void Reset()
		{
			targetStates = new List<bool>();
			targetStates.Add (false);
			targetStates.Add (false);
			targetStates.Add (false);
			LEDScriptsDict["rightTargetHigh"] = LEDHelpers.BlinkLED(p3, LEDScriptsDict["rightTargetHigh"],  Multimorphic.P3.Colors.Color.blue);
			LEDScriptsDict["rightTargetLow"] = LEDHelpers.BlinkLED(p3, LEDScriptsDict["rightTargetLow"],  Multimorphic.P3.Colors.Color.blue);
			LEDScriptsDict["leftTarget"] = LEDHelpers.BlinkLED(p3, LEDScriptsDict["leftTarget"],  Multimorphic.P3.Colors.Color.blue);

			allTargetsHit = false;
			Multimorphic.P3App.Logging.Logger.Log ("Triple targets mode reset");
		}

		public override void mode_stopped ()
		{
			base.mode_stopped();
		}

		public bool sw_leftTarget_active(Switch sw)
		{
			LEDScriptsDict["leftTarget"] = LEDHelpers.PulseLED(p3, LEDScriptsDict["leftTarget"],  Multimorphic.P3.Colors.Color.red,  Multimorphic.P3.Colors.Color.white);
			TargetHit(0);
			return SWITCH_CONTINUE;
		}

		public bool sw_rightTargetHigh_active(Switch sw)
		{
			LEDScriptsDict["rightTargetHigh"] = LEDHelpers.PulseLED(p3, LEDScriptsDict["rightTargetHigh"],  Multimorphic.P3.Colors.Color.red,  Multimorphic.P3.Colors.Color.white);
			TargetHit(2);
			return SWITCH_CONTINUE;
		}

		public bool sw_rightTargetLow_active(Switch sw)
		{
			LEDScriptsDict["rightTargetLow"] = LEDHelpers.PulseLED(p3, LEDScriptsDict["rightTargetLow"],  Multimorphic.P3.Colors.Color.red,  Multimorphic.P3.Colors.Color.white);
			TargetHit(1);
			return SWITCH_CONTINUE;
		}

		protected virtual void TargetHit (int index)
		{
			Multimorphic.P3App.Logging.Logger.Log ("triple target hit " + index.ToString());
			if (!targetStates[index])
			{
				TargetHitGUI(index);
			}
			else
			{
				TargetAlreadyHitGUI(index);
			}
			targetStates[index] = true;
			CheckForTargetsOff();
		}

		protected virtual void TargetHitGUI(int index)
		{
		}

		protected virtual void TargetAlreadyHitGUI(int index)
		{
		}

		private void CheckForTargetsOff()
		{
			Multimorphic.P3App.Logging.Logger.Log ("allTargetsHit: " + allTargetsHit.ToString());
			for (int i=0; i<targetStates.Count; i++)
			{
				if (!targetStates[i])
				{
					Multimorphic.P3App.Logging.Logger.Log ("target " + i.ToString() + "isn't hit");
					return;
				}
			}

			if (!allTargetsHit)
			{
				allTargetsHit = true;
				ProcessAllTargetsHit();
			}
		}

		protected virtual void ProcessAllTargetsHit()
		{
			Multimorphic.P3App.Logging.Logger.Log ("triple targets all hit");
			PostModeEventToModes ("Evt_TripleTargetsComplete", true);
		}

	}
}

