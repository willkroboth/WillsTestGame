
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.NetProcMachine.LEDs;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using System.Collections.Generic;

namespace PinballClub.TestGame.Modes
{

    public class RotatingSmoothFadeMode : TestGameGameMode
	{
		List<List<string>> nameGroups;
		List<LEDScript> LocalLEDScripts;

		public RotatingSmoothFadeMode (P3Controller controller, int priority)
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
			group.Add("wall0");
			group.Add("scoop0");
			nameGroups.Add(group);

			group = new List<string>();
			group.Add("leftTarget");
			group.Add("flasherShip");
			group.Add("wall1");
			group.Add("scoop1");
			nameGroups.Add(group);

			group = new List<string>();
			group.Add("wall2");
			group.Add("scoop2");
			nameGroups.Add(group);

			group = new List<string>();
			group.Add("flasherBBCenter");
			group.Add("wall3");
			group.Add("scoop3");
			nameGroups.Add(group);

			group = new List<string>();
			group.Add("wall4");
			group.Add("scoop4");
			group.Add("topTarget0");
			group.Add("topTarget1");
			group.Add("rightTargetHigh");
			group.Add("rightTargetLow");
			nameGroups.Add(group);

			group = new List<string>();
			group.Add("wall5");
			group.Add("scoop5");
			group.Add("topTarget2");
			group.Add("topTarget3");
			group.Add("topTarget4");
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
			double cycleTime = 5;
			double ledFadeTime = cycleTime/3;
			double ledOnTime = cycleTime/3;
			foreach (List<string> group in nameGroups)
			{
				foreach (string name in group)
				{
					LEDScript script = new LEDScript( p3.LEDs[name], this.Priority);
					script.AddCommand (Multimorphic.P3.Colors.Color.red, ledFadeTime, ledOnTime);
					script.AddCommand (Multimorphic.P3.Colors.Color.green, ledFadeTime, ledOnTime);
					script.AddCommand (Multimorphic.P3.Colors.Color.blue, ledFadeTime, ledOnTime);
					LocalLEDScripts.Add (script);
					p3.LEDController.AddScript(script, -1, (groupIndex/numGroups)*cycleTime);
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

