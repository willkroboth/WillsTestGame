using System;
using System.Collections.Generic;
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3App.Modes.Menu;
using Multimorphic.P3;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.Modes.Menu
{

    public class TestGameConfirmationBoxMode : ConfirmationBoxMode
	{
		Dictionary<string, string> legend;
		protected bool buttonAlreadyActive;
		protected bool active;

		public TestGameConfirmationBoxMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
		}

		public override void mode_started ()
		{
			base.mode_started ();
			active = false;
		}

		protected override bool ShowConfirmationBoxEventHandler(string evtName, object evtData)
		{
			CheckForButtonAlreadyActive();
			active = true;
			List<string> text = (List<string>)evtData;
			ShowButtonLegend(text);
			return base.ShowConfirmationBoxEventHandler(evtName, evtData);
		}

		protected void ShowButtonLegend (List<string> text)
		{
			legend = new Dictionary<string, string>();

			if (text.Count == 3)
			{
				legend["LeftWhiteButton"] = text[2];
				legend["RightWhiteButton"] = text[2];
				legend["LeftRedButton"] = text[2];
				legend["RightRedButton"] = text[2];
				legend["LeftYellowButton"] = text[2];
				legend["RightYellowButton"] = text[2];
				legend["StartButton"] = text[2];
				legend["LaunchButton"] = text[2];
			}
			else if (text.Count == 4)
			{
				legend["LeftWhiteButton"] = text[2];
				legend["RightWhiteButton"] = text[2];
				legend["LeftRedButton"] = text[2];
				legend["RightRedButton"] = text[3];
				legend["LeftYellowButton"] = text[3];
				legend["RightYellowButton"] = text[3];
				legend["StartButton"] = text[2];
				legend["LaunchButton"] = text[3];
			}
			else if (text.Count > 4)
			{
				legend["LeftWhiteButton"] = text[2];
				legend["RightWhiteButton"] = text[2];
				legend["LeftRedButton"] = text[2];
				legend["RightRedButton"] = text[4];
				legend["LeftYellowButton"] = text[4];
				legend["RightYellowButton"] = text[4];
				legend["StartButton"] = text[3];
				legend["LaunchButton"] = text[3];
			}

			PostModeEventToGUI("Evt_ShowButtonLegend", legend);
		}

		protected void CheckForButtonAlreadyActive()
		{
			if (p3.Switches["start"].IsActive() ||
			    p3.Switches["launch"].IsActive() )
				buttonAlreadyActive = true;
			else 
				buttonAlreadyActive = false;
		}

		protected override void End()
		{
			base.End();
			active = false;
			Multimorphic.P3App.Logging.Logger.Log ("TestGameConfirmationBoxMode: End - Hiding button legend.");
			PostModeEventToGUI("Evt_HideButtonLegend", 0);
		}

		public bool sw_start_inactive(Switch sw)
		{
			if (active)
			{
				if (buttonAlreadyActive)
					buttonAlreadyActive = false;
				else
					BottomLeftButton();
				return SWITCH_STOP;
			}
			else
				return SWITCH_CONTINUE;
		}

		public bool sw_start_active(Switch sw)
		{
			if (active)
				return SWITCH_STOP;
			else
				return SWITCH_CONTINUE;
		}

		public bool sw_buttonLeft2_active(Switch sw)
		{
			if (active)
			{
				LeftButton();
				return SWITCH_STOP;
			}
			return SWITCH_CONTINUE;
		}

		public bool sw_buttonRight2_active(Switch sw)
		{
			if (active)
			{
				RightButton();
				return SWITCH_STOP;
			}
			return SWITCH_CONTINUE;
		}
		
		public bool sw_up_active(Switch sw)
		{
			if (active)
			{
				return SWITCH_STOP;
			}
			return SWITCH_CONTINUE;
		}

		public bool sw_up_inactive(Switch sw)
		{
			if (active)
			{
				return SWITCH_STOP;
			}
			return SWITCH_CONTINUE;
		}
		
		public bool sw_down_active(Switch sw)
		{
			if (active)
			{
				return SWITCH_STOP;
			}
			return SWITCH_CONTINUE;
		}

		public bool sw_down_inactive(Switch sw)
		{
			if (active)
			{
				return SWITCH_STOP;
			}
			return SWITCH_CONTINUE;
		}
		
		public bool sw_exit_active(Switch sw)
		{
			if (active)
			{
				return SWITCH_STOP;
			}
			return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonLeft0_active(Switch sw)
		{
			if (active)
			{
				return SWITCH_STOP;
			}
			else
				return SWITCH_CONTINUE;
		}

		public bool sw_buttonLeft0_inactive(Switch sw)
		{
			if (active)
			{
				LeftButton();
				return SWITCH_STOP;
			}
			else
				return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonRight0_active(Switch sw)
		{
			if (active)
			{
				return SWITCH_STOP;
			}
			else
				return SWITCH_CONTINUE;
		}

		public bool sw_buttonRight0_inactive(Switch sw)
		{
			if (active)
			{
				RightButton();
				return SWITCH_STOP;
			}
			else
				return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonLeft1_active(Switch sw)
		{
			if (active)
			{
				LeftButton ();
				return SWITCH_STOP;
			}
			else
				return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonRight1_active(Switch sw)
		{
			if (active)
			{
				RightButton();
				return SWITCH_STOP;
			}
			else
				return SWITCH_CONTINUE;
		}

		public bool sw_launch_active(Switch sw)
		{
			if (active)
				return SWITCH_STOP;
			else
				return SWITCH_CONTINUE;
		}

		public bool sw_launch_inactive(Switch sw)
		{
			if (active)
			{
				if (buttonAlreadyActive)
					buttonAlreadyActive = false;
				else
					BottomRightButton();
				return SWITCH_STOP;
			}
			else
				return SWITCH_CONTINUE;
		}


	} 

}

