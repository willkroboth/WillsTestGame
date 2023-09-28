using UnityEngine;
using System.Collections;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.GUI.Selector;

namespace PinballClub.TestGame.GUI {

	public class ProfileSelectorItem : SelectorItem {

		// Use this for initialization
		public override void Start () {
			base.Start ();	
		}

		// Update is called once per frame
		public override void Update () {
			base.Update ();
			if (selected) {
				// Change some properties of gameObject or child gameObjects here to indicate that this item is the current item.
			}
		}
	}
}
