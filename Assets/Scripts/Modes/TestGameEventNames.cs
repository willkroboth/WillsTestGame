using Multimorphic.P3App.Modes;

namespace PinballClub.TestGame.Modes
{
    /// <summary>
    /// Named events used in M2M and M2G events.
    /// </summary>
    public static class TestGameEventNames
    {
        // Framework events
        public const string EnableFlippers = "Evt_EnableFlippers";
        public const string EnableBumpers = "Evt_EnableBumpers";
        public const string BallStarted = "Evt_BallStarted";
        public const string BallEnded = "Evt_BallEnded";
        public const string ChangeGameState = "Evt_ChangeGameStates";
        public const string EnableBallSearch = "Evt_EnableBallSearch";
        public const string SideTargetHit = "Evt_SideTargetHit";
        public const string LeftSlingHit = "Evt_LeftSlingHit";
        public const string RightSlingHit = "Evt_RightSlingHit";

        // App events
        public const string TestGameHomeSetup = "Evt_TestGameHomeSetup";
        public const string InitialLaunch = "Evt_InitialLaunch";
        public const string BallDrained = "Evt_BallDrained";
        public const string MainTimerExpired = "Evt_MainTimerExpired";

        // Bird events
        public const string SpawnBird = "Evt_SpawnBird";
        public const string SpawnBirdFromIndex = "Evt_SpawnBirdFromIndex";
        public const string BirdHit = "Evt_BirdHit";
    }
}