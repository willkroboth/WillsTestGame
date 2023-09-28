using UnityEngine;
using System.Collections;
using PinballClub.TestGame.Modes;
using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI {
	public class MultiballStatusDisplay : P3Aware {

		public int targetsRemaining = 0;
		public int targetCount = 0;
		public Vector3 tipMaxPosition;
		public Vector3 tipMinPosition;
		public float speed = 5f;
		private GameObject tip;
		private Animator windowAnimator;
		private int chasingState;
		private int openingState;
		private int closingState;
		private int state;

		public Color textColor;
		public Color updatedTextColor;

		private TextMesh ballCountText;
		private int ballCount;
		private TextMesh multiplierText;
		private int multiplier;
		private TextMesh totalText;
		private long total;
		private TextMesh jackpotValueText;
		private long jackpotValue;
		private TextMesh [] targetCountText;

		private long jackpotScore;
		private long totalSaved;

		private float chaserTimer;
		private float jackpotScoreTimer;

		private MeshRenderer totalTitle;
		private MeshRenderer jackpotTitle;

		public int BallCount { get { return(ballCount); } set { ballCount = value; ballCountText.text = ballCount.ToString(); ballCountText.color = updatedTextColor; } }
		public int Multiplier { get { return(multiplier); } set { multiplier = value; multiplierText.text = multiplier.ToString() + "x"; multiplierText.color = updatedTextColor; } }
		public long Total { get { return(total); } set { total = value; totalText.text = total.ToString("N0"); totalText.color = updatedTextColor; } }
		public long JackpotValue { get { return(jackpotValue); } set { jackpotValue = value; jackpotValueText.text = jackpotValue.ToString("N0"); jackpotValueText.color = updatedTextColor; } }

		// Use this for initialization
		public override void Start () {
			base.Start ();
			windowAnimator = GetComponent<Animator>();
			chasingState = Animator.StringToHash("Base Layer.MultiballStatusWindow_TotalTrailing");
			openingState = Animator.StringToHash("Base Layer.MultiballStatusWindow_Start");
			closingState = Animator.StringToHash("Base Layer.MultiballStatusWindow_End");

			tip = gameObject.transform.Find("MultiballProgressStatusBar_mesh/Root/Tip").gameObject;  
			ballCountText = (TextMesh) gameObject.transform.Find("BallCount").gameObject.GetComponent<TextMesh>();  
			multiplierText = (TextMesh) gameObject.transform.Find("Multiplier").gameObject.GetComponent<TextMesh>();  

			totalText = (TextMesh) gameObject.transform.Find("Total").gameObject.GetComponent<TextMesh>();  
			jackpotValueText = (TextMesh) gameObject.transform.Find("JackpotValue").gameObject.GetComponent<TextMesh>();  
			targetCountText = new TextMesh[5];
			for (int i=0; i<5; i++)
			{
				targetCountText[i] = (TextMesh) gameObject.transform.Find("NumAgentsValue" + (i+1).ToString()).gameObject.GetComponent<TextMesh>();
			}
				
			totalTitle = gameObject.transform.Find("TotalTitle_mesh").gameObject.GetComponent<MeshRenderer>();
			jackpotTitle = gameObject.transform.Find("JackpotTitle_mesh").gameObject.GetComponent<MeshRenderer>();

			UpdateText();

			if (targetsRemaining == 0) {
				AllTargetsHit();
			}

		}

		protected override void CreateEventHandlers() {
			base.CreateEventHandlers ();
			AddModeEventHandler("Evt_MultiballJackpot", JackpotEventHandler);
			AddModeEventHandler("Evt_MultiballStatus", StatusEventHandler);
			AddModeEventHandler("Evt_MultiballEnd", MultiballEndEventHandler);
		}

		// Update is called once per frame
		public override void Update () {
			base.Update ();

			state = windowAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
			if ( (state == chasingState) && (!windowAnimator.IsInTransition(0)) )
				windowAnimator.SetBool("LightChasing", false); // so we don't over-chase

			if (Input.GetKeyDown(KeyCode.F9))
				BallCount = BallCount + 1;

			if (jackpotScoreTimer > 0) {
				chaserTimer -= Time.deltaTime;
				jackpotScoreTimer -= Time.deltaTime;
				if (chaserTimer <= 0) {
					StartChaserFlash();
				}
				if (jackpotScoreTimer <= 0) {
					HideJackpotScore();
				}
			}

			UpdateText();

			tip.transform.localPosition = Vector3.Lerp(tip.transform.localPosition, 
			                                           (float) targetsRemaining / (float) targetCount * (tipMaxPosition - tipMinPosition) + tipMinPosition,
			                                           Time.deltaTime * speed);
		}

		private void UpdateText() {
			bool showText = ((state != openingState) && (state != closingState));
			ballCountText.gameObject.SetActive(showText);
			multiplierText.gameObject.SetActive(showText);
			totalText.gameObject.SetActive(showText);
			jackpotValueText.gameObject.SetActive(showText);
			for (int i=0; i<5; i++)
			{
				targetCountText[i].text = ((i+1)*(targetCount / 5)).ToString();
			}
			
			ballCountText.color = Color.Lerp(ballCountText.color, textColor, Time.deltaTime * speed);
			multiplierText.color = Color.Lerp(multiplierText.color, textColor, Time.deltaTime * speed);
			totalText.color = Color.Lerp(totalText.color, textColor, Time.deltaTime * speed);
			jackpotValueText.color = Color.Lerp(jackpotValueText.color, textColor, Time.deltaTime * speed);
		}

		private void JackpotEventHandler(string evtName, object evtData)
		{
			MultiballStatusStruct mbStatus = (MultiballStatusStruct)evtData;
			jackpotScore = mbStatus.jackpotScore;
			ShowJackpotScore();

			targetCount = mbStatus.jackpotsRequired;
			if (mbStatus.jackpotsRemaining <= 0 && targetsRemaining > 0) {
				AllTargetsHit();
			}
			targetsRemaining = mbStatus.jackpotsRemaining;

		}

		public void SetStatus(MultiballStatusStruct status)
		{
			StatusEventHandler("Dummy", status);
		}
		
		private void StatusEventHandler(string evtName, object evtData)
		{
			MultiballStatusStruct mbStatus = (MultiballStatusStruct)evtData;
			if (jackpotScoreTimer <= 0) {
				Total = mbStatus.totalScore;
			}
			else {
				Total = mbStatus.jackpotScore;
			}
			totalSaved = mbStatus.totalScore;
			JackpotValue = mbStatus.jackpotBase;
			Multiplier = mbStatus.multiplier;
			BallCount = mbStatus.ballCount;
			targetCount = mbStatus.jackpotsRequired;
			targetsRemaining = mbStatus.jackpotsRemaining;
		}

		private void MultiballEndEventHandler(string evtName, object evtData)
		{
			CloseWindow();
		}

		private void ShowJackpotScore()
		{
			jackpotScoreTimer = 3.0f;
			Total = jackpotScore;
			StartChaserFlash();
			jackpotTitle.enabled = true;
			totalTitle.enabled = false;
		}

		private void HideJackpotScore()
		{
			jackpotTitle.enabled = false;
			totalTitle.enabled = true;
			Total = totalSaved;
		}

		private void StartChaserFlash()
		{
			chaserTimer = 0.50f;
			ChaserFlash();
		}

		void CloseWindow() {
			windowAnimator.SetBool("Closing", true);
		}

		void ChaserFlash() {
			windowAnimator.SetBool("LightChasing", true);
		}

		void AllTargetsHit() {
			windowAnimator.SetBool("AllTargetsHit", true);
		}
	}
}
