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

    public enum HapticFeedbackType
    {
        Selection,
        Success,
        Failure,
        Light,
        Medium,
        Heavy,
        Warning
    }

    /// <summary>
    /// Mobile haptic feedback dispatcher.
    /// Provides platform-safe vibration requests on mobile devices while gracefully no-oping on PC/Editor.
    /// </summary>
    public class HapticFeedback : MonoBehaviour
    {
        public static HapticFeedback Instance { get; private set; }

        [SerializeField] private bool hapticsEnabled = true;

        [Header("Telemetry")]
        [SerializeField] private int triggerCount = 0;
        [SerializeField] private HapticType lastTriggered = HapticType.Light;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static void Trigger(HapticFeedbackType type)
        {
            HapticType mapped = type switch
            {
                HapticFeedbackType.Selection => HapticType.Light,
                HapticFeedbackType.Success => HapticType.Success,
                HapticFeedbackType.Failure => HapticType.Failure,
                HapticFeedbackType.Light => HapticType.Light,
                HapticFeedbackType.Medium => HapticType.Medium,
                HapticFeedbackType.Heavy => HapticType.Heavy,
                HapticFeedbackType.Warning => HapticType.Warning,
                _ => HapticType.Light
            };
            Trigger(mapped);
        }

        public static void Trigger(HapticType type)
        {
            if (Instance != null)
            {
                Instance.TriggerHaptic(type);
            }
            else
            {
                #if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                Handheld.Vibrate();
                #endif
            }
        }

        public static void SetHapticsEnabled(bool enabled)
        {
            if (Instance != null)
            {
                Instance.HapticsEnabled = enabled;
            }
        }

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
