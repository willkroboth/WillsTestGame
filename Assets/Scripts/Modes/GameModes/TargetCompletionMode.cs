using Multimorphic.NetProcMachine.Machine;
using Multimorphic.NetProcMachine.LEDs;
using Multimorphic.P3;
using System.Collections.Generic;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.Modes
{

    public class TargetCompletionMode : P3Mode
	{
		private int launchIndex;
		private int count;
		private List<List<Switch>> targets;
		private List<List<LED>> leds;
		private List<bool> targetStates;
		protected List<List<LEDScript>> ledScripts;
		private string name;
		private ushort [] unhitColor;
		private ushort [] hitColor;
		private ushort [] finishedColor;
		private ushort [] pulseColor;
		private bool unhitBlink;
		private bool finished;

		public TargetCompletionMode (P3Controller controller, int priority, string Name, List<Switch> Targets, List<LED> LEDs)
			: base(controller, priority)
		{

			// Turn single list of targets into double list
			List<List<Switch>> fullTargetList = new List<List<Switch>>();

			foreach (Switch target in Targets)
			{
				List<Switch> singleTargetList = new List<Switch>();
				singleTargetList.Add (target);
				fullTargetList.Add (singleTargetList);
			}

			// Turn single list of LEDs into double list
			List<List<LED>> fullLEDList = new List<List<LED>>();
			
			foreach (LED led in LEDs)
			{
				List<LED> singleLEDList = new List<LED>();
				singleLEDList.Add (led);
				fullLEDList.Add (singleLEDList);
			}

			SetupMode(Name, fullTargetList, fullLEDList);
		}

		public TargetCompletionMode (P3Controller controller, int priority, string Name, List<List<Switch>> Targets, List<List<LED>> LEDs)
			: base(controller, priority)
		{
			SetupMode (Name, Targets, LEDs);
		}

		private void SetupMode(string Name, List<List<Switch>> Targets, List<List<LED>> LEDs)
		{
			targets = Targets;
			leds = LEDs;
			name = Name;
			targetStates = new List<bool>();

			for (int i=0; i<targets.Count; i++)
			{
				for (int j=0; j<targets[i].Count; j++)
				{
					add_switch_handler(targets[i][j].Name, "active", 0, TargetEventHandler);
				}

				targetStates.Add (false);
			}

			ledScripts = new List<List<LEDScript>>();

			for (int i=0; i<leds.Count; i++)
			{
				List<LEDScript> newScriptList = new List<LEDScript>();

				for (int j=0; j<leds[i].Count; j++)
				{
					LEDScript script = new LEDScript( p3.LEDs[leds[i][j].Name], this.Priority);
					script.autoRemove = false;
					newScriptList.Add (script);
				}

				ledScripts.Add (newScriptList);
			}

		}

 		public override void mode_started ()
		{
			base.mode_started ();

			//for (int i=0; i<targets.Count; i++)
			//{
			//	for (int j=0; j<targets[i].Count; j++)
			//	{
			//		Multimorphic.P3App.Logging.Logger.Log("Target " + i.ToString() + "/" + j.ToString() + ": Switch - " + targets[i][j].Name + ", LED - " + leds[i][j].Name);
			//	}
			//}

			Reset (Multimorphic.P3.Colors.Color.blue,  Multimorphic.P3.Colors.Color.blue,  Multimorphic.P3.Colors.Color.blue,  Multimorphic.P3.Colors.Color.red, true);
		}

		public override void mode_stopped ()
		{
			RemoveLEDScripts();
			base.mode_stopped ();
		}

		public void Reset(ushort [] UnhitColor, ushort [] HitColor, ushort [] FinishedColor, ushort [] PulseColor, bool UnhitBlink)
		{
			unhitColor = UnhitColor;
			hitColor = HitColor;
			finishedColor = FinishedColor;
			pulseColor = PulseColor;
			unhitBlink = UnhitBlink;
			finished = false;

			SetAllTargetStates(false);
			InitLEDs();
		}

		private void InitLEDs()
		{
			for (int i=0; i<leds.Count; i++)
			{ 
				for (int j=0; j<leds[i].Count; j++)
				{
					if (unhitBlink)
						ledScripts[i][j] = LEDHelpers.BlinkLED(p3, ledScripts[i][j], unhitColor);
					else
						ledScripts[i][j] = LEDHelpers.OnLED(p3, ledScripts[i][j], unhitColor);
				}
			}
		}

		private void RemoveLEDScripts()
		{
			for (int i=0; i<leds.Count; i++)
			{
				for (int j=0; j<leds[i].Count; j++)
				{
					p3.LEDController.RemoveScript(ledScripts[i][j]);
				}
			}
		}

		private bool IsComplete()
		{
			for (int i=0; i<targetStates.Count; i++)
			{
				if (!targetStates[i])
				{
					return false;

				}
			}
			return true;
		}

		private bool TargetEventHandler(Switch sw)
		{
			int targetIndex = GetTargetListIndex(sw);
			if (!targetStates[targetIndex])
			{
				CompleteTarget(targetIndex);
				if (IsComplete ())
					CompleteMode();
				else
					PostModeEventToModes ("Evt_TargetsHit", name);
			}
			else 
			{
				PulseTarget(targetIndex, IsComplete ());
				PostModeEventToModes ("Evt_TargetsMiss", name);
			}

			return SWITCH_CONTINUE;
		}

		private int GetTargetListIndex(Switch sw)
		{
			for (int i=0; i<targets.Count; i++)
			{
				if (targets[i].Contains (sw))
					return i;
			}

			return 0;
		}


		private void CompleteTarget(int targetIndex)
		{
			CompleteTarget(targetIndex, false);
		}


		private void CompleteTarget(int targetIndex, bool stealth)
		{
			targetStates[targetIndex] = true;
			for (int j=0; j<leds[targetIndex].Count; j++)
			{
				if (stealth)
					ledScripts[targetIndex][j] = LEDHelpers.OnLED(p3, ledScripts[targetIndex][j], hitColor);
				else
				{
					ledScripts[targetIndex][j] = LEDHelpers.PulseLED(p3, ledScripts[targetIndex][j], pulseColor, hitColor);
				}
			}
		}

		private void PulseTarget(int targetIndex, bool finished)
		{
			for (int j=0; j<leds[targetIndex].Count; j++)
			{
				if (finished)
					ledScripts[targetIndex][j] = LEDHelpers.PulseLED(p3, ledScripts[targetIndex][j], pulseColor, finishedColor);
				else
					ledScripts[targetIndex][j] = LEDHelpers.PulseLED(p3, ledScripts[targetIndex][j], pulseColor, hitColor);
			}
		}

		private void SetAllTargetStates(bool state)
		{
			for (int i=0; i<targetStates.Count; i++)
				targetStates[i] = state;
		}

		private void OnAllLEDs(ushort [] color)
		{
			for (int i=0; i<leds.Count; i++)
			{
				for (int j=0; j<leds[i].Count; j++)
				{
					ledScripts[i][j] = LEDHelpers.OnLED(p3, ledScripts[i][j], color);
				}
			}
		}

		public List<bool> GetTargetStates()
		{
			return targetStates;
		}

		public void ForceTargetStates(List<bool> TargetStates, bool stealth)
		{
			for (int i=0; i<targetStates.Count; i++)
			{
				if (TargetStates[i] && (targetStates.Count >= i+1))
					CompleteTarget(i, stealth);
			}
		}

		public void ForceComplete(bool stealth)
		{
			SetAllTargetStates(true);
			if (stealth)
				OnAllLEDs(finishedColor);
			else
				CompleteMode ();
		}

		public void RefreshLEDs()
		{
			for (int i=0; i<leds.Count; i++)
			{
				for (int j=0; j<leds[i].Count; j++)
				{
					if (finished) 
						ledScripts[i][j] = LEDHelpers.OnLED(p3, ledScripts[i][j], finishedColor);
					else if (targetStates[i]) 
						ledScripts[i][j] = LEDHelpers.OnLED(p3, ledScripts[i][j], hitColor);
					else if (unhitBlink) 
						ledScripts[i][j] = LEDHelpers.BlinkLED(p3, ledScripts[i][j], unhitColor);
					else
						ledScripts[i][j] = LEDHelpers.OnLED(p3, ledScripts[i][j], unhitColor);
				}
			}
		}

		private void CompleteMode()
		{
			finished = true;
			OnAllLEDs(finishedColor);
			PostModeEventToModes ("Evt_TargetsComplete", name);
		}


	}

}
