using UnityEngine;
using System.Collections;
using PinballClub.TestGame.Modes;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {
	public class Inventory : P3Aware {

		private GameObject [] OnIcons;
		private GameObject [] OffIcons;

		// Use this for initialization
		public override void Start () {
			// Find the game objects before Start() is called, just in case start 
			// causes something to happen that actually uses the objects.
			InitIcons();

			base.Start();
		}

		private void InitIcons() {
			OnIcons = new GameObject[HUDMode.ICON_COUNT];

			OffIcons = new GameObject[HUDMode.ICON_COUNT];
		}

		public override void SetupGameItems() {
			base.SetupGameItems ();
			AddModeEventHandler("Evt_HUDInventoryClear", ClearEventHandler);
			AddModeEventHandler("Evt_HUDOnIcon", OnIconEventHandler);
			AddModeEventHandler("Evt_HUDOffIcon", OffIconEventHandler);
		}
		
		// Update is called once per frame
		public override void Update () {
			base.Update ();
		}

		public void ClearEventHandler(string eventName, object eventData) {
			foreach (Renderer item in gameObject.GetComponent<Transform>().GetComponentsInChildren<Renderer>())
			{
				item.enabled = false;
			}
		}

		public void OnIconEventHandler(string eventName, object eventData) {
			SetIconState((int)eventData, true);
		}

        public void OffIconEventHandler(string eventName, object eventData) {
			SetIconState((int)eventData, false);
		}

		private void SetIconState(int index, bool state)
		{
			if (index == -1)
			{
				for (int i=0; i<HUDMode.ICON_COUNT; i++) {
					OnIcons[i].GetComponent<Renderer>().enabled = state;
					OffIcons[i].GetComponent<Renderer>().enabled = !state;
				}
			}
			else {
				OnIcons[index].GetComponent<Renderer>().enabled = state;
				OffIcons[index].GetComponent<Renderer>().enabled = !state;
			}
		}

		protected override void OnDestroy ()
		{
			RemoveModeEventHandler("Evt_HUDInventoryClear", ClearEventHandler);
			RemoveModeEventHandler("Evt_HUDOnIcon", OnIconEventHandler);
			RemoveModeEventHandler("Evt_HUDOffIcon", OffIconEventHandler);
			base.OnDestroy ();
		}
	}
}
