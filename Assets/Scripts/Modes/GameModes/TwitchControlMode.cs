using Multimorphic.NetProcMachine.Machine;
using Multimorphic.NetProcMachine.LEDs;
using Multimorphic.P3;
using System.Collections.Generic;
using System.Linq;
using Multimorphic.P3App.Modes;
using PinballClub.TestGame.Modes;
using Multimorphic.P3App.Modes.Data;
using Multimorphic.P3App.Twitch;
using PinballClub.TestGame.GUI;

namespace PinballClub.TestGame.Modes
{

    public class TwitchAttacker
    {
        public string Name { get; protected set; }
        public double TimeActivated { get; protected set; }
        public bool Activated { get; protected set; }
        public long BitsPlayed { get; protected set; }

        public TwitchAttacker(string UserName, long Bits)
        {
            Name = UserName;
            BitsPlayed = Bits;
            Activated = false;
        }

        public void Activate()
        {
            if (!Activated)
            {
                TimeActivated = Multimorphic.P3.Tools.Time.GetTime();
                Activated = true;
            }
        }
    }

	/// <summary>
	/// A mode which handles the rollovers in the lower lanes of the playfield, including sequenced advancement via flipper buttons. 
	/// </summary>
    public class TwitchControlMode : P3Mode
	{
        bool powerupsEnabled;
        bool powerupAllowedRightNow;

        bool blackoutActive = false;
        bool invertActive = false;
        bool reverseActive = false;

        RGBFadeOnAndHoldMode blackoutLEDsMode;
        long blackoutBitsCollected;
        List<TwitchAttacker> blackoutAttackers = new List<TwitchAttacker>();

        long reverseBitsCollected;
        List<TwitchAttacker> reverseAttackers = new List<TwitchAttacker>();

        long invertBitsCollected;
        List<TwitchAttacker> invertAttackers = new List<TwitchAttacker>();

        private bool blackoutEnabled
        {
            get { return data.GetGameAttributeValue("TwitchBlackoutEnabled").ToBool(); }
        }

        private bool reverseEnabled
        {
            get { return data.GetGameAttributeValue("TwitchReverseEnabled").ToBool(); }
        }

        private bool invertEnabled
        {
            get { return data.GetGameAttributeValue("TwitchInvertEnabled").ToBool(); }
        }

        private long bitsForBlackout
        {
            get { return data.GetGameAttributeValue("TwitchBlackoutBitsRequired").ToLong(); }
        }

        private long bitsForReverse
        {
            get { return data.GetGameAttributeValue("TwitchReverseBitsRequired").ToLong(); }
        }

        private long bitsForInvert
        {
            get { return data.GetGameAttributeValue("TwitchInvertBitsRequired").ToLong(); }
        }

        private int timeForBlackout
        {
            get { return data.GetGameAttributeValue("TwitchBlackoutTime").ToInt(); }
        }

        private int timeForReverse
        {
            get { return data.GetGameAttributeValue("TwitchReverseTime").ToInt(); }
        }

        private int timeForInvert
        {
            get { return data.GetGameAttributeValue("TwitchInvertTime").ToInt(); }
        }

        private int viewerFeatureTime
        {
            get { return data.GetGameAttributeValue("TwitchViewerFeatureTime").ToInt(); }
        }

        private bool isTwitchAffiliate
        {
            get { return data.GetGameAttributeValue("GlobalSettings", "TwitchAffiliateEnabled").ToBool(); }
        }

        private const int BLACKOUT_INDEX = 876;
        private const int REVERSE_INDEX = 877;
        private const int INVERT_INDEX = 878;

        public TwitchControlMode (P3Controller controller, int priority)
			: base(controller, priority)
		{
            AddGUIEventHandler("Evt_ReceiveTwitchMessage", ReceiveTwitchMessageEventHandler);
            AddModeEventHandler("Evt_TwitchAnnounceRules", TwitchAnnounceRulesEventHandler, Priority);
            AddModeEventHandler("Evt_TwitchAllowPowerupRequests", TwitchAllowPowerupRequestsEventHandler, Priority);
            blackoutLEDsMode = new RGBFadeOnAndHoldMode(p3, TestGamePriorities.PRIORITY_BLACKOUT_LED_SHOW, Multimorphic.P3.Colors.Color.off, 1);
        }

        public override void mode_started ()
		{
			base.mode_started ();
		}

		public override void mode_stopped ()
		{
            DisableBlackout();
            DisableReverse();
            DisableInvert();
            base.mode_stopped();
		}

        private void AnnounceRules()
        {
            powerupAllowedRightNow = false;
            powerupsEnabled = false;

            if (data.GetGameAttributeValue("TwitchControlEnabled").ToBool())
            {
                SendInitialPowerUpRequestMessage();
                Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "TwitchControlMode AnnounceRules");
            }
        }

        private bool SetTwitchGameTypeEventHandler(string evtName, object evtData)
        {
            return EVENT_CONTINUE;
        }

        private bool TwitchAnnounceRulesEventHandler(string evtName, object evtData)
        {
            AnnounceRules();
            return EVENT_CONTINUE;
        }

        private bool TwitchAllowPowerupRequestsEventHandler(string evtName, object evtData)
        {
            bool on = (bool)evtData;
            if (data.GetGameAttributeValue("TwitchControlEnabled").ToBool())
            {
                if (on)
                    StartAllowingPowerupRequests();
                else
                    StopAllowingPowerupRequests();
            }
            return EVENT_CONTINUE;
        }

        public void StartAllowingPowerupRequests()
        {
            // See this high to force a powerup Ready
            powerupsEnabled = true;
            SendNextPowerUpRequestMessage();

            if (blackoutEnabled && blackoutBitsCollected >= bitsForBlackout)
                EnableBlackout();

            if (reverseEnabled && reverseBitsCollected >= bitsForReverse)
                EnableReverse();

            if (invertEnabled && invertBitsCollected >= bitsForInvert)
                EnableInvert();

            Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "TwitchControlMode StartAllowingPowerupRequests");
            if (blackoutEnabled && data.GetGameAttributeValue("TwitchBlackoutTest").ToBool())
                SendFakeBlackoutMessage();

            if (reverseEnabled && data.GetGameAttributeValue("TwitchReverseTest").ToBool())
                SendFakeReverseMessage();

            if (invertEnabled && data.GetGameAttributeValue("TwitchInvertTest").ToBool())
                SendFakeInvertMessage();
        }

        public void StopAllowingPowerupRequests()
        {
            powerupAllowedRightNow = false;
            powerupsEnabled = false;
            cancel_delayed("PowerUpRequestRefreshDelay");
            SendPowerUpStopMessage();
            DisableBlackout();
            DisableReverse();
            DisableInvert();
        }

        public void ReceiveTwitchMessageEventHandler(string evtName, object evtData)
        {
            TwitchMessage twitchMsg = (TwitchMessage)evtData;

            if (twitchMsg.IsPrivMsg)
            {
                if (data.GetGameAttributeValue("TwitchControlEnabled").ToBool())
                {
                    ProcessTwitchMsg(twitchMsg);
                }
            }
        }

        private void ProcessTwitchMsg(TwitchMessage twitchMsg)
        {
            string ctrlMsg = twitchMsg.Msg.ToLower();
            Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, twitchMsg.RawMsg);

            if (powerupsEnabled)
            {
                if (powerupAllowedRightNow)
                {
                    if (ctrlMsg.Contains("score:"))
                    {
                        string scoreValueString = twitchMsg.Msg.Substring(ctrlMsg.IndexOf("score:") + 6);
                        long scoreValue = 0;
                        bool isNumeric = long.TryParse(scoreValueString, out scoreValue);
                        if (isNumeric)
                        {
                            ScoreManager.Score(scoreValue);
                            PostModeEventToGUI("Evt_PlaySound", "FX/HitSound");
                            string response = scoreValue.ToString() + " points gifted by: " + twitchMsg.User;
                            PostModeEventToGUI("Evt_SendTwitchMessage", response);
                            DisablePowerupsTemporarily();
                        }
                        else
                            Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "not numeric");
                    }
                    else if (blackoutEnabled && (!isTwitchAffiliate || bitsForBlackout == 0) && ctrlMsg.Contains("blackout"))
                    {
                        blackoutAttackers.Clear();
                        blackoutAttackers.Add(new TwitchAttacker(twitchMsg.User, 0));
                        EnableBlackout();
                    }
                    else if (reverseEnabled && (!isTwitchAffiliate || bitsForReverse == 0) && ctrlMsg.Contains("reverse"))
                    {
                        reverseAttackers.Clear();
                        reverseAttackers.Add(new TwitchAttacker(twitchMsg.User, 0));
                        EnableReverse();
                    }
                    else if (invertEnabled && (!isTwitchAffiliate || bitsForInvert == 0) && ctrlMsg.Contains("invert"))
                    {
                        invertAttackers.Clear();
                        invertAttackers.Add(new TwitchAttacker(twitchMsg.User, 0));
                        EnableInvert();
                    }
                }

                if (ctrlMsg.Contains("moo"))
                    PlayEasterEggSound("moo");
                else if (ctrlMsg.Contains("neigh"))
                    PlayEasterEggSound("neigh");
                else if (ctrlMsg.Contains("cluck"))
                    PlayEasterEggSound("cluck");
                else if (ctrlMsg.Contains("oink"))
                    PlayEasterEggSound("oink");
                else if (ctrlMsg.Contains("baa"))
                    PlayEasterEggSound("baa");
                else if (ctrlMsg.Contains("fail"))
                    PlayEasterEggSound("fail");
                else if (ctrlMsg.Contains("muhaha"))
                {
                    if (twitchMsg.User == "multimorphic")
                        PlayEasterEggSound("laugh_evil");
                    else
                        PostModeEventToGUI("Evt_SendTwitchMessage", twitchMsg.User + " is not authorized for \"muhaha\"");
                }
            }

            if (isTwitchAffiliate) {
                if (blackoutEnabled && bitsForBlackout > 0 && ctrlMsg.Contains("blackout"))
                {
                    long bits = twitchMsg.GetBits();
                    if (bits > 0)
                    {
                        // Remove current message user from the list so he's not in there twice.  Re-add instead of just
                        // modifying the entry so that it's treated like a new bid.
                        blackoutAttackers = blackoutAttackers.Where(x => x.Name != twitchMsg.User).ToList();
                        blackoutAttackers.Add(new TwitchAttacker(twitchMsg.User, bits));
                        blackoutBitsCollected += bits;
                        Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "blackout bits: " + bits.ToString() + " total:" + blackoutBitsCollected.ToString());
                        if (blackoutBitsCollected >= bitsForBlackout)
                        {
                            if (powerupsEnabled)
                                EnableBlackout();
                            else
                            {
                                string response = "\"blackout\" request confirmed.  It will activate when gameplay resumes.";
                                PostModeEventToGUI("Evt_SendTwitchMessage", response);
                            }
                        }
                        else
                        {
                            string response = "\"blackout\" request from: " + twitchMsg.User + " : " + bits.ToString() + " bits.  " + (bitsForBlackout - blackoutBitsCollected).ToString() + " remaining!";
                            PostModeEventToGUI("Evt_SendTwitchMessage", response);
                        }
                    }
                }
                else if (reverseEnabled && bitsForReverse > 0 && ctrlMsg.Contains("reverse"))
                {
                    long bits = twitchMsg.GetBits();
                    if (bits > 0)
                    {
                        // Remove current message user from the list so he's not in there twice.  Re-add instead of just
                        // modifying the entry so that it's treated like a new bid.
                        reverseAttackers = reverseAttackers.Where(x => x.Name != twitchMsg.User).ToList();
                        reverseAttackers.Add(new TwitchAttacker(twitchMsg.User, bits));
                        reverseBitsCollected += bits;
                        Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "reverse bits: " + bits.ToString() + " total:" + reverseBitsCollected.ToString());
                        if (reverseBitsCollected >= bitsForReverse)
                        {
                            if (powerupsEnabled)
                                EnableReverse();
                            else
                            {
                                string response = "\"reverse\" request confirmed.  It will activate when gameplay resumes.";
                                PostModeEventToGUI("Evt_SendTwitchMessage", response);
                            }
                        }
                        else
                        {
                            string response = "\"reverse\" request from: " + twitchMsg.User + " : " + bits.ToString() + " bits.  " + (bitsForReverse - reverseBitsCollected).ToString() + " remaining!";
                            PostModeEventToGUI("Evt_SendTwitchMessage", response);
                        }
                    }
                }
                else if (invertEnabled && bitsForInvert > 0 && ctrlMsg.Contains("invert"))
                {
                    long bits = twitchMsg.GetBits();
                    if (bits > 0)
                    {
                        // Remove current message user from the list so he's not in there twice.  Re-add instead of just
                        // modifying the entry so that it's treated like a new bid.
                        invertAttackers = invertAttackers.Where(x => x.Name != twitchMsg.User).ToList();
                        invertAttackers.Add(new TwitchAttacker(twitchMsg.User, bits));
                        invertBitsCollected += bits;
                        Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "invert bits: " + bits.ToString() + " invert:" + invertBitsCollected.ToString());
                        if (invertBitsCollected >= bitsForInvert)
                        {
                            if (powerupsEnabled)
                                EnableInvert();
                            else
                            {
                                string response = "\"invert\" request confirmed.  It will activate when gameplay resumes.";
                                PostModeEventToGUI("Evt_SendTwitchMessage", response);
                            }
                        }
                        else
                        {
                            string response = "\"invert\" request from: " + twitchMsg.User + " : " + bits.ToString() + " bits.  " + (bitsForInvert - invertBitsCollected).ToString() + " remaining!";
                            PostModeEventToGUI("Evt_SendTwitchMessage", response);
                        }
                    }
                }
            }
        }

        private void DisablePowerupsTemporarily()
        {

            double delayTime = viewerFeatureTime;
            powerupAllowedRightNow = false;
            delay("PowerUpRequestRefreshDelay", Multimorphic.NetProc.EventType.None, delayTime, new Multimorphic.P3.VoidDelegateNoArgs(SendNextPowerUpRequestMessage));
        }

        private void SendInitialPowerUpRequestMessage()
        {
            string msg = "Welcome! Viewers can participate in gameplay!";
            PostModeEventToGUI("Evt_SendTwitchMessage", msg);
            List<string> freePowerups = new List<string>();
            freePowerups.Add("score");
            if (blackoutEnabled && (!isTwitchAffiliate || bitsForBlackout == 0))
                freePowerups.Add("blackout");
            if (reverseEnabled && (!isTwitchAffiliate || bitsForReverse == 0))
                freePowerups.Add("reverse");
            if (invertEnabled && (!isTwitchAffiliate || bitsForInvert == 0))
                freePowerups.Add("invert");

            List<string> bitPowerups = new List<string>();
            string exampleBitPowerupName = "";
            if (blackoutEnabled && isTwitchAffiliate && bitsForBlackout > 0)
            {
                bitPowerups.Add("blackout (" + bitsForBlackout.ToString() + ")");
                exampleBitPowerupName = "blackout";
            }
            if (reverseEnabled && isTwitchAffiliate && bitsForReverse > 0)
            {
                bitPowerups.Add("reverse (" + bitsForReverse.ToString() + ")");
                exampleBitPowerupName = "reverse";
            }
            if (invertEnabled && isTwitchAffiliate && bitsForInvert > 0)
            {
                bitPowerups.Add("invert (" + bitsForInvert.ToString() + ")");
                exampleBitPowerupName = "invert";
            }

            string freePowerupText = "Free powerups: ";
            for (int i = 0; i < freePowerups.Count; i++)
            {
                if (i > 0 && i == freePowerups.Count - 1)
                    freePowerupText += " and ";
                freePowerupText += freePowerups[i];
                if (i < freePowerups.Count - 1)
                    freePowerupText += ", ";
                else
                    freePowerupText += ".";

            }
            freePowerupText += "  Wait to be prompted.";
            string freeExampleText = "Examples: \"score:50\" adds 50 points to the player's score.";

            string bitPowerupText = "";
            if (bitPowerups.Count > 0)
            {
                bitPowerupText = "  Bit-supported powerups: ";
                for (int i = 0; i < bitPowerups.Count; i++)
                {
                    if (i == freePowerups.Count - 1)
                        bitPowerupText += ", and ";
                    bitPowerupText += bitPowerups[i];
                    if (i < bitPowerups.Count - 1)
                        bitPowerupText += ", ";
                    else
                        bitPowerupText += ".";
                }

                bitPowerupText += "  Bits are accumulated until threshold is met.  These can be played any time during a ball.";
            }

            string bitExampleText = "";
            if (bitPowerups.Count > 0)
            {
                bitExampleText += "  \"cheer100 " + exampleBitPowerupName + "\" adds 100 bits towards a " + exampleBitPowerupName + " attack";
            }

            msg = freePowerupText + bitPowerupText;
            PostModeEventToGUI("Evt_SendTwitchMessage", msg);

            msg = freeExampleText + bitExampleText;
            PostModeEventToGUI("Evt_SendTwitchMessage", msg);
        }

        private void SendNextPowerUpRequestMessage()
        {
            SendPowerUpRequestMessage("Ready!");
        }

        private void SendPowerUpRequestMessage(string msg)
        {
            powerupAllowedRightNow = true;
            PostModeEventToGUI("Evt_SendTwitchMessage", msg);
        }

        private void SendPowerUpStopMessage()
        {
            string twitchMessage = "Powerup requests are currently disabled";
            PostModeEventToGUI("Evt_SendTwitchMessage", twitchMessage);
        }

        protected void EnableBlackout()
        {
            blackoutActive = true;

            Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "Enabling blackout");
            blackoutBitsCollected = 0;

            double currTime = Multimorphic.P3.Tools.Time.GetTime();

            // Cull out attackers that have already activated a blackout and have timed out.
            blackoutAttackers = blackoutAttackers.Where(x => !x.Activated || (x.Activated && x.TimeActivated > currTime - timeForBlackout)).ToList();

            // Now activate all remaining attackers (if they're already activate, the class code won't reset their timer)
            // Also get a list of names to send to GUI
            List<string> attackerNames = new List<string>();
            foreach (TwitchAttacker attacker in blackoutAttackers)
            {
                attacker.Activate();
                attackerNames.Add(attacker.Name);
            }

            p3.RemoveMode(blackoutLEDsMode);
            p3.AddMode(blackoutLEDsMode);

            delay("BlackoutDelay", Multimorphic.NetProc.EventType.None, timeForBlackout, new Multimorphic.P3.VoidDelegateNoArgs(DisableBlackout));

            PostModeEventToGUI("Evt_EnableBlackout", attackerNames);

            string twitchMessage = GetPlayedByNamesFromList(attackerNames) + " played a Blackout attack for " + timeForBlackout.ToString() + " seconds.";
            PostModeEventToGUI("Evt_SendTwitchMessage", twitchMessage);


            if (!isTwitchAffiliate)
                DisablePowerupsTemporarily();
        }

        protected string GetPlayedByNamesFromList(List<string> attackerNames)
        {
            string playedByName = "";
            for (int i = 0; i < attackerNames.Count; i++)
            {
                if (i > 0)
                {
                    if (i == attackerNames.Count - 1)
                        playedByName += ", and ";
                    else
                        playedByName += ", ";
                }

                playedByName += attackerNames[i];
            }
            return playedByName;
        }

        protected void DisableBlackout()
        {
            if (blackoutActive)
            {
                p3.RemoveMode(blackoutLEDsMode);
                PostModeEventToGUI("Evt_DisableBlackout", 0);
                blackoutActive = false;
            }
            cancel_delayed("FakeBlackoutDelay");
        }

        protected void SendFakeBlackoutMessage()
        {
            if (powerupsEnabled)
            {
                System.Random rnd = new System.Random();
                //List<string> names = new List<string> { "bill", "bob", "john", "mark", "michael", "michael", "michael", "bob", "mark", "sarah", "gerry" };
                //string name = names[rnd.Next(0, names.Count)];
                string name = "name" + rnd.Next(0, 5000).ToString();
                Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "Processing fake blackout message");
                //ProcessTwitchMsg(new TwitchMessage("@badge-info=;badges=broadcaster/1;client-nonce=25c6d8d227c5f910b5651762605371fe;color=;display-name=multimorphic_p3;emotes=;flags=;id=509b0066-88fa-4148-8271-9a6effdb0e8c;mod=0;room-id=563638041;subscriber=0;tmi-sent-ts=1598580574324;turbo=0;user-id=563638041;user-type= :ccr_fan!ccr_fan@ccr_fan.tmi.twitch.tv PRIVMSG #ccr_fan :emp:1"));
                ProcessTwitchMsg(new TwitchMessage("@badge-info=;badges=broadcaster/1;bits=25;client-nonce=25c6d8d227c5f910b5651762605371fe;color=;display-name=multimorphic_p3;emotes=;flags=;id=509b0066-88fa-4148-8271-9a6effdb0e8c;mod=0;room-id=563638041;subscriber=0;tmi-sent-ts=1598580574324;turbo=0;user-id=563638041;user-type= :" + name + "!" + name + "@" + name + ".tmi.twitch.tv PRIVMSG #" + name + " :blackout"));
                delay("FakeBlackoutDelay", Multimorphic.NetProc.EventType.None, 10, new Multimorphic.P3.VoidDelegateNoArgs(SendFakeBlackoutMessage));
            }
        }

        protected void SendFakeBitsMessage()
        {
            if (powerupsEnabled)
            {
                System.Random rnd = new System.Random();
                int numBits = rnd.Next(20, 100);
                string name = "donor";
                Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "Processing fake bits message.  numBits:" + numBits.ToString());
                ProcessTwitchMsg(new TwitchMessage("@badge-info=;badges=broadcaster/1;bits=" + numBits.ToString() + ";client-nonce=25c6d8d227c5f910b5651762605371fe;color=;display-name=multimorphic_p3;emotes=;flags=;id=509b0066-88fa-4148-8271-9a6effdb0e8c;mod=0;room-id=563638041;subscriber=0;tmi-sent-ts=1598580574324;turbo=0;user-id=563638041;user-type= :" + name + "!" + name + "@" + name + ".tmi.twitch.tv PRIVMSG #" + name + " :bits test with " + numBits.ToString() + " bits."));
                delay("FakeBitsDelay", Multimorphic.NetProc.EventType.None, 5, new Multimorphic.P3.VoidDelegateNoArgs(SendFakeBitsMessage));
            }
        }

        protected void EnableReverse()
        {
            reverseActive  = true;
            reverseBitsCollected = 0;

            double currTime = Multimorphic.P3.Tools.Time.GetTime();

            // Cull out attackers that have already activated a blackout and have timed out.
            reverseAttackers = reverseAttackers.Where(x => !x.Activated || (x.Activated && x.TimeActivated > currTime - timeForReverse)).ToList();

            // Now activate all remaining attackers (if they're already activate, the class code won't reset their timer)
            // Also get a list of names to send to GUI
            List<string> attackerNames = new List<string>();
            foreach (TwitchAttacker attacker in reverseAttackers)
            {
                attacker.Activate();
                attackerNames.Add(attacker.Name);
            }

            delay("ReverseDelay", Multimorphic.NetProc.EventType.None, timeForReverse, new Multimorphic.P3.VoidDelegateNoArgs(EndReverse));

            PostModeEventToModes(EventNames.ReverseFlippers, true);
            PostModeEventToGUI("Evt_EnableReverseFlippers", attackerNames);
            SendPowerUpAckMessage(REVERSE_INDEX, GetPlayedByNamesFromList(attackerNames), 1);

            if (!isTwitchAffiliate)
                DisablePowerupsTemporarily();
        }

        protected void EndReverse()
        {
            DisableReverse();
        }

        protected void DisableReverse()
        {
            if (reverseActive)
            {
                Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "Disabling reverse");
                PostModeEventToModes(EventNames.ReverseFlippers, false);
                PostModeEventToGUI("Evt_DisableReverseFlippers", 0);
                reverseActive = false;
            }
            cancel_delayed("FakeReverseDelay");
        }

        protected void SendFakeReverseMessage()
        {
            if (powerupsEnabled)
            {
                System.Random rnd = new System.Random();
                List<string> names = new List<string> { "bill", "bob", "john", "mark", "michael", "michael", "michael", "bob", "mark", "sarah", "gerry" };
                string name = names[rnd.Next(0, names.Count)];
                Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "Processing fake reverse message");
                //ProcessTwitchMsg(new TwitchMessage("@badge-info=;badges=broadcaster/1;client-nonce=25c6d8d227c5f910b5651762605371fe;color=;display-name=multimorphic_p3;emotes=;flags=;id=509b0066-88fa-4148-8271-9a6effdb0e8c;mod=0;room-id=563638041;subscriber=0;tmi-sent-ts=1598580574324;turbo=0;user-id=563638041;user-type= :ccr_fan!ccr_fan@ccr_fan.tmi.twitch.tv PRIVMSG #ccr_fan :emp:1"));
                ProcessTwitchMsg(new TwitchMessage("@badge-info=;badges=broadcaster/1;bits=100;client-nonce=25c6d8d227c5f910b5651762605371fe;color=;display-name=multimorphic_p3;emotes=;flags=;id=509b0066-88fa-4148-8271-9a6effdb0e8c;mod=0;room-id=563638041;subscriber=0;tmi-sent-ts=1598580574324;turbo=0;user-id=563638041;user-type= :" + name + "!" + name + "@" + name + ".tmi.twitch.tv PRIVMSG #" + name + " :reverse"));
                delay("FakeReverseDelay", Multimorphic.NetProc.EventType.None, 7, new Multimorphic.P3.VoidDelegateNoArgs(SendFakeReverseMessage));
            }
        }

        protected void EnableInvert()
        {
            invertActive  = true;
            invertBitsCollected = 0;

            double currTime = Multimorphic.P3.Tools.Time.GetTime();

            // Cull out attackers that have already activated a blackout and have timed out.
            invertAttackers = invertAttackers.Where(x => !x.Activated || (x.Activated && x.TimeActivated > currTime - timeForInvert)).ToList();

            // Now activate all remaining attackers (if they're already activate, the class code won't reset their timer)
            // Also get a list of names to send to GUI
            List<string> attackerNames = new List<string>();
            foreach (TwitchAttacker attacker in invertAttackers)
            {
                attacker.Activate();
                attackerNames.Add(attacker.Name);
            }

            delay("InvertDelay", Multimorphic.NetProc.EventType.None, timeForInvert, new Multimorphic.P3.VoidDelegateNoArgs(EndInvert));

            PostModeEventToModes(EventNames.InvertFlippers, true);
            PostModeEventToGUI("Evt_EnableInvertFlippers", attackerNames);
            SendPowerUpAckMessage(INVERT_INDEX, GetPlayedByNamesFromList(attackerNames), 1);

            if (!isTwitchAffiliate)
                DisablePowerupsTemporarily();
        }

        protected void EndInvert()
        {
            DisableInvert();
        }

        protected void DisableInvert()
        {
            if (invertActive)
            {
                Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "Disabling invert");
                PostModeEventToModes(EventNames.InvertFlippers, false);
                PostModeEventToGUI("Evt_DisableInvertFlippers", 0);
                invertActive = false;
            }
            cancel_delayed("FakeInvertDelay");
        }

        protected void SendFakeInvertMessage()
        {
            if (powerupsEnabled)
            {
                System.Random rnd = new System.Random();
                List<string> names = new List<string> { "bill", "bob", "john", "mark", "michael", "michael", "michael", "bob", "mark", "sarah", "gerry" };
                string name = names[rnd.Next(0, names.Count)];
                Multimorphic.P3App.Logging.Logger.Log(Multimorphic.P3App.Logging.LogCategories.Game, "Processing fake invert message");
                //ProcessTwitchMsg(new TwitchMessage("@badge-info=;badges=broadcaster/1;client-nonce=25c6d8d227c5f910b5651762605371fe;color=;display-name=multimorphic_p3;emotes=;flags=;id=509b0066-88fa-4148-8271-9a6effdb0e8c;mod=0;room-id=563638041;subscriber=0;tmi-sent-ts=1598580574324;turbo=0;user-id=563638041;user-type= :ccr_fan!ccr_fan@ccr_fan.tmi.twitch.tv PRIVMSG #ccr_fan :emp:1"));
                ProcessTwitchMsg(new TwitchMessage("@badge-info=;badges=broadcaster/1;bits=100;client-nonce=25c6d8d227c5f910b5651762605371fe;color=;display-name=multimorphic_p3;emotes=;flags=;id=509b0066-88fa-4148-8271-9a6effdb0e8c;mod=0;room-id=563638041;subscriber=0;tmi-sent-ts=1598580574324;turbo=0;user-id=563638041;user-type= :" + name + "!" + name + "@" + name + ".tmi.twitch.tv PRIVMSG #" + name + " :invert"));
                delay("FakeInvertDelay", Multimorphic.NetProc.EventType.None, 7, new Multimorphic.P3.VoidDelegateNoArgs(SendFakeInvertMessage));
            }
        }

        protected void PlayEasterEggSound(string key)
        {
		    PostModeEventToGUI("Evt_PlaySound", key);
        }

        private void SendPowerUpAckMessage(int powerupIndex, string playedByName, int destPos)
        {
            string twitchMessage;
            if (powerupIndex == BLACKOUT_INDEX)
                twitchMessage = playedByName + " played a Blackout attack for " + timeForBlackout.ToString() + " seconds.";
            else if (powerupIndex == REVERSE_INDEX)
                twitchMessage = playedByName + " played a Reverse-Flippers attack for " + timeForReverse.ToString() + " seconds.";
            else if (powerupIndex == INVERT_INDEX)
                twitchMessage = playedByName + " played an Invert-Flippers attack for " + timeForInvert.ToString() + " seconds.";
            else
                twitchMessage = playedByName + " played a powerup on player " + destPos.ToString();
            PostModeEventToGUI("Evt_SendTwitchMessage", twitchMessage);
        }

    }
}

