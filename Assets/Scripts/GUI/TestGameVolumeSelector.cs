using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.GUI.Selector;

namespace PinballClub.TestGame.GUI {
		
	public class TestGameVolumeSelector : Multimorphic.P3App.GUI.VolumeSelector {

        public float spacing = 0.1f;
		public Text currentVolumeText;
	
		// Use this for initialization
		public override void Start () {
			base.Start ();
		}

        protected override void OnReceiptOfDataFromMode(object dataFromMode)
        {
            this.Clear(true, false);
            base.OnReceiptOfDataFromMode(dataFromMode);
        }

        // Update is called once per frame
        public override void Update () {
			base.Update ();
			SelectorItem item = CurrentItem();
			if (currentVolumeText && item)
				currentVolumeText.text = Mathf.Round ((float)item.dataObject * 100).ToString ();
		}

		protected override void InitializeSelectorItem(SelectorItem item)
		{
			base.InitializeSelectorItem (item);
			int value = Mathf.RoundToInt((float)item.dataObject * 100);

			// Larger graduations to demark the tenths.
			float rem = (value % 10);
			if (rem == 0f)
				(item as TestGameVolumeSelectorItem).enlarge = true;
		}

		protected override void PrepareDataObjects ()
		{
			base.PrepareDataObjects ();
	
			dataObjectsArePrepared = true;
		}

        protected override void CreateNodes()
        {
            // Assumption: itemCount has been set
            base.CreateNodes();

            int nodeCount = visibleCount;
                
			float x = (float)(visibleCount / -2f * spacing);

			for (int index = 0; index < nodeCount; index++) {
                string nodeName = nodePrefix + index.ToString();
                GameObject node = null;
                Transform nodeTransform = gameObject.transform.Find(nodeName);
                if (nodeTransform != null)
                    node = nodeTransform.gameObject;
                if (node == null)
                    GameObject.Find(nodeName);

                if (node == null)
                {
                    node = new GameObject();
                    node.transform.parent = gameObject.transform;
                    node.name = nodeName;
                }

                node.transform.localPosition = new Vector3(x, 0, 0);
                node.transform.localScale = Vector3.one;
				node.transform.localRotation = Quaternion.identity;

                while (nodes.Count <= index)
				    nodes.Add(null);

                nodes[index] = node;
                x += spacing;
			}
        } 
	} 
}