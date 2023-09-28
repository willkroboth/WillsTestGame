using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Multimorphic.P3App.GUI;

namespace PinballClub.TestGame.GUI
{
	public class Bush : P3Aware
	{
		public float startAngle;
		public float angle;

		// Use this for initialization
		public override void Start()
		{
			base.Start();
		}

		protected override void CreateEventHandlers()
		{
			base.CreateEventHandlers();
		}

		// Select a random direction for a bird from this bush
		public Vector3 RandomDirection()
		{
			// Point at random angle
			float t = Random.Range(0.0f, angle);
			Vector3 arcVector = Quaternion.AngleAxis(startAngle + t, Vector3.up) * Vector3.forward;

			// Set random vertical velocity
			arcVector.y = Random.Range(0.3f, 1);
			arcVector.Normalize();
			return arcVector;
		}

		// Update is called once per frame
		void Update()
		{
			base.Update();
		}

#if UNITY_EDITOR
        // Visualize and edit range birds may spawn in
        public void OnDrawGizmosSelected()
        {
			Handles.color = new Color(1, 0, 1, 0.2f);

			Vector3 arcVectorMin = Quaternion.AngleAxis(startAngle, Vector3.up) * Vector3.forward;
			Handles.DrawBezier(
				transform.position, transform.position + arcVectorMin * 3,
				transform.position, transform.position + arcVectorMin * 3,
				Color.magenta, null, 7.0f
			);

			Vector3 arcVectorMax = Quaternion.AngleAxis(angle, Vector3.up) * arcVectorMin;
			Handles.DrawBezier(
				transform.position, transform.position + arcVectorMax * 3,
				transform.position, transform.position + arcVectorMax * 3,
				Color.magenta, null, 7.0f
			);

			Handles.DrawSolidArc(transform.position, Vector3.up, arcVectorMin, angle, 3);
        }

        public void OnValidate()
        {
            while(startAngle < 0)
            {
				startAngle += 360;
            }
			while(startAngle > 360)
            {
				startAngle -= 360;
            }

			if(angle < 0)
            {
				angle = 0;
            }
			if(angle > 360)
            {
				angle = 360;
            }
        }
#endif
    }
}
