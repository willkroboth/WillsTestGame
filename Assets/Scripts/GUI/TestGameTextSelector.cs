using UnityEngine;
using System.Collections.Generic;
using Multimorphic.P3App.GUI.Selector;

namespace PinballClub.TestGame.GUI
{
	public class TestGameTextSelector : TextSelector {

        public List<string> dataFromMode;

        protected override void OnReceiptOfDataFromMode(object dataFromMode)
        {
            value = "";
            this.Clear();
 
            base.OnReceiptOfDataFromMode(dataFromMode);
            this.dataFromMode = dataFromMode as List<string>;
        }

        protected override void PrepareDataObjects ()
		{
			Multimorphic.P3App.Logging.Logger.LogWarning ("====== Preparing TextSelector dataobjects.");
			base.PrepareDataObjects ();
		}

		/// <summary>
		/// Creates the nodes.  In this descendant class, it is assumed that the ancestor created the nodes in a single horizontal row.
		/// In this method, those nodes will now be scaled so that the nodes further from the center are smaller.
		/// </summary>
		protected override void CreateNodes () {
			base.CreateNodes();
			// At this point, the ancestor has created the nodes, assumed to be centered around x=0.

            const float SCALE_FACTOR = 0.80f; 

			float mid = Mathf.Round(nodes.Count / 2f) - 1f;
			int midIndex = Mathf.RoundToInt (nodes.Count / 2);

			for (int i=0; i < nodes.Count; i++) {
				float factor = Mathf.Pow(SCALE_FACTOR, Mathf.Abs(mid - i)); // Further from the center -> smaller
//                if (i != midIndex)
                    nodes[i].transform.localScale = nodes[midIndex].transform.localScale * factor;
                //else
                //{
                //    nodes[i].transform.localPosition = new Vector3(0, 60, 0);  // Raise the middle (selected) node to the cursor position
                //    nodes[i].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);  // Raise the middle (selected) node to the cursor position
                //}

                //	float displacement = Mathf.Abs(mid - i) / mid * -0.5f - 0.5f;
                //	if (i != Mathf.RoundToInt(nodes.Count / 2) )
                //		nodes[i].transform.Translate(0, displacement, 0);
            }
//			nodes[Mathf.RoundToInt(nodes.Count / 2)].transform.Translate(0, 1f, 0);
		}

        protected override void OnDisable()
        {
            base.OnDisable();
            dataFromMode = null;
        }
    }
}
