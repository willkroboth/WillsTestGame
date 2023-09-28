using UnityEngine;
using System.Collections;
using PinballClub.TestGame.Modes;

namespace PinballClub.TestGame.GUI {

	public class LaneCompletionDisplay : MonoBehaviour {

		private int count;
		private string subtext;

		private TextMesh Count;
		private TextMesh Subtext;

		// Use this for initialization
		public void Start () {
			Count = (TextMesh) gameObject.transform.Find("Offset/CentralInsert/Count").gameObject.GetComponent<TextMesh>(); 
			Subtext = (TextMesh) gameObject.transform.Find("Offset/CentralInsert/Subtext").gameObject.GetComponent<TextMesh>(); 
		}

		public void SetData( LanesCompletedStatus data ) {
			count = data.numCompleted;
			subtext = data.nextGoal;
		}

		public void Update () {
			if (subtext == "")
				Subtext.GetComponent<Renderer>().enabled = false;
			else {
				Subtext.GetComponent<Renderer>().enabled = true;
				Subtext.text = subtext;
			}

			Count.text = count.ToString();

		}

	}
}
