using System;
using System.Collections.Generic;
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Modes.Data;

namespace PinballClub.TestGame.Modes
{
	public struct HUDModeHighlightInstruction
	{
		public int index;
		public bool state;

		public HUDModeHighlightInstruction(int Index, bool State)
		{
			index = Index;
			state = State;
		}
	}

    public class HUDMode : P3Mode
	{
	    public readonly static int ICON_COUNT = 9;
		public readonly static int Swamp_MODE_INDEX = 0;
		public readonly static int Warehouse_MODE_INDEX = 1;
		public readonly static int ShootinRange_MODE_INDEX = 2;
		public readonly static int Hyperdrive_MODE_INDEX = 3;
		public readonly static int WeaponsLab_MODE_INDEX = 4;
		public readonly static int Bar_MODE_INDEX = 5;
		public readonly static int Agents_MODE_INDEX = 6;
		public readonly static int Aliens_MODE_INDEX = 7;
		public readonly static int Spotlights_MODE_INDEX = 8;
		
		private int numBalls;
		private bool profileChosen;
		private string profile;
		private bool teamGameEnabled = false;

		public HUDMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			AddModeEventHandler("Evt_ClearHUD", ClearHUDEventHandler, Priority);
			AddModeEventHandler("Evt_UpdateHUD", UpdateHUDEventHandler, Priority);
			AddModeEventHandler("Evt_HUDShowApronLCD", ShowApronLCDEventHandler, Priority);
			AddModeEventHandler("Evt_HUDShowInventory", ShowInventoryEventHandler, Priority);
			AddModeEventHandler("Evt_HUDShowEB", ShowEBEventHandler, Priority);
			AddModeEventHandler("Evt_HUDShowPlayerNumber", ShowPlayerNumberEventHandler, Priority);
			AddModeEventHandler("Evt_HUDShowBallNumber", ShowBallNumberEventHandler, Priority);
			AddModeEventHandler("Evt_HUDShowScoringX", ShowScoringXEventHandler, Priority);
			AddModeEventHandler("Evt_ShowCredits", ShowCreditsEventHandler, Priority);
			AddModeEventHandler("Evt_TeamGameEnabled", TeamGameEnabledEventHandler, Priority);
		}

 		public override void mode_started ()
		{
			base.mode_started ();
		}

		public override void mode_stopped ()
		{
			base.mode_stopped();
		}

		public bool TeamGameEnabledEventHandler(string evtName, object evtData)
		{
			teamGameEnabled = (bool)evtData;
			ShowTeamGame();
			return SWITCH_CONTINUE;
		}

		private void ShowTeamGame()
		{
			PostModeEventToGUI("Evt_StateTeamGame", teamGameEnabled);
		}

		private bool UpdateHUDEventHandler(string evtName, object evtData)
		{
			UpdateHUD();
			return SWITCH_STOP;
		}

		private void UpdateHUD()
		{
			ClearHUD();
			ShowApronLCD();
			PostModeEventToModes("Evt_ScoreRefresh", 0);
			PostModeEventToModes("Evt_UpdatingHUD", 0);
		}

		private bool ClearHUDEventHandler(string evtName, object evtData)
		{
			ClearHUD ();
			return SWITCH_STOP;
		}

		private void ClearHUD()
		{
			PostModeEventToGUI("Evt_HUDClear", 0);
		}

		private bool ShowApronLCDEventHandler(string evtName, object evtData)
		{
			ShowApronLCD();
			return SWITCH_STOP;
		}

		private bool ShowBallNumberEventHandler(string evtName, object evtData)
		{
#if AGENT_MB_MINI_GAME
			PostModeEventToGUI("Evt_TextBallNum", "Ball " + ((int)evtData).ToString() + " of 8");
#else
			ShowBallNumber();
#endif
			return SWITCH_STOP;
		}

		private void ShowBallNumber()
		{
#if AGENT_MB_MINI_GAME
#else
			int numBalls  = data.GetGameAttributeValue("NumBalls").ToInt();
			PostModeEventToGUI("Evt_TextBallNum", "Ball " + (data.ball).ToString() + " of " + numBalls.ToString());
#endif
		}

		private bool ShowPlayerNumberEventHandler(string evtName, object evtData)
		{
			ShowPlayerNumber();
			return SWITCH_STOP;
		}
		
		private void ShowPlayerNumber()
		{
			string text;

			if (data.currentPlayer.GetData(ProfileManagerMode.DATAKEY_PROFILE, ProfileManagerMode.NONE_PROFILE) != ProfileManagerMode.NONE_PROFILE)
				//text = (data.currentPlayerIndex+1).ToString() + ": " + data.currentPlayer.GetData("Name"];
				text = (data.currentPlayerIndex+1).ToString() + ": " + data.currentPlayer.GetData(ProfileManagerMode.DATAKEY_PROFILE).ToString();
			else
				text = "Player " + (data.currentPlayerIndex+1).ToString();

			PostModeEventToGUI("Evt_TextPlayerNum", text);
		}
		
		private void ShowApronLCD()
		{
			PostModeEventToGUI("Evt_StateApronLCD", true);
			PostModeEventToGUI("Evt_StateBallNum", true);
			PostModeEventToGUI("Evt_StatePlayerNum", true);
			ShowPlayerNumber ();
			ShowBallNumber ();
			ShowTeamGame ();
		}

		private bool ShowInventoryEventHandler(string evtName, object evtData)
		{
			ShowInventory();
			return SWITCH_STOP;
		}

		private void ShowInventory()
		{
			for (int i=0; i< ICON_COUNT; i++)
			{
				ShowInventory(i);
			}

			ShowEB();
			ShowScoringX();
			ShowTricon();
		}

		private void ShowInventory(int index)
		{
		}

		private void ShowTricon()
		{
		}

		private bool ShowEBEventHandler(string evtName, object evtData)
		{
			ShowEB ();
			return SWITCH_STOP;
		}

		private void ShowEB()
		{
			if (data.currentPlayer != null)
			{
				bool EB =  data.currentPlayer.extraBallCount > 0;
				PostModeEventToGUI("Evt_StateExtraBallOnIcon", EB);
				PostModeEventToGUI("Evt_StateExtraBallOffIcon", !EB);
			}
		}

		private bool ShowScoringXEventHandler(string evtName, object evtData)
		{
			ShowScoringX();
			return SWITCH_STOP;
		}

		private void ShowScoringX()
		{	
			float scoreX = ScoreManager.GetX();
			float bonusX = ScoreManager.GetBonusX();

			PostModeEventToGUI("Evt_StateBonusXOffIcon", bonusX <= 1);
			PostModeEventToGUI("Evt_StateBonusXOnIcon", bonusX > 1);
			if (bonusX > 1)
				PostModeEventToGUI("Evt_TextBonusXOnIcon", bonusX.ToString());

			PostModeEventToGUI("Evt_StateScoreXOffIcon", scoreX <= 1);
			PostModeEventToGUI("Evt_StateScoreXOnIcon", scoreX > 1);
			if (scoreX > 1)
				PostModeEventToGUI("Evt_TextScoreXOnIcon", scoreX.ToString());
		}

		private bool ShowCreditsEventHandler(string evtName, object evtData)
		{
			PostModeEventToGUI("Evt_StateApronLCD", true);
			PostModeEventToGUI("Evt_StateCredits", true);
			PostModeEventToGUI("Evt_TextCredits", (string)evtData);
			//PostModeEventToGUI("Evt_ShowCredits", (string)evtData + "\nActive Profile: " + data.GetActiveKey());

			return SWITCH_CONTINUE;
		}

		
	}
}

