using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardboardXR
{
    /// <summary>
    /// Can be placed on any GameObject, but the GameObject should persist  
    /// 
    /// Singleton that exposes necessary static functions to control the cardboard session:  
    ///  - Enable cardboard view  
    ///  - Disable cardboard view
    ///  - Scan QR code  
    /// </summary>
    public class CardboardSession : MonoBehaviour
    {
        [SerializeField]
        private bool defaultEnableCardboardView = false;
        [SerializeField]
        private ScreenOrientation disabledOrientation = ScreenOrientation.AutoRotation;

        private static CardboardSession instance = null;


        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError($"Cannot have more than one instance of {nameof(CardboardSession)} in the scene!");
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }

        private void Start()
        {
            if (defaultEnableCardboardView)
            {
                EnableCardboardView();
            }
        }

        private IEnumerator EnableCardboardViewCoroutine()
        {
            while (Screen.width < Screen.height && Screen.orientation != ScreenOrientation.LandscapeLeft)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                yield return new WaitForEndOfFrame();
            }
            CardboardManager.SetCardboardViewOn(true);
            CardboardManager.RefreshParameters();
        }

        public static void EnableCardboardView()
        {
            if (!CardboardManager.isCardboardViewOn)
            {
                instance.StartCoroutine(instance.EnableCardboardViewCoroutine());
            }
        }

        public static void DisableCardboardView()
        {
            if (CardboardManager.isCardboardViewOn)
            {
                CardboardManager.SetCardboardViewOn(false);
                Screen.orientation = instance.disabledOrientation;
            }
        }

        public static void SetCardboardView(bool viewEnabled)
        {
            if (viewEnabled)
            {
                EnableCardboardView();
            }
            else
            {
                DisableCardboardView();
            }
        }

        public static void StartScanQRCode()
        {
            CardboardManager.ScanQrCode();
        }
    }
}
