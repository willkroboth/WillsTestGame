using System;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.NetProcMachine.Config;
using System.Collections.Generic;

namespace PinballClub.TestGame.Modes
{

	public class TestGameHomeMode : SceneMode
	{

		// private SomeRelatedMode someRelatedMode;
		private bool _ballStarted;
		private BirdRampMode _birdRampMode;
		private TimerMode _mainTimer;

		public TestGameHomeMode (P3Controller controller, int priority, string SceneName)
			: base(controller, priority, SceneName)
		{
			_ballStarted = false;
			_birdRampMode = new BirdRampMode(p3, TestGamePriorities.PRIORITY_BIRD_RAMPS);
			_mainTimer = new TimerMode(p3, priority, MainTimerExpired, "Main");

			AddModeEventHandler(TestGameEventNames.InitialLaunch, InitialLaunchEventHandler, Priority);

			// Handle ball falling down holes
			foreach (BallPathDefinition shot in p3.BallPaths.Values)
			{
				if (shot.ExitType == BallPathExitType.Hole)
				{
					Multimorphic.P3App.Logging.Logger.Log(
						Multimorphic.P3App.Logging.LogCategories.Game,
						"Installing ShotHitHandler for : " + shot.CompletedEvent + " exitType: " + shot.ExitType.ToString());
					AddModeEventHandler(shot.CompletedEvent, HolePathEventHandler, priority);
				}
			}
			AddModeEventHandler("Evt_TroughLauncherEntry", HolePathEventHandler, priority);
		}

		// Handle launching the ball
		public bool InitialLaunchEventHandler(string eventName, object eventData)
		{
			PostModeEventToModes(TestGameEventNames.BallStarted, 0);
			PostModeEventToModes(TestGameEventNames.ChangeGameState, GameState.BallInPlay);
			TestGameBallLauncher.launch(LaunchCallback);
			return EVENT_STOP;
		}
		protected override void LaunchCallback()
        {
			Multimorphic.P3App.Logging.Logger.Log("LaunchCallback called. Ball launched.");
			PostModeEventToModes(TestGameEventNames.EnableBallSearch, true);

			p3.AddMode(_mainTimer);
#if DEBUG
			_mainTimer.Start(10);
#else
			_mainTimer.Start(45);
#endif
		}
		public bool sw_launch_active(Switch sw)
        {
			if(!_ballStarted)
            {
				_ballStarted = true;
				PostModeEventToModes(TestGameEventNames.InitialLaunch, null);
            }
			return SWITCH_CONTINUE;
        }
		public bool sw_launch_active_for_2s(Switch sw)
        {
			Multimorphic.P3App.Logging.Logger.Log("Debug Launch.");
			TestGameBallLauncher.launch();
			return SWITCH_CONTINUE;
        }

		// Handle the ball falling into a hole
		public bool HolePathEventHandler(string eventName, object eventData)
        {
			TestGameBallLauncher.launch();
			return EVENT_STOP;
        }


        public override void mode_started ()
		{
			base.mode_started ();

			 p3.AddMode(_birdRampMode);

			// AddGUIEventHandler ("Evt_SomeGUIEventName", SomeHandlerFunction);
			// AddModeEventHandler ("SomeModeEventName", SomeHandlerFunction, priority);
		}

		public override void mode_stopped ()
		{
			// p3.RemoveMode (someRelatedMode);
			// RemoveGUIEventHandler ("Evt_SomeGUIEventName", SomeHandlerFunction);
			// RemoveModeEventHandler ("Evt_SomeModeEventName", SomeHandlerFunction, priority);
			// p3.RemoveMode(someRelatedMode);
			base.mode_stopped();
			p3.RemoveMode(_birdRampMode);
		}

		public override void LoadPlayerData()
		{
			base.LoadPlayerData();
			// Add any special data loading needed here for this scene and this player
		}

		public override void SavePlayerData()
		{
			base.SavePlayerData();
			// Add any special data loading needed here for this scene and this player
		}

		public override void SceneLiveEventHandler( string evtName, object evtData )
		{
			//base.SceneLiveEventHandler(evtName, evtData);
			// Add any special setup that the scene requires here, including sending messages to the GUI.

			// Nope, we're not doing that just start the game
			StartPlaying();
		}

		protected override void StartPlaying()
		{
			base.StartPlaying();
			_ballStarted = false;

			// Enable flippers, slings, and pop bumpers
			PostModeEventToModes(TestGameEventNames.EnableFlippers, true);
			PostModeEventToModes(TestGameEventNames.EnableBallSearch, true);
			PostModeEventToModes(TestGameEventNames.EnableBumpers, true);

			// Initialize the GUI
			PostModeEventToGUI(TestGameEventNames.TestGameHomeSetup, 0);

			// PostInstructionEvent("Some instructions");
			//TestGameBallLauncher.launch ();
		}

		protected override void Completed(long score)
		{
			base.Completed (score);
			PostModeEventToModes ("Evt_TestGameHomeCompleted", 0);
		}


		public bool sw_slingL_active(Switch sw)
		{
			// Add code here to let he GUI side know about that a sling has been hit
			//e.g. PostModeEventToGUI("Evt_TestGameHomeSlingHit", false);

			return SWITCH_CONTINUE;   // use SWITCH_STOP to prevent other modes from receiving this notification.
		}

		public bool sw_slingR_active(Switch sw)
		{
			// Add code here to let he GUI side know about that a sling has been hit
			// e.g. PostModeEventToGUI("Evt_TestGameHomeSlingHit", false);
			return SWITCH_CONTINUE;   // use SWITCH_STOP to prevent other modes from receiving this notification.
		}

		public bool sw_drain_active(Switch sw)
        {
			// Timed game, just relaunch the ball if drained
			TestGameBallLauncher.launch();
			return SWITCH_CONTINUE;
        }

		public override void End()
		{
			// someRelatedMode.End();

			Pause();
			// Save any remaining stats

			base.End ();
		}

		public void Pause()
		{
			p3.ModesToGUIEventManager.Post("Evt_ScenePause", null);
		}

		public override ModeSummary getModeSummary()
		{
			ModeSummary modeSummary = new ModeSummary();
			modeSummary.Title = sceneName;
			modeSummary.Completed = modeCompleted;
			if (modeCompleted) 
				modeSummary.SetItemAndValue(0, "TestGameHome completed!", "");
			else
				modeSummary.SetItemAndValue(1, "TestGameHome not yet completed!", "");
			modeSummary.SetItemAndValue(2, "", "");
			return modeSummary;
		}

		private void MainTimerExpired()
        {
			Multimorphic.P3App.Logging.Logger.Log("Main timer expiered. Ending game.");
			PostModeEventToModes(TestGameEventNames.MainTimerExpired, null);
			End();
        }
	}
}
