using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Parkour
{
    public class PerformanceMetrics : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI scaleText;
        [SerializeField] TextMeshProUGUI frameTimeText;
        [SerializeField] TextMeshProUGUI cpuFrameTimeText;
        [SerializeField] TextMeshProUGUI gpuFrameTimeText;

        float currentScale;
        FrameTiming[] timings = new FrameTiming[1];

        void Update()
        {
            currentScale = DynamicResolutionHandler.instance.GetCurrentScale();
            scaleText.text = (currentScale * 100) + "%";

            frameTimeText.text = (Time.unscaledDeltaTime * 1000.0f) + "ms";

            FrameTimingManager.CaptureFrameTimings();
            FrameTimingManager.GetLatestTimings(1, timings);
            cpuFrameTimeText.text = "CPU: " + timings[0].cpuFrameTime + "ms";
            gpuFrameTimeText.text = "GPU: " + timings[0].gpuFrameTime + "ms";
        }
    }
}