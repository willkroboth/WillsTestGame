using System;
using System.Collections.Generic;
using Multimorphic.NetProcMachine.Machine;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Modes.Selector;
using Multimorphic.P3App.Modes.Menu;

namespace PinballClub.TestGame.Modes.Menu
{

    public class TestGameSelectorManagerMode : SelectorManagerMode
	{
//		TestGameServiceMode serviceMode;
//		TestGameSelectMenuMode selectMenuMode;
//		TestGameNameEditorMode nameEditorMode;
//		TestGameConfirmationBoxMode confirmationBoxMode;

		public TestGameSelectorManagerMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
//			RegisterMenuMode(new TestGameServiceMode (p3, Priorities.PRIORITY_SERVICE_MODE), "SettingsMenu");
//			RegisterMenuMode(new Multimorphic.P3App.Modes.Menu.VolumeMode (p3, Priorities.PRIORITY_VOLUME_MODE), "VolumeMenu");
//			RegisterMenuMode(new TestGameSelectMenuMode (p3, Priorities.PRIORITY_PROFILE_MANAGER), "ProfileMenu");
//			RegisterMenuMode(new TestGameNameEditorMode (p3, Priorities.PRIORITY_HIGH_SCORES), "NameEditor");
//			RegisterMenuMode(new TestGameConfirmationBoxMode (p3, Priority + 1), "ConfirmationMenu");
		}
	} 

}

