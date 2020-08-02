using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardboardXR
{
    public class CardboardPoseDriver: MonoBehaviour
    {
        public bool UseCardboardHeadTracker = true;
        private Transform targetTransform;

        private void Awake()
        {
            targetTransform = GetComponent<Transform>();

            if (Application.isEditor)
                enabled = false;
        }

        private void Update()
        {
            if (UseCardboardHeadTracker)
            {
                CardboardHeadTracker.UpdatePose();
                if (CardboardManager.isCardboardViewOn)
                {
                    targetTransform.localPosition = CardboardHeadTracker.trackerUnityPosition;
                    targetTransform.localRotation = CardboardHeadTracker.trackerUnityRotation;
                }
            }
        }
    }
}