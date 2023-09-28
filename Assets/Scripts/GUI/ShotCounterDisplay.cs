using UnityEngine;
using System.Collections;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {

	public class ShotCounterDisplay : P3Aware {

		public Color textColor;
		public int count = 0;
		public TextMesh countText;

		// Use this for initialization
		public override void Start () {
			base.Start();
			countText.text = count.ToString();
			countText.gameObject.SetActive(true);
		}

	}
}
