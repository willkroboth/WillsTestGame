using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Data;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Modes.Data;
using System;
using System.Collections.Generic;

namespace PinballClub.TestGame.Modes.Data
{
	public class TestGameStatisticsMode : StatisticsMode
	{
		public TestGameStatisticsMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			Setup ();
		}

		public TestGameStatisticsMode (P3Controller controller, int priority, string eventName, string eventDir)
			: base(controller, priority, eventName, eventDir)
		{
			Setup();
		}

		private void Setup()
		{
			DataVersion = 5;
			DBVersion = 1;

			// Add event handlers here for the things you want to keep statistics of.
			// e.g. AddModeEventHandler("Evt_RightPopHit", PopEventHandler, Priority);
		}

		// Called in base.mode_started()
		protected override void CreateDefaultAttrs()
		{
			base.CreateDefaultAttrs();

			// Add attributes here so that your statistics go into the database.
			// e.g. InitAttr(5, "RightPops", "Right pop bumper activations", "Right pop bumper activations", "Service Menu/Statistics/" + Event + "/Mechs", (GameAttributeOptions.ReadOnly), 0, 0, 0, 1, 0);
		}

		private bool PopEventHandler(string evtName, object evtData)
		{
			// Increment your statistic attributes as appropriate
			// if (evtName.Contains("Right")) 
			//	IncAttr("RightPops");

			return SWITCH_CONTINUE;
		}

	} 

}

