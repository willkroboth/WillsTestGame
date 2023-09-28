using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using System.Collections.Generic;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Modes.Data;
using System;

namespace PinballClub.TestGame.Modes
{

	// Data the GUI will need when a jackpot is hit.
	public struct MultiballStatusStruct
	{
		public bool jackpot;
		public long jackpotBase;
		public long jackpotScore;
		public long totalScore;
		public int ballCount;
		public int multiplier;
		public bool super;
		public bool superDuper;
		public bool dbl;
		public int jackpotsRequired;
		public int jackpotsRemaining;
		public bool rightRamp;
		public int totalBalls;
	}

    public class Multiball : TestGameGameMode
	{
		private LiteLockMode liteLockMode;
		private RGBRandomFlashMode Show_RGBRandomFlash;

		// Number of balls in play during multiball (unused when not in multiball)
		private int ballsInPlay;
		private int debugBallLaunchRequests;
		private int debugSuccessfulLaunches;
		private int debugPopEscapeCtr;
		private int debugPopEscapeCtr2;
		private int debugShipBottomCtr;
		private int debugShipBottomCtr2;
		private int debugModeHoleCtr;
		private int debugModeHoleCtr2;
		private int debugLockCtr;
		private int debugRetriedLaunchesCtr;
		private int debugBallsLaunchedCtr;
		private bool debugEnabled = true;

		private bool locklit;
		private int ballsLocked;
		private int lockDifficulty;
		private int ballSaveTime;

		private int numBallsInSaucer;
		private int numBallsEjectingToStartMB;
		private bool saucerBallDraining;

		private bool multiballActive;
		// Multiplier for base jackpot value.  This is set at multiball start time based
		// on the number of balls to be used in multiball.
		private int jackpotBaseValueMultiplier;
		private long jackpotBase;
		// Number of jackpots hit so far.
		private int jackpots;
		// Number of jackpots required to complete Agent Multiball
		private int numJackpotsRequired;
		// Flag to indicate jackpots are worth double 
		private bool doubleJackpots;
		// Super Duper Jackpots are enabled.
		private bool superDuperJackpotEnabled;
		// Super Duper Jackpots are ready to be enabled
		private bool superDuperJackpotReady;
		// Flag indicating multiball is over, but jackpots are still enabled for a short period of time.
		private bool multiballGracePeriodActive;
		// Flag to indicate the side of the last ramp.  True for right.
		private bool lastJackpotSideRight;
		// Flag to indicate if multiball stops or alls the left ramp switch to fall through.
		private bool leftRampFallThrough;

		private long cumulativeMBScore;

		// List holding the 
		private List<string> jackpotInsertBasenames = new List<string>() {"LeftRamp", "RightRamp"};
		private List<string> allJackpotInsertBasenames = new List<string>() {"LeftRamp", "RightRamp"};

		// Flag to use Bottom Ship Exit (usually only if top exit isn't working.

		public Multiball (P3Controller controller, int priority)
			: base(controller, priority)
		{
			liteLockMode = new LiteLockMode(p3, Priority+1);
			Show_RGBRandomFlash = new RGBRandomFlashMode(controller, priority + 1,  Multimorphic.P3.Colors.Color.red);
			AddModeEventHandler("Evt_LeftLoopHit", LeftLoopHitEventHandler, Priority);
			AddModeEventHandler("Evt_LeftLoopBackwardsHit", LeftLoopHitEventHandler, Priority);
			AddModeEventHandler("Evt_RightRampInc", RightRampEventHandler, Priority);
			AddModeEventHandler("Evt_LeftRampInc", LeftRampEventHandler, Priority);
			AddModeEventHandler("Evt_SideTargetHit", SideTargetHitEventHandler, Priority);
			AddModeEventHandler("Evt_LITELOCKComplete", LITELOCKEventHandler, Priority);
			AddModeEventHandler("Evt_BallInSaucer", LockEventHandler, Priority);
			AddModeEventHandler("Evt_SaucerBallEjected", SaucerEjectEventHandler, Priority);
			AddModeEventHandler("Evt_SaucerBallDrained", SaucerDrainEventHandler, Priority);
			AddModeEventHandler("Evt_SaucerNumBalls", SaucerNumBallsEventHandler, Priority);
			AddModeEventHandler("Evt_BallSaved", BallSaveEventHandler, Priority); 
			AddModeEventHandler("Evt_PopEscape", PopEscapeHitEventHandler, Priority); 
			AddModeEventHandler("Evt_ModeHole", ModeHoleHitEventHandler, Priority); 
			AddModeEventHandler("Evt_ShipExitBottom", ShipExitBottomHitEventHandler, Priority); 
			AddModeEventHandler("Evt_RetriedLaunch", RetriedLaunchesHandler, Priority); 
			AddModeEventHandler("Evt_BallLaunched", BallLaunchedHandler, Priority); 
			AddModeEventHandler("Evt_ScoringX", UpdateStatusEventHandler, Priority); 
			AddModeEventHandler("Evt_AddDoubleShotBall", AddDoubleShotBallEventHandler, Priority); 
		}

		public override void mode_started ()
		{
			base.mode_started ();

			multiballActive = false;
			multiballGracePeriodActive = false;
			saucerBallDraining = false;

			numJackpotsRequired  = data.GetGameAttributeValue("NumJackpots").ToInt();

			if (!modeCompleted)
			{
				p3.AddMode(liteLockMode);
			}

			if (locklit)
			{
				LightLock ();
			}
			else if (ballsLocked > 0)
			{
				RefreshInserts();
			}
			else
			{
				PostModeEventToModes ("Evt_SaucerInactive", 1);
				RefreshInserts();
			}

			PostModeEventToModes ("Evt_SaucerQueryNumBalls", 1);

			debugBallLaunchRequests = 0;
			debugSuccessfulLaunches = 0;
			debugPopEscapeCtr = 0;
			debugPopEscapeCtr2 = 0;
			debugShipBottomCtr = 0;
			debugShipBottomCtr2 = 0;
			debugModeHoleCtr = 0;
			debugModeHoleCtr2 = 0;
			debugLockCtr = 0;
			debugRetriedLaunchesCtr = 0;
		}

		public override void mode_stopped ()
		{
			EndMultiballGracePeriod();
			p3.RemoveMode(liteLockMode);
			base.mode_stopped();
		}

		public override void LoadPlayerData()
		{
			base.LoadPlayerData();

		    jackpots = data.currentPlayer.GetData("MultiballJackpots", 0);
			ballsLocked = data.currentPlayer.GetData("MultiballBallsLocked", 0);
			lockDifficulty = data.currentPlayer.GetData("MultiballLockDifficulty", data.GetGameAttributeValue("LiteLockDifficulty").ToInt());
			locklit = data.currentPlayer.GetData("MultiballLockLit", false);
			modeCompleted = data.currentPlayer.GetData("MultiballCompleted", false);
		}
		
		public override void SavePlayerData()
		{
			base.SavePlayerData();
			data.currentPlayer.SaveData("MultiballJackpots", jackpots);
			data.currentPlayer.SaveData("MultiballLockLit", locklit);
			data.currentPlayer.SaveData("MultiballBallsLocked", ballsLocked);
			data.currentPlayer.SaveData("MultiballLockDifficulty", lockDifficulty);
			data.currentPlayer.SaveData("MultiballCompleted", modeCompleted);
		}

		public void Reset()
		{
			jackpots = 0;
			locklit = false;
			ballsLocked = 0;
			lockDifficulty = 0;
			modeCompleted = false;
			SavePlayerData();
		}

		public bool SaucerNumBallsEventHandler(string eventName, object eventData)
		{
			numBallsInSaucer = (int)eventData;

			return SWITCH_CONTINUE;
		}

		public bool LITELOCKEventHandler(string eventName, object eventData)
		{
			LightLock ();
			return false;
		}

		private void LightLock()
		{
			PostModeEventToGUI("Evt_MultiballLockLit", ballsLocked);
			locklit = true;
			//PostModeEventToModes ("Evt_SaucerLock", 1);
			// Move saucer to skip a hole and try to keep it balanced.
			PostModeEventToModes ("Evt_SaucerMove", 1);
			RefreshInserts();
		}

		private void RefreshInserts()
		{
			if (multiballActive)
			{
				for (int i=0; i<GUIInsertScripts.Count; i++)
				{
					GUIInsertScript script = GUIInsertScripts[i];
					if (script.insertName.Contains("ModeHole"))
					{
						script = GUIInsertHelpers.OnInsert (p3, script,  Multimorphic.P3.Colors.Color.black);
					}
				}

				for (int i=0; i<GUIInsertScripts.Count; i++)
				{
					GUIInsertScript script = GUIInsertScripts[i];
					foreach (string name in allJackpotInsertBasenames)
					{
						if (script.insertName.Contains(name))
					    {
							if (!superDuperJackpotEnabled && jackpotInsertBasenames.Contains (name))
							{
								if (script.insertName.Contains ("Arrow"))
									script = GUIInsertHelpers.BlinkInsert (p3, script,  Multimorphic.P3.Colors.Color.blue);
								else if (script.insertName.Contains ("Box1"))
									script = GUIInsertHelpers.OnInsert (p3, script, "Jackpot",  Multimorphic.P3.Colors.Color.on,  Multimorphic.P3.Colors.Color.blue);
								else if (script.insertName.Contains ("Box2"))
									script = GUIInsertHelpers.OnInsert (p3, script,  Multimorphic.P3.Colors.Color.black);
							}
							else 
							{
								script = GUIInsertHelpers.OnInsert (p3, script,  Multimorphic.P3.Colors.Color.black);
							}
						}
					}
					
					if (GUIInsertScripts[i].insertName.Contains ("Entrance"))
					{
						GUIInsertScripts[i] = GUIInsertHelpers.OnInsert (p3, GUIInsertScripts[i],  Multimorphic.P3.Colors.Color.black);
					}
				}
				LEDScriptsDict["flasherShip"] = LEDHelpers.BlinkLED (p3, LEDScriptsDict["flasherShip"],  Multimorphic.P3.Colors.Color.blue);
				
				for (int i=0; i<LEDScripts.Count; i++)
				{
					if (LEDScripts[i].led.Name.Contains ("flasherBB") || LEDScripts[i].led.Name.Contains ("SideModule"))
						LEDScripts[i] = LEDHelpers.BlinkLED (p3, LEDScripts[i],  Multimorphic.P3.Colors.Color.blue);
					else if (LEDScripts[i].led.Name.Contains ("wall") || LEDScripts[i].led.Name.Contains ("scoop"))
						LEDScripts[i] = LEDHelpers.BlinkLED (p3, LEDScripts[i],  Multimorphic.P3.Colors.Color.blue);
				}

			}
			else
			{

				// Disable all LED scripts that aren't used when multiball isn't active.
				for (int i=0; i<LEDScripts.Count; i++)
				{
					if (LEDScripts[i].led.Name.Contains ("flasherBB") || LEDScripts[i].led.Name.Contains ("SideModule") || LEDScripts[i].led.Name.Contains ("wall") || LEDScripts[i].led.Name.Contains ("scoop"))
						p3.LEDController.RemoveScript(LEDScripts[i]);
				}
				
				// Disable all GUI Insert scripts that aren't used when multiball isn't active.
				for (int i=0; i<GUIInsertScripts.Count; i++)
				{
					GUIInsertScript script = GUIInsertScripts[i];
					if (script.insertName.Contains("ModeHole"))
					{
						GUIInsertHelpers.AddorRemoveScript(GUIInsertScripts[i], false);
					}

					foreach (string name in jackpotInsertBasenames)
					{
						if (script.insertName.Contains(name))
						{
							GUIInsertHelpers.AddorRemoveScript(script, false);
						}
					}
				}

				// When lock is lit, turn on the ball lock GUI insert.
				if (locklit)
				{
					for (int i=0; i<GUIInsertScripts.Count; i++)
					{
						GUIInsertScript script = GUIInsertScripts[i];
						if (script.insertName.Contains("ShipEntrance"))
						{
							if (script.insertName.Contains ("Arrow"))
								script = GUIInsertHelpers.BlinkInsert (p3, script,  Multimorphic.P3.Colors.Color.yellow);
							else if (script.insertName.Contains ("Box1"))
								script = GUIInsertHelpers.OnInsert (p3, script, "Lock",  Multimorphic.P3.Colors.Color.black,  Multimorphic.P3.Colors.Color.yellow);
							else if (script.insertName.Contains ("Box2"))
								script = GUIInsertHelpers.OnInsert (p3, script, "Ball: " + (ballsLocked+1).ToString(),  Multimorphic.P3.Colors.Color.black,  Multimorphic.P3.Colors.Color.yellow);
						}
					}
					LEDScriptsDict["flasherShip"] = LEDHelpers.BlinkLED (p3, LEDScriptsDict["flasherShip"],  Multimorphic.P3.Colors.Color.ledyellow);
				}
				// When lock is not lit, turn on the start multiball insert if at least 1 ball is locked.
				else if (ballsLocked > 0)
				{
					for (int i=0; i<GUIInsertScripts.Count; i++)
					{
						GUIInsertScript script = GUIInsertScripts[i];
						if (script.insertName.Contains("ShipEntrance"))
							if (script.insertName.Contains ("Arrow"))
								script = GUIInsertHelpers.BlinkInsert (p3, script,  Multimorphic.P3.Colors.Color.blue);
						else if (script.insertName.Contains ("Box1"))
							script = GUIInsertHelpers.OnInsert (p3, script, (ballsLocked+1).ToString() + " Ball",  Multimorphic.P3.Colors.Color.on,  Multimorphic.P3.Colors.Color.blue);
						else if (script.insertName.Contains ("Box2"))
							script = GUIInsertHelpers.OnInsert (p3, script, "Multiball",  Multimorphic.P3.Colors.Color.on,  Multimorphic.P3.Colors.Color.blue);
					}

					if (saucerBallDraining)
						LEDScriptsDict["flasherShip"] = LEDHelpers.OnLED (p3, LEDScriptsDict["flasherShip"],  Multimorphic.P3.Colors.Color.red);
					else
						LEDScriptsDict["flasherShip"] = LEDHelpers.BlinkLED (p3, LEDScriptsDict["flasherShip"],  Multimorphic.P3.Colors.Color.blue);
				}
				// Nothing lit yet lit
				else
				{
					for (int i=0; i<GUIInsertScripts.Count; i++)
					{
						GUIInsertHelpers.AddorRemoveScript(GUIInsertScripts[i], false);
					}
					if (saucerBallDraining)
						LEDScriptsDict["flasherShip"] = LEDHelpers.OnLED (p3, LEDScriptsDict["flasherShip"],  Multimorphic.P3.Colors.Color.red);
					else
						LEDScriptsDict["flasherShip"] = LEDHelpers.OnLED (p3, LEDScriptsDict["flasherShip"],  Multimorphic.P3.Colors.Color.lightyellow);
				}
			}

		}

		// Handle events from the ship indicating a new ball was shot into it.
		public bool LockEventHandler(string eventName, object eventData)
		{
			// Increment a counter to track the number of balls in the ship.
			numBallsInSaucer++;

			// If multiball is already active, ask the ship to eject the new ball.
			if (multiballActive)
			{
				UpdateStatus ();
				PostModeEventToModes ("Evt_SaucerEject", 1);
				ScoreManager.Score (250);
			}
			// If lock is lit, process the lock.
			else if (locklit) {

				if (ballsLocked == 0)	
					debugBallsLaunchedCtr = 0;

				// Tell the ship to hold the ball.
				PostModeEventToModes ("Evt_SaucerMove", 1);
				// Keep track of how many balls are locked for this player.
				ballsLocked++;


				// locklit should never be true for the 8th ball, but deal with it just in case
				// by starting multiball.
				if (ballsLocked == 8)
				{
					startMultiball(ballsLocked);
				}
				else 
				{
					ScoreManager.Score (Scores.MULTIBALL_BALL_LOCKED);

					// Tell the GUI another ball is locked.
					PostModeEventToGUI("Evt_MultiballBallLocked", ballsLocked);

					// If there are more than this player's balls in the ship, eject one.
					if (ballsLocked < numBallsInSaucer)
					{
						PostModeEventToModes ("Evt_SaucerEject", 1);
					}
					// Otherwise, launch one after a delay (to play an animation or something).
					else
					{
						debugLockCtr++;
						TestGameBallLauncher.delayed_launch(IncDebugSuccessfulLaunches);
						IncDebugBallLaunchRequests();
					}

					// If 7 balls in saucer, 8-ball multiball is lit.  So no need to lite lock for the 8th ball.
					if (ballsLocked == 7) 
					{
						StopLiteLockMode();
					}
					else
					{
						// Start the next LITE LOCK sequence.
						liteLockMode.start (lockDifficulty);
						locklit = false;
					}

					// Change the indicators
					RefreshInserts();
				}
			}
			// If lock isn't lit and there are balls in the saucer, start multiball.
			else if (ballsLocked > 0)
			{
				ScoreManager.Score (Scores.MULTIBALL_START);
				ballsLocked++;
				startMultiball(ballsLocked);
			}
			else
			{
				ScoreManager.Score (Scores.MULTIBALL_BALL_NOT_LOCKED);

				// Tell the GUI
				PostModeEventToGUI("Evt_MultiballBallNotLocked", ballsLocked);

				if (isShipDrainEnabled())
				{
					PostModeEventToModes ("Evt_SaucerDrain", 1);
					saucerBallDraining = true;
					RefreshInserts();
				}
				else
				{
					PostModeEventToModes ("Evt_SaucerEject", 1);
				}
			}
	
			return false;
		}

		public bool AddDoubleShotBallEventHandler(string eventName, object eventData)
		{
			leftRampFallThrough = data.GetGameAttributeValue("MultiballDoubleShotAliens").ToBool(); 
			ballsInPlay++;
			return SWITCH_CONTINUE;
		}

		private void startMultiball(int numBallsLocked)
		{

			cumulativeMBScore = 0;

			// Set flag used by other logic.
			multiballActive = true;

			// Jackpot can't start with balls in play.  So reset count to 0 to prepare for counting balls
			// exiting the ship or being launched (if the ship doesn't still have balls due to another player
			// already playing multiball.
			ballsInPlay = 0;

			leftRampFallThrough = false;

			// Post an event telling other modes that Multiball has started.
			PostModeEventToModes ("Evt_MultiballStarting", true);

			PostModeEventToModes ("Evt_EnableBallSavedFeedback", false);
			PostModeEventToModes ("Evt_RespawnEnable", false);
			PostModeEventToModes ("Evt_EnableLanes", false);

			// Set the base jackpot multiplier.
			// ballsInPlay might not be 0 if the MultiballStarting event caused an event handler to increment it.
			// ie via Evt_AddDoubleShotBall.
			jackpotBaseValueMultiplier = (ballsInPlay + numBallsLocked) - 1;
			jackpotBase = Scores.MULTIBALL_JACKPOT_BASE_VALUE*jackpotBaseValueMultiplier;

			// Super Duper Jackpot is when all 8 balls are in play.  Since it takes time to get them into play
			// by ship ejects or launches, set a flag that'll be checked later.
			superDuperJackpotReady = numBallsLocked == 8;

			// Super Duper Jackpot cannot be enabled immediately.  It takes time to get balls into play.
			superDuperJackpotEnabled = false;

			// Jackpots are worth double when Bar Mode has been completed (player is "seeing double")
			doubleJackpots = data.currentPlayer.GetData("BarCompleted", false);

			// Reset the grace period flag.  Grace Period gives the play a chance to hit more jackpots
			// with a short time after draining the 2nd to last ball (thereby ending multiball).
			multiballGracePeriodActive = false;

			// Reset list of enabled jackpots
			if (data.GetGameAttributeValue("MultiballAlternateJPs").ToBool() )
				jackpotInsertBasenames = new List<string>() {"LeftRamp"};
			else
				jackpotInsertBasenames = new List<string>() {"LeftRamp", "RightRamp"};

			MultiballStatusStruct status = GetStatus(0);
			status.totalBalls = ballsLocked;
			// Issue event to the GUI for multiball start graphics/sound.
			PostModeEventToGUI("Evt_MultiballStart", status);

			// See if the ship still has all of the balls that were locked by this player.
			// First request the ship ejects all that it has.
			// Then issue launch requests for the remaining number of balls.
			if (ballsLocked > numBallsInSaucer)
			{
				PostModeEventToModes ("Evt_SaucerEject", numBallsInSaucer);
				numBallsEjectingToStartMB = numBallsInSaucer;
				int ballsToLaunch = ballsLocked - numBallsInSaucer;
				for (int i=0; i<ballsToLaunch; i++)
				{
					TestGameBallLauncher.launch (LaunchCallback);
				}
			}
			// Otherwise the requisite number of balls from the ship.
			else
			{
				PostModeEventToModes ("Evt_SaucerEject", ballsLocked);
				numBallsEjectingToStartMB = ballsLocked;
			}

			// Start ball save time according to the settings.
			ballSaveTime  = data.GetGameAttributeValue("MultiballBallSaveTime").ToInt();
			if (ballSaveTime > 0)
			{
				PostModeEventToModes ("Evt_BallSaveAdd", ballSaveTime);
				PostModeEventToModes ("Evt_BallSavePauseUntilGrid", ballSaveTime);
			}

			// Refresh lights so that jackpots are visible.
			RefreshInserts();

			// Prepare for the next multiball.
			lockDifficulty++;
			StopLiteLockMode();
		}

		private void StopLiteLockMode()
		{
			// Stop the current lock logic and prepare for the next multiball.
			locklit = false;
			liteLockMode.stop ();
		}

		private void EndMultiball()
		{
			// Issue event to the GUI that MB has ended.  At the very least it will end the multiball music.
			PostModeEventToGUI("Evt_MultiballEnd", ballsLocked);

			ballsLocked = 0;
			multiballActive = false;
			multiballGracePeriodActive = true;
			this.delay("MultiballGracePeriodDelay", Multimorphic.NetProc.EventType.None, 2, new Multimorphic.P3.VoidDelegateNoArgs (EndMultiballGracePeriod));
						// Only start liteLockMode again if multiball isn't yet completed.
			if (!modeCompleted)
			{
				liteLockMode.start (lockDifficulty);
			}

			RefreshInserts();

			PostModeEventToModes ("Evt_MultiballEnded", this);
			PostModeEventToModes ("Evt_EnableBallSavedFeedback", true);
			PostModeEventToModes ("Evt_RespawnEnable", true);
			PostModeEventToModes ("Evt_EnableLanes", true);
		}

		private void EndMultiballGracePeriod()
		{
			multiballGracePeriodActive = false;
		}

		private void StartSuperDuperJackpot()
		{
			PostModeEventToGUI("Evt_MultiballSuperDuperJackpotLit", true);
			superDuperJackpotReady = false;
			superDuperJackpotEnabled = true;
			RefreshInserts();
		}

		private void StartSuperDuperJackpotGracePeriod()
		{
			superDuperJackpotReady = false;
			if (superDuperJackpotEnabled)
			{
				RefreshInserts();
				PostModeEventToGUI("Evt_MultiballSuperDuperJackpotLit", false);
				this.delay("SuperDuperJackpotGracePeriodDelay", Multimorphic.NetProc.EventType.None, 2, new Multimorphic.P3.VoidDelegateNoArgs (EndSuperDuperJackpot));
			}
		}

		private void EndSuperDuperJackpot()
		{
			// Tell GUI to stop super duper here.  Could have gotten here due to the shot being hit.
			PostModeEventToGUI("Evt_MultiballSuperDuperJackpotLit", false);
			superDuperJackpotReady = false;
			superDuperJackpotEnabled = false;
			RefreshInserts();
		}

		public bool LeftLoopHitEventHandler(string evtName, object evtData)
		{
			if (multiballActive)
				return SWITCH_STOP;
			else
				return SWITCH_CONTINUE;
		}

		public bool RightRampEventHandler(string evtName, object evtData)
		{
			if (multiballActive || superDuperJackpotEnabled)
			{
				if (jackpotInsertBasenames.Contains("RightRamp"))
				{
					lastJackpotSideRight = true;
					Jackpot ();
					if (modeCompleted || data.GetGameAttributeValue("MultiballAlternateJPs").ToBool()) 
					{
						if (!jackpotInsertBasenames.Contains ("LeftRamp"))
							jackpotInsertBasenames.Add ("LeftRamp");
						jackpotInsertBasenames.Remove ("RightRamp");
						RefreshInserts();
					}
				}
				return SWITCH_STOP;
			}
			else if (multiballGracePeriodActive)
			{
				if (jackpotInsertBasenames.Contains("RightRamp"))
				{
					lastJackpotSideRight = true;
					Jackpot ();
				}
			}
			return SWITCH_CONTINUE;
		}

		public bool LeftRampEventHandler(string evtName, object evtData)
		{
			if (multiballActive || superDuperJackpotEnabled)
			{
				if (jackpotInsertBasenames.Contains("LeftRamp"))
				{
					lastJackpotSideRight = false;
					Jackpot ();
					if (modeCompleted || data.GetGameAttributeValue("MultiballAlternateJPs").ToBool()) 
					{
						if (!jackpotInsertBasenames.Contains ("RightRamp"))
							jackpotInsertBasenames.Add ("RightRamp");
						jackpotInsertBasenames.Remove ("LeftRamp");
						RefreshInserts();
					}
				}
				if (leftRampFallThrough)
					return SWITCH_CONTINUE;
				else
					return SWITCH_STOP;
			}
			else if (multiballGracePeriodActive)
			{
				if (jackpotInsertBasenames.Contains("LeftRamp"))
				{
					lastJackpotSideRight = false;
					Jackpot ();
				}
			}
			return SWITCH_CONTINUE;
		}

		private void Jackpot()
		{
			// Could theoretically get a jackpot when only one ball has been put into play so far at the start of multiball.
			// So bump the count up to 2 so that the jackpot logic works correctly.
			int multipliers = GetJackpotX();

			StartJackpotShow();

			int doubleX = 1;
			if (doubleJackpots) doubleX = 2;


			int jackpotX;
			// Jackpot multiplier
			if (superDuperJackpotEnabled) {
				if (jackpots + (Scores.MULTIBALL_SUPER_DUPER_X * doubleX) < numJackpotsRequired) {
					jackpotX = numJackpotsRequired - jackpots;
					jackpots = numJackpotsRequired;
				}
				else {
					jackpots += Scores.MULTIBALL_SUPER_DUPER_X * doubleX;
					jackpotX = Scores.MULTIBALL_SUPER_DUPER_X * doubleX;
				}
			}
			else {
				jackpotX = multipliers;
				jackpots += jackpotX;
			}

			long jackpotScore = jackpotBase*jackpotX;
			long totalJackpotScore = ScoreManager.Score (jackpotScore);

			cumulativeMBScore += totalJackpotScore;


			UpdateStatus(totalJackpotScore);

			// Now that jackpotData has been sent to the GUI, clean up.
			if (superDuperJackpotEnabled) {
				EndSuperDuperJackpot();
			}

			if (jackpots >= numJackpotsRequired)
			{
				CompleteMultiball();
			}

			RefreshInserts();

			//removePopup();
			//GUIInsertScriptsDict["InfoPopup"] = GUIInsertHelpers.OnInsert(p3, GUIInsertScriptsDict["InfoPopup"],"Jackpot: " + jackpots,  Multimorphic.P3.Colors.Color.on,  Multimorphic.P3.Colors.Color.on);
			//this.delay("jackpot delay", Multimorphic.NetProc.EventType.None, 1, new Multimorphic.P3.VoidDelegateNoArgs (removePopup));
		}

		private bool UpdateStatusEventHandler(string evtName, object evtData)
		{
			UpdateStatus (0);
			return SWITCH_CONTINUE;
		}

		private void UpdateStatus()
		{
			UpdateStatus (0);
		}

		private int GetJackpotX()
		{
			bool dynamicMultiplier = data.GetGameAttributeValue("MultiballDynamicX").ToBool();
			int multipliers;

			int doubleX = 1;
			if (doubleJackpots) doubleX = 2;

			if (dynamicMultiplier)
				multipliers = (ballsInPlay - 1) * doubleX;
			else
				multipliers = jackpotBaseValueMultiplier * doubleX;

			// So ensure the multiplier is always at least 1 to avoid giving the player jackpots of 0 points.
			if (multipliers <= doubleX) {
				multipliers = doubleX;
			}

			return multipliers;
		}

		private void UpdateStatus(long totalJackpotScore)
		{
			MultiballStatusStruct mbStatus = GetStatus(totalJackpotScore);
			PostModeEventToGUI("Evt_MultiballStatus", mbStatus);
			if (totalJackpotScore != 0)
				PostModeEventToGUI("Evt_MultiballJackpot", mbStatus);
		}

		private MultiballStatusStruct GetStatus(long totalJackpotScore)
		{
			int numBallsActive = ballsInPlay;
			int multipliers = GetJackpotX();

			MultiballStatusStruct mbStatus = new MultiballStatusStruct();
			mbStatus.totalScore = cumulativeMBScore;
			mbStatus.jackpotsRequired = numJackpotsRequired;
			mbStatus.jackpotsRemaining = numJackpotsRequired - jackpots;
			if (mbStatus.jackpotsRemaining < 0)
				mbStatus.jackpotsRemaining = 0;
			mbStatus.jackpotScore = totalJackpotScore;
			mbStatus.ballCount = numBallsActive;
			mbStatus.multiplier = multipliers;
			mbStatus.rightRamp = lastJackpotSideRight;

			mbStatus.jackpotBase = jackpotBase * (long)ScoreManager.GetX ();

			mbStatus.dbl = doubleJackpots;
			mbStatus.super = modeCompleted;
			mbStatus.superDuper = superDuperJackpotEnabled;

			return mbStatus;
		}

		private void CompleteMultiball()
		{
			modeCompleted = true;
			PostModeEventToModes ("Evt_MultiballCompleted", this);
			PostModeEventToGUI("Evt_MultiballCompleted", 0);
		}

		private void removePopup()
		{
			cancel_delayed("jackpot delay");
			GUIInsertHelpers.AddorRemoveScript(GUIInsertScriptsDict["InfoPopup"], false);
		}

		public bool SaucerEjectEventHandler(string eventName, object eventData)
		{
			numBallsInSaucer--;

			if (multiballActive)
			{
				if (numBallsEjectingToStartMB > 0)
				{
					PostModeEventToGUI("Evt_MultiballBallEject", 0);
					ballsInPlay++;
					numBallsEjectingToStartMB--;
				}

				// If all 8 balls are in play and superDuperJackpot is ready, start it!  Exciting!
				if (superDuperJackpotReady && ballsInPlay == 8)
				{
					StartSuperDuperJackpot();
				}

				UpdateStatus();
			}

			return SWITCH_CONTINUE;
		}

		public bool SaucerDrainEventHandler(string eventName, object eventData)
		{
			saucerBallDraining = false;
			numBallsInSaucer--;
			TestGameBallLauncher.launch ();
			RefreshInserts();

			return SWITCH_CONTINUE;
		}

		public bool ShipExitBottomHitEventHandler( string evtName, object evtData )
		{
			debugShipBottomCtr2++;
			return SWITCH_CONTINUE;
		}

		public bool sw_shipExitBottom_active(Switch sw)
		{
			if (multiballActive)
			{
				debugShipBottomCtr++;
				TestGameBallLauncher.delayed_launch(IncDebugSuccessfulLaunches);
				IncDebugBallLaunchRequests();
				return SWITCH_STOP;
			}
			
			return SWITCH_CONTINUE;
		}

		public bool RetriedLaunchesHandler( string evtName, object evtData )
		{
			debugRetriedLaunchesCtr++;
			return SWITCH_CONTINUE;
		}

		public bool BallLaunchedHandler( string evtName, object evtData )
		{
			debugBallsLaunchedCtr++;
			return SWITCH_CONTINUE;
		}
		
		public bool SideTargetHitEventHandler( string evtName, object evtData )
		{
			if (multiballActive)
				return SWITCH_STOP;
			else
				return SWITCH_CONTINUE;
		}

		public bool ModeHoleHitEventHandler( string evtName, object evtData )
		{
			debugModeHoleCtr2++;
			return SWITCH_CONTINUE;
		}

		public bool sw_modeHole_active(Switch sw)
		{
			if (multiballActive)
			{
				debugModeHoleCtr++;
				TestGameBallLauncher.delayed_launch(IncDebugSuccessfulLaunches);
				IncDebugBallLaunchRequests();
				return SWITCH_STOP;
			}

			return SWITCH_CONTINUE;
		}

		public bool PopEscapeHitEventHandler( string evtName, object evtData )
		{
			debugPopEscapeCtr2++;
			return SWITCH_CONTINUE;
		}

		public bool sw_popEscape_active(Switch sw)
		{
			if (multiballActive)
			{
				debugPopEscapeCtr++;
				TestGameBallLauncher.delayed_launch(IncDebugSuccessfulLaunches);
				IncDebugBallLaunchRequests();
				return SWITCH_STOP;
			}

			return SWITCH_CONTINUE;
		}

		// This is used by BallLaunchRequests that start multiball (because the saucer didn't have enough balls -
		// due to ball stealing by another player in a multiplayer game.
		private void LaunchCallback()
		{
			ballsInPlay++;

			if (superDuperJackpotReady)
			{
				if (ballsInPlay == 8)
				{
					StartSuperDuperJackpot();
				}
			}
		}

		public bool sw_buttonRight1_active(Switch sw)
		{
			if (multiballActive)
				return SWITCH_STOP;
			else
				return SWITCH_CONTINUE;
		}
		
		public bool sw_buttonLeft1_active(Switch sw)
		{
			if (multiballActive)
				return SWITCH_STOP;
			else
				return SWITCH_CONTINUE;
		}

		public bool AlienAttackCompleteHandler( string evtName, object evtData )
		{
			if (multiballActive)
			{
				AddBall (true);
			}
			return SWITCH_CONTINUE;
		}

		private void AddBall(bool addBallSaveTime)
		{
			TestGameBallLauncher.launch ();
			IncDebugBallLaunchRequests();
			ballsInPlay++;
			jackpotBaseValueMultiplier++;
			UpdateStatus();
			if (addBallSaveTime && ballSaveTime > 0)
			{
				PostModeEventToModes ("Evt_BallSaveAdd", ballSaveTime);
			}
		}

		private void IncDebugBallLaunchRequests()
		{
			debugBallLaunchRequests++;
			UpdateDebug();
		}

		private void IncDebugSuccessfulLaunches()
		{
			debugSuccessfulLaunches++;
			UpdateDebug();
		}

		private void UpdateDebug()
		{
			if (debugEnabled)
			{

				int totalReasonsForLaunch = debugLockCtr + debugModeHoleCtr + debugPopEscapeCtr + debugShipBottomCtr;
				string debug;
				debug = "";
				debug += "Locks: " + debugLockCtr.ToString();
				debug += "\nMode Holes2: " + debugModeHoleCtr2.ToString();
				debug += "\nMode Holes: " + debugModeHoleCtr.ToString();
				debug += "\nPop Escapes2: " + debugPopEscapeCtr2.ToString();
				debug += "\nPop Escapes: " + debugPopEscapeCtr.ToString();
				debug += "\nShip Bottoms2: " + debugShipBottomCtr2.ToString();
				debug += "\nShip Bottoms: " + debugShipBottomCtr.ToString();
				debug += "\n\nTotal reasons: " + totalReasonsForLaunch.ToString();
				debug += "\nLaunch requests: " + debugBallLaunchRequests.ToString();
				debug += "\nRetried Launches: " + debugRetriedLaunchesCtr.ToString();
				debug += "\nLaunch calls compl: " + debugBallsLaunchedCtr.ToString();
				debug += "\nSuccessful launches: " + debugSuccessfulLaunches.ToString();
				//PostModeEventToGUI("Evt_Debug", debug);
			}
		}

		public bool BallSaveEventHandler(string evtName, object evtData)
		{

			StartSuperDuperJackpotGracePeriod();
			return SWITCH_CONTINUE;
		}

		public bool sw_drain_active(Switch sw)
		{
			StartSuperDuperJackpotGracePeriod();
			ballsInPlay--;

			if (multiballActive)
			{
				if (ballsInPlay <= 1)
				{
					EndMultiball ();

				}
				UpdateStatus();
				if (ballsInPlay <= 0)
					return SWITCH_CONTINUE;
				else
					return SWITCH_STOP;
			}
			else
				return SWITCH_CONTINUE;
		}

		private void StartJackpotShow()
		{
			// Just in case completed was accidentally called multiple times.
			EndJackpotLampShow();
			
			p3.AddMode (Show_RGBRandomFlash);
			this.delay("JackpotShowDelay", Multimorphic.NetProc.EventType.None, 2, new Multimorphic.P3.VoidDelegateNoArgs (EndJackpotLampShow));
		}
		
		private void EndJackpotLampShow()
		{
			p3.RemoveMode (Show_RGBRandomFlash);
		}

		private bool isShipDrainEnabled()
		{
			return data.GetGameAttributeValue("ShipBottomExitEnabled").ToBool();
		}

		public override ModeSummary getModeSummary()
		{
			ModeSummary modeSummary = new ModeSummary();
			modeSummary.Title = "Multiball";
			modeSummary.Completed = modeCompleted;
			int targetsRemaining = numJackpotsRequired - jackpots;
			if (targetsRemaining < 0) 
				targetsRemaining = 0;
			modeSummary.SetItemAndValue(0, "Agents Remaining", targetsRemaining.ToString("n0"));
			modeSummary.SetItemAndValue(1, "Score", cumulativeMBScore.ToString("n0"));
			modeSummary.SetItemAndValue(2, "", "");
			return modeSummary;
		}

	}
}

