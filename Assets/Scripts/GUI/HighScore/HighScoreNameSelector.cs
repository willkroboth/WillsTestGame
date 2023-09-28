// Copyright � 2019 Multimorphic, Inc. All Rights Reserved

using Multimorphic.P3App.GUI.Selector;

namespace PinballClub.TestGame.GUI {

	public class HighScoreNameSelector : TestGameTextSelector {
		//private readonly List<string> highScoreNameSelectorData;  // Could be any type, depending on what data might come from the mode side to populate the selector.

		// Use this for initialization
		public override void Start() {
			base.Start();
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers();
			// The following event handler is only required if the content of the selector is derived from data that comes from the mode side.
		}

		// Update is called once per frame
		public override void Update() {
			base.Update();
		}

		protected override void PrepareDataObjects() {
			base.PrepareDataObjects();
			/*
						if (highScoreNameSelectorData != null) {
							foreach(string item in highScoreNameSelectorData) {
								string path = Path.GetDirectoryName(item);
								string caption = Path.GetFileName(item);
								AddDataObject(caption, path);
							}
						} */

			// Uncomment this next line if all the items are to be visible all the time.
			// Leave it commented if the visible items are only a subset of a larger list of data objects.
			// visibleCount = dataObjects.Count;

			//			dataObjectsArePrepared = true; // So that the current (or next) Refresh() will use the dataObjects to rebuild the selectable items
		}

		protected override void InitializeSelectorItem(SelectorItem item) {
			base.InitializeSelectorItem(item);
			//			if (item.dataObject)
			//				item.caption = (string) item.dataObject;
		}

		public override void Select() {
			base.Select();
			// Add code here to determine what to do when the user selects an item.
			//			PostGUIEventToModes ("Evt_HighScoreNameSelectorSelect", currentItemIndex);
		}
	}
}