using System;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.NetProcMachine.Config;

namespace PinballClub.TestGame.Modes
{
	public class BirdRampMode : TestGameGameMode
	{
		public BirdRampMode(P3Controller controller, int priority)
			: base(controller, priority)
        {
			AddModeEventHandler(TestGameEventNames.SpawnBird, SpawnBirdEventHandler, priority);
			AddGUIEventHandler(TestGameEventNames.BirdHit, BirdHitEventHandler);

            foreach(BallPathDefinition shot in p3.BallPaths.Values)
            {
                if(shot.ExitName == "LeftInlane" || shot.ExitName == "RightInlane")
                {
                    AddModeEventHandler(shot.CompletedEvent, RampHitEventHandler, priority);
                    Multimorphic.P3App.Logging.Logger.Log("[BirdRampMode] Setup shot: " + shot.CompletedEvent);
                }
            }
        }

        public override void mode_started()
        {
            base.mode_started();
        }

        public bool SpawnBirdEventHandler(string eventName, object eventData)
        {
            int bushIndex = Int32.Parse((string)eventData);
            PostModeEventToGUI(TestGameEventNames.SpawnBirdFromIndex, bushIndex);
            return EVENT_STOP;
        }

        public void BirdHitEventHandler(string eventName, object eventData)
        {
            int score = (int)eventData;
            ScoreManager.Score(score);
        }

        public bool RampHitEventHandler(string eventName, object eventData)
        {
            PostModeEventToGUI(TestGameEventNames.SpawnBirdFromIndex, UnityEngine.Random.Range(0, 4));
            return EVENT_CONTINUE;
        }
    }
}
