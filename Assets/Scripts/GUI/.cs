using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.GUI.Selector;

namespace Multimorphic.P3SA.GUI {

	public class  : Selector {
	
		private List<string> Data;

		// Use this for initialization
		public override void Start () {
			base.Start ();	
		}

		protected override void CreateEventHandlers () 
		{
			base.CreateEventHandlers ();
			// The following event handler is only required if the content of the selector is derived from data that comes from the mode side.
			AddModeEventHandler ("Evt_Data", DataEventHandler);
		}

		// Update is called once per frame
		public override void Update () {
			base.Update ();
		}

		protected override void PrepareDataObjects ()
		{
			base.PrepareDataObjects ();

			foreach(string item in Data) {
				AddDataObject(item);
			}

			// visibleCount = dataObjects.Count;   // Uncomment this line if the number of visible items is not a fixed number

			dataObjectsArePrepared = true; // So that the current (or next) Refresh() will use the dataObjects to rebuild the selectable items
		}

		protected override void InitializeSelectableItem (SelectableItem item)
		{
			base.InitializeSelectableItem (item);
			item.caption = (string) item.dataObject;
		}

		public override void Select ()
		{
			base.Select ();
			// Add code here to determine what to do when the user selects an item.
			PostGUIEventToModes ("Evt_Select", currentItemIndex);
		}

		public void DataEventHandler(string eventName, object eventData) {
			// This event handler is only required if this selector's content is derived from data that comes from the mode side.
			Multimorphic.P3App.Logging.Logger.Log(" receiving  data");
			// Store the data locally so that it can be later used in PrepareDataObjects.
			// Data = (List<string>) eventData;     // for example

			this.Refresh();
		}
	}
}
