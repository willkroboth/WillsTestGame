using System;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;

namespace PinballClub.TestGame.Modes
{

    public class ShotCounter : TestGameGameMode
	{
		int count;
		string evtNameOut;

		public ShotCounter (P3Controller controller, int priority, string shotEvtNameIn, string shotEvtNameOut)
			: base(controller, priority)
		{
			AddModeEventHandler(shotEvtNameIn, ShotEventHandler, Priority);
			evtNameOut = shotEvtNameOut;
		}

 		public override void LoadPlayerData()
		{
			count = data.currentPlayer.GetData(evtNameOut, 0);
		}
		
		public override void SavePlayerData()
		{
			data.currentPlayer.SaveData(evtNameOut, count);
		}

		public bool ShotEventHandler(string evtName, object evtData)
		{
			count++;
			PostModeEventToGUI(evtNameOut, count);
			PostModeEventToModes (evtNameOut, count);
			return SWITCH_CONTINUE;
		}
	} 

}

