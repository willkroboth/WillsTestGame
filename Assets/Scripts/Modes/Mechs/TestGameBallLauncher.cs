using Multimorphic.P3.Mechs;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Data;

namespace PinballClub.TestGame.Modes
{
	/// <summary>
	/// A mode to control ball launches from the various VUKs.
	/// </summary>
	public class TestGameBallLauncher
	{
		private static string LaunchIndex;
		public const string VUK_LEFT = "left";
		public const string VUK_RIGHT = "right";
		private const int DELAYED_LAUNCH_TIME = 2;
		public static bool useLeftVUK = true;
		public static bool useRightVUK = true;
		private static DataManager data;

		public static void Initialize (Multimorphic.P3.P3Controller p3, DataManager Data)
		{
			data = Data;
			BallLauncher.AssignP3(p3);
		
			// Assign primaries
			BallLauncher.AssignTroughLauncher (BallLauncher.GetTroughLaunchersForDestination (LaunchDestination.LeftInlane), VUK_LEFT);
			BallLauncher.AssignTroughLauncher (BallLauncher.GetTroughLaunchersForDestination (LaunchDestination.RightInlane), VUK_RIGHT);
			
			// Assign backups for each VUK.  The backup is the opposite VUK.
			BallLauncher.AssignTroughLauncher (BallLauncher.GetTroughLaunchersForDestination (LaunchDestination.LeftInlane), VUK_RIGHT);
			BallLauncher.AssignTroughLauncher (BallLauncher.GetTroughLaunchersForDestination (LaunchDestination.RightInlane), VUK_LEFT);
		}

		public static void launch()
		{
			string currLaunchIndex = LaunchIndex;
			LaunchIndex = getAlternateVUK(LaunchIndex);
			
			if (currLaunchIndex != VUK_LEFT)
				launch(VUK_LEFT);
			else
				launch(VUK_RIGHT);
		}

		public static void launch(string key)
		{
			PopupDebug ("launch: ");
			BallLauncher.Launch(key, Multimorphic.P3.Mechs.BallLaunchStrength.Hard);
		}
		
		public static void launch(Multimorphic.P3.VoidDelegateNoArgs callback)
		{
			LaunchIndex = getAlternateVUK(LaunchIndex);
			
			if (LaunchIndex != VUK_LEFT)
				launch(VUK_LEFT, callback);
			else
				launch(VUK_RIGHT, callback);
		}
		
		public static void launch(string vuk, Multimorphic.P3.VoidDelegateNoArgs callback)
		{
			PopupDebug ("launch: ");
			BallLauncher.Launch(vuk, Multimorphic.P3.Mechs.BallLaunchStrength.Hard, 0, callback);
		}

		private static void PopupDebug(string prefix)
		{

			if (data.GetGameAttributeValue("LaunchDebug").ToBool())
			{
				System.Diagnostics.StackFrame frame;
				frame = new System.Diagnostics.StackFrame(1);
				for (int i=1; i<10; i++)
				{
					frame = new System.Diagnostics.StackFrame(i);
					var method = frame.GetMethod();
					//if (method.ReflectedType.Name.Contains("TestGameBallLauncher"))
					var name = method.Name;
					if (name.Contains("sw_") || name.Contains("Event") || name.Contains("Handler"))
					{
						Multimorphic.NetProcMachine.EventManager.Post ("Evt_ShowPopup", method.ReflectedType.Name + ":" + name);
						break;
					}
				}
			}
		}

		public static void delayed_launch()
		{
			delayed_launch (DELAYED_LAUNCH_TIME);
		}
		
		public static void delayed_launch(double delay)
		{
			BallLauncher.DummyLaunch(delay);
			launch ();
		}
		
		public static void delayed_launch(string key, double delay)
		{
			BallLauncher.DummyLaunch(delay);
			launch (key);
		}
		
		public static void delayed_launch(Multimorphic.P3.VoidDelegateNoArgs callback)
		{
			delayed_launch (DELAYED_LAUNCH_TIME, callback);
		}
		
		public static void delayed_launch(int delay, Multimorphic.P3.VoidDelegateNoArgs callback)
		{
			BallLauncher.DummyLaunch(delay);
			launch(callback);
		}

		private static string getVUK(string vuk)
		{
			if (vuk == VUK_LEFT) {
				if (useLeftVUK)
					return VUK_LEFT;
				else
					return VUK_RIGHT;
			}
			else {
				if (useRightVUK)
					return VUK_RIGHT;
				else
					return VUK_LEFT;
			}
			
		}
		
		private static string getAlternateVUK(string vuk)
		{
			if (vuk == VUK_LEFT)
				return VUK_RIGHT;
			else
				return VUK_LEFT;
		}

	}
}

