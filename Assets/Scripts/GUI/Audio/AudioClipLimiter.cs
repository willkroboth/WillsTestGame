// Copyright © 2020 Multimorphic, Inc. All Rights Reserved

using System.Collections.Generic;
using UnityEngine;

namespace PinballClub.TestGame.GUI
{

    public class AudioClipLimiter : MonoBehaviour
    {
        public int simultaneousClipCount = 999;

        private List<AudioSource> spawnedSources = new List<AudioSource>();  // known clips

        public bool simultaneousClipCountReached
        {
            get
            {
                if (simultaneousClipCount < 0)
                {
                    return (false);
                }

                int stillPlayingCount = 0;

                for (int i = spawnedSources.Count - 1; i >= 0; i--)
                {
                    AudioSource source = spawnedSources[i];
                    if (source == null)
                    {
                        spawnedSources.RemoveAt(i);  // delete stale references along the way
                    }
                    else if (source.isPlaying)
                    {
                        stillPlayingCount++;
                    }
                }

                return (stillPlayingCount >= simultaneousClipCount);
            }
        }

        public void AddSource(AudioSource source)
        {
            spawnedSources.Add(source);
        }
    }
}