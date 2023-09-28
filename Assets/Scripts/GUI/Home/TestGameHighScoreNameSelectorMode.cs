using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3;
using Multimorphic.P3App.Modes.Selector;

namespace PinballClub.TestGame.Modes.Menu {

	public class HighScoreNameSelectorMode : SelectorMode {
	
		public HighScoreNameSelectorMode(P3Controller controller, int priority)
			: base(controller, priority)
		{
            selectorId = "HighScoreNameSelector";

			buttonLegend["LeftWhiteButton"] = "";
			buttonLegend["RightWhiteButton"] = "";
			buttonLegend["LeftRedButton"] = "Up";
			buttonLegend["RightRedButton"] = "Down";
			buttonLegend["LeftYellowButton"] = "";
			buttonLegend["RightYellowButton"] = "Select";
			buttonLegend["StartButton"] = "Select";
			buttonLegend["LaunchButton"] = "";

			AddSwitchHandlerMap("buttonLeft0", Left);
			AddSwitchHandlerMap("buttonRight0", Right);
			AddSwitchHandlerMap("buttonLeft1", Exit);
			AddSwitchHandlerMap("buttonRight1", Enter);
			AddSwitchHandlerMap("start", Exit);
			AddSwitchHandlerMap("launch", Enter);
			AddSwitchHandlerMap("buttonLeft2", Shift);
			AddSwitchHandlerMap("buttonRight2", Shift);

            AddModeEventHandler("Evt_DataForHighScoreNameSelector", DataForHighScoreNameSelectorEventHandler, Priority);

            AddGUIEventHandler("Evt_HighScoreNameSelectorSelect", HighScoreNameSelectorSelectEventHandler);
			AddGUIEventHandler ("Evt_HighScoreNameEntryCompleted", HighScoreNameEntryCompleteEventHandler);
        }

		public override void mode_started ()
		{
			base.mode_started ();
		}

        private bool DataForHighScoreNameSelectorEventHandler(string evtName, object evtData)
        {
			 // Pass relevant data from here to the GUI-side selector so that the selector items can be created.
			 // PostModeEventToGUI ("Evt_HighScoreNameSelectorData", someData);
            return SWITCH_CONTINUE;
        }

        protected void HighScoreNameSelectorSelectEventHandler(string evtName, object evtData)
        {
            PostModeEventToModes("Evt_HighScoreNameSelectorSelect", evtData);
        }

		void HighScoreNameEntryCompleteEventHandler(string evtName, object evtData)
		{
			PostModeEventToModes("Evt_HighScoreNameEntryCompleted", evtData);
			PostModeEventToModes("Evt_CloseDialog", "HighScoreNameEntry");
		}

	}
}
