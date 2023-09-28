using Multimorphic.NetProcMachine.Machine;
using Multimorphic.NetProcMachine.LEDs;
using Multimorphic.P3;
using System.Collections.Generic;
using System.Linq;
using Multimorphic.P3App.Modes;
using PinballClub.TestGame.Modes;
using Multimorphic.P3App.Modes.Data;

namespace PinballClub.TestGame.Modes
{
	public struct LanesCompletedStatus
	{
		public int numCompleted;
		public string nextGoal;
		public string award;
	}

	/// <summary>
	/// A mode which handles the rollovers in the lower lanes of the playfield, including sequenced advancement via flipper buttons. 
	/// </summary>
    public class LanesMode : TestGameGameMode
	{

		private int numCompletions;
		private List<bool> laneStates;
		private List<bool> laneActivating;
		private List<int> laneActivateCtr;
		private bool enabled;
		private bool resetting;

		private const int LEFT_OUTLANE_INDEX = 0;
		private const int LEFT_INLANE_INDEX = 1;
		private const int RIGHT_INLANE_INDEX = 2;
		private const int RIGHT_OUTLANE_INDEX = 3;

		private const int NUM_FLASHES_FOR_LANE_COMPLETION = 10;
		private const int NUM_FLASHES_FOR_ALL_LANE_COMPLETION = 20;
		private const double COMPLETION_FLASH_DELAY = 0.05;
		private const double RESET_DELAY = 0.5;

		private List<int> awardLevels = new List<int>() {1, 2, 4, 6, 10, 20, 40, 100, 1000};
		private List<string> awards = new List<string>() {"BonusX", "BonusX", "BonusX", "BonusX", "Respawn", "Respawn", "Respawn", "Respawn", "Respawn"};

		public LanesMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			AddModeEventHandler("Evt_RefreshLanes", RefreshLanesEventHandler, Priority);
			AddModeEventHandler("Evt_OutlaneRightLower", RightOutlaneEventHandler, Priority);
			AddModeEventHandler("Evt_InlaneRightLower", RightInlaneEventHandler, Priority);
			AddModeEventHandler("Evt_OutlaneLeftLower", LeftOutlaneEventHandler, Priority);
			AddModeEventHandler("Evt_InlaneLeftLower", LeftInlaneEventHandler, Priority);
			AddModeEventHandler("Evt_EnableLanes", EnableLanesEventHandler, Priority);
		}

 		public override void mode_started ()
		{
			base.mode_started ();
			enabled = true;
			resetting = false;
			ResetLanesActivated();
		}

		public override void mode_stopped ()
		{
			base.mode_stopped();
		}

		public override void LoadPlayerData()
		{
		    numCompletions = data.currentPlayer.GetData("NumLaneCompletions", 0);

			laneStates = new List<bool>();
			for (int i=0; i<4; i++)
				laneStates.Add (false);

			for (int i=0; i<4; i++)
			{
				laneStates[i] = data.currentPlayer.GetData("LaneStates" + i.ToString(), false);
			}
		}

		public override void SavePlayerData()
		{
			for (int i=0; i<4; i++)
				data.currentPlayer.SaveData("LaneStates" + i.ToString(), laneStates[i]);
			data.currentPlayer.SaveData("NumLaneCompletions", numCompletions);
		}

		private void ResetLanes()
		{
			laneStates = new List<bool>();
			for (int i=0; i<4; i++)
				laneStates.Add (false);
			UpdateLanes();
			resetting = true;
			this.delay("ResetLanesDelay", Multimorphic.NetProc.EventType.None, RESET_DELAY, new Multimorphic.P3.VoidDelegateNoArgs (ResetLanesFinished));
		}

		private void ResetLanesFinished()
		{
			resetting = false;
		}

		private void ResetLanesActivated()
		{
			laneActivating = new List<bool>();
			laneActivateCtr = new List<int>();
			for (int i=0; i<4; i++)
			{
				laneActivating.Add (false);
				laneActivateCtr.Add (0);
			}
		}

		public bool RefreshLanesEventHandler(string evtName, object evtData)
		{
			RefreshInserts();
			return SWITCH_CONTINUE;
		}

		public bool EnableLanesEventHandler(string evtName, object evtData)
		{
			enabled = (bool)evtData;
			RefreshInserts();
			return SWITCH_CONTINUE;
		}

		public bool RightOutlaneEventHandler(string evtName, object evtData)
		{
			LaneHit(RIGHT_OUTLANE_INDEX);
			return SWITCH_CONTINUE;
		}

		public bool LeftOutlaneEventHandler(string evtName, object evtData)
		{
			LaneHit(LEFT_OUTLANE_INDEX);
			return SWITCH_CONTINUE;
		}

		public bool RightInlaneEventHandler(string evtName, object evtData)
		{
			LaneHit(RIGHT_INLANE_INDEX);
			return SWITCH_CONTINUE;
		}

		public bool LeftInlaneEventHandler(string evtName, object evtData)
		{
			LaneHit(LEFT_INLANE_INDEX);
			return SWITCH_CONTINUE;
		}

		private void LaneHit(int lane)
		{
			if (enabled && !resetting)
			{
				if (laneStates[lane])
					PostModeEventToGUI("Evt_LaneAlreadyActivated", lane);
				else
				{
					ActivateLane (lane);
					CheckForCompletion();
				}
			}
		}

		private void ActivateLane(int lane)
		{
			PostModeEventToGUI("Evt_LaneActivated", lane);
			laneStates[lane] = true;
			laneActivating[lane] = true;
			laneActivateCtr[lane] = NUM_FLASHES_FOR_LANE_COMPLETION;
			this.delay("LaneCompletedDelay", Multimorphic.NetProc.EventType.None, COMPLETION_FLASH_DELAY, new Multimorphic.P3.VoidDelegateNoArgs (Flash));
		}

		private void Flash()
		{
			for (int i=0; i<4; i++)
			{
				if (laneActivating[i])
				{
					laneActivateCtr[i]--;
					laneActivating[i] = laneActivateCtr[i] > 0;
				}
			}

			bool repeat = false;

			for (int i=0; i<4; i++)
			{
				if (laneActivating[i])
				{
					repeat = true;
					break;
				}
			}

			if (repeat)
				this.delay("LaneCompletedDelay", Multimorphic.NetProc.EventType.None, COMPLETION_FLASH_DELAY, new Multimorphic.P3.VoidDelegateNoArgs (Flash));

			RefreshInserts();
		}

		private void CheckForCompletion()
		{
			bool completed = true;
			for (int i=0; i<4; i++)
			{
				if (!laneStates[i])
				{
					completed = false;
					break;
				}
			}

			if (completed)
				Award ();
			else
				UpdateLanes();
		}

		private void Award()
		{
			//for (int i=0; i<4; i++)
			//{
			//	laneActivating[i] = true;
			//	laneActivateCtr[i] = NUM_FLASHES_FOR_ALL_LANE_COMPLETION;
			//}
			//this.delay("LaneCompletedDelay", Multimorphic.NetProc.EventType.None, COMPLETION_FLASH_DELAY, new Multimorphic.P3.VoidDelegateNoArgs (Flash));
			numCompletions++;

			LanesCompletedStatus status = new LanesCompletedStatus();
			status.numCompleted = numCompletions;
			if (awardLevels.Contains(numCompletions))
			{
				status.award = awards[awardLevels.IndexOf(numCompletions)];
				if (status.award == "BonusX")
					ScoreManager.IncBonusX();
				else if (status.award == "Respawn")
					PostModeEventToModes("Evt_RespawnAdd", 1);
			}
			else
			{
				status.award = Scores.LANES_COMPLETED.ToString();
				ScoreManager.Score (Scores.LANES_COMPLETED);
			}

			if (numCompletions >= awardLevels[awards.Count-1])
				status.nextGoal = "";
			else 
			{
				int i;
				for (i=0; i<awardLevels.Count; i++)
				{
					if (awardLevels[i] > numCompletions)
						break;
				}
				status.nextGoal = (awardLevels[i] - numCompletions).ToString() + " more for " + awards[i];
			}

			PostModeEventToGUI("Evt_LanesCompleted", status);
			ResetLanes();
			PostModeEventToModes ("Evt_UpdateHUD", 0);
		}

		private void UpdateLanes()
		{
			//PostModeEventToGUI("Evt_UpdateLanes", laneStates);
			RefreshInserts ();
		}

		public bool sw_buttonRight2_active( Switch sw )
		{
			Rotate(1);
			return SWITCH_CONTINUE;
		}

		public bool sw_buttonLeft2_active( Switch sw )
		{
			Rotate(-1);
			return SWITCH_CONTINUE;
		}

		private void Rotate(int amount)
		{
			ResetLanesActivated();
			List<bool> newLaneStates = new List<bool>();
			if (amount == 1)
			{
				PostModeEventToGUI("Evt_AnimateLanesRight", laneStates);
				for (int i=3; i<7; i++)
				{
					newLaneStates.Add (laneStates[i%laneStates.Count]);
				}
			}
			else
			{
				PostModeEventToGUI("Evt_AnimateLanesLeft", laneStates);
				for (int i=1; i<5; i++)
				{
					newLaneStates.Add (laneStates[i%laneStates.Count]);
				}
			}

			laneStates = newLaneStates;
			RefreshInserts();
		}

		private void RefreshInserts()
		{
			UpdateInsert(LEFT_INLANE_INDEX, "LeftInlaneIcon", "LeftInlaneOffIcon");
			UpdateInsert(LEFT_OUTLANE_INDEX, "LeftOutlaneIcon", "LeftOutlaneOffIcon");
			UpdateInsert(RIGHT_INLANE_INDEX, "RightInlaneIcon", "RightInlaneOffIcon");
			UpdateInsert(RIGHT_OUTLANE_INDEX, "RightOutlaneIcon", "RightOutlaneOffIcon");
		}

		private void UpdateInsert(int index, string onInsertName, string offInsertName)
		{
			bool activating = false;
			if (laneActivating != null)
				activating = (laneActivateCtr[index] & 0x1) == 0x1;
			bool on = enabled && ((laneStates[index] && !laneActivating[index]) || activating);
			bool off = !on && enabled;
			PostModeEventToGUI("Evt_State" + onInsertName, on);
			PostModeEventToGUI("Evt_State" + offInsertName, off);
		}

		public List<BonusItem> getBonusItems()
		{
			List<BonusItem> items = new List<BonusItem>();
			if (numCompletions > 0)
			{
				BonusItem item = new BonusItem(numCompletions.ToString("n0") + " Lane Completions", numCompletions*Scores.BONUS_LANE_COMPLETIONS);
				items.Add (item);
			}
			return items;
		}

	}
}

