﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardboardXR
{
    public class CardboardPostCamera: MonoBehaviour
    {
        [SerializeField]
        private Material eyeMaterialLeft;
        [SerializeField]
        private Material eyeMaterialRight;

        private Camera postCam;

        private void Awake()
        {
            postCam = GetComponent<Camera>();
            postCam.projectionMatrix = Matrix4x4.Ortho(-1, 1, -1, 1, -0.1f, 0.5f);
        }

        // Start is called before the first frame update
        private void Start()
        {
            ApplyRenderTexture();
            OnCardboardEnabledChanged();
            CardboardManager.renderTextureResetEvent += ApplyRenderTexture;
            CardboardManager.isCardboardViewOnChangedEvent += OnCardboardEnabledChanged;
        }

        private void OnDestroy()
        {
            CardboardManager.renderTextureResetEvent -= ApplyRenderTexture;
            CardboardManager.isCardboardViewOnChangedEvent -= OnCardboardEnabledChanged;
        }

        private void OnPostRender()
        {
            if (!CardboardManager.isProfileAvailable)
                return;

            eyeMaterialLeft.SetPass(0);
            Graphics.DrawMeshNow(CardboardManager.viewMeshLeft, transform.position, transform.rotation);
            eyeMaterialRight.SetPass(0);
            Graphics.DrawMeshNow(CardboardManager.viewMeshRight, transform.position, transform.rotation);
        }

        private void ApplyRenderTexture()
        {
            eyeMaterialLeft.mainTexture = CardboardManager.viewTextureLeft;
            eyeMaterialRight.mainTexture = CardboardManager.viewTextureRight;
        }

        private void OnCardboardEnabledChanged()
        {
            postCam.enabled = CardboardManager.isCardboardViewOn;
        }
    }
}