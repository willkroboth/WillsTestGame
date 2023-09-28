using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Data;
using Multimorphic.P3App.Modes.Data;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Modes.Selector;
using PinballClub.TestGame.Modes.Data;
using PinballClub.TestGame.Modes.Menu;
using System.Collections.Generic;
using System.Xml;
using PinballClub.TestGame.Modes;

namespace PinballClub.TestGame.Modes
{

	/// <summary>
	/// The base class which handles the game-level flow of this app's games.
	/// This mode is the host for the primary game modes: Attract, Home and Money modes, for example.
	/// Scene modes are hosted separately, with the exception of Home mode, which is the start point of a player's game.
	/// </summary>
    public class TestGameBaseGameMode : BaseGameMode
	{
		private TestGameButtonCombosMode buttonCombosMode;
		private HUDMode hudMode;

		private FlippersMode flippersMode; //!< Enables/Disables/Remaps flippers
        private BumpersMode bumpersMode; //!< Enables/Disables bumpers (slings, pops, etc)
		
		private PopupMode popupMode;
		private TestGameHomeMode homeMode;
		private ShotsMode shotsMode;
		private IntroVideoMode introVideoMode;

		public TestGameBaseGameMode (P3Controller controller)
			: base(controller)
		{

			TestGameBallLauncher.Initialize(controller, data);

            enableTwitchIntegration = true;

            // Instantiate DataInitMode simply to ensure all necessary data items have valid entries in
            // the storage mechanism.  If they don't, they will be created automatically.
            gameAttributeManagerMode = new TestGameGameAttributeManagerMode (p3, Priority);

            // Instantiate other modes in preparation of them being added when necessary.
            flippersMode = new FlippersMode (p3, Priorities.PRIORITY_UTILITIES);
            bumpersMode = new BumpersMode (p3, Priorities.PRIORITY_UTILITIES);

			attractMode = new TestGameAttractMode (p3, Priorities.PRIORITY_ATTRACT, "Attract");
			buttonCombosMode = new TestGameButtonCombosMode (p3, TestGamePriorities.PRIORITY_BUTTON_COMBOS);
			//mechManagerMode = new TestGameMechManager (p3, Priorities.PRIORITY_MECH);

			hudMode = new HUDMode (p3, Priorities.PRIORITY_MECH);
			popupMode = new PopupMode (p3, Priorities.PRIORITY_MECH);
			homeMode = new TestGameHomeMode (p3, TestGamePriorities.PRIORITY_HOME, "TestGameHome");
			introVideoMode = new IntroVideoMode (p3, TestGamePriorities.PRIORITY_HOME, "IntroVideo");

			// Shots should be higher priority than mechs because shots could be used by mechs.
			shotsMode = new ShotsMode (p3, Priorities.PRIORITY_MECH+2);

			InitBallSearchMode();

            // *** Debugging events ***

            // * Exposure Level *
            // You can set the exposure level for logging messages.  This allows you decide who should be able
            // to see which messages.  You can set the exposure level to Dev while debugging and then change it
            // to public before releasing.
#if DEBUG
            Multimorphic.P3App.Logging.Logger.SetExposureLevel(Multimorphic.P3App.Logging.Logger.Exposure.Dev);
#else
            Multimorphic.P3App.Logging.Logger.SetExposureLevel(Multimorphic.P3App.Logging.Logger.Exposure.Public);
#endif

            // * Switch events *
            // The following will add the launch button (switchname: "launch") to the event logging list.
            // Any time the launch button is activated or deactivated, the system will log the sequence of modes that
            // are allowed to process the event.
            Multimorphic.NetProcMachine.Logging.Logger.AddSwitchName("launch");

            // * Mode to Mode events *
            // The following will add mode to mode events to the logging list
            //Multimorphic.NetProcMachine.EventManager.LogEventName("Evt_MagnetRingCatch", true);

            // * Mode to GUI events *
            // The following will add mode to GUI events to the logging list
            //p3.ModesToGUIEventManager.LogEventName("Evt_ShowGUIObject", "objectName");

            // * GUI to Mode events *
            // The following will add GUI to mode events to the logging list
            //p3.GUIToModesEventManager.LogEventName("Evt_SceneGoneLive", true);

        }

        protected override void InitializeGameData()
		{
			base.InitializeGameData();
			Multimorphic.P3App.Data.DataManager.gameName = "TestGame";
		}

		private void InitBallSearchMode()
		{
			List<Switch> resetSwitches = new List<Switch>();
			resetSwitches.Add (p3.Switches["drain"]);
			resetSwitches.Add (p3.Switches["moatInA"]);
			resetSwitches.Add (p3.Switches["moatInB"]);
			resetSwitches.Add (p3.Switches["moatExit"]);

			List<Switch> pauseSwitches = new List<Switch>();
			pauseSwitches.Add (p3.Switches["buttonLeft0"]);
			pauseSwitches.Add (p3.Switches["buttonRight0"]);
			pauseSwitches.Add (p3.Switches["buttonLeft1"]);
			pauseSwitches.Add (p3.Switches["buttonRight1"]);
			pauseSwitches.Add (p3.Switches["buttonLeft2"]);
			pauseSwitches.Add (p3.Switches["buttonRight2"]);

			List<Driver> searchCoils = new List<Driver>();
			for (int i=0; i<6; i++) {
				searchCoils.Add (p3.Coils["wall" + i.ToString() + "Up"]);
			}
			for (int i=0; i<6; i++) {
				searchCoils.Add (p3.Coils["scoop" + i.ToString() + "Up"]);
			}

			ballSearchMode.Setup (resetSwitches, pauseSwitches, searchCoils);
		}

 		public override void mode_started ()
		{
			base.mode_started ();

			p3.AddMode(flippersMode);
            p3.AddMode (bumpersMode);

            // Use the following line if your game shouldn't check high scores when the game is over.
            //highScoreCheckEnabled = false;

            // Use the following line if your game should not attempt to use bonusMode at end-of-ball to
            // compute and show a bonus using 'bonusInfo' that you need to provide in the BallEndedEventHandler().
            bonusModeEnabled = false;
    }

    public override void mode_stopped ()
		{
			base.mode_stopped ();
		}

		// ##########################################
		// # App Startup 
		// ##########################################

		protected override void Start ()
		{
			base.Start ();

			// Always monitor for various button combos.  Note - the buttons being monitored won't
			// be seen by buttonCombosMode if a higher priority mode issues a SWITCH_STOP on the event.
			p3.AddMode(buttonCombosMode);
			p3.AddMode (shotsMode);
			p3.AddMode (hudMode);
			p3.AddMode (popupMode);
		}

		protected override bool GameIntroCompleteEventHandler(string eventName, object eventData)
		{
			// RemoveModeEventHandler ("Evt_GameIntroComplete", GameIntroCompleteHandler, 0);
			p3.RemoveMode(introVideoMode);
			StartNewBall();
			
			return SWITCH_CONTINUE;
		}

		protected override void StartNewBall()
		{
			base.StartNewBall();
			p3.AddMode(homeMode);
		}

		// ##########################################
		// # Ball End
		// ##########################################

		// Wait for an event from the active game mode signalling the ball is over.
		protected override bool BallEndedEventHandler(string eventName, object eventData)
		{
			if (!forcePlayerChangeActive)
			{
				// Reset scoring multiplier before applying bonus.
				ScoreManager.SetX (1);
				//bonusInfo = homeMode.getBonusInfo();
			}

			return base.BallEndedEventHandler(eventName, eventData);
		}

		protected override void removeGameModes()
		{
			p3.RemoveMode (homeMode);
			// Call the base after removing homeMode in case ending homeMode results in events that cause base modes 
			// to start (like bonusMode).
			base.removeGameModes();
		}


		/// <summary>
		/// Add selector and dialog registration in this method.
		/// This method is called at the beginning of the game in order to make selector modes and selector GUI objects ready for later use.
		/// When used, the selectorManagerMode will help decide which selector receives navigation events if more than one selector is active.
		/// </summary>
		protected override void RegisterSelectors() {
			base.RegisterSelectors();

   			selectorManagerMode.RegisterSelector(new SettingsSelectorMode(p3, Priorities.PRIORITY_SERVICE_MODE), "SettingsEditor", "Prefabs/Framework/SettingsEditor");
			selectorManagerMode.RegisterSelector(new SettingsTextEditorMode(p3, Priorities.PRIORITY_SERVICE_MODE_TEXT_ENTRY), "SettingsTextEditor", "Prefabs/Framework/SettingsTextEditor", true);
			selectorManagerMode.RegisterSelector(new ProfileSettingsSelectorMode(p3, TestGamePriorities.PRIORITY_PROFILEATTRIBUTESELECTOR), "ProfileSettingsEditor", "Prefabs/Framework/ProfileSettingsEditor");
			selectorManagerMode.RegisterSelector(new VolumeSelectorMode(p3, Priorities.PRIORITY_VOLUME_MODE), "VolumeEditor", "Prefabs/Framework/VolumeEditor");
			selectorManagerMode.RegisterSelector(new TextSelectorMode(p3, Priorities.PRIORITY_HIGH_SCORES), "HighScoreNameEditor", "Prefabs/Framework/HighScoreNameEditor");
			selectorManagerMode.RegisterSelector(new ConfirmationSelectorMode(p3, TestGamePriorities.PRIORITY_CONFIRMATION), "ConfirmationDialog", "Prefabs/Framework/ConfirmationDialog", true);
			selectorManagerMode.RegisterSelector(new ProfileSelectorMode(p3, TestGamePriorities.PRIORITY_PROFILESELECTOR), "ProfileDialog", "Prefabs/Framework/ProfileDialog", true);
			selectorManagerMode.RegisterSelector(new HighScoreNameSelectorMode(p3, TestGamePriorities.PRIORITY_HIGH_SCORE_NAME_SELECTOR), "HighScoreNameEntry", "Prefabs/Framework/HighScoreNameEntry", true);
			selectorManagerMode.RegisterSelector(new TestGameEventNameTextEditorMode(p3, TestGamePriorities.PRIORITY_EVENT_NAME_EDITOR), "EventNameEditor", "Prefabs/Framework/EventNameEditor", true);
			selectorManagerMode.RegisterSelector(new TestGameProfileNameTextEditorMode(p3, TestGamePriorities.PRIORITY_EVENT_NAME_EDITOR), "ProfileNameEditor", "Prefabs/Framework/ProfileNameEditor", true);


			// Register more menus (and their parent dialog prefabs) here using more calls to RegisterSelector.
		}

		private void RegisterGUIInserts()
        {
            base.RegisterGUIInserts();
            GUIInsertHelpers.AddGUIInsert("InfoPopup");
            GUIInsertHelpers.AddGUIInsert("SideTargetHighlight0");
            GUIInsertHelpers.AddGUIInsert("SideTargetHighlight1");
            GUIInsertHelpers.AddGUIInsert("SideTargetHighlight2");
            GUIInsertHelpers.AddGUIInsert("SideTargetHighlight3");
            GUIInsertHelpers.AddGUIInsert("SideTargetHighlight4");
            GUIInsertHelpers.AddGUIInsert("SideTargetHighlight5");
            GUIInsertHelpers.AddGUIInsert("SideTargetHighlight6");
            GUIInsertHelpers.AddGUIInsert("SideTargetHighlight7");
        }

    }

}

