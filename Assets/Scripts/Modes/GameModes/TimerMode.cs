using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using System.Collections.Generic;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.Modes
{
    public class TimerMode : TestGameGameMode
	{
		private int timer;
		private ushort [] color;
		private Multimorphic.P3.VoidDelegateNoArgs completeCallback;
		private string name = "unnamed";

		public TimerMode (P3Controller controller, int priority, Multimorphic.P3.VoidDelegateNoArgs CompleteCallback, string timerName)
			: base(controller, priority)
		{
			timer = 60;
			completeCallback = CompleteCallback;
			name = timerName;
		}

		public TimerMode (P3Controller controller, int priority, Multimorphic.P3.VoidDelegateNoArgs CompleteCallback, string timerName, bool UseMiniLCD)
			: base(controller, priority)
		{
			timer = 60;
			completeCallback = CompleteCallback;
			name = timerName;
		}

		public override void mode_started ()
		{
			base.mode_started ();
		}

		public override void mode_stopped ()
		{
			base.mode_stopped ();
		}

		public void Start()
		{
			Start (timer);
		}

		public void Start(int seconds)
		{
			// First call Stop to clear out stop any active delays.
			Stop ();
			timer = seconds;
			SetTickDelay();
			PostModeEventToGUI("Evt_" + name + "TimerStart", seconds);
		}

		private void SetTickDelay()
		{
			this.delay("Timer delay", Multimorphic.NetProc.EventType.None, 1, new Multimorphic.P3.VoidDelegateNoArgs (Tick));
		}

		private void Tick()
		{
			timer--;
			PostModeEventToGUI("Evt_" + name + "TimerTick", timer);
			PostModeEventToModes("Evt_" + name + "TimerTick", timer);
			Multimorphic.P3App.Logging.Logger.Log ("Timer: " + name + " : " + timer.ToString());
			Multimorphic.P3App.Logging.Logger.Log ("Posting Evt_ " + name + "TimerTick");
			if (timer <= 0)
			{
				completeCallback();
			}
			else
				SetTickDelay();
		}

		public void Pause(int pauseTime)
		{
			this.delay("Timer delay", Multimorphic.NetProc.EventType.None, pauseTime, new Multimorphic.P3.VoidDelegateNoArgs (Tick));
		}

		public void Stop()
		{
			cancel_delayed("Timer delay");
		}

		private string convertSecondsToTimeString(int seconds)
		{
			string timeStr = string.Format ("{0:0}:{1:00}", seconds/60, seconds%60);
			return timeStr;
		}

	}
}

