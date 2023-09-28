using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3;
using Multimorphic.P3App.Modes.Selector;

namespace PinballClub.TestGame.Modes {

	public class TestGameTextEditorMode : SelectorMode {
	
		public TestGameTextEditorMode(P3Controller controller, int priority)
			: base(controller, priority)
		{
            selectorId = "TextSelector";

			buttonLegend["LeftWhiteButton"] = "";
			buttonLegend["RightWhiteButton"] = "";
			buttonLegend["LeftRedButton"] = "Up";
			buttonLegend["RightRedButton"] = "Down";
			buttonLegend["LeftYellowButton"] = "Exit";
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

            AddSwitchHandlerMap("down", Left);
            AddSwitchHandlerMap("up", Right);
            //AddSwitchHandlerMap("enter", Enter);
            AddSwitchHandlerMap("exit", Exit);

            AddModeEventHandler("Evt_DataForTextEditor", DataForTextEditorEventHandler, Priority);

            AddGUIEventHandler("Evt_TextEditorCharacterSelect", TextEditorCharacterSelectEventHandler);
			AddGUIEventHandler ("Evt_TextEditorEntryCompleted", TextEditorEntryCompletedEventHandler);
        }

		public override void mode_started ()
		{
			base.mode_started ();
		}

        private bool DataForTextEditorEventHandler(string evtName, object evtData)
        {
			 // Pass relevant data from here to the GUI-side selector so that the selector items can be created.
			 // PostModeEventToGUI ("Evt_TextEditorData", someData);
            return SWITCH_CONTINUE;
        }

        protected void TextEditorCharacterSelectEventHandler(string evtName, object evtData)
        {
            PostModeEventToModes("Evt_TextEditorCharacterSelect", evtData);
        }

		void TextEditorEntryCompletedEventHandler(string evtName, object evtData)
		{
			PostModeEventToModes("Evt_TextEditorEntryCompleted", evtData);
			PostModeEventToModes("Evt_CloseDialog", "TextEditor");
		}

	}
}
