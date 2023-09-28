// Copyright � 2019 Multimorphic, Inc. All Rights Reserved

using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Modes.Data;

namespace PinballClub.TestGame.Modes.Data {

	public class TestGameEventProfileMode : EventProfileMode {

		public TestGameEventProfileMode(P3Controller controller, int priority, string eventProfileName, string eventProfileDir)
			: base(controller, priority, eventProfileName, eventProfileDir) {
			highScoresMode = new TestGameHighScoresMode(p3, Priorities.PRIORITY_HIGH_SCORES, eventProfileName, eventProfileDir);
			statisticsMode = new TestGameStatisticsMode(p3, Priority, eventProfileName, eventProfileDir);
		}
	}
}