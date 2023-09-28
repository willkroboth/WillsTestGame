
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.NetProcMachine.LEDs;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using System.Collections.Generic;

namespace PinballClub.TestGame.Modes
{

    public class TargetScoresMode : P3Mode
	{
		private List<List<Switch>> targets;
		private List<List<LED>> leds;
		private List<int> scores;
		private List<ushort []> colors;
		protected List<List<LEDScript>> ledScripts;
		private bool unhitBlink;

		public TargetScoresMode (P3Controller controller, int priority, List<Switch> Targets, List<LED> LEDs, List<int> Scores, List<ushort []> Colors)
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

			SetupMode(fullTargetList, fullLEDList, Scores, Colors);
		}

		public TargetScoresMode (P3Controller controller, int priority, List<List<Switch>> Targets, List<List<LED>> LEDs, List<int> Scores, List<ushort []> Colors)
			: base(controller, priority)
		{
			SetupMode (Targets, LEDs, Scores, Colors);
		}

		private void SetupMode(List<List<Switch>> Targets, List<List<LED>> LEDs, List<int> Scores, List<ushort []> Colors)
		{
			targets = Targets;
			leds = LEDs;
			colors = Colors;
			scores = Scores;

			// Assign the switch handler to each target
			for (int i=0; i<targets.Count; i++)
			{
				for (int j=0; j<targets[i].Count; j++)
				{
					add_switch_handler(targets[i][j].Name, "active", 0, TargetEventHandler);
				}
			}


			// Create and initialize LED scripts for each target.
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

			for (int i=0; i<targets.Count; i++)
			{
				for (int j=0; j<targets[i].Count; j++)
				{
					ledScripts[i][j] = OnLED(ledScripts[i][j], colors[i]);
				}
			}

		}

		public override void mode_stopped ()
		{
			RemoveLEDScripts();
			base.mode_stopped ();
		}

		private LEDScript PulseLED(LEDScript script, ushort [] pulseColor, ushort [] endColor)
		{
			p3.LEDController.RemoveScript(script);
			script.Clear ();
			script.AddCommand(pulseColor, 0, 0.10);
			script.AddCommand(endColor, 0.2, 0.5);
			p3.LEDController.AddScript(script, 0.5);
			return script;
		}

		private LEDScript OnLED(LEDScript script, ushort [] color)
		{
			p3.LEDController.RemoveScript(script);
			script.Clear ();
			script.AddCommand(color, 0.2, 0.5);
			p3.LEDController.AddScript(script, 0.5);
			return script;
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

		private bool TargetEventHandler(Switch sw)
		{
			int targetIndex = GetTargetListIndex(sw);
			PulseTarget(targetIndex);
			PostModeEventToModes("Evt_ScoreTargetHit", scores[targetIndex]);

			return SWITCH_STOP;
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

		private void PulseTarget(int targetIndex)
		{
			for (int j=0; j<leds[targetIndex].Count; j++)
			{
				ledScripts[targetIndex][j] = PulseLED(ledScripts[targetIndex][j],  Multimorphic.P3.Colors.Color.white, colors[targetIndex]);
			}
		}

	}

}
