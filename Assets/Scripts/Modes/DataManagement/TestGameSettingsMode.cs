using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Multimorphic.P3App.Data;
using Multimorphic.P3App.Modes.Data;
using System;

namespace PinballClub.TestGame.Modes.Data
{

    public class TestGameSettingsMode : SettingsMode
	{
		public TestGameSettingsMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
			DataVersion = 41;
			DBVersion = 28;
			Key = "Settings";
			Filename = "settings.db";

            // Set this to true to enable the replay logic.
            replayScoreLevelsEnabled = true;
		}

		protected override GameAttribute CustomizeGameAttribute(GameAttribute attr)
		{
			attr = base.CustomizeGameAttribute(attr);

			// This game doesn't have an intro video; so hide the setting from the user.
			if (attr.item == "PlayGameIntro")
			{
				attr.value.Set (false);
				attr.options |= GameAttributeOptions.Hidden;
			}
            // Set the following values based on what's appropriate for your game
            else if (attr.item == FrameworkAppGameAttributes.ReplaysEnabled)
            {
                attr.defaultValue.Set(true);
                attr.compareOptions = GameAttributeCompareOptions.UpdateValueIfDefaultChanged;
            }
            else if (attr.item == FrameworkAppGameAttributes.MinReplayScore)
            {
                attr.Set(5000000L);
                attr.max.Set(5000000L);
                attr.defaultValue.Set(5000000L);
                attr.compareOptions = GameAttributeCompareOptions.UpdateValueIfDefaultChanged;
            }
            else if (attr.item == FrameworkAppGameAttributes.CurrentReplayScore)
            {
                // This sets the default.  It doesn't overwrite the value that may have adjusted automatically based on previous gameplay
                attr.Set(8000000L);
                attr.defaultValue.Set(8000000L);
                attr.compareOptions = GameAttributeCompareOptions.UpdateValueIfDefaultChanged;
            }
            else if (attr.item == FrameworkAppGameAttributes.DynamicReplayScoreIncValue)
            {
                attr.Set(1000000L);
                attr.defaultValue.Set(1000000L);
                attr.compareOptions = GameAttributeCompareOptions.UpdateValueIfDefaultChanged;
            }
            else if (attr.item == FrameworkAppGameAttributes.DynamicReplayScoreDecValue)
            {
                attr.Set(250000L);
                attr.defaultValue.Set(250000L);
                attr.compareOptions = GameAttributeCompareOptions.UpdateValueIfDefaultChanged;
            }
			// My stuf :)
			else if (attr.item == "NumBalls")
            {
				attr.value.Set(1);
				attr.defaultValue.Set(1);
				attr.options |= GameAttributeOptions.Hidden;
				// and since you probably already ran the game, which creates the settings file
				// before running the above code, you'll want to force it to change back:
				attr.compareOptions |= GameAttributeCompareOptions.UpdateValueIfDefaultChanged;
			}
			else if (attr.item == "MaxPlayers")
            {
				attr.value.Set(1);
				attr.defaultValue.Set(1);
				attr.options |= GameAttributeOptions.Hidden;
				// and since you probably already ran the game, which creates the settings file
				// before running the above code, you'll want to force it to change back:
				attr.compareOptions |= GameAttributeCompareOptions.UpdateValueIfDefaultChanged;
			}

			return attr;
		}

		// Called in base.mode_started()
		protected override void CreateDefaultAttrs()
		{
			base.CreateDefaultAttrs();

			InitAttr(38, "UserProfileEditingEnabled", "Allow user to edit this profile", "Allow user to edit this profile", "Service Menu/Settings/General/Profiles", PTV, true, true, new string[] {"No", "Yes"});
			InitAttr(37, "GlobalStateSaveEnabled", "Global enable of state saving", "Global enable of state saving", "Service Menu/Settings/General", TV, true, true, new string[] {"No", "Yes"});
			InitAttr(37, "ProfileStateSaveEnabled", "Profile enable of state saving", "Profile enable of state saving", "Service Menu/Settings/General/Profiles", PTV, true, true, new string[] {"No", "Yes"});

            InitAttr(37, "SoundtrackVolume", "Soundtrack volume", "Volume of background music", "Service Menu/Settings/Audio", GameAttributeOptions.ReadWrite, 0.7f, 0.0f, 1.0f, 0.01f, 0.7f, new string[1]);
            InitAttr(37, "BassGain", "Bass gain", "Bass gain", "Service Menu/Settings/Audio", GameAttributeOptions.ReadWrite, 1.0f, 0.0f, 3.0f, 0.05f, 1.0f, new string[1]);

            // ******** Coils ********
//            InitAttr(37, "PopBumperPulseTime", "Pop Bumper Pulse Time", "Pop Bumper Pulse Time", "Service Menu/Settings/Mechs/Coils", RW, 8, 2, 14, 1, 8);

			// ****************
			// Gameplay
			// ****************

			// ******** General Gameplay Config ********
			InitAttr(37, "ModeLitOnBallStart", "Each ball starts with mode lit", "Mode Lit on Ball Start", "Service Menu/Settings/Gameplay/General", PTV, true, true, new string[] {"No", "Yes"});
			InitAttr(37, "BallSaveTime", "Ball Save Time", "Ball Save Time", "Service Menu/Settings/Gameplay/General", PRW, 15, 0, 20, 1, 15);
			InitAttr(37, "BallSaveGracePeriod", "Ball Save grace period time", "Ball Save grace period time", "Service Menu/Settings/Gameplay/General", PRW, 3, 0, 5, 1, 3);
			InitAttr(37, "SideTargetDifficulty", "Side Target Difficulty", "Side Target Difficulty", "Service Menu/Settings/Gameplay/General", PRW, 0, 0, 2, 1, 0);

            InitAttr(37, "TwitchViewerFeatureTime", "Number of seconds between viewer-controlled features", "Time between viewer features", "Service Menu/Settings/Twitch/Gameplay", GameAttributeOptions.ReadWrite, 12, 5, 60, 1, 12);
            InitAttr(37, "TwitchBlackoutEnabled", "Blackout command enabled", "Blackouts enabled", "Service Menu/Settings/Twitch/Gameplay", TV, true, true, new string[] { "No", "Yes" });
            InitAttr(37, "TwitchBlackoutBitsRequired", "Number of accumulated bits required to enable blackout", "Bits for blackout", "Service Menu/Settings/Twitch/Gameplay", GameAttributeOptions.ReadWrite, 200, 0, 50000, 100, 200);
            InitAttr(37, "TwitchBlackoutTime", "Number of seconds blackout attacks last", "Time for blackouts", "Service Menu/Settings/Twitch/Gameplay", GameAttributeOptions.ReadWrite, 20, 5, 40, 1, 20);
            InitAttr(37, "TwitchReverseEnabled", "Reverse command enabled", "Reverses enabled", "Service Menu/Settings/Twitch/Gameplay", TV, true, true, new string[] { "No", "Yes" });
            InitAttr(37, "TwitchReverseBitsRequired", "Number of accumulated bits required to enable reversed flippers", "Bits for reversed flippers", "Service Menu/Settings/Twitch/Gameplay", GameAttributeOptions.ReadWrite, 500, 0, 50000, 100, 500);
            InitAttr(37, "TwitchReverseTime", "Number of seconds reverse-flipper attacks last", "Time for reversed flippers", "Service Menu/Settings/Twitch/Gameplay", GameAttributeOptions.ReadWrite, 20, 5, 40, 1, 20);
            InitAttr(37, "TwitchInvertEnabled", "Invert command enabled", "Inverts enabled", "Service Menu/Settings/Twitch/Gameplay", TV, true, true, new string[] { "No", "Yes" });
            InitAttr(37, "TwitchInvertBitsRequired", "Number of accumulated bits required to enable inverted flippers", "Bits for inverted flippers", "Service Menu/Settings/Twitch/Gameplay", GameAttributeOptions.ReadWrite, 500, 0, 50000, 100, 500);
            InitAttr(37, "TwitchInvertTime", "Number of seconds invert-flipper attacks last", "Time for invert flippers", "Service Menu/Settings/Twitch/Gameplay", GameAttributeOptions.ReadWrite, 20, 5, 40, 1, 20);
            InitAttr(37, "TwitchBlackoutTest", "Enable periodic local blackout simulation", "Test blackouts during gameplay", "Service Menu/Settings/Twitch/Gameplay/Test Features", GameAttributeOptions.ReadWrite, false, false, new string[] { "No", "Yes" });
            InitAttr(37, "TwitchReverseTest", "Enable periodic local reverse flippers simulation", "Test reverse flippers during gameplay", "Service Menu/Settings/Twitch/Gameplay/Test Features", GameAttributeOptions.ReadWrite, false, false, new string[] { "No", "Yes" });
            InitAttr(37, "TwitchInvertTest", "Enable periodic local invert flippers simulation", "Test invert flippers during gameplay", "Service Menu/Settings/Twitch/Gameplay/Test Features", GameAttributeOptions.ReadWrite, false, false, new string[] { "No", "Yes" });
        }

        public override void mode_started ()
		{
			base.mode_started ();

            float soundTrackRatio = data.GetGameAttributeValue("SoundtrackVolume").ToFloat();
            PostModeEventToGUI("Evt_SetSoundtrackVolume", soundTrackRatio);

			float bassGain = data.GetGameAttributeValue("BassGain").ToFloat();
			PostModeEventToGUI("Evt_SetBassGain", bassGain);
        }

        protected override bool ProcessSavedAttributeEventHandler(string evtName, object evtData)
		{
            // This function allows monitoring of a GameAttribute being saved so that a corresponding action can be initiated.
            GameAttribute attribute = (GameAttribute)evtData;

            if (attribute.item == "SoundtrackVolume")
                PostModeEventToGUI("Evt_SetSoundtrackVolume", attribute.value.ToFloat());
			else if (attribute.item == "BassGain") {
				PostModeEventToGUI("Evt_SetBassGain", attribute.value.ToFloat());
			}
            

            // Call the base method or the attribute won't get saved permanently.
            return base.ProcessSavedAttributeEventHandler(evtName, evtData);
		}

		public override void UpdateStockProfileAttributes(string name)
		{
            /*

			if (name == "Default")
			{
				UpdateProfileAttribute (name, "NumJackpots", "25", true);
			}
			else if (name == "Custom")
			{
				UpdateProfileAttribute (name, "NumJackpots", "25", false);
			}
			else if (name == "Easy")
			{
				// NOTE - ALWAYS write the value as a string, regardless of its actual value.  It'll
				// be converted to the correct type when the attribute is updated in the lower level logic.
				UpdateProfileAttribute (name, "NumJackpots", "25", true);
			}
			else if (name == "Medium")
			{
				UpdateProfileAttribute (name, "NumJackpots", "40", true);
			}
			else if (name == "Hard")
			{
				UpdateProfileAttribute (name, "NumJackpots", "50", true);
			}

    */
		}

	} 

}

