using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Modes.Data;
using Multimorphic.P3App.Modes;

namespace PinballClub.TestGame.Modes
{
	public class RespawnMode : TestGameGameMode
	{
		private int count;
		private bool enabled;
		private int launchIndex;

		public RespawnMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			AddModeEventHandler("Evt_RespawnAdd", AddRespawnHandler, Priority);
			AddModeEventHandler("Evt_RespawnEnable", EnableRespawnHandler, Priority);
		}

		public override void mode_started ()
		{
			base.mode_started ();
			count = 0;
			enabled = false;
		}

		public bool AddRespawnHandler( string evtName, object evtData)
		{
			count += (int)evtData;
			UpdateDisplay();
			return SWITCH_CONTINUE;
		}

		public bool EnableRespawnHandler( string evtName, object evtData)
		{
			enabled = (bool)evtData;
			UpdateDisplay();
			return SWITCH_CONTINUE;
		}

		private void UpdateDisplay()
		{
			if (enabled)
				PostModeEventToGUI("Evt_RespawnStatus", count);
			else
				PostModeEventToGUI("Evt_RespawnStatus", 0);
		}

		public bool sw_drain_active(Switch sw)
		{
			if (count > 0 && enabled)
			{
				PostModeEventToModes("Evt_BallSaved", 0); 
				count--;
				UpdateDisplay();
				return SWITCH_STOP;
			}
			else 
				return SWITCH_CONTINUE;
		}

	} 
}
