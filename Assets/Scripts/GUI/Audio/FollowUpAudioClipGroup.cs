using UnityEngine;
using System.Collections;

namespace PinballClub.TestGame.GUI
{
    public enum FollowAudioClipGroupDelayFrom { WhenParentClipStarts, WhenParentClipFinishes };

    public class FollowUpAudioClipGroup : MonoBehaviour {

        public AudioClipGroup nextClipGroup;
        public float delay = 1f;
        /// <summary>
        /// From what point in time should the delay start?  From the beginning or end of the parent clip?
        /// </summary>
        public FollowAudioClipGroupDelayFrom delayFrom = FollowAudioClipGroupDelayFrom.WhenParentClipStarts;

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }
}
