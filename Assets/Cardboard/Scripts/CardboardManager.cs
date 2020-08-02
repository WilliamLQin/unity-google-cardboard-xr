using System;
using UnityEngine;

namespace CardboardXR
{
    public static class CardboardManager
    {
        private static bool initiated;
        private static bool retrieving;
        private static Pose headPoseTemp;

        public static DeviceParams deviceParameter { get; private set; }
        public static RenderTexture viewTextureLeft { get; private set; }
        public static RenderTexture viewTextureRight { get; private set; }
        public static Mesh viewMeshLeft { get; private set; }
        public static Mesh viewMeshRight { get; private set; }
        public static CardboardMesh viewMeshLeftRaw { get; private set; }
        public static CardboardMesh viewMeshRightRaw { get; private set; }
        public static Matrix4x4 projectionMatrixLeft { get; private set; }
        public static Matrix4x4 projectionMatrixRight { get; private set; }
        public static Matrix4x4 eyeFromHeadMatrixLeft { get; private set; }
        public static Matrix4x4 eyeFromHeadMatrixRight { get; private set; }

        public static bool profileAvailable { get; private set; }
        public static bool enableVRView { get; private set; }

        public static event Action deviceParamsChangeEvent;
        public static event Action renderTextureResetEvent;
        public static event Action enableVRViewChangedEvent;

        /// <summary>
        /// Initializes the Cardboard session
        /// </summary>
        public static void InitCardboard()
        {
            if (!initiated)
            {
                #if UNITY_ANDROID
                CardboardAndroidInitialization.InitAndroid();

                #endif

                CardboardHeadTracker.CreateTracker();
                CardboardHeadTracker.ResumeTracker();

                CardboardQrCode.RegisterObserver();
                Application.quitting += () => {
                    CardboardQrCode.DeRegisterObserver();
                };
                initiated = true;
            }

            RefreshParameters();
        }

        /// <summary>
        /// Changes the current device parameters  
        /// i.e. load a preset cardboard profile from url (instead of a QR code)
        /// </summary>
        /// <param name="url"></param>
        public static void SetCardboardProfile(string url)
        {
            CardboardQrCode.SetCardboardProfile(url);
        }

        /// <summary>
        /// Sets the cardboard profile ONLY when a cardboard QR code has not yet been scanned by camera.  
        /// This behaviour is the same as the legacy cardboard gvr_set_default_viewer_profile
        /// </summary>
        /// <param name="url"></param>
        public static void SetCardboardInitialProfile(string url)
        {
            CardboardQrCode.SetCardboardInitialProfile(url);
        }

        /// <summary>
        /// Brings up the QR code scanning intent to load a new profile
        /// </summary>
        public static void ScanQrCode()
        {
            CardboardQrCode.StartScanQrCode();
        }

        /// <summary>
        /// Enables or disables the Cardboard view
        /// </summary>
        /// <param name="shouldEnable"></param>
        public static void SetVRViewEnable(bool shouldEnable)
        {
            enableVRView = shouldEnable;
            enableVRViewChangedEvent?.Invoke();
        }

        /// <summary>
        /// Recenters the camera in the head tracker
        /// </summary>
        /// <param name="horizontalOnly"></param>
        public static void RecenterCamera(bool horizontalOnly = true)
        {
            CardboardHeadTracker.RecenterCamera(horizontalOnly);
        }

        /// <summary>
        /// Gets the current head tracker pose  
        /// If withUpdate is set to false, this method will return the pose from the last update
        /// </summary>
        /// <param name="withUpdate"></param>
        /// <returns></returns>
        public static Pose GetHeadPose(bool withUpdate = false)
        {
            if (withUpdate)
            {
                CardboardHeadTracker.UpdatePose();
            }

            headPoseTemp.position = CardboardHeadTracker.trackerUnityPosition;
            headPoseTemp.rotation = CardboardHeadTracker.trackerUnityRotation;

            return headPoseTemp;
        }

        /// <summary>
        /// Sets the target cardboard eye render textures
        /// </summary>
        /// <param name="newLeft"></param>
        /// <param name="newRight"></param>
        public static void SetRenderTexture(RenderTexture newLeft, RenderTexture newRight)
        {
            if (viewTextureLeft != null)
                viewTextureLeft.Release();
            if (viewTextureRight != null)
                viewTextureRight.Release();

            viewTextureLeft = newLeft;
            viewTextureRight = newRight;

            renderTextureResetEvent?.Invoke();
        }

        /// <summary>
        /// Refreshes the parameters set by the Cardboard profile  
        /// This method should be run whenever the Cardboard profile is updated and when 
        /// the device orientation has changed.
        /// </summary>
        public static void RefreshParameters()
        {
            CardboardQrCode.RetrieveDeviceParam();

            if (retrieving)
                return;

            retrieving = true;

            InitDeviceProfile();
            InitCameraProperties();

            retrieving = false;

            deviceParamsChangeEvent?.Invoke();
        }

        private static void InitDeviceProfile()
        {
            (IntPtr, int) par = CardboardQrCode.GetDeviceParamsPointer();

            if (par.Item2 == 0 && !Application.isEditor)
            {
                profileAvailable = false;
                LoadDefaultProfile();
                par = CardboardQrCode.GetDeviceParamsPointer();
            }

            // if (par.Item2 == 0 && !Application.isEditor)
            // {
            //     CardboardQrCode.RetrieveCardboardDeviceV1Params();
            //     par = CardboardQrCode.GetDeviceParamsPointer();
            // }

            if (par.Item2 > 0 || Application.isEditor)
            {
                deviceParameter = CardboardQrCode.GetDecodedDeviceParams();
                //todo do we need to destroy it before create it?

                CardboardLensDistortion.CreateLensDistortion(par.Item1, par.Item2);
                profileAvailable = true;
            }
        }

        private static void LoadDefaultProfile()
        {
            if (profileAvailable)
                return;

            SetCardboardProfile(CardboardUtility.defaultCardboardUrl);
        }

        private static void InitCameraProperties()
        {
            if (!profileAvailable)
                return;

            CardboardLensDistortion.RetrieveEyeMeshes();
            CardboardLensDistortion.RefreshProjectionMatrix();

            projectionMatrixLeft = CardboardLensDistortion.GetProjectionMatrix(CardboardEye.kLeft);
            projectionMatrixRight = CardboardLensDistortion.GetProjectionMatrix(CardboardEye.kRight);

            eyeFromHeadMatrixLeft = CardboardLensDistortion.GetEyeFromHeadMatrix(CardboardEye.kLeft);
            eyeFromHeadMatrixRight = CardboardLensDistortion.GetEyeFromHeadMatrix(CardboardEye.kRight);

            (CardboardMesh, CardboardMesh) eyeMeshes = CardboardLensDistortion.GetEyeMeshes();
            viewMeshLeftRaw = eyeMeshes.Item1;
            viewMeshRightRaw = eyeMeshes.Item2;

            viewMeshLeft = CardboardUtility.ConvertCardboardMesh_Triangle(eyeMeshes.Item1);
            viewMeshRight = CardboardUtility.ConvertCardboardMesh_Triangle(eyeMeshes.Item2);
        }
    }
}