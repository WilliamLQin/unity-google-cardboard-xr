using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CardboardXR
{
    public class CardboardUIOverlay: MonoBehaviour
    {

        [Header("View Elements")]
        [SerializeField]
        private Button openCardboardButton;
        [SerializeField]
        private Button scanQRButton;
        [SerializeField]
        private Button closeCardboardButton;
        [SerializeField]
        private GameObject splitLine;

        [Header("QR Overlay")]
        [SerializeField]
        private Text profileParamText;
        [SerializeField]
        private Button continueButton;
        [SerializeField]
        private GameObject continuePanel;

        [Header("Options")]
        public bool useOpenCardboardButton;
        public bool useScanQRButton;
        public bool useCloseCardboardButton;
        public bool useSplitLine;

#region MonoBehaviour Events
        private void Awake()
        {
            continueButton.onClick.AddListener(ContinueClicked);
            scanQRButton.onClick.AddListener(ScanQRCode);
            openCardboardButton.onClick.AddListener(OpenCardboardView);
            closeCardboardButton.onClick.AddListener(CloseCardboardView);
        }

        // Start is called before the first frame update
        void Start()
        {
            OnCardboardEnabledChanged();
            CardboardManager.deviceParamsChangeEvent += OnDeviceParamsChanged;
            CardboardManager.enableVRViewChangedEvent += OnCardboardEnabledChanged;
        }

        private void OnDestroy()
        {
            CardboardManager.deviceParamsChangeEvent -= OnDeviceParamsChanged;
            CardboardManager.enableVRViewChangedEvent -= OnCardboardEnabledChanged;
        }
#endregion

#region Observers
        private void OnCardboardEnabledChanged()
        {
            SetEnableQROverlay(false);
            UpdateUIStatus();
            if (CardboardManager.enableVRView)
                OnDeviceParamsChanged();
        }

        private void OnDeviceParamsChanged()
        {
            if (CardboardManager.enableVRView && !CardboardManager.profileAvailable)
            {
                SetEnableQROverlay(true);
            }

            if (CardboardManager.deviceParameter != null)
            {
                profileParamText.text =
                    CardboardManager.deviceParameter.Vendor + "\r\n" + CardboardManager.deviceParameter.Model;
            }
        }
#endregion

#region Helper functions
        private void UpdateUIStatus()
        {
            bool isVREnabled = CardboardManager.enableVRView;

            scanQRButton.gameObject.SetActive(useScanQRButton && isVREnabled);
            closeCardboardButton.gameObject.SetActive(useCloseCardboardButton && isVREnabled);
            splitLine.SetActive(useSplitLine && isVREnabled);

            // openCardboardButton.gameObject.SetActive(useOpenCardboardButton && !isVREnabled);
            openCardboardButton.gameObject.SetActive(useOpenCardboardButton && true);
        }

        private void SetEnableQROverlay(bool shouldEnable)
        {
            continuePanel.SetActive(shouldEnable);
        }
#endregion

#region Button events
        // Scans QR code and enables Cardboard view
        private void ScanQRCode()
        {
            // Screen.orientation = ScreenOrientation.LandscapeLeft;
            // CardboardManager.SetVRViewEnable(true);
            CardboardManager.ScanQrCode();
            SetEnableQROverlay(true);
        }

        // Switches to Cardboard view
        private void OpenCardboardView()
        {
            StartCoroutine(EnableCardboardViewCoroutine());
        }

        private IEnumerator EnableCardboardViewCoroutine()
        {
            while (Screen.width < Screen.height && Screen.orientation != ScreenOrientation.LandscapeLeft)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                yield return new WaitForEndOfFrame();
            }
            CardboardManager.SetVRViewEnable(true);
            CardboardManager.RefreshParameters();
        }

        private void CloseCardboardView()
        {
            CardboardManager.SetVRViewEnable(false);
            Screen.orientation = ScreenOrientation.AutoRotation;
        }

        private void ContinueClicked()
        {
            SetEnableQROverlay(false);
        }
#endregion
    }
}