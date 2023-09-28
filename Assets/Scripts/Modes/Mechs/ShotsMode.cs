using System;
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.Modes
{

	/// <summary>
	/// A mode which propogates GUI events as mode events. 
	/// Used to effectively convert virtual switch events into mode switch events. Examples include the side targets and lane rollovers.
	/// Also used to detect combinations of switch events and report them as another single event.  For example, a ramp passage might be interpreted only if the ramp entry and ramp exit switches are both triggered in succession.
	/// This mode must have a very high priority.
	/// </summary>
    public class ShotsMode : P3Mode
	{
		public ShotsMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
		}

 		public override void mode_started ()
		{
			base.mode_started ();

			p3.GUIToModesEventManager.Subscribe ("Evt_SideTarget", SideTargetEventHandler);
			p3.GUIToModesEventManager.Subscribe ("EVT_ReturnLaneRightUpper", ReturnLaneRightUpperEventHandler);
			p3.GUIToModesEventManager.Subscribe ("EVT_ReturnLaneRightLower", ReturnLaneRightLowerEventHandler);
			p3.GUIToModesEventManager.Subscribe ("EVT_ReturnLaneLeftUpper", ReturnLaneLeftUpperEventHandler);
			p3.GUIToModesEventManager.Subscribe ("EVT_ReturnLaneLeftLower", ReturnLaneLeftLowerEventHandler);
			p3.GUIToModesEventManager.Subscribe ("EVT_OutLaneRightUpper", OutLaneRightUpperEventHandler);
			p3.GUIToModesEventManager.Subscribe ("EVT_OutLaneRightLower", OutLaneRightLowerEventHandler);
			p3.GUIToModesEventManager.Subscribe ("EVT_OutLaneLeftUpper", OutLaneLeftUpperEventHandler);
			p3.GUIToModesEventManager.Subscribe ("EVT_OutLaneLeftLower", OutLaneLeftLowerEventHandler);
		}

		public bool sw_slingL_active(Switch sw)
		{
			PostModeEventToModes ("Evt_LeftSlingHit", sw);
			return SWITCH_CONTINUE;
		}

		public bool sw_slingR_active(Switch sw)
		{
			PostModeEventToModes ("Evt_RightSlingHit", sw);
			return SWITCH_CONTINUE;
		}

		private void ReturnLaneRightUpperEventHandler(string evtName, object evtData)
		{
			PostModeEventToModes ("Evt_InlaneRightUpper", true);
		}

		private void ReturnLaneRightLowerEventHandler(string evtName, object evtData)
		{
			PostModeEventToModes ("Evt_InlaneRightLower", true);
		}

		private void ReturnLaneLeftUpperEventHandler(string evtName, object evtData)
		{
			PostModeEventToModes ("Evt_InlaneLeftUpper", false);
		}

		private void ReturnLaneLeftLowerEventHandler(string evtName, object evtData)
		{
			PostModeEventToModes ("Evt_InlaneLeftLower", false);
		}

		private void OutLaneRightUpperEventHandler(string evtName, object evtData)
		{
			PostModeEventToModes ("Evt_OutlaneRightUpper", true);
		}
		
		private void OutLaneRightLowerEventHandler(string evtName, object evtData)
		{
			PostModeEventToModes ("Evt_OutlaneRightLower", true);
		}
		
		private void OutLaneLeftUpperEventHandler(string evtName, object evtData)
		{
			PostModeEventToModes ("Evt_OutlaneLeftUpper", false);
		}
		
		private void OutLaneLeftLowerEventHandler(string evtName, object evtData)
		{
			PostModeEventToModes ("Evt_OutlaneLeftLower", false);
		}

		private void SideTargetEventHandler(string evtName, object evtData)
		{
			Multimorphic.P3App.Logging.Logger.Log("Shots mode side event");
			int index = 0;
			if (int.TryParse ((string)evtData, out index)) {
				Multimorphic.P3App.Logging.Logger.Log("Shots mode" + index.ToString ());
				PostModeEventToModes ("Evt_SideTargetHit", index);
			}
		}

	} 

}

