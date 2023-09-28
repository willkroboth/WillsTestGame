//using Multimorphic.NetProcMachine.Machine;
//using Multimorphic.P3;
using Multimorphic.P3App.Modes;

namespace PinballClub.TestGame.Modes
{
	/// <summary>
	/// The priorities for the modes in this app.
	/// </summary>
	public static class TestGamePriorities
	{
		public const int PRIORITY_HOME = Priorities.PRIORITY_BASE + 10;

		public const int PRIORITY_HOME_UTILITIES = PRIORITY_HOME + 2;

		public const int PRIORITY_SCENE_MODE = PRIORITY_HOME + 20;

		public const int PRIORITY_SIDE_TARGET = PRIORITY_HOME + 22;

		public const int PRIORITY_MULTIBALL = PRIORITY_HOME + 34;
		
		public const int PRIORITY_FINALE = PRIORITY_HOME + 40;

		public const int PRIORITY_SHOT_COUNTERS = PRIORITY_HOME + 50;
		public const int PRIORITY_LANES = PRIORITY_HOME + 50;

		public const int PRIORITY_BIRD_RAMPS = PRIORITY_HOME + 51;

		public const int PRIORITY_RESPAWN = PRIORITY_HOME + 54;
		public const int PRIORITY_BALL_SAVE = PRIORITY_HOME + 55;

		public const int PRIORITY_MOVING_TARGET = PRIORITY_HOME + 60;
        public const int PRIORITY_BLACKOUT_LED_SHOW = PRIORITY_HOME + 399;

        public const int PRIORITY_BUTTON_COMBOS = Priorities.PRIORITY_SELECTOR_BASE - 1;

        public const int PRIORITY_CONFIRMATION = Priorities.PRIORITY_SERVICE_MODE + 2;
        public const int PRIORITY_PROFILESELECTOR = Priorities.PRIORITY_SELECTOR_BASE + 1;
        public const int PRIORITY_PROFILEATTRIBUTESELECTOR = Priorities.PRIORITY_SERVICE_MODE - 1;
        public const int PRIORITY_TEXT_EDITOR = Priorities.PRIORITY_SELECTOR_BASE + 123;
        public const int PRIORITY_EVENT_NAME_EDITOR = TestGamePriorities.PRIORITY_TEXT_EDITOR + 1;
		public const int PRIORITY_HIGH_SCORE_NAME_SELECTOR = TestGamePriorities.PRIORITY_TEXT_EDITOR + 2;

    }
}

