using UnityEngine;
using System.Collections.Generic;
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Modes.Data;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.Modes
{

	/// <summary>
	/// A mode which detects various button combinations.
	/// </summary>
	//    Combo syntax:   upper case = pressed, lower case = released
	//      
	//     A  L                 R  B
	//      X                     Y

    public class TestGameButtonCombosMode : P3Mode
	{
		private string comboHistory;
		private int comboHistoryDepth = 20;
		public List<string> combos;
		public string lastCombo;

		private string ballPathOnCombo = "XALYBR";
		private string ballPathOffCombo = "RBYLAX";
		private string IRGridOnCombo = "AXLBYR";
		private string IRGridOffCombo = "RYBLXA";
		private string FrameRateOnCombo = "LXARYB";
		private string FrameRateOffCombo = "BYRAXL";

		public TestGameButtonCombosMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			Multimorphic.P3App.Logging.Logger.Log ("TestGameButtonCombosMode initialized");
			combos = new List<string>();
			combos.Add (ballPathOnCombo);
			combos.Add (ballPathOffCombo);
			combos.Add (IRGridOnCombo);
			combos.Add (IRGridOffCombo);
		}

 		public override void mode_started ()
		{
			base.mode_started ();
			Multimorphic.P3App.Logging.Logger.Log ("ButtonCombosMode started");
		}

		private void DetectCombos(string buttonCode) {
			if (combos.Count > 0) {
				comboHistory += buttonCode;
				comboHistory = comboHistory.Substring(Mathf.Clamp(comboHistory.Length - comboHistoryDepth, 0, comboHistoryDepth));
				Multimorphic.P3App.Logging.Logger.Log (comboHistory);
				
				lastCombo = "";
				foreach (string combo in combos) {
					//P3App.Logging.Logger.Log(combo + "==" + comboHistory.Substring(Mathf.Clamp(comboHistory.Length - combo.Length, 0, comboHistoryDepth)));
					if (comboHistory.Substring(Mathf.Clamp(comboHistory.Length - combo.Length, 0, comboHistoryDepth)) == combo) {
						lastCombo = combo;
						OnCombo();
					}
				}
			}
		}

		public void OnCombo()
		{
			if (lastCombo == ballPathOnCombo)
			{
				Multimorphic.P3App.Logging.Logger.Log ("BallPath On Combo Detected");
				p3.ModesToGUIEventManager.Post ("Evt_EnableBallPath", true);
			}
			else if (lastCombo == ballPathOffCombo)
			{
				Multimorphic.P3App.Logging.Logger.Log ("BallPath Off Combo Detected");
				p3.ModesToGUIEventManager.Post ("Evt_EnableBallPath", false);
			}
			else if (lastCombo == IRGridOnCombo)
			{
				Multimorphic.P3App.Logging.Logger.Log ("IR On Combo Detected");
				p3.ModesToGUIEventManager.Post ("Evt_EnableIRGrid", true);
			}
			else if (lastCombo == IRGridOffCombo)
			{
				Multimorphic.P3App.Logging.Logger.Log ("IR Off Combo Detected");
				p3.ModesToGUIEventManager.Post ("Evt_EnableIRGrid", false);
			}
			else if (lastCombo == FrameRateOnCombo)
			{
				Multimorphic.P3App.Logging.Logger.Log ("FrameRate On Combo Detected");
				p3.ModesToGUIEventManager.Post ("Evt_EnableFrameRate", true);
			}
			else if (lastCombo == FrameRateOffCombo)
			{
			Multimorphic.P3App.Logging.Logger.Log ("FrameRate Off Combo Detected");
				p3.ModesToGUIEventManager.Post ("Evt_EnableFrameRate", false);
			}
		}

		public bool sw_buttonLeft1_active(Switch sw)
		{
			DetectCombos("X");
  			return SWITCH_CONTINUE;
		}

		public bool sw_buttonRight1_active(Switch sw)
		{
			DetectCombos("Y");
			return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonLeft2_active(Switch sw)
		{
			DetectCombos("A");
			return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonRight2_active(Switch sw)
		{
			DetectCombos("B");
  			return SWITCH_CONTINUE;
		}

		public bool sw_buttonLeft0_active(Switch sw)
		{
			DetectCombos("L");
			return SWITCH_CONTINUE;
		}

		public bool sw_buttonRight0_active(Switch sw)
		{
			DetectCombos("R");
  			return SWITCH_CONTINUE;
		}


		public bool sw_buttonLeft1_inactive(Switch sw)
		{
			DetectCombos("x");
			return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonRight1_inactive(Switch sw)
		{
			DetectCombos("y");
			return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonLeft2_inactive(Switch sw)
		{
			DetectCombos("a");
			return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonRight2_inactive(Switch sw)
		{
			DetectCombos("b");
			return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonLeft0_inactive(Switch sw)
		{
			DetectCombos("l");
			return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonRight0_inactive(Switch sw)
		{
			DetectCombos("r");
			return SWITCH_CONTINUE;
		}

	} 

}

