using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Modes.Data;
using System.Collections.Generic;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.Modes
{

	public struct SceneCompleteInfo
	{
		public string sceneName;
		public long score;
	}

	/// <summary>
	/// The base class for modes which are scene-specific. Particularly useful for scenes which have an objective to be completed.
	/// </summary>
	public class SceneMode : TestGameGameMode
	{
		private RGBRandomFlashMode Show_RGBRandomFlash;
		protected InstructionMode instructionMode;
		protected List<string> Instructions;
		protected int bonusX;

		protected bool modeAttempted;
		protected bool modeAttemptedOnce;
		protected bool modeHoleLaunch;
		protected string activeInstruction = "Start Game";

		private bool showBallSaved;

		protected ushort [] flasherShipColorNormal =  Multimorphic.P3.Colors.Color.black;
		protected ushort [] flasherShipColorDrain =  Multimorphic.P3.Colors.Color.red;

		private const double DEFAULT_LAUNCH_DELAY = 3.0;

		public bool isAttempted { get { return(GetIsAttempted()); } }
		public bool isAttemptedOnce { get { return(GetIsAttemptedOnce()); } }
		public bool isAvailable { get { return(GetIsAvailable()); } }

		public SceneMode (P3Controller controller, int priority, string SceneName)
			: base(controller, priority)
		{
			sceneName = SceneName;
			modeCompleted = false;
			Show_RGBRandomFlash = new RGBRandomFlashMode(controller, priority + 1);
			Instructions = new List<string>();
			Instructions.Add ("Mode Instructions");
			Instructions.Add ("Instruction 1");
			Instructions.Add ("Instruction 2");
			Instructions.Add ("Instruction 3");
			instructionMode = new InstructionMode(controller, priority + 1, Instructions, 0);

			AddModeEventHandler("Evt_SideTargetHit", SideTargetHitEventHandler, Priority);
			AddModeEventHandler("Evt_BallLaunched", BallLaunchedHandler, Priority);
			AddModeEventHandler("Evt_BallSaved", BallSavedEventHandler, Priority);
			AddModeEventHandler("Evt_EnableBallSavedFeedback", EnableBallSavedFeedbackEventHandler, Priority);
		}

 		public override void mode_started ()
		{
			base.mode_started ();


			// Set up handler to respond to the scene going live in GUI.

			// Pause any previously running Ball Save
			PostModeEventToModes ("Evt_BallSavePause", 0);

			Reload ();

			modeHoleLaunch = false;

			PostActiveInstructions();

			DefaultLEDs();

			showBallSaved = true;
		}

		private void DefaultLEDs()
		{
			for (int i=0; i<LEDScripts.Count; i++)
			{
				if (LEDScripts[i].led.Name.Contains ("CenterArrow") ||
				    LEDScripts[i].led.Name.Contains ("wall") ||
				    LEDScripts[i].led.Name.Contains ("flasher") ||
				    LEDScripts[i].led.Name.Contains ("scoop")) 
				{
					LEDScripts[i] = LEDHelpers.OnLED (p3, LEDScripts[i],  Multimorphic.P3.Colors.Color.black);
				}
			}
		}

		public override void LoadPlayerData()
		{
			base.LoadPlayerData();

			bonusX = data.currentPlayer.GetData("BonusX", 1);
			modeAttempted = data.currentPlayer.GetData(sceneName + "Attempted", false);
			modeAttemptedOnce = data.currentPlayer.GetData(sceneName + "AttemptedOnce", false);
			modeCompleted = data.currentPlayer.GetData(sceneName + "Completed", false);
		}

		public override void SavePlayerData()
		{
			base.SavePlayerData();
			data.currentPlayer.SaveData(sceneName + "Attempted", modeAttempted);
			data.currentPlayer.SaveData(sceneName + "AttemptedOnce", modeAttemptedOnce);
			data.currentPlayer.SaveData(sceneName + "Completed", modeCompleted);
		}

		public virtual void Reset()
		{
			LoadPlayerData ();
			modeAttempted = false;
			modeAttemptedOnce = false;
			modeCompleted = false;
			SavePlayerData();
		}
		
		public void ResetAttempted()
		{
			LoadPlayerData ();
			modeAttempted = false;
			SavePlayerData ();
		}

		protected void Reload()
		{
			// Request GUI to load the scene.
			PostModeEventToGUI("Evt_LoadScene", sceneName);
		}
		
		public override void SceneLiveEventHandler( string evtName, object evtData )
		{

			foreach (GUIInsertScript script in GUIInsertScripts)
			{
				script.AddCommand("",  Multimorphic.P3.Colors.Color.black,  Multimorphic.P3.Colors.Color.black, 0, 0, 10);
				GUIInsertHelpers.AddorRemoveScript(script, true);
			}

			if (!modeAttempted) {
				PostModeEventToGUI("Evt_PlayIntro", sceneName);
				AddGUIEventHandler("Evt_IntroComplete", StartPlayingEventHandler);
			}
			else {
				StartPlaying();
			}

			modeAttempted = true;
			modeAttemptedOnce = true;
		}

		public void StartPlayingEventHandler( string evtName, object evtData )
		{
			PostModeEventToGUI("Evt_SceneResume", sceneName);
			modeHoleLaunch = true;
			RemoveGUIEventHandler("Evt_IntroComplete", StartPlayingEventHandler);
			StartPlaying ();
		}

		protected virtual void StartPlaying()
		{

			// Resume the Ball Save.  If one wasn't running, the resume logic in Ball Save should deal with that.
			if (sceneName != "Home" && sceneName != "Attract")
			{
				PostModeEventToModes ("Evt_EnableBallSearch", true);

				// Start ball save.  Some scenes might not have ball save settings.  So wrap in a try/catch.
				try
				{
					int ballSaveTime = data.GetGameAttributeValue(sceneName + "BallSaveTime").ToInt();
					if (ballSaveTime > 0)
						PostModeEventToModes ("Evt_BallSaveStart", ballSaveTime);
					PostModeEventToModes ("Evt_BallSavePauseUntilGrid", ballSaveTime);
				}
				catch
				{
				}
			}

			PostModeEventToModes ("Evt_ScoreRefresh", null);
			PostModeEventToModes ("Evt_RespawnEnable", true);
			PostModeEventToModes ("Evt_RefreshLanes", true);

			UpdateHUD();
		}

		public override void mode_stopped ()
		{
			EndCompletedLampShow();
			// In case mode ends unexpectedly due to s spurious ball drain or something
			p3.RemoveMode(instructionMode);

			base.mode_stopped();
		}

		public virtual bool SideTargetHitEventHandler( string evtName, object evtData )
		{
			PostModeEventToGUI("Evt_NullTargetHit", sceneName);
			return SWITCH_STOP;
		}

		protected void PostActiveInstructions()
		{
		}

		protected virtual bool BallSavedEventHandler(string evtName, object evtData)
		{
			TestGameBallLauncher.delayed_launch (2.25);
			if (showBallSaved)
				PostModeEventToGUI("Evt_BallSavePlayAnimation", sceneName);
			return SWITCH_STOP;
		}
		
		private bool EnableBallSavedFeedbackEventHandler(string evtName, object evtData)
		{
			showBallSaved = (bool)evtData;
			return SWITCH_STOP;
		}

		public override void End()
		{
			PostModeEventToModes ("Evt_SceneModeEnded", sceneName);
			base.End ();
		}

		public virtual void Pause()
		{
			PostModeEventToGUI("Evt_ScenePause", null);
		}

		protected virtual void Completed(long score)
		{
			StartCompletedShow();

			// Set completed and save it immediately so calls to isCompleted properly return true.
			modeCompleted = true;
			SavePlayerData();

			// No tell the rest of the game that the mode is completed.
			SceneCompleteInfo info = new SceneCompleteInfo();
			info.sceneName = sceneName;
			info.score = ScoreManager.Score (score);

			PostModeEventToGUI("Evt_SceneCompleted", info);
			PostModeEventToModes ("Evt_SceneModeCompleted", this);
		}

		private bool GetIsAttempted()
		{
			return (data.currentPlayer.GetData(sceneName + "Attempted", false));
		}

		private bool GetIsAttemptedOnce()
		{
			return (data.currentPlayer.GetData(sceneName + "AttemptedOnce", false));
		}

		public override bool GetIsCompleted()
		{
			return (data.currentPlayer.GetData(sceneName + "Completed", false));
		}

		protected virtual bool GetIsAvailable()
		{
			bool available =  !isAttempted && !isCompleted;
            Multimorphic.P3App.Logging.Logger.Log (sceneName + " : available: " + available.ToString());
            return (available);
		}

		private void StartCompletedShow()
		{
			// Just in case completed was accidentally called multiple times.
			EndCompletedLampShow();

			p3.AddMode (Show_RGBRandomFlash);
			this.delay("CompletedShowDelay", Multimorphic.NetProc.EventType.None, 2, new Multimorphic.P3.VoidDelegateNoArgs (EndCompletedLampShow));
		}

		private void EndCompletedLampShow()
		{
			p3.RemoveMode (Show_RGBRandomFlash);
		}

		public virtual bool sw_buttonRight1_active(Switch sw)
		{
			return SWITCH_STOP;
		}
		
		public virtual bool sw_buttonLeft1_active(Switch sw)
		{
			return SWITCH_STOP;
		}

		public virtual bool sw_buttonRight0_active(Switch sw)
		{
			PostModeEventToGUI("Evt_FlipperAction", sw);
			return SWITCH_CONTINUE;
		}
		
		public virtual bool sw_buttonLeft0_active(Switch sw)
		{
			PostModeEventToGUI("Evt_FlipperAction", sw);
			return SWITCH_CONTINUE;
		}

		public virtual bool sw_buttonRight0_inactive(Switch sw)
		{
			PostModeEventToGUI("Evt_FlipperAction", sw);
			return SWITCH_CONTINUE;
		}
		
		public virtual bool sw_buttonLeft0_inactive(Switch sw)
		{
			PostModeEventToGUI("Evt_FlipperAction", sw);
			return SWITCH_CONTINUE;
		}

		protected virtual void LaunchCallback()
		{
		}
		
		public virtual bool BallLaunchedHandler( string evtName, object evtData )
		{
			Multimorphic.P3App.Logging.Logger.Log ("BallLaunchedHandler");
			if (modeHoleLaunch)
			{
				PostModeEventToGUI("Evt_RightHoleEject", 0);
			}
			else
			{
				PostModeEventToGUI("Evt_BallLaunched", 0);
			}
			
			modeHoleLaunch = false;
			return SWITCH_STOP;
		}

		protected void SideTargetScore(int index, long score)
		{
			LocationScore info = new LocationScore();
			info.location = "sideTarget" + index.ToString();
			info.score = ScoreManager.Score (score);
			PostModeEventToGUI("Evt_SideTargetScore", info);
		}
		

		protected virtual void UpdateHUD()
		{
			PostModeEventToModes ("Evt_HUDClear", null);
			PostModeEventToModes ("Evt_HUDShowApronLCD", null);
		}

		protected virtual void ShowEB()
		{
			PostModeEventToModes ("Evt_HUDShowEB", null);
		}

		protected virtual void ShowScoringX()
		{	
			PostModeEventToModes ("Evt_HUDShowScoringX", null);
		}

		protected virtual void PostInstructionEvent(string instruction)
		{
			PostModeEventToGUI("Evt_PlaySceneInstruction", instruction);
		}

		public override ModeSummary getModeSummary()
		{
			ModeSummary modeSummary = new ModeSummary();
			modeSummary.Title = sceneName;
			modeSummary.Completed = modeCompleted;
			modeSummary.SetItemAndValue(0, "abc", "123");
			modeSummary.SetItemAndValue(1, "def", "456");
			modeSummary.SetItemAndValue(2, "ghi", "789");
			return modeSummary;
		}

		public virtual List<BonusItem> getBonusItems()
		{
			List<BonusItem> items = new List<BonusItem>();
			return items;
		}

	}
}

