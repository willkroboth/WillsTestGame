using UnityEngine;
using System.Collections;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {

	public class ModeSummaryDisplay : P3Aware {

		private TextMesh [] summaryItemTextMeshes;
		private string [] summaryItemTexts = new string[3] {"a", "b", "c"};
		private TextMesh [] summaryValueTextMeshes;
		private string [] summaryValueTexts = new string[3] {"1", "2", "3"};

		private TextMesh [] extraItemTextMeshes;
		private string [] extraItemTexts = new string[2] {"", ""};
		private TextMesh [] extraValueTextMeshes;
		private string [] extraValueTexts = new string[2] {"", ""};

		private MeshRenderer titleMeshRenderer;
		private MeshRenderer modeCompleteMesh;
		private MeshRenderer modeCompleteCheckmark;
		private MeshRenderer modeCompleteXmark;
		private MeshRenderer modeCompleteBox;
		private MeshRenderer multiballMesh;
		private MeshRenderer extraMeshBox;

		private string title;
		bool completed;
		bool useCompletionMesh;
		bool useExtraMesh;

		private Animator windowAnimator;

		// Use this for initialization
		public override void Start () {
			base.Start();

			windowAnimator = GetComponent<Animator>();

			summaryItemTextMeshes = new TextMesh[3];
			summaryValueTextMeshes = new TextMesh[3];
			extraItemTextMeshes = new TextMesh[2];
			extraValueTextMeshes = new TextMesh[2];

			for (int i=0; i<3; i++) {
				summaryItemTextMeshes[i] = (TextMesh) gameObject.transform.Find("SummaryText/TextField" + (i+1).ToString()).gameObject.GetComponent<TextMesh>(); 
				summaryValueTextMeshes[i] = (TextMesh) gameObject.transform.Find("SummaryValues/TextField" + (i+1).ToString()).gameObject.GetComponent<TextMesh>(); 
			}

			for (int i=0; i<2; i++) {
				extraItemTextMeshes[i] = (TextMesh) gameObject.transform.Find("ExtraInfoText/TextField" + (i+1).ToString()).gameObject.GetComponent<TextMesh>(); 
				extraValueTextMeshes[i] = (TextMesh) gameObject.transform.Find("ExtraInfoValues/TextField" + (i+1).ToString()).gameObject.GetComponent<TextMesh>(); 
			}

			modeCompleteMesh = gameObject.transform.Find("Window/ModeSummary_Completion_mesh").gameObject.GetComponent<MeshRenderer>();
			modeCompleteCheckmark = gameObject.transform.Find("Window/ModeSummary_Checkmark_mesh").gameObject.GetComponent<MeshRenderer>();
			modeCompleteXmark = gameObject.transform.Find("Window/ModeSummary_Xmark_mesh").gameObject.GetComponent<MeshRenderer>();
			modeCompleteBox = gameObject.transform.Find("Window/ModeSummary_CompletedBox_mesh").gameObject.GetComponent<MeshRenderer>();
			multiballMesh = gameObject.transform.Find("Title_Text/Multiball_Title").gameObject.GetComponent<MeshRenderer>();
			extraMeshBox = gameObject.transform.Find("Window/ModeSummary_ExtraInfoBox_mesh").gameObject.GetComponent<MeshRenderer>();
		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_CloseModeSummary", CloseModeSummaryEventHandler);
		}
		
		// Update is called once per frame
		public override void Update () {
			base.Update ();

			UpdateText();
		}
		
		public void UpdateText() {
			for (int i=0; i<3; i++) {
				summaryItemTextMeshes[i].text = summaryItemTexts[i];
				summaryValueTextMeshes[i].text = summaryValueTexts[i];
			}

			extraMeshBox.GetComponent<Renderer>().enabled = useExtraMesh;

			for (int i=0; i<2; i++) {
				extraItemTextMeshes[i].text = extraItemTexts[i];
				extraValueTextMeshes[i].text = extraValueTexts[i];
			}

			modeCompleteCheckmark.enabled = useCompletionMesh && completed;
			modeCompleteXmark.enabled = useCompletionMesh && !completed;
			modeCompleteMesh.enabled = useCompletionMesh;
			modeCompleteBox.enabled = useCompletionMesh;
			multiballMesh.GetComponent<Renderer>().enabled = title == "Multiball";

		}

		public void SetItemText(int index, string Description, string Value) {
			summaryItemTexts[index] = Description;
			summaryValueTexts[index] = Value;
		}

		public void EnableTitleMeshRenderer(string mode) {
			titleMeshRenderer = (MeshRenderer) gameObject.transform.Find("Title_Text/" + mode + "_Title").gameObject.GetComponent<MeshRenderer>(); 
			titleMeshRenderer.enabled = true;
		}

		public void SetComplete(bool complete) {
			completed = complete;
			useCompletionMesh = true;
		}

		public void SetExtraItemText(int index, string Description, string value) {
			useExtraMesh = true;
			extraItemTexts[index] = Description;
			extraValueTexts[index] = value;
		}

		public void SetTitle(string name) {
			title = name;
		}

		private void CloseModeSummaryEventHandler(string evtName, object evtData)
		{
			windowAnimator.SetBool("Closing", true);
		}

	}
}
