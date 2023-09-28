using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3;
using Multimorphic.P3App.Modes.Selector;

namespace PinballClub.TestGame.Modes.Menu {

	public class ProfileSelectorMode : SelectorMode {
	
		public ProfileSelectorMode(P3Controller controller, int priority)
			: base(controller, priority)
		{
            selectorId = "ProfileSelector";

            AddModeEventHandler("Evt_DataForProfileSelector", DataForProfileSelectorEventHandler, Priority);
            AddGUIEventHandler("Evt_ProfileSelectorExit", ProfileSelectorExitEventHandler);
            AddGUIEventHandler("Evt_ProfileSelectorResult", ProfileSelectorResultEventHandler);
        }

        public override void mode_started ()
		{
			base.mode_started ();


            buttonLegend["LeftWhiteButton"] = "";
            buttonLegend["RightWhiteButton"] = "";
            buttonLegend["LeftRedButton"] = "Previous";
            buttonLegend["RightRedButton"] = "Next";
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


            // Pass relevant data from here to the GUI-side selector so that the selector items can be created.
            // Although a list of strings are the example here, any objects could be sent.
            //			List<string> choices = new List<string>();
            //			choices.Add("Apple");
            //			choices.Add("Orange");
            //			choices.Add("Pear");
            //			PostModeEventToGUI ("Evt_ProfileSelectorData", choices);
        }

        public override void mode_stopped()
        {
            Multimorphic.P3App.Logging.Logger.Log("ProfileSelectorMode stopping.");
            base.mode_stopped();
        }

        private bool DataForProfileSelectorEventHandler(string evtName, object evtData)
        {
			 // Pass relevant data from here to the GUI-side selector so that the selector items can be created.
			 // PostModeEventToGUI ("Evt_ProfileSelectorData", someData);
            return SWITCH_CONTINUE;
        }

        protected void ProfileSelectorExitEventHandler(string evtName, object evtData)
        {
            PostModeEventToModes("Evt_ProfileSelectorCancelled", evtData);
        }

        protected void ProfileSelectorResultEventHandler(string evtName, object evtData)
        {
            PostModeEventToModes("Evt_ProfileSelectorResult", evtData);
        }

    }
}
