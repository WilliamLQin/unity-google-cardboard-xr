using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardboardXR
{
    public class CardboardPoseDriver: MonoBehaviour
    {
        public bool UseCardboardHeadTracker = true;

        private void Awake()
        {
            if (Application.isEditor)
                enabled = false;
        }

        private void Update()
        {
            if (UseCardboardHeadTracker && CardboardManager.isCardboardViewOn)
            {
                Pose headPose = CardboardManager.GetHeadPose();
                CardboardHeadTracker.UpdatePose();
                transform.localPosition = headPose.position;
                transform.localRotation = headPose.rotation;
            }
        }
    }
}