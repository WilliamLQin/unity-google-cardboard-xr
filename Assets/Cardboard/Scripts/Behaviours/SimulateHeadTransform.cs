﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardboardXR
{
    public class SimulateHeadTransform: MonoBehaviour
    {
        [SerializeField]
        private Transform targetTransform;
        [SerializeField]
        private float moveSpeed;

        private void Awake()
        {
            if (targetTransform == null)
                targetTransform = GetComponent<Transform>();

            if (!Application.isEditor)
                enabled = false;
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                Vector3 currentEulerAngle = transform.localEulerAngles;
                float targetRotX = currentEulerAngle.x - Input.GetAxis("Mouse Y");
                if (targetRotX < 90 || targetRotX > -90)
                {
                    currentEulerAngle.x = targetRotX;
                }
                float targetRotY = currentEulerAngle.y + Input.GetAxis("Mouse X");
                if (targetRotY > 360)
                    targetRotY -= 360;
                else if (targetRotY < -360)
                    targetRotY += 360;
                currentEulerAngle.y = targetRotY;

                transform.localEulerAngles = currentEulerAngle;
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                Vector3 currentEulerAngle = transform.localEulerAngles;
                float targetRotZ = currentEulerAngle.z - Input.GetAxis("Mouse X");

                currentEulerAngle.z = targetRotZ;
                transform.localEulerAngles = currentEulerAngle;
            }

            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical) * moveSpeed * Time.deltaTime;
            transform.position += transform.TransformVector(movement);
        }
    }
}