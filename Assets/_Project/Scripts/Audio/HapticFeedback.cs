using UnityEngine;

namespace Lattirune.Audio
{
    public enum HapticType
    {
        Light,
        Medium,
        Heavy,
        Success,
        Warning,
        Failure
    }

    /// <summary>
    /// Mobile haptic feedback dispatcher.
    /// Provides platform-safe vibration requests on mobile devices while gracefully no-oping on PC/Editor.
    /// </summary>
    public class HapticFeedback : MonoBehaviour
    {
        [SerializeField] private bool hapticsEnabled = true;

        [Header("Telemetry")]
        [SerializeField] private int triggerCount = 0;
        [SerializeField] private HapticType lastTriggered = HapticType.Light;

        public bool HapticsEnabled
        {
            get => hapticsEnabled;
            set => hapticsEnabled = value;
        }

        public int TriggerCount => triggerCount;
        public HapticType LastTriggered => lastTriggered;

        public void TriggerHaptic(HapticType type)
        {
            if (!hapticsEnabled)
            {
                return;
            }

            triggerCount++;
            lastTriggered = type;

            #if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            // Safe mobile standard vibration dispatch
            Handheld.Vibrate();
            #endif
        }

        public void ResetTelemetry()
        {
            triggerCount = 0;
        }
    }
}
