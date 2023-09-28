using System;
using System.Collections.Generic;
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.NetProcMachine.LEDs;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.Modes
{
	/// <summary>
	/// A mode in which LED scripts are declared and their logic is implemented.
	/// </summary>
	public class LEDShow : P3Mode
	{
		protected List<LEDScript> ledScripts;
		protected List<GUIInsertScript> insertScripts;

		public LEDShow (P3Controller controller, int priority)
			: base(controller, priority)
		{
			ledScripts = new List<LEDScript>();
			insertScripts = new List<GUIInsertScript>();
		}

		public override void mode_started ()
		{
			base.mode_started();

			foreach (LEDScript script in ledScripts)
			{
				p3.LEDController.AddScript(script, -1);
			}

			foreach (GUIInsertScript script in insertScripts)
			{
				PostModeEventToModes ("Evt_AddGUIInsertScript", script);
			}
		}	

		public override void mode_stopped ()
		{
			RemoveLEDScripts();
			base.mode_stopped ();
		}

		private void RemoveLEDScripts()
		{
			foreach (LEDScript script in ledScripts)
				p3.LEDController.RemoveScript(script);
			foreach (GUIInsertScript script in insertScripts)
				PostModeEventToModes ("Evt_RemoveGUIInsertScript", script);
		}
	}


	public struct ShowData
	{
		public ShowData(Mode Show, int ShowTime)
		{
			show = Show;
			showTime = ShowTime;
		}
		public Mode show;
		public int showTime;
	}

	public class LEDShowControllerMode : P3Mode
	{
		RGBFadeMode Show_RGBFade;
		//RotatingSmoothFadeMode Show_RotatingSmoothFade;
		//RGBRandomGroupsMainMode Show_RGBRandomGroupsMain;
		//RGBRandomFlashMode Show_RGBRandomFlash;
		List<ShowData> shows;
		int showIndex;

		public LEDShowControllerMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			Show_RGBFade = new RGBFadeMode(controller, priority);
			//Show_RotatingSmoothFade = new RotatingSmoothFadeMode(controller, priority);
			//Show_RGBRandomGroupsMain= new RGBRandomGroupsMainMode(controller, priority);
			//Show_RGBRandomFlash = new RGBRandomFlashMode(controller, priority);

			shows = new List<ShowData>();

			//shows.Add (new ShowData (Show_RotatingSmoothFade, 30));
			shows.Add (new ShowData (Show_RGBFade, 30));
			//shows.Add (new ShowData (Show_RGBRandomGroupsMain, 2));
			//shows.Add (new ShowData (Show_RGBRandomFlash, 2));
		}

		public override void mode_started ()
		{
			base.mode_started();
			StartShow();
		}

		private void StartShow()
		{
			Random random = new Random();
			showIndex = random.Next (0,shows.Count);
			p3.AddMode (shows[showIndex].show);

			this.delay("ShowDelay", Multimorphic.NetProc.EventType.None, shows[showIndex].showTime, new Multimorphic.P3.VoidDelegateNoArgs (EndShow));
		}

		private void EndShow()
		{
			p3.RemoveMode (shows[showIndex].show);
			StartShow ();
		}

		public override void mode_stopped ()
		{
			p3.RemoveMode (shows[showIndex].show);
			base.mode_stopped();
		}
	}

	public class RGBFadeMode : LEDShow
	{
		public RGBFadeMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			foreach (LED led in p3.LEDs.Values)
			{
				LEDScript script = new LEDScript( p3.LEDs[led.Name], this.Priority);
				script.AddCommand(LEDHelpers.AdjustColor(led.Name,  Multimorphic.P3.Colors.Color.red), 1, 1.25 );
				script.AddCommand(LEDHelpers.AdjustColor(led.Name,  Multimorphic.P3.Colors.Color.green), 1, 1.25 );
				script.AddCommand(LEDHelpers.AdjustColor(led.Name,  Multimorphic.P3.Colors.Color.blue), 1, 1.25);
				script.AddCommand(LEDHelpers.AdjustColor(led.Name,  Multimorphic.P3.Colors.Color.black), 1, 1.25);
				ledScripts.Add(script);
			}

			List<GUIInsertScript> insertScriptsOrig = GUIInsertHelpers.GetAllGUIInsertScripts(priority);

			for (int i=0; i<insertScriptsOrig.Count; i++)
			{
				if (!insertScriptsOrig[i].insertName.Contains("Box") && !insertScriptsOrig[i].insertName.Contains("opup"))
				{
					insertScriptsOrig[i].AddCommand(Multimorphic.P3.Colors.Color.red, 255, 1.50, 1.25 );
					insertScriptsOrig[i].AddCommand(Multimorphic.P3.Colors.Color.green, 255, 1.50, 1.25 );
					insertScriptsOrig[i].AddCommand(Multimorphic.P3.Colors.Color.blue, 255, 1.50, 1.25);
					insertScriptsOrig[i].AddCommand(Multimorphic.P3.Colors.Color.black, 0, 1.50, 1.25);
					insertScripts.Add (insertScriptsOrig[i]);
				}
			}
		}
	}
	
	public class RGBRandomGroupsMainMode : LEDShow
	{

		List<ushort []> colors;
		
		public RGBRandomGroupsMainMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			colors = new List<ushort[]>();
			colors.Add (Multimorphic.P3.Colors.Color.red);
			colors.Add (Multimorphic.P3.Colors.Color.green);
			colors.Add (Multimorphic.P3.Colors.Color.blue);
			colors.Add (Multimorphic.P3.Colors.Color.yellow);
			colors.Add (Multimorphic.P3.Colors.Color.cyan);
			colors.Add (Multimorphic.P3.Colors.Color.purple);
			colors.Add (Multimorphic.P3.Colors.Color.white);

			List<List<string>> nameGroups = new List<List<string>>();
			nameGroups.Add (new List<string>() {"wall"});
			nameGroups.Add (new List<string>() {"scoop"});
			nameGroups.Add (new List<string>() {"leftTarg", "rightTarg"});
			nameGroups.Add (new List<string>() {"flasherSide"});

			foreach (List<string> group in nameGroups)
			{
//REINSTATE		LEDHelpers.Shuffle(colors);
				foreach (string name in group)
				{
					foreach (LED led in p3.LEDs.Values)
					{
						if (led.Name.Contains (name))
						{
							LEDScript script = new LEDScript( p3.LEDs[led.Name], this.Priority);
							foreach (ushort [] color in colors)
							{
								script.AddCommand(LEDHelpers.AdjustColor(name, color), 0.25, 1);
							}
							ledScripts.Add(script);
						}
					}
				}
			}
		}
	}

	public class RGBRandomFlashMode : LEDShow
	{
		private List<List<LEDScript>> localLedScripts;
		private List<ushort []> colors;

		public RGBRandomFlashMode (P3Controller controller, int priority, ushort [] color)
			: base(controller, priority)
		{
			Init ();
			colors = new List<ushort[]>();
			colors.Add (Multimorphic.P3.Colors.Color.red);
		}

		public RGBRandomFlashMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			Init ();
			colors = new List<ushort[]>();
			colors.Add (Multimorphic.P3.Colors.Color.red);
			colors.Add (Multimorphic.P3.Colors.Color.green);
			colors.Add (Multimorphic.P3.Colors.Color.blue);
			colors.Add (Multimorphic.P3.Colors.Color.yellow);
			colors.Add (Multimorphic.P3.Colors.Color.cyan);
			colors.Add (Multimorphic.P3.Colors.Color.purple);
			colors.Add (Multimorphic.P3.Colors.Color.white);
		}

		private void Init()
		{
			localLedScripts = new List<List<LEDScript>>();

			List<LEDScript> list = new List<LEDScript>();
			for (int i=0; i<4; i++)
			{
				try {
					LEDScript script = new LEDScript( p3.LEDs["flasherSideModuleLeft" + i.ToString()], this.Priority);
					list.Add (script);
				}
				catch
				{
					Multimorphic.P3App.Logging.Logger.Log ("Error: p3.LEDs does not contain index " + "flasherSideModuleLeft" + i.ToString());
					throw;
				}
			}
			localLedScripts.Add (list);

			list = new List<LEDScript>();
			for (int i=0; i<4; i++)
			{
				LEDScript script = new LEDScript( p3.LEDs["flasherSideModuleRight" + i.ToString()], this.Priority);
				list.Add (script);
			}
			localLedScripts.Add (list);

			foreach (LED led in p3.LEDs.Values)
			{
				list = new List<LEDScript>();
				if (led.Name.Contains ("flasher") && !led.Name.Contains("SideModule") && !led.Name.Contains ("Ship") && !led.Name.Contains ("CenterArrow"))
				{
					LEDScript script = new LEDScript( led, this.Priority);
					list.Add (script);
				}
				if (list.Count > 0)
					localLedScripts.Add (list);
			}
		}

		public override void mode_started ()
		{
			base.mode_started();

			ShowColor();
		}

		public override void mode_stopped ()
		{
			cancel_delayed("ShowDelay");
			RemoveLEDScripts();
			base.mode_stopped();
		}

		private void RemoveLEDScripts()
		{
			for (int i=0; i<localLedScripts.Count; i++)
				for (int j=0; j<localLedScripts[i].Count; j++)
					p3.LEDController.RemoveScript(localLedScripts[i][j]);
		}

		private void ShowColor()
		{
			Random random = new Random();
			int colorIndex = random.Next (0, colors.Count);
			ushort [] color = colors[colorIndex];
			int group = random.Next(0, localLedScripts.Count);
			int index = random.Next(0, localLedScripts[group].Count);
			p3.LEDController.RemoveScript(localLedScripts[group][index]);
			localLedScripts[group][index].Clear();
			localLedScripts[group][index].AddCommand (color, 0, 0.060);
			localLedScripts[group][index].AddCommand (Multimorphic.P3.Colors.Color.black, 0, 1);
			p3.LEDController.AddScript(localLedScripts[group][index], 0);

			this.delay("ShowDelay", Multimorphic.NetProc.EventType.None, 0.10, new Multimorphic.P3.VoidDelegateNoArgs (ShowColor));
		}

	}

	public class RGBFlashMode : LEDShow
	{
		public RGBFlashMode (P3Controller controller, int priority, ushort [] color)
			: base(controller, priority)
		{
			foreach (LED led in p3.LEDs.Values)
			{
				LEDScript script = new LEDScript( led, this.Priority);
				script.AddCommand(Multimorphic.P3.Colors.Color.red, 0, 5);
				ledScripts.Add(script);
			}
		}
	}

	public class RGBFlashAndFadeMode : LEDShow
	{
		public RGBFlashAndFadeMode (P3Controller controller, int priority, ushort [] color, ushort [] intermediateColor)
			: base(controller, priority)
		{
			foreach (LED led in p3.LEDs.Values)
			{
				LEDScript script = new LEDScript( led, this.Priority);
				script = LEDHelpers.PulseLEDWithIntermediate(p3, script, 0.1f, 0.4f, 0.5f, color, intermediateColor,  Multimorphic.P3.Colors.Color.black);
				ledScripts.Add(script);
			}
		}
	}

	public class RGBFlasherPulseMode : LEDShow
	{
		public RGBFlasherPulseMode (P3Controller controller, int priority, ushort [] startColor, ushort [] endColor, float cycleTime)
			: base(controller, priority)
		{
			foreach (LED led in p3.LEDs.Values)
			{
				if (led.Name.Contains ("flasher"))
				{
					LEDScript script = new LEDScript( led, this.Priority);
					//script = LEDHelpers.PulseLED(p3, script, cycleTime, cycleTime, cycleTime, startColor, endColor);
					script = LEDHelpers.BlinkLED(p3, script, startColor);
					ledScripts.Add(script);
				}
			}
		}
	}

    public class RGBFadeOnAndHoldMode : LEDShow
    {

        public RGBFadeOnAndHoldMode(P3Controller controller, int priority, ushort[] color, float fadeOnTime)
            : base(controller, priority)
        {
            foreach (LED led in p3.LEDs.Values)
            {
                LEDScript script = new LEDScript(led, Priority);

                double runTime;
                if (fadeOnTime <= 0)
                    runTime = 0.1;
                else
                    runTime = fadeOnTime;

                script.autoRemove = false;
                script.AddCommand(color, fadeOnTime, runTime);
                ledScripts.Add(script);
            }
        }

    }

}

