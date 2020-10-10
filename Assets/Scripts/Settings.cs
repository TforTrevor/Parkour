using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

namespace Parkour
{
    public class Settings : MonoBehaviour
    {
        [SerializeField] RenderPipelineAsset[] presetAssets;
        [SerializeField] Volume[] presetSettings;
        [SerializeField] IntEvent resolutionScale;
        [SerializeField] IntVariable fpsTarget;
        [SerializeField] FloatVariable sensitivity;
        [SerializeField] IntEvent fov;
        [SerializeField] Canvas canvas;
        [SerializeField] TMP_InputField resolutionScaleInput;
        [SerializeField] TMP_InputField fpsTargetInput;
        [SerializeField] TMP_InputField fpsCapInput;
        [SerializeField] TextMeshProUGUI sensitivityText;
        [SerializeField] TextMeshProUGUI fovText;

        float timeScale;

        void Start()
        {
            fpsTargetInput.text = fpsTarget.Value.ToString();
            SetResolutionScale("100");
            SetFOV(90);

            canvas.enabled = false;
        }

        public void Show()
        {
            canvas.enabled = true;
            timeScale = Time.timeScale;
            Time.timeScale = 0;
        }

        public void Hide()
        {
            canvas.enabled = false;
            Time.timeScale = timeScale;
        }

        public void SetPreset(int value)
        {
            QualitySettings.renderPipeline = presetAssets[value];

            //HDAdditionalCameraData test = GetComponent<HDAdditionalCameraData>();
            //FrameSettings frameSettings = test.renderingPathCustomFrameSettings;
            //FrameSettingsOverrideMask frameSettingsOverrideMask = test.renderingPathCustomFrameSettingsOverrideMask;
            //test.customRenderingSettings = true;
            //frameSettingsOverrideMask.
            //https://forum.unity.com/threads/hdrp-change-custom-frame-settings-through-scripts.749171/
        }

        public void SetResolutionScale(string value)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                result = Mathf.Clamp(result, 10, 100);
                resolutionScaleInput.text = result.ToString();
                resolutionScale.Raise(result);
            }
            else
            {
                resolutionScale.Raise(-1);
            }
        }

        public void SetFPSTarget(string value)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                if (QualitySettings.vSyncCount > 0)
                {
                    result = Screen.currentResolution.refreshRate;
                    fpsTargetInput.text = result.ToString();
                }
                else if (result > Application.targetFrameRate && Application.targetFrameRate > 0)
                {
                    result = Application.targetFrameRate;
                    fpsTargetInput.text = result.ToString();
                }
                fpsTarget.Value = result;
            }            
        }

        public void SetSensitivity(float value)
        {
            sensitivity.Value = value;
            sensitivityText.text = value.ToString("F2");
        }

        public void SetFPSCap(string value)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                Application.targetFrameRate = result;
                if (fpsTarget.Value > result)
                {
                    fpsTarget.Value = result;
                    fpsTargetInput.text = result.ToString();
                }
            }
        }

        public void SetVsync(int value)
        {
            switch (value)
            {
                case 0:
                    QualitySettings.vSyncCount = 0;
                    QualitySettings.maxQueuedFrames = 0;
                    break;
                case 1:
                    QualitySettings.vSyncCount = 1;
                    QualitySettings.maxQueuedFrames = 1;
                    break;
                case 2:
                    QualitySettings.vSyncCount = 1;
                    QualitySettings.maxQueuedFrames = 2;
                    break;
            }
        }

        public void SetFOV(float value)
        {
            fov.Raise((int)value);
            fovText.text = value.ToString();
        }

        public void Quit()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}