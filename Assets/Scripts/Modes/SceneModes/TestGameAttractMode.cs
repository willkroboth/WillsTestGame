using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using System.Collections.Generic;

namespace PinballClub.TestGame.Modes
{
    public class TestGameAttractMode : AttractMode
	{
		LEDShowControllerMode LEDShowController;
		ResultsMode resultsMode;
		private List<string> stageNames;
		private int stageIndex;
		private string stage;
        private bool keepButtonLegendActive;

		public TestGameAttractMode (P3Controller controller, int priority, string SceneName)
			: base(controller, priority, SceneName)
		{
			LEDShowController = new LEDShowControllerMode(controller, priority);
			resultsMode = new ResultsMode(controller, priority+1);

			stageNames = new List<string> ();
			stageNames.Add ("CharacterIntros");
			stageNames.Add ("Results");
			stageNames.Add ("HighScores");
			//stageNames.Add ("Empty");

			AddModeEventHandler("Evt_CreditsChanged", CreditsChangedEventHandler, Priority);
			AddModeEventHandler("Evt_HighScoreResultsFinished", HighScoreResultsFinishedHandler, Priority);
            AddModeEventHandler("Evt_ProfileChosen", ProfileChosenHandler, Priority);
            AddModeEventHandler("Evt_RequestProfileChange", RequestProfileChangeHandler, Priority);
            AddModeEventHandler("Evt_DialogOpened", DialogOpenedEventHandler, Priority);
            AddModeEventHandler("Evt_DialogClosed", DialogClosedEventHandler, Priority);
            AddModeEventHandler("Evt_RemoveEventProfile", RemoveEventProfileEventHandler, Priority);

            AddGUIEventHandler("Evt_MenuEnabled", MenuEnabledEventHandler);

        }

 		public override void mode_started ()
		{
			base.mode_started ();

			// Set default state of GI, lamps, etc.
			EnableGI();
			DefaultLamps();
            keepButtonLegendActive = false;

            // Here's how to make the profile selection menu not include <None> or "Add Profile"
            /*
            Multimorphic.P3App.Data.GameAttribute attr = data.GetGameAttribute("SelectAddProfileAllowed");
            attr.Set(false);
            Multimorphic.P3App.Data.GameAttribute attr2 = data.GetGameAttribute("SelectNoneProfileAllowed");
            attr2.Set(false);
            */

            //p3.AddMode (Show_RGBFade);

            //This was a test of using events to start profile specific functionality
            //this.delay("delete delay", NetProc.EventType.None, 0.25, new Multimorphic.P3.VoidDelegateNoArgs(PromptForNewProfileName));

        }

        private void SendTwitchMessage()
        {
            SendMessageToTwitch();
            this.delay("twitch delay", Multimorphic.NetProc.EventType.None, 2.25, new Multimorphic.P3.VoidDelegateNoArgs(SendTwitchMessage));
        }

        private void SendMessageToTwitch()
        {
            PostModeEventToGUI("Evt_SendTwitchMessage", "Hi");
        }

        //This was for a test of using events to start profile specific functionality
        private void PromptForNewProfileName()
        {
            PostModeEventToModes("Evt_ChangeProfile", "");
        }

        public override void mode_stopped ()
		{
			p3.RemoveMode (LEDShowController);
			cancel_delayed("stage delay");
			FinishStage(99);
			base.mode_stopped ();
		}

		public void EnableGI()
		{
			//p3.Coils["giBB"].Pulse (0);
			//p3.Coils["giCab"].Pulse (0);
			//p3.Coils["giBottomR"].Pulse (0);
			//p3.Coils["giBottomG"].Pulse (0);
			//p3.Coils["giBottomB"].Pulse (0);
		}

		private void DefaultLamps()
		{
			//foreach (LED led in p3.LEDs.Values)
			//{
			//	led.SetColor(Multimorphic.P3.Colors.Color.blue);
			//}
		}

		private void MenuEnabledEventHandler( string evtName, object evtData ) {
			Multimorphic.P3App.Logging.Logger.LogWarning (" -- attract mode sees menu enabled");
		}

		public override void SceneLiveEventHandler( string evtName, object evtData )
		{
			base.SceneLiveEventHandler(evtName, evtData);
			foreach (GUIInsertScript script in GUIInsertScripts)
			{
				script.AddCommand("",  Multimorphic.P3.Colors.Color.black,  Multimorphic.P3.Colors.Color.black, 0, 0, 10);
				GUIInsertHelpers.AddorRemoveScript(script, true);
			}
			
			stageIndex = 0;

			p3.AddMode (LEDShowController);
			UpdateHUD ();

			if (PostGame)
			{
				NextStage (stageNames.IndexOf("Results"));
			}
			else
			{
				NextStage(0);
			}

            //SendTwitchMessage();
		}

		private void NextStage()
		{
			NextStage (1);
		}

		private void NextStage(int inc)
		{
			stageIndex += inc;
			stageIndex %= stageNames.Count;
			if (stageIndex < 0)
				stageIndex = stageNames.Count-1;

			if (stageNames[stageIndex] == "Results")
			{
				if (!PostGame)
					NextStage (inc);
				else
					ShowResults();
			}
			else if (stageNames[stageIndex] == "HighScores")
			{
				ShowHighScores(inc);
			}
			else 
			{
				ShowCharacterIntros();
			}
		}

		private void FinishStage()
		{
			FinishStage (1);
		}

		private void FinishStage(int inc)
		{
			// In case we get here due to a button press rather than normal cycling, kill the delay.
			cancel_delayed ("stage delay");

			if (stageNames[stageIndex] == "Results")
			{
				FinishResults();
			}
			if (stageNames[stageIndex] == "HighScores")
			{
				FinishHighScores();
			}
			else 
			{
				FinishCharacterIntros();
			}

			if (inc != 99)
				NextStage (inc);
		}

		private void ShowCharacterIntros()
		{
			this.delay("stage delay", Multimorphic.NetProc.EventType.None, 50, new Multimorphic.P3.VoidDelegateNoArgs (FinishStage));
		}

		private void FinishCharacterIntros()
		{
		}

		private void StartEmptyPeriod()
		{
			this.delay("stage delay", Multimorphic.NetProc.EventType.None, 15, new Multimorphic.P3.VoidDelegateNoArgs (FinishStage));
		}

		private void FinishEmptyPeriod()
		{
		}

		private void ShowResults()
		{
			// Call finishResults first.
			// TODO: Figure out why AttractMode sometimes goes live 2 times (resulting in potentially 2 calls to ShowResults).
			FinishResults ();
			p3.AddMode(resultsMode);
			this.delay("stage delay", Multimorphic.NetProc.EventType.None, 10, new Multimorphic.P3.VoidDelegateNoArgs (FinishStage));
		}

		private void FinishResults()
		{
			cancel_delayed ("stage delay");
			p3.RemoveMode(resultsMode);
		}

		private void ShowHighScores(int inc)
		{
			FinishHighScores ();
			PostModeEventToModes ("Evt_ShowHighScores", inc);
		}

		private bool HighScoreResultsFinishedHandler( string evtName, object evtData )
		{
			FinishStage ((int)evtData);
			return SWITCH_CONTINUE;
		}

		private void FinishHighScores()
		{
			PostModeEventToModes ("Evt_HideHighScores", true);
			cancel_delayed ("stage delay");
		}

		private bool CreditsChangedEventHandler(string evtName, object evtData )
		{
			PostModeEventToModes ("Evt_ShowCreditsInGUI", 0);
			return SWITCH_CONTINUE;
		}

		private void UpdateHUD()
		{
			PostModeEventToModes ("Evt_ShowCreditsInGUI", 0);
		}

		public bool sw_buttonRight0_active(Switch sw)
		{
            SendMessageToTwitch();
			ShowButtonLegend();
			return SWITCH_CONTINUE;
		}

		public bool sw_buttonLeft0_active(Switch sw)
		{
			ShowButtonLegend();
			return SWITCH_CONTINUE;
		}

		public bool sw_buttonLeft1_active(Switch sw)
		{
			ShowButtonLegend();
			return SWITCH_CONTINUE;
		}

		public bool sw_buttonRight1_active(Switch sw)
		{
			ShowButtonLegend();
			return SWITCH_CONTINUE;
		}

		public bool sw_buttonLeft2_active(Switch sw)
		{
			ShowButtonLegend();
			return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonRight2_active(Switch sw)
		{
			ShowButtonLegend();
			return SWITCH_CONTINUE;
		}

		public bool sw_buttonRight0_inactive(Switch sw)
		{
			FinishStage(1);
			return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonLeft0_inactive(Switch sw)
		{
			FinishStage(-1);
            return SWITCH_CONTINUE;
		}
		
		private bool ProfileChosenHandler( string evtName, object evtData )
		{
            delay("EditProfileSettings", Multimorphic.NetProc.EventType.None, 1f, new Multimorphic.P3.VoidDelegateNoArgs(DelayToEditProfileSettings));
			return SWITCH_STOP;
		}

        private void DelayToEditProfileSettings()
        {
            PostModeEventToModes("Evt_EditProfileSettings", true);
        }

        private bool RequestProfileChangeHandler( string evtName, object evtData )
		{
			PostModeEventToModes ("Evt_ChooseProfile", true);
			return SWITCH_STOP;
		}

		private bool RequestProfileSettingsHandler( string evtName, object evtData )
		{
			PostModeEventToModes ("Evt_ChooseProfile", true);
			return SWITCH_STOP;
		}

        private void PossiblyShowResults()
		{
		}

		private void ShowButtonLegend()
		{
			Dictionary<string, string> buttonLegend = new Dictionary<string, string>();

            buttonLegend["LeftWhiteButton"] = "";
            buttonLegend["RightWhiteButton"] = "";
            buttonLegend["LeftRedButton"] = "Prev Display Item\n(+Start): Menu";
            buttonLegend["RightRedButton"] = "Next Display Item\n(+Start): Menu";
            buttonLegend["LeftYellowButton"] = "";
            buttonLegend["RightYellowButton"] = "";
            buttonLegend["LaunchButton"] = "";
			buttonLegend["StartButton"] = "Start Game";

			PostModeEventToGUI("Evt_ShowButtonLegend", buttonLegend);
            DelayToHideButtonLegend();
		}

		private void DelayToHideButtonLegend()
		{
            if (p3.Switches["buttonLeft0"].IsInactive() && p3.Switches["buttonLeft1"].IsInactive() && p3.Switches["buttonLeft2"].IsInactive() &&
                p3.Switches["buttonRight0"].IsInactive() && p3.Switches["buttonRight1"].IsInactive() && p3.Switches["buttonRight2"].IsInactive())
            {
                if (!keepButtonLegendActive)
                    HideButtonLegend();
            }
            else
                this.delay("legend delay", Multimorphic.NetProc.EventType.None, 5, new Multimorphic.P3.VoidDelegateNoArgs(DelayToHideButtonLegend));
		}
		
		private void HideButtonLegend()
		{
			PostModeEventToGUI("Evt_HideButtonLegend", 0);
		}

        public bool DialogOpenedEventHandler(string evtName, object evtData)
        {
            Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "****** Open Dialog Received");
            keepButtonLegendActive = true;
            cancel_delayed("legend delay");
            return (SWITCH_CONTINUE);
        }

        public bool DialogClosedEventHandler(string evtName, object evtData)
        {
            Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "****** Close Dialog Received");
            keepButtonLegendActive = false;
            ShowButtonLegend();
            return (SWITCH_CONTINUE);
        }

        private bool RemoveEventProfileEventHandler(string evtName, object evtData)
        {
            CloseDialog("SettingsEditor");
            this.delay("Reopen Settings", Multimorphic.NetProc.EventType.None, 0.7f, new Multimorphic.P3.VoidDelegateNoArgs(ReopenSettings));
            return (SWITCH_CONTINUE);
        }

        private void ReopenSettings()
        {
            OpenDialog("SettingsEditor");
        }

    }

}

