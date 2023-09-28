using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Modes.Data;
using PinballClub.TestGame.Modes.Data;
using System;
using System.Collections.Generic;

namespace PinballClub.TestGame.Modes.Data
{
    public class TestGameGameAttributeManagerMode : GameAttributeManagerMode
	{
		List<HighScoreCategory> highScoreCats;
		public TestGameGameAttributeManagerMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
		}

		protected override void InstantiateAndAddModes()
		{
			base.InstantiateAndAddModes();

			settingsMode = new TestGameSettingsMode (p3, Priority);
			// Use PRIORITY_HIGH_SCORES mode for the high score logic so the score boards work properly in Attract Mode.
			highScoresMode = new TestGameHighScoresMode (p3, Priorities.PRIORITY_HIGH_SCORES);
			statisticsMode = new TestGameStatisticsMode (p3, Priority);

			eventProfileManagerMode = new TestGameEventProfileManagerMode(p3, Priority+1);
		}

		public override void mode_started ()
		{
			base.mode_started ();
			highScoreCats = TestGameHighScoreCategories.GetCategories();
			SendHSCatsToHSModes();
		}


		private void SendHSCatsToHSModes()
		{
			foreach (HighScoreCategory cat in highScoreCats)
			{
				PostModeEventToModes ("Evt_AddHighScoreCategory", cat);
			}
		}

		protected override bool NewEventProfileAddedEventHandler (string evtName, object evtData)
		{
			SendHSCatsToHSModes();
			return base.NewEventProfileAddedEventHandler (evtName, evtData);
		}

		protected override void CreateStockProfiles()
		{
			base.CreateStockProfiles();
            /*
			profileManagerMode.AddStockProfile ("Default");
			profileManagerMode.AddStockProfile ("Easy");
			profileManagerMode.AddStockProfile ("Medium");
			profileManagerMode.AddStockProfile ("Hard");
			profileManagerMode.AddStockProfile ("Custom");
            */

            /*
			settingsMode.UpdateStockProfileAttributes ("Default");
			settingsMode.UpdateStockProfileAttributes ("Easy");
			settingsMode.UpdateStockProfileAttributes ("Medium");
			settingsMode.UpdateStockProfileAttributes ("Hard");
			settingsMode.UpdateStockProfileAttributes ("Custom");
            */
		}

	} 

}

