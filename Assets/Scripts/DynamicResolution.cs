using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Parkour
{
    public class DynamicResolution : MonoBehaviour
    {
        [SerializeField] IntVariable frameRateTarget;
        [SerializeField] float secondsToNextChange = 1.0f;
        [SerializeField] float fractionDeltaStep = 0.1f;

        float currentScale = 1.0f;

        // Since this call uses DynamicResScalePolicyType.ReturnsMinMaxLerpFactor, HDRP uses currentScale in the following context:
        // finalScreenPercentage = Mathf.Lerp(minScreenPercentage, maxScreenPercentage, currentScale);
        float SetDynamicResolutionScale()
        {
            float frameTimeTarget = 1f / frameRateTarget.Value;

            if (Time.unscaledDeltaTime > frameTimeTarget + (0.2f * frameTimeTarget))
            {
                currentScale -= 0.01f;
                currentScale = Mathf.Clamp01(currentScale);
            }
            else
            {
                currentScale += 0.01f;
                currentScale = Mathf.Clamp01(currentScale);
            }

            return currentScale;
        }

        float SetResolutionScale()
        {
            return currentScale;
        }

        public void SetResolutionScale(int value)
        {
            if (value > 0)
            {
                DynamicResolutionHandler.SetDynamicResScaler(SetResolutionScale, DynamicResScalePolicyType.ReturnsPercentage);
                currentScale = value;
            }
            else
            {
                DynamicResolutionHandler.SetDynamicResScaler(SetDynamicResolutionScale, DynamicResScalePolicyType.ReturnsMinMaxLerpFactor);
            }
        }

        void Start()
        {
            // Binds the dynamic resolution policy defined above.
            DynamicResolutionHandler.SetDynamicResScaler(SetDynamicResolutionScale, DynamicResScalePolicyType.ReturnsMinMaxLerpFactor);
        }
    }
}