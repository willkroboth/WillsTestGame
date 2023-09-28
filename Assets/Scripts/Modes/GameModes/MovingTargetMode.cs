using UnityEngine;
using System.Collections.Generic;
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Logging;

namespace PinballClub.TestGame.Modes
{
	public class MovingTargetMode : P3Mode
	{		
		private int MAX_HITS = 5;
		private int hitCount;

		public MovingTargetMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			AddGUIEventHandler("Evt_TargetHit", TargetHitEventHandler);
		}

		public override void mode_started ()
		{
			base.mode_started ();
			hitCount = 0;
		}
	
		public void TargetHitEventHandler(string evtName, object evtData)
		{
			// Target has been hit.  Score some points.
			ScoreManager.Score (10000);
			
			if (hitCount++ > MAX_HITS)
				hitCount = 0;
			
			// Hit by whom?
			string hitBy = (string)evtData;
			Multimorphic.P3App.Logging.Logger.Log ("Mode layer: Received TargetHit event from GUI layer.  Target hit by " + hitBy);

			// Tell the UI to move the target to a new position
			Multimorphic.P3App.Logging.Logger.Log ("Mode layer: Posting MoveTarget event to GUI layer to move to position " + hitCount.ToString());
			PostModeEventToGUI("Evt_MoveTarget", hitCount);
		}
	}
}
