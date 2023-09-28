using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using System.Collections.Generic;
using Multimorphic.P3App.Modes;

namespace PinballClub.TestGame.Modes
{

    public class TargetSaveMode : TestGameGameMode
	{

		const int SAVE_TIME = 2;
		private bool active;

		public TargetSaveMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			AddHandlers();
			active = false;
		}

		private void AddHandlers()
		{
			foreach (Switch sw in p3.Switches.Values)
			{
				if (sw.Name.Contains ("alienTarget") ||
				    sw.Name.Contains ("leftTarget") ||
				    sw.Name.Contains ("rightTarget"))
				{
					add_switch_handler(sw.Name, "active", 0, TargetEventHandler);
				}
			}
		}

 		public override void mode_started ()
		{
			base.mode_started ();
		}

		public override void mode_stopped ()
		{
			base.mode_stopped();
		}

		public void Enable(bool on)
		{
			active = on;
		}

		private bool TargetEventHandler(Switch sw)
		{
			if (active)
				PostModeEventToModes ("Evt_BallSaveStart", SAVE_TIME);
			return SWITCH_CONTINUE;
		}

	}
}

