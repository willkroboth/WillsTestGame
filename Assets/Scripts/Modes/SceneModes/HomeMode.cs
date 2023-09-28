using System;
using System.Collections.Generic;
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.NetProcMachine.Config;
using Multimorphic.NetProcMachine.LEDs;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Modes.Data;
using Multimorphic.P3App.Logging;
using Multimorphic.P3App.Modes.PlayfieldModule;

namespace PinballClub.TestGame.Modes
{
	/// <summary>
	/// The starting mode for the players' game.  
	/// </summary>
    public class HomeMode : SceneMode
	{
		//private Multiball multiball;

		private ShotCounter leftRampCounterMode;
		private ShotCounter rightRampCounterMode;

		private BallStartMode ballStartMode;
		private BallSaveMode ballSaveMode;
		private RespawnMode respawnMode;
		private MovingTargetMode movingTargetMode;
		private SideTargetMode sideTargetMode;
		private LanesMode lanesMode;
		private TestGameGameMode modeSummaryPendingMode;

        // Declare TwitchControlMode to handle twitch viewer interactions
        private TwitchControlMode twitchControlMode;

		private bool firstTimePerBall;

		private int scoreX;
		private int lastBallNum;
		private bool ballStarted;

		private bool instructionsLaunch;
		private Random random;
		private bool allowStealthLaunch;

		public HomeMode (P3Controller controller, int priority, string SceneName)
			: base(controller, priority, SceneName)
		{
			//multiball = new Multiball (p3, TestGamePriorities.PRIORITY_MULTIBALL);
			rightRampCounterMode = new ShotCounter(p3, TestGamePriorities.PRIORITY_SHOT_COUNTERS, "Evt_RightRampHit", "Evt_RightRampInc");
			leftRampCounterMode = new ShotCounter(p3, TestGamePriorities.PRIORITY_SHOT_COUNTERS, "Evt_LeftRampHit", "Evt_LeftRampInc");

			ballStartMode = new BallStartMode (p3, TestGamePriorities.PRIORITY_HOME_UTILITIES);
			lanesMode = new LanesMode (p3, TestGamePriorities.PRIORITY_LANES);

			ballSaveMode = new BallSaveMode (p3, TestGamePriorities.PRIORITY_BALL_SAVE);
			respawnMode = new RespawnMode (p3, TestGamePriorities.PRIORITY_RESPAWN);
			movingTargetMode = new MovingTargetMode (p3, TestGamePriorities.PRIORITY_MOVING_TARGET);
			sideTargetMode = new SideTargetMode (p3, TestGamePriorities.PRIORITY_SIDE_TARGET);

            // Instantiate TwitchControlMode to handle twitch viewer interactions - don't forget to add it to the mode queue
            // in mode_started or StartPlaying (or later) and remove it from the mode queue in mode_stopped or earlier.
            twitchControlMode = new TwitchControlMode(p3, Priority);

			AddModeEventHandler("Evt_BallStartComplete", BallStartCompleteEventHandler, Priority);
			AddModeEventHandler("Evt_IncBonusX", IncBonusXEventHandler, Priority);
			AddModeEventHandler("Evt_PopHit", PopHitEventHandler, Priority);
			AddModeEventHandler("Evt_LeftSlingHit", SlingHitEventHandler, Priority);
			AddModeEventHandler("Evt_RightSlingHit", SlingHitEventHandler, Priority);
			// AlienAttack only ends when it is completed.  So use the event to call both Completed and Ended Handlers
			AddModeEventHandler("Evt_InstructionsLaunch", InstructionsLaunchEventHandler, Priority);
			AddModeEventHandler("Evt_ScoopHit", ScoopEventHandler, Priority);
			AddModeEventHandler("Evt_BallSearchBallLaunchRequest", BallSearchBallLaunchRequestEventHandler, Priority);

			// Used for disabling stealth launch for lost balls
			AddModeEventHandler("Evt_AllowLaunch", AllowLaunchEventHandler, Priority);

			AddModeEventHandler("Evt_PlayerAdded", RefreshButtonLegendEventHandler, Priority);
			AddModeEventHandler("Evt_PlayerRemoved", RefreshButtonLegendEventHandler, Priority);

            AddModeEventHandler("Evt_DialogClosed", DialogClosedEventHandler, Priority);

            // Here's an example of how to subscribe to generically-defined BallPaths in the playfield module drivers.
            // You might use code like this if you want your game to work with all playfield modules or if you want
            // to use the module drivers detection logic for when playfield shots, targets, holes, etc are hit.
            foreach (BallPathDefinition shot in p3.BallPaths.Values)
            {
                if (shot.ExitType == BallPathExitType.Target)
                {
                    string[] strippedSwitchName = shot.StartedEvent.Split('_');            // format of switch event must be "sw_<switch name>_active" for started events
                    string swName = strippedSwitchName[1];
                    add_switch_handler(swName, "active", 0, TargetHitEventHandler);
                }
                else if (shot.ExitType != BallPathExitType.Hole)
                {
                    Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "Installing ShotHitHandler for : " + shot.CompletedEvent + " exitType: " + shot.ExitType.ToString());
                    AddModeEventHandler(shot.CompletedEvent, ShotHitEventHandler, priority);
                }
                else
                {
                    Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "Installing HoleHitHandler for : " + shot.CompletedEvent);
                    AddModeEventHandler(shot.CompletedEvent, HoleHitEventHandler, priority);
                }

            }

            // Playfields that have module drivers must include Hole paths in the BallPathDefinitions and prevent Evt_TroughLauncherEntry from
            // getting to the app.  So those are handled above.
            // Playfields without module drivers will have nothing to process hole entries.  So subscribe to the Evt_TroughLauncherEntry event
            // for any hole entries to go into launch tubes.  If you'd prefer to handle these entries in a child mode, move this subscription and 
            // the event handler there.
            AddModeEventHandler("Evt_TroughLauncherEntry", TroughLauncherEntryEventHandler, this.Priority);

            AddModeEventHandler(EventNames.PlayfieldDeviceCapabilities, PlayfieldDeviceCapabilitiesEventHandler, Priority);


            //todo: release locks

            allowStealthLaunch = true;

			random = new Random();
		}

		/// <summary>
		/// Executed when this mode is mode is started.
		/// </summary>
 		public override void mode_started ()
		{
			activeInstruction = "Home";
						
			base.mode_started ();

			DefaultLamps();

			ScoreManager.SetX (scoreX);

			instructionsLaunch = false;
			firstTimePerBall = true;
			ballStarted = false;

            // Request capabilities from any playfield specific that balls might not simply flow through (like the ship in LL-EE)
            PostModeEventToModes(EventNames.PlayfieldGetDeviceCapabilities, true);
        }

        public override void mode_stopped ()
		{
			p3.RemoveMode (ballSaveMode);
			p3.RemoveMode (respawnMode);
			p3.RemoveMode (movingTargetMode);
			p3.RemoveMode (sideTargetMode);
			//p3.RemoveMode (multiball);
			p3.RemoveMode (lanesMode);
			p3.RemoveMode (rightRampCounterMode);
			p3.RemoveMode (leftRampCounterMode);
			p3.RemoveMode (twitchControlMode);

			// This shouldn't be necessary.  Figure out why ballStartMode sometimes isn't removed normally.
			p3.RemoveMode (ballStartMode);
			base.mode_stopped();
		}

		/// <summary>
		/// Loads the player data, specifically for the home mode.
		/// </summary>
		public override void LoadPlayerData()
		{
			base.LoadPlayerData();

			lastBallNum = data.currentPlayer.GetData("LastBallNumber", 0);

			scoreX = data.currentPlayer.GetData("ScoreX", 1);

			int bonusX = 1;
			if (data.currentPlayer.ContainsKey ("HoldBonusX") && 
			    (data.currentPlayer.ContainsKey ("BonusX")))
		    {
				bool holdBonusX = data.currentPlayer.GetData("HoldBonusX").ToBool();
				if (holdBonusX)
					bonusX = data.currentPlayer.GetData("BonusX").ToInt();
			}

			// Clear Hold Bonus X at start of ball
			ScoreManager.SetHoldBonusX(false);
			ScoreManager.SetBonusX(bonusX);
		}

		/// <summary>
		/// Saves the player data.
		/// </summary>
		public override void SavePlayerData()
		{
			base.SavePlayerData();
			data.currentPlayer.SaveData("LastBallNumber", data.ball);
			data.currentPlayer.SaveData("HoldBonusX", ScoreManager.GetHoldBonusX());
			data.currentPlayer.SaveData("BonusX", ScoreManager.GetBonusX());
		}

		private void DefaultLamps()
		{
			//foreach (LED led in p3.LEDs.Values)
			//{
			//	led.SetColor(Multimorphic.P3.Colors.Color.red);
			//}

			for (int i=0; i<LEDScripts.Count; i++)
			{
				if (LEDScripts[i].led.Name.Contains ("SideModule"))
					LEDScripts[i] = LEDHelpers.OnLED (p3, LEDScripts[i],  Multimorphic.P3.Colors.Color.lightgreen);
				else if (LEDScripts[i].led.Name.Contains ("CenterArrow"))
					LEDScripts[i] = LEDHelpers.OnLED (p3, LEDScripts[i],  Multimorphic.P3.Colors.Color.black);
				else if (LEDScripts[i].led.Name.Contains ("flasher"))
					LEDScripts[i] = LEDHelpers.OnLED (p3, LEDScripts[i],  Multimorphic.P3.Colors.Color.barelywhite);
				else if (LEDScripts[i].led.Name.Contains ("wall") || LEDScripts[i].led.Name.Contains("scoop"))
					LEDScripts[i] = LEDHelpers.OnLED (p3, LEDScripts[i],  Multimorphic.P3.Colors.Color.green);
				else 
					LEDScripts[i] = LEDHelpers.OnLED (p3, LEDScripts[i],  Multimorphic.P3.Colors.Color.white);
			}
		}

		/// <summary>
		/// Event handler to react to an event indicating the scene instructions should be launched.
		/// </summary>
		/// <returns><c>true</c>, if launch event was instructionsed, <c>false</c> otherwise.</returns>
		/// <param name="evtName">Event name.</param>
		/// <param name="evtData">Eventt data.</param>
		private bool InstructionsLaunchEventHandler(string evtName, object evtData )
		{
			instructionsLaunch = true;
			return false;
		}

		/// <summary>
		/// Handler to react to the event which signals that the scene has gone live.
		/// </summary>
		/// <param name="evtName">.</param>
		/// <param name="evtData">Event data.</param>
		public override void SceneLiveEventHandler( string evtName, object evtData )
		{
			foreach (GUIInsertScript script in GUIInsertScripts)
			{
				script.AddCommand("",  Multimorphic.P3.Colors.Color.black,  Multimorphic.P3.Colors.Color.black, 0, 0, 10);
				GUIInsertHelpers.AddorRemoveScript(script, true);
			}

			if ((string)evtData == "Home")
			{
				PostModeEventToModes ("Evt_EnableLanes", true);
				UpdateHUD();
				PostModeEventToModes ("Evt_ScoreRefresh", null);
				StartPlaying ();
			}
		}

		/// <summary>
		/// Start playing a game.  Ensures that modes are created to enable the game logic.
		/// </summary>
		protected override void StartPlaying()
		{
			//p3.AddMode (multiball);
			p3.AddMode (ballSaveMode);
			p3.AddMode (respawnMode);
			p3.AddMode (movingTargetMode);
			p3.AddMode (sideTargetMode);
			p3.AddMode (rightRampCounterMode);
			p3.AddMode (leftRampCounterMode);
			p3.AddMode (lanesMode);
            p3.AddMode(twitchControlMode);

			PostModeEventToModes ("Evt_EnableFlippers", true);
			PostModeEventToModes ("Evt_EnableBumpers", true);

			PostModeEventToModes ("Evt_ScoreRefresh", null);
			if (firstTimePerBall)
			{
				PostModeEventToModes ("Evt_ChangeGameState", GameState.LaunchPending);
				p3.AddMode (ballStartMode);
				ballStartMode.Start (lastBallNum == data.ball);
				firstTimePerBall = false;
				ShowButtonLegend();

                // Now that the scene has loaded the first time, tell the twitch mode to 
                // announce the twitch rules to twitch streamers.
                PostModeEventToModes("Evt_TwitchAnnounceRules", true);
            }
            base.StartPlaying ();

			PostModeEventToModes ("Evt_RespawnEnable", true);
			if (instructionsLaunch)
			{
				int ballSaveTime = (data.GetGameAttributeValue("BallSaveTime").ToInt());
				PostModeEventToModes ("Evt_BallSaveStart", ballSaveTime);
				PostModeEventToModes ("Evt_BallSavePauseUntilGrid", 0);
			}
			PostActiveInstructions();

			PostModeEventToModes ("Evt_SendActiveMusic", 0);
		}

		private bool RefreshButtonLegendEventHandler(string evtName, object evtData)
		{
			if (!ballStarted)
				delay("Legend Delay", Multimorphic.NetProc.EventType.None, 0.25, new Multimorphic.P3.VoidDelegateNoArgs (ShowButtonLegend));
			return SWITCH_CONTINUE;
		}

		private void ShowButtonLegend()
		{
			Dictionary<string, string> buttonLegend = new Dictionary<string, string>();

            buttonLegend["LeftWhiteButton"] = "Rotate Active Lane Icons";
            buttonLegend["RightWhiteButton"] = "Rotate Active Lane Icons";
			buttonLegend["LeftRedButton"] = "Left Flipper\n(+Start): Menu";
			buttonLegend["RightRedButton"] = "Right Flipper\n(+Start): Menu";
            buttonLegend["LeftYellowButton"] = "";
            buttonLegend["RightYellowButton"] = "";
			buttonLegend["LaunchButton"] = "Launch Ball";
			
			if (data.ball == 1) 
			{
				if (data.Players.Count != data.GetGameAttributeValue("MaxPlayers").ToInt())
				{
					buttonLegend["StartButton"] = "Add Player";
					if (data.Players.Count > 1)
						buttonLegend["StartButton"] += "\n(Hold 2s): Remove Player";

				}
				else if (data.Players.Count > 1)
					buttonLegend["StartButton"] = "(Hold 2s): Remove Player";
			}
			else if (data.GetGameAttributeValue("SoftRestartEnabled").ToBool())
				buttonLegend["StartButton"] = "(Hold 2s): Reset Game";

			PostModeEventToGUI("Evt_ShowButtonLegend", buttonLegend);
		}
		
		private void HideButtonLegend()
		{
			PostModeEventToGUI("Evt_HideButtonLegend", 0);
		}

		protected override void LaunchCallback()
		{
			PostModeEventToModes ("Evt_EnableBallSearch", true);
		}

		protected bool AllowLaunchEventHandler(string evtName, object evtData)
		{
			allowStealthLaunch = (bool)evtData;
			return SWITCH_CONTINUE;
		}
		
		public bool sw_launch_active_for_2s(Switch sw)
		{
			if (allowStealthLaunch)
			{
				TestGameBallLauncher.launch();
			}
			return SWITCH_CONTINUE;
		}

		// ##########################################
		// # Ball Start
		// ##########################################
		
		public bool BallStartCompleteEventHandler(string eventName, object eventData)
		{
			HideButtonLegend();
			PostModeEventToModes ("Evt_BallStarted", 0);
			PostModeEventToModes ("Evt_ChangeGameState", GameState.BallInPlay);

            // Now that the ball has started, tell the twitch mode to enable viewer interactions.
            PostModeEventToModes("Evt_TwitchAllowPowerupRequests", true);

            if (p3.Switches["buttonLeft0"].IsActive())
				TestGameBallLauncher.launch (TestGameBallLauncher.VUK_LEFT, LaunchCallback);
			else
				TestGameBallLauncher.launch (TestGameBallLauncher.VUK_RIGHT, LaunchCallback);
			int ballSaveTime = data.GetGameAttributeValue("BallSaveTime").ToInt();
			PostModeEventToModes ("Evt_BallSaveStart", ballSaveTime);
			PostModeEventToModes ("Evt_BallSavePauseUntilGrid", 0);
			ballStarted = true;
			return false;
		}	

		public override bool sw_buttonLeft0_active(Switch sw)
		{
			base.sw_buttonLeft0_active(sw);

            
            return SWITCH_CONTINUE;
		}

		public override bool sw_buttonLeft0_inactive(Switch sw)
		{
			base.sw_buttonLeft0_inactive(sw);
			return SWITCH_CONTINUE;
		}

		public bool sw_drain_active(Switch sw)
        {
            Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "HomeMode has received a drain event.");

            if (ballStarted)
            {
                PostModeEventToModes("Evt_BallDrained", 0);
                EndOfBall();
            }
            else
            {
                //this is a multiball drain event
                Multimorphic.P3App.Logging.Logger.Log(
                    Multimorphic.P3App.Logging.LogCategories.Game,
                    "Ball drained, but we're not processing it because ballStarted:" + ballStarted.ToString());
            }

			return SWITCH_CONTINUE;
		}

        private void DisableFeatures() {
			PostModeEventToModes ("Evt_EnableFlippers", false);
			PostModeEventToModes ("Evt_EnableBumpers", false);
			PostModeEventToModes ("Evt_EnableBallSearch", false);
            // Now that the ball has ended, tell the twitch mode to disable viewer interactions.
            PostModeEventToModes("Evt_TwitchAllowPowerupRequests", false);
        }

		private void EndOfBall()
		{
            ballStarted = false;
            DisableFeatures();
			PostModeEventToModes ("Evt_BallEnded", null);
        }

        public override void End()
        {
            base.End();
            DisableFeatures();
        }

        private bool HoleHitEventHandler(string evtName, object evtData)
        {
            // Add hole hit logic here or move the subscription and this code into a relevant child mode to process hole hits
            // for your game.
            return SWITCH_CONTINUE;
        }

        private bool TargetHitEventHandler(Switch sw)
        {
            // Add target hit logic here or move the subscription and this code into a relevant child mode to process target hits
            // for your game.
            return SWITCH_CONTINUE;
        }

        private bool TroughLauncherEntryEventHandler(string eventName, object eventData)
        {
            int troughLauncherIndex = (int)eventData;
            Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "TroughLauncherEntry event received for trough launcher index: " + troughLauncherIndex.ToString());

            // Add logic here to process the troughLauncherEntry (ie. score, launch a new ball, run a lightshow, etc)
            return EVENT_CONTINUE;
        }


        private bool ShotHitEventHandler(string evtName, object evtData)
        {
            // Add shot hit logic here or move the subscription and this code into a relevant child mode to process shot hits
            // for your game.
            return SWITCH_CONTINUE;
        }

        public bool SlingHitEventHandler(string eventName, object eventData)
		{
			long score = ScoreManager.Score (random.Next((int)Scores.SLING_HITS_MIN,(int)Scores.SLING_HITS_MAX));
			LocationScore info = new LocationScore();
			info.score = score;
			bool rightSide = ((Switch)eventData).Name == "slingR";
			Multimorphic.P3App.Logging.Logger.LogError ("Sling hit : " + rightSide.ToString());
			if (rightSide) info.location = "slingR";
			else info.location = "slingL";
			PostModeEventToGUI("Evt_SlingshotHit", info);

			return false;
		}

		public bool PopHitEventHandler(string eventName, object eventData)
		{
			LEDHelpers.LEDFlashPop(p3, "flasherBBRight",  Multimorphic.P3.Colors.Color.blue, 2, Priority+1);
			LEDHelpers.LEDFlashPop(p3, "flasherBBCenter",  Multimorphic.P3.Colors.Color.blue, 2, Priority+1);
			long score = ScoreManager.Score (random.Next((int)Scores.POP_HITS_MIN,(int)Scores.POP_HITS_MAX));

			LocationScore info = new LocationScore();
			info.score = score;
			string swName = ((Switch)eventData).Name;
			info.location = swName;
			PostModeEventToGUI("Evt_PopBumperHit", info);
			return false;
		}

        List<PlayfieldDeviceCapabilitiesStruct> deviceCapabilities = new List<PlayfieldDeviceCapabilitiesStruct>();
        private bool PlayfieldDeviceCapabilitiesEventHandler(string evtName, object evtData)
        {
            //Multimorphic.P3App.Logging.Logger.LogError ("PlayfieldDeviceCaps: ");
            deviceCapabilities = (List<PlayfieldDeviceCapabilitiesStruct>)evtData;
            foreach (PlayfieldDeviceCapabilitiesStruct caps in deviceCapabilities)
            {
                //Multimorphic.P3App.Logging.Logger.LogError("Name: " + caps.name);
                //Multimorphic.P3App.Logging.Logger.LogError("Can Lock.  Configuring ball entering");
                AddModeEventHandler(caps.fromEventNameBallEntered, DeviceEnteredHandler, Priority);
            }

            return SWITCH_CONTINUE;
        }

        private bool DeviceEnteredHandler(string evtName, object evtData)
        {
            if (deviceCapabilities != null)
            {
                if (deviceCapabilities[0].canEject)
                    PostModeEventToModes(deviceCapabilities[0].toEventNameEject, 1);
                else if (deviceCapabilities[0].canDrain)
                {
                    PostModeEventToModes(deviceCapabilities[0].toEventNameDrain, 1);
                }
            }
            return SWITCH_CONTINUE;
        }

        private bool DeviceDrainHandler(string evtName, object evtData)
        {
            // Add logic here to process the ball draining out of the device (ie. score, launch a new ball, run a lightshow, etc)
            return SWITCH_CONTINUE;
        }


        protected override void UpdateHUD()
		{
			base.UpdateHUD();
		}

		public void HUDElementColorEventHandler(string eventName, object eventData)
		{
			List<float> colorList = (List<float>)eventData;
			ushort [] elementColors = {1, 2, 3};
			for (int i=0; i<3; i++)
				elementColors[i] = (ushort)(colorList[i]*255);
		}

		public bool IncBonusXEventHandler(string eventName, object eventData)
		{
			bonusX += (int)eventData;
			UpdateHUD ();
			return false;
		}

		public bool ScoopEventHandler(string eventName, object eventData)
		{
			TestGameBallLauncher.delayed_launch();
			return SWITCH_STOP;
		}

        public bool DialogClosedEventHandler(string evtName, object evtData)
        {
            ShowButtonLegend();
            return (SWITCH_CONTINUE);
        }

		public virtual BonusInfo getBonusInfo()
		{
			BonusInfo info = new BonusInfo();
			info.Append (getBonusItems());
			info.SetMultiplier(ScoreManager.GetBonusX());
			return info;
		}

		private bool BallSearchBallLaunchRequestEventHandler(string evtName, object evtData)
		{
			TestGameBallLauncher.launch();
			return SWITCH_CONTINUE;
		}

		public override List<BonusItem> getBonusItems()
		{
			List<BonusItem> items = new List<BonusItem>();

			items.AddRange (lanesMode.getBonusItems());

			if (items.Count == 0)
			{
				BonusItem item = new BonusItem("Pity award", 5000);
				items.Add (item);
			}

			return items;
		}


	}
}

