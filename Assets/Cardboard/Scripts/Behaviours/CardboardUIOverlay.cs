﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CardboardXR
{
    public class CardboardUIOverlay: MonoBehaviour
    {
        //Only used in dontDestroyAndSingleton
        private static CardboardUIOverlay instance;

        [Header("NoVRViewElements")]
        public Button switchVRButton;

        [Header("VRViewElements")]
        public Button scanQRButton;
        public Button closeButton;
        public GameObject splitLine;

        [Header("QROverlay")]
        public Text profileParamText;
        public Button continueButton;
        public GameObject continuePanel;

        [Header("Options")]
        [SerializeField]
        private bool dontDestroyAndSingleton;

        private bool overlayIsOpen;

        private void Awake()
        {
            if (dontDestroyAndSingleton)
            {
                if (instance == null)
                {
                    DontDestroyOnLoad(gameObject);
                    instance = this;
                }
                else if (instance != this)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            continueButton.onClick.AddListener(ContinueClicked);
            scanQRButton.onClick.AddListener(ScanQRCode);
            switchVRButton.onClick.AddListener(SwitchVRView);
            closeButton.onClick.AddListener(CloseVRView);
        }

        // Start is called before the first frame update
        void Start()
        {
            VRViewChanged();
            CardboardManager.deviceParamsChangeEvent += TriggerRefresh;
            CardboardManager.enableVRViewChangedEvent += VRViewChanged;
        }

        private void OnDestroy()
        {
            CardboardManager.deviceParamsChangeEvent -= TriggerRefresh;
            CardboardManager.enableVRViewChangedEvent -= VRViewChanged;
        }

        // Scans QR code and enables VR view
        private void ScanQRCode()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            CardboardManager.SetVRViewEnable(true);
            CardboardManager.ScanQrCode();
            SetEnableQROverlay(true);
        }

        // Switches CardboardXR view to non-CardboardXR view
        private void SwitchVRView()
        {
            CardboardManager.SetVRViewEnable(!CardboardManager.enableVRView);
        }

        // Resets orientation
        private void CloseVRView()
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
        }

        private void VRViewChanged()
        {
            SetEnableQROverlay(false);
            SetUIStatus(CardboardManager.enableVRView);
        }

        private void SetUIStatus(bool isVREnabled)
        {
            scanQRButton.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
            splitLine.SetActive(isVREnabled);

            switchVRButton.gameObject.SetActive(true);

            if (isVREnabled)
                TriggerRefresh();
        }

        private void SetEnableQROverlay(bool shouldEnable)
        {
            continuePanel.SetActive(shouldEnable);
            overlayIsOpen = shouldEnable;
        }

        private void ContinueClicked()
        {
            TriggerRefresh();

            SetEnableQROverlay(false);
        }

        private void TriggerRefresh()
        {
            //CardboardManager.RefreshParameters();

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
    }
}