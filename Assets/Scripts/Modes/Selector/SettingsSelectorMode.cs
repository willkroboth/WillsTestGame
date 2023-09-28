using Multimorphic.P3App.Modes;
using Multimorphic.P3;

namespace PinballClub.TestGame.Modes
{

    public class SettingsSelectorMode : AttributeSelectorMode
	{
		public SettingsSelectorMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			selectorId = "AttributeSelector";

			buttonLegend["LeftWhiteButton"] = "";
			buttonLegend["RightWhiteButton"] = "";
			buttonLegend["LeftRedButton"] = "Up";
			buttonLegend["RightRedButton"] = "Down";
			buttonLegend["LeftYellowButton"] = "Exit";
			buttonLegend["RightYellowButton"] = "Select";
			buttonLegend["StartButton"] = "Exit";
			buttonLegend["LaunchButton"] = "Select";

			AddSwitchHandlerMap("buttonLeft0", Left);
			AddSwitchHandlerMap("buttonRight0", Right);
			AddSwitchHandlerMap("buttonLeft1", Exit);
			AddSwitchHandlerMap("buttonRight1", Enter);
			AddSwitchHandlerMap("start", Exit);
			AddSwitchHandlerMap("launch", Enter);
			AddSwitchHandlerMap("buttonLeft2", Shift);
			AddSwitchHandlerMap("buttonRight2", Shift);
			
			AddSwitchHandlerMap("down", Left);
			AddSwitchHandlerMap("up", Right);
			//AddSwitchHandlerMap("enter", Enter);
			AddSwitchHandlerMap("exit", Exit);
		}

	}
}

