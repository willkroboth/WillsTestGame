using UnityEngine;
using System.Collections;

namespace PinballClub.TestGame.GUI
{

    public class VariableAudioClip : MonoBehaviour
    {

        public string clipName;
        [Range(0.1f, 5.0f)]
        public float minPitch = 1f;
        [Range(0.1f, 5.0f)]
        public float maxPitch = 1f;
        [Range(0f, 1f)]
        public float minVolume = 1f;
        [Range(0f, 1f)]
        public float maxVolume = 1f;
        public int weight = 1;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}