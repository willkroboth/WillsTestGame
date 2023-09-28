using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.GUI.Selector;
using UnityEngine.UI;

namespace PinballClub.TestGame.GUI {

	public class ProfileSelector : GridShapedSelector {
	
		private List<string> profileSelectorData = new List<string>();
        public Text titleDisplay;

		// Use this for initialization
		public override void Start () {
			base.Start ();	
		}

        protected override void OnReceiptOfDataFromMode(object dataFromMode)
        {
            base.OnReceiptOfDataFromMode(dataFromMode);

            // Multimorphic.P3App.Logging.Logger.LogWarning("  === ProfileSelector receiving ProfileSelector data " + (dataFromMode as List<string>).Count.ToString());
            // Store the data locally so that it can be later used in PrepareDataObjects.
            if (dataFromMode != null)
            {
                profileSelectorData.Clear();
                profileSelectorData.AddRange((List<string>)dataFromMode);

                if (titleDisplay)
                    titleDisplay.text = profileSelectorData[0];

                profileSelectorData.RemoveAt(0);
            }
            this.Clear(true, true);
        }

        // Update is called once per frame
        public override void Update () {
			base.Update ();

        }

        protected override void PrepareDataObjects ()
		{
			base.PrepareDataObjects ();

			if (profileSelectorData != null) {
				foreach(string item in profileSelectorData) {
   					AddDataObject(item);
				}
			}

			// Uncomment this next line if all the items are to be visible all the time. 
			// Leave it commented if the visible items are only a subset of a larger list of data objects.
			// visibleCount = dataObjects.Count;

			dataObjectsArePrepared = true; // So that the current (or next) Refresh() will use the dataObjects to rebuild the selectable items
		}

		protected override void InitializeSelectorItem (SelectorItem item)
		{
			base.InitializeSelectorItem (item);
			item.caption = (string) item.dataObject;
		}

		public override void Select ()
		{
			base.Select ();

            Multimorphic.P3App.Logging.Logger.LogWarning("  === ProfileSelector sending Evt_ProfileSelectorSelect");
            PostGUIEventToModes("Evt_ProfileSelectorResult", CurrentItem().dataObject);
		}

        public override void Exit()
        {
            PostGUIEventToModes("Evt_ProfileSelectorExit", null);
            // Multimorphic.P3App.Logging.Logger.LogWarning ("  ==== ProfileSelector sending Evt_ProfileSelectorExit");
            base.Exit();
        }

	}
}
