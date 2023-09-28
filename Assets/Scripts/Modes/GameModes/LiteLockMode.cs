using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using System.Collections.Generic;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Modes.Data;

namespace PinballClub.TestGame.Modes
{

    public class LiteLockMode : TestGameGameMode
	{

		private List<string> InsertNames;
		private List<bool> states;
		private int difficulty;
		private bool complete;
		private List<string> LEDNames;

		public const int DIFFICULTY_EASY = 0;
		public const int DIFFICULTY_MEDIUM = 1;
		public const int DIFFICULTY_HARD = 2;

		public LiteLockMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			InsertNames = new List<string>();

			InsertNames.Add ("litE");
			InsertNames.Add ("liTe");
			InsertNames.Add ("lIte");
			InsertNames.Add ("Lite");
			InsertNames.Add ("locK");
			InsertNames.Add ("loCk");
			InsertNames.Add ("lOck");
			InsertNames.Add ("Lock");

			LEDNames = new List<string>();
			LEDNames.Add("flasherSideModuleLeft0");
			LEDNames.Add("flasherSideModuleLeft1");
			LEDNames.Add("flasherSideModuleLeft2");
			LEDNames.Add("flasherSideModuleLeft3");
			LEDNames.Add("flasherSideModuleRight0");
			LEDNames.Add("flasherSideModuleRight1");
			LEDNames.Add("flasherSideModuleRight2");
			LEDNames.Add("flasherSideModuleRight3");
		}

		public override void mode_started ()
		{
			base.mode_started ();
			AddModeEventHandler ("Evt_SideTargetHit", SideTargetHitEventHandler, Priority);
			refreshInserts();	
		}

		public override void mode_stopped ()
		{
			RemoveModeEventHandler ("Evt_SideTargetHit", SideTargetHitEventHandler, Priority);
			base.mode_stopped ();
		}

		public override void LoadPlayerData()
		{
			complete = data.currentPlayer.GetData("LITELOCKComplete", false);

			difficulty = data.currentPlayer.GetData("LITELOCKDifficulty", data.GetGameAttributeValue("LiteLockDifficulty").ToInt());

			resetStates();
			for (int i=0; i<8; i++)
			{
				states[i] = (bool)data.currentPlayer.GetData("LITELOCKStates" + i.ToString(), false);
			}
		}
		
		public override void SavePlayerData()
		{
			data.currentPlayer.SaveData("LITELOCKComplete", complete);
			data.currentPlayer.SaveData("LITELOCKDifficulty", difficulty);
			for (int i=0; i<8; i++)
				data.currentPlayer.SaveData("LITELOCKStates" + i.ToString(), states[i]);
		}

		private void resetStates()
		{
			states = new List<bool>();
			for (int i=0; i<8; i++)
				states.Add(false);
		}

		public void start(int Difficulty)
		{
			complete = false;
			difficulty = Difficulty;
			resetStates ();
			refreshInserts();
		}

		public void stop()
		{
			complete = true;
			refreshInserts();
		}

		private void refreshInserts()
		{
			for (int i=0; i<states.Count; i++)
			{
				if (!complete)
				{
					if (states[i])
					{
						LEDScriptsDict[LEDNames[i]] = LEDHelpers.OnLED(p3, LEDScriptsDict[LEDNames[i]],Multimorphic.P3.Colors.Color.green);
						GUIInsertScriptsDict[InsertNames[i]] = GUIInsertHelpers.OnInsert(p3, GUIInsertScriptsDict[InsertNames[i]],  Multimorphic.P3.Colors.Color.green);
					}
					else
					{
						LEDScriptsDict[LEDNames[i]] = LEDHelpers.BlinkLED(p3, LEDScriptsDict[LEDNames[i]],Multimorphic.P3.Colors.Color.green);
						GUIInsertScriptsDict[InsertNames[i]] = GUIInsertHelpers.BlinkInsert(p3, GUIInsertScriptsDict[InsertNames[i]],  Multimorphic.P3.Colors.Color.green);
					}

				}
				else
				{
					GUIInsertHelpers.AddorRemoveScript(GUIInsertScriptsDict[InsertNames[i]], false);
					LEDScriptsDict[LEDNames[i]] = LEDHelpers.OnLED(p3, LEDScriptsDict[LEDNames[i]],Multimorphic.P3.Colors.Color.lightgreen);
				}
			}
		}

		public bool SideTargetHitEventHandler(string eventName, object eventData)
		{
			int index = (int)eventData;

			LocationScore info = new LocationScore();
			info.location = "sideTarget" + index.ToString();

			bool right = index >= 4;

			if (!complete && !states[index])
			{
				info.score = ScoreManager.Score (Scores.LIGHT_LOCK_PROGRESS);
				ProcessTarget(index);
				PostModeEventToGUI("Evt_LockLetterHit", right);
			}
			else 
			{
				info.score = ScoreManager.Score (Scores.LIGHT_LOCK_NO_PROGRESS);
				PostModeEventToGUI("Evt_LockLetterMiss", right);
				PulseTarget(index);
			}
			PostModeEventToGUI("Evt_SideTargetScore", info);

			return SWITCH_STOP;
		}

		private void ProcessTarget(int index)
		{
			int minIndex;
			int maxIndex;

			if (difficulty == DIFFICULTY_EASY)
			{
				if (index < 4)
				{
					minIndex = 0;
					maxIndex = 4;
				}
				else
				{
					minIndex = 4;
					maxIndex = 8;
				}
			}
			else if (difficulty == DIFFICULTY_MEDIUM)
			{
				if (index < 2)
				{
					minIndex = 0;
					maxIndex = 2;
				}
				else if (index < 4 && index >= 2)
				{
					minIndex = 2;
					maxIndex = 4;
				}
				else if (index < 6 && index >= 4)
				{
					minIndex = 4;
					maxIndex = 6;
				}
				else
				{
					minIndex = 6;
					maxIndex = 8;
				}
			}
			else
			{
				minIndex = index;
				maxIndex = index+1;
			}

			if (!states[index])
			{
				for (int i=minIndex; i<maxIndex; i++)
				{
					LEDHelpers.LEDFlashPop(p3, LEDNames[i],Multimorphic.P3.Colors.Color.blue, 2, Priority+1);
					states[i] = true;
				}
			}

			refreshInserts();

			CheckForComplete();

		}

		private void PulseTarget(int index)
		{
			LEDScriptsDict[LEDNames[index]] = LEDHelpers.PulseLED(p3, LEDScriptsDict[LEDNames[index]],Multimorphic.P3.Colors.Color.red,  Multimorphic.P3.Colors.Color.lightgreen);
		}

		private void CheckForComplete()
		{
			for (int i=0; i<states.Count; i++)
			{
				if (!states[i])
				{
					return;
				}
			}
			ProcessComplete();
		}

		private void ProcessComplete()
		{
			ScoreManager.Score (2500);
			complete = true;
			PostModeEventToModes ("Evt_LITELOCKComplete", 0);

			for (int i=0; i<InsertNames.Count; i++)
			{
				LEDScriptsDict[LEDNames[i]] = LEDHelpers.BlinkLED(p3, LEDScriptsDict[LEDNames[i]],Multimorphic.P3.Colors.Color.green, 0.05, 0.1, 0.05, 0.1);
				GUIInsertScriptsDict[InsertNames[i]] = GUIInsertHelpers.BlinkInsert(p3, GUIInsertScriptsDict[InsertNames[i]],  Multimorphic.P3.Colors.Color.green, 0.05, 0.1, 0.05, 0.1);
			}
			this.delay("completion delay", Multimorphic.NetProc.EventType.None, 0.5, new Multimorphic.P3.VoidDelegateNoArgs (refreshInserts));
		}

	}
}

