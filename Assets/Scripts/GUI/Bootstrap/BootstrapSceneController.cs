using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PinballClub.TestGame.GUI {
	public class BootstrapSceneController : TestGameSceneController {

		private TestGameSetup setup;

        public Text appName;
        public Text appVersion;
        public Text companyName;
        public Text module;
        public Text error;

		// Update is called once per frame
		public override void Update () {
			base.Update ();

			if (setup == null) 
			{
				setup = GameObject.FindObjectOfType<TestGameSetup>();
			}
            else
            {
                if (appName != null)
                    appName.text = setup.appName;
                if (companyName != null)
                    companyName.text = setup.companyName;
                if (error != null)
                    error.text = setup.errorText;
                if (appVersion != null)
                    appVersion.text = setup.appVersion;
                if ((module != null) && isConnectedToP3)
                    module.text = "Module:\n" + setup.playfieldModuleId + "\n" + setup.playfieldModuleDriverVersion;
            }
        }
    }
}
