using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3;
using Multimorphic.P3App.Modes.Selector;

namespace PinballClub.TestGame.Modes.Menu {

	public class TestGameEventNameTextEditorMode : SelectorMode {
	
		public TestGameEventNameTextEditorMode(P3Controller controller, int priority)
			: base(controller, priority)
		{
            selectorId = "EventNameTextSelector";

			buttonLegend["LeftWhiteButton"] = "Shift";
			buttonLegend["RightWhiteButton"] = "Shift";
			buttonLegend["LeftRedButton"] = "Previous";
			buttonLegend["RightRedButton"] = "Next";
			buttonLegend["LeftYellowButton"] = "Back";
			buttonLegend["RightYellowButton"] = "Select";
			buttonLegend["StartButton"] = "Select";
			buttonLegend["LaunchButton"] = "Select";

            AddSwitchHandlerMap("buttonLeft0", Left);
            AddSwitchHandlerMap("buttonRight0", Right);
            AddSwitchHandlerMap("buttonLeft1", Exit);
            AddSwitchHandlerMap("buttonRight1", Enter);
            AddSwitchHandlerMap("start", Enter);
            AddSwitchHandlerMap("launch", Enter);
            AddSwitchHandlerMap("buttonLeft2", Shift);
            AddSwitchHandlerMap("buttonRight2", Shift);

            AddSwitchHandlerMap("down", Left);
            AddSwitchHandlerMap("up", Right);
            //AddSwitchHandlerMap("enter", Enter);
            AddSwitchHandlerMap("exit", Exit);

            AddGUIEventHandler("Evt_TextEditorCharacterSelect", TextEditorCharacterSelectEventHandler);
			AddGUIEventHandler ("Evt_EventNameEntryCompleted", EventNameEntryCompletedEventHandler);
        }

		public override void mode_started ()
		{
			base.mode_started ();
		}

        protected void TextEditorCharacterSelectEventHandler(string evtName, object evtData)
        {
            PostModeEventToModes("Evt_TextEditorCharacterSelect", evtData);
        }

		void EventNameEntryCompletedEventHandler(string evtName, object evtData)
		{
   //         Multimorphic.P3App.Logging.Logger.LogWarning("  === Sending text entry complete.");
			PostModeEventToModes("Evt_EventProfileNameEntered", evtData);
		}

	}
}
