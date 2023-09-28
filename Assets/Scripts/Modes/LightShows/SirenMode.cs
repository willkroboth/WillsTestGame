
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.NetProcMachine.LEDs;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using System.Collections.Generic;

namespace PinballClub.TestGame.Modes
{

    public class SirenMode : TestGameGameMode
	{
		List<List<string>> nameGroups;
		List<LEDScript> LocalLEDScripts;

		public SirenMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			nameGroups = new List<List<string>>();
			List<string> group;

			for (int i=0; i<4; i++)
			{
				group = new List<string>();
				group.Add("flasherSideModuleLeft" + i.ToString());
				nameGroups.Add(group);
			}

			group = new List<string>();
			group.Add("flasherLeftRamp");
			group.Add("flasherBBLeft");
			group.Add("leftTarget");
			nameGroups.Add(group);

			group = new List<string>();
			group.Add("flasherBBCenter");
			nameGroups.Add(group);

			group = new List<string>();
			group.Add("rightTargetHigh");
			group.Add("rightTargetLow");
			group.Add("flasherBBRight");
			nameGroups.Add(group);

			for (int i=3; i>=0; i--)
			{
				group = new List<string>();
				group.Add("flasherSideModuleRight" + i.ToString());
				nameGroups.Add(group);
			}
	
		}

 		public override void mode_started ()
		{
			double groupIndex=0;
			double numGroups = (double)nameGroups.Count;
			LocalLEDScripts = new List<LEDScript>();;
			double sirenCycleTime = 0.75;
			double ledFadeTime = sirenCycleTime/12;
			double ledOnTime = sirenCycleTime/4;
			foreach (List<string> group in nameGroups)
			{
				foreach (string name in group)
				{
					LEDScript script = new LEDScript( p3.LEDs[name], this.Priority);
					script.AddCommand (Multimorphic.P3.Colors.Color.red, ledFadeTime, ledOnTime);
					script.AddCommand (Multimorphic.P3.Colors.Color.black, ledFadeTime, ledOnTime);
					script.AddCommand (Multimorphic.P3.Colors.Color.blue, ledFadeTime, ledOnTime);
					script.AddCommand (Multimorphic.P3.Colors.Color.black, ledFadeTime, ledOnTime);
					LocalLEDScripts.Add (script);
					p3.LEDController.AddScript(script, -1, (groupIndex/numGroups)*sirenCycleTime);
				}
				groupIndex++;
			}
		}

		public override void mode_stopped ()
		{
			for (int i=0; i<LocalLEDScripts.Count; i++)
			{
				p3.LEDController.RemoveScript(LocalLEDScripts[i]);
			}
			base.mode_stopped();
		}

	}
}

