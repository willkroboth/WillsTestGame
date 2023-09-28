using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Multimorphic.P3App.GUI;
using Multimorphic.P3App.GUI.Menu;
using Multimorphic.P3App.GUI.Selector;

namespace PinballClub.TestGame.GUI
{
    public class TestGameTextEditor : Multimorphic.P3App.GUI.TextEditor
    {
        private GameObject bullet;
        private TextMesh bulletText;
        private float bulletModulation = 0;
        private int bulletIndex = 0;
        const float bulletSpeed = 2f;
        const float bulletFadeFactor = 3.0f;
        private List<string> bulletPoints;
        private bool firstTimeShowingBullets;
        private GameObject splash;
        public TextMesh captionTextMesh;
        public TextMesh bulletTextMesh;

        public void Awake()
        {
            bulletPoints = new List<string>();
        }

        // Use this for initialization
        override public void Start()
        {
            base.Start();

            bullet = gameObject.transform.Find("BulletPoint").gameObject;
            if (bullet)
            {
                bulletText = bullet.GetComponent<TextMesh>();
                splash = bullet.transform.Find("Splash").gameObject;
            }
        }

        protected override void CreateEventHandlers()
        {
            base.CreateEventHandlers();
        }

        // Update is called once per frame
        override public void Update()
        {
            base.Update();

            if (textSelector != null)
            {
                TestGameTextSelector selector = textSelector as TestGameTextSelector;
                if ((selector.dataFromMode != null) && (selector.dataFromMode.Count > 0))
                    SetCaptionAndBulletPoints(selector.dataFromMode);
            }

            if (bulletPoints.Count > 0)
            {
                //						if (bulletPoints.Count > 1) {
                bulletModulation += Time.deltaTime * bulletSpeed;

                if (bulletModulation >= 1.8 * Mathf.PI)
                {
                    bulletModulation = 0;
                    bulletIndex = (bulletIndex + 1) % bulletPoints.Count;
                    if ((bulletIndex) == 0)
                    {
                        firstTimeShowingBullets = false;
                        splash.SetActive(false);
                    }
                }

                if ((bulletModulation == 0) && splash)
                {
                    splash.SetActive(false);
                    splash.SetActive(true);
                }

                if ((bulletModulation == 0) && firstTimeShowingBullets)
                {
                    // Audio.PlaySound3D("NameEditorBulletPoint", gameObject.Position);
                }

                if (bulletText)
                {
                    bulletText.text = bulletPoints[bulletIndex];
                    Color bulletColor = bulletText.color;
                    bulletColor.a = (Mathf.Clamp01(Mathf.Sin(bulletModulation) + 0.7f) * bulletFadeFactor);
                    bulletText.color = bulletColor;
                }

                if (bulletTextMesh)
                {
                    bulletTextMesh.text = bulletPoints[bulletIndex];
                    Color bulletColor = bulletTextMesh.color;
                }
            }
        }

        public void SetCaptionAndBulletPoints(List<string> captionAndBulletPoints)
        {
            if (captionText)
                captionText.text = captionAndBulletPoints[0];
            if (captionTextMesh)
                captionTextMesh.text = captionAndBulletPoints[0];

            bulletPoints.Clear();

            foreach (string s in captionAndBulletPoints)
                bulletPoints.Add(s);

            bulletPoints.RemoveAt(0);
            if (bulletText)
                bulletText.text = "";
            bulletModulation = 0;
            bulletIndex = 0;
            firstTimeShowingBullets = true;
        }
    }
}
