using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace CardboardXR
{
    [RequireComponent(typeof(Camera))]
    public class CardboardMainCamera: MonoBehaviour
    {
        [SerializeField]
        private bool defaultEnableCardboardView;

        private Camera mainCamera;
        private Camera leftCamera;
        private Camera rightCamera;

        private RenderTextureDescriptor eyeRenderTextureDesc;

        private const string LEFT_CAMERA_NAME = "CardboardLeftCamera";
        private const string RIGHT_CAMERA_NAME = "CardboardRightCamera";

        private void Awake()
        {
            #if UNITY_IOS
            Application.targetFrameRate = 60;

            #endif

            mainCamera = GetComponent<Camera>();
            leftCamera = SpawnCamera(LEFT_CAMERA_NAME);
            rightCamera = SpawnCamera(RIGHT_CAMERA_NAME);

            SetupRenderTexture();

            CardboardManager.InitCardboard();
            CardboardManager.SetCardboardViewOn(defaultEnableCardboardView);
        }

        private Camera SpawnCamera(string name)
        {
            GameObject cameraGO = new GameObject(name);
            cameraGO.transform.parent = transform;
            Camera cam = cameraGO.AddComponent<Camera>();
            DuplicateCameraSettings(mainCamera, cam);
            cam.allowHDR = false;
            return cam;
        }

        private void Start()
        {
            RefreshCamera();
            CardboardManager.deviceParamsChangedEvent += RefreshCamera;
            OnCardboardEnabledChanged();
            CardboardManager.isCardboardViewOnChangedEvent += OnCardboardEnabledChanged;
        }

        private void OnDestroy()
        {
            CardboardManager.deviceParamsChangedEvent -= RefreshCamera;
            CardboardManager.isCardboardViewOnChangedEvent -= OnCardboardEnabledChanged;
        }

        private void SetupRenderTexture()
        {
            SetupEyeRenderTextureDescription();

            RenderTexture newLeft = new RenderTexture(eyeRenderTextureDesc);
            RenderTexture newRight = new RenderTexture(eyeRenderTextureDesc);
            leftCamera.targetTexture = newLeft;
            rightCamera.targetTexture = newRight;

            CardboardManager.SetRenderTexture(newLeft, newRight);
        }

        private void SetupEyeRenderTextureDescription()
        {
            eyeRenderTextureDesc = new RenderTextureDescriptor()
            {
                dimension = TextureDimension.Tex2D,
                width = Screen.width / 2,
                height = Screen.height,
                depthBufferBits = 16,
                volumeDepth = 1,
                msaaSamples = 1,
                vrUsage = VRTextureUsage.OneEye
            };

            #if UNITY_2019_1_OR_NEWER

            eyeRenderTextureDesc.graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
            Debug.LogFormat("CardboardMainCamera.SetupEyeRenderTextureDescription(), graphicsFormat={0}",
                eyeRenderTextureDesc.graphicsFormat);

            #endif
        }

        private void OnCardboardEnabledChanged()
        {
            bool cardboardViewOn = CardboardManager.isCardboardViewOn;
            mainCamera.enabled = !cardboardViewOn;
            leftCamera.enabled = cardboardViewOn;
            rightCamera.enabled = cardboardViewOn;
            // Post camera is enabled/disabled via CardboardPostCamera script
        }

        private void RefreshCamera()
        {
            if (!CardboardManager.isProfileAvailable)
            {
                return;
            }

            RefreshCamera_Eye(leftCamera,
                CardboardManager.projectionMatrixLeft, CardboardManager.eyeFromHeadMatrixLeft);
            RefreshCamera_Eye(rightCamera,
                CardboardManager.projectionMatrixRight, CardboardManager.eyeFromHeadMatrixRight);

            // Deprecated method of setting eye position
            // if (CardboardManager.deviceParameter != null)
            // {
            //     leftCam.transform.localPosition =
            //         new Vector3(-CardboardManager.deviceParameter.InterLensDistance / 2, 0, 0);
            //     rightCam.transform.localPosition =
            //         new Vector3(CardboardManager.deviceParameter.InterLensDistance / 2, 0, 0);
            // }
        }

        private static void RefreshCamera_Eye(Camera eyeCam, Matrix4x4 projectionMat, Matrix4x4 eyeFromHeadMat)
        {
            if (!projectionMat.Equals(Matrix4x4.zero))
                eyeCam.projectionMatrix = projectionMat;

            //https://github.com/googlevr/cardboard/blob/master/sdk/lens_distortion.cc
            if (!eyeFromHeadMat.Equals(Matrix4x4.zero))
            {
                Pose eyeFromHeadPoseGL = CardboardUtility.GetPoseFromTRSMatrix(eyeFromHeadMat);
                eyeFromHeadPoseGL.position.x = -eyeFromHeadPoseGL.position.x;
                eyeCam.transform.localPosition = eyeFromHeadPoseGL.position;
                eyeCam.transform.localRotation = eyeFromHeadPoseGL.rotation;
            }
        }

        private void LateUpdate()
        {
            DuplicateCameraSettings(mainCamera, leftCamera);
            DuplicateCameraSettings(mainCamera, rightCamera);
        }

        private void DuplicateCameraSettings(Camera sourceCamera, Camera destinationCamera)
        {
            destinationCamera.clearFlags = sourceCamera.clearFlags;
            destinationCamera.backgroundColor = sourceCamera.backgroundColor;
            destinationCamera.cullingMask = sourceCamera.cullingMask;
            destinationCamera.nearClipPlane = sourceCamera.nearClipPlane;
            destinationCamera.farClipPlane = sourceCamera.farClipPlane;
            destinationCamera.depth = sourceCamera.depth;
        }
    }
}