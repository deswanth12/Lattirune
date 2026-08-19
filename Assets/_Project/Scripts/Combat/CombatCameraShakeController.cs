using System;
using UnityEngine;

namespace Lattirune.Combat
{
    /// <summary>
    /// Smooth procedural screen shake and juice controller for critical hits, boss attacks, and elemental detonations.
    /// Strictly adheres to PLAN.md Section 15 and Section 35.
    /// </summary>
    public class CombatCameraShakeController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float maxTranslation = 12f;
        [SerializeField] private float maxRotation = 2.5f;
        [SerializeField] private float traumaDecay = 2.5f;

        [Header("State")]
        [SerializeField] private float currentTrauma = 0f;
        [SerializeField] private Vector3 currentOffset = Vector3.zero;

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Camera _targetCamera;

        public float CurrentTrauma => currentTrauma;
        public Vector3 CurrentOffset => currentOffset;

        public void Initialize(Camera cam = null, CombatSystem combat = null)
        {
            _targetCamera = cam != null ? cam : Camera.main;
            if (_targetCamera != null)
            {
                _originalPosition = _targetCamera.transform.localPosition;
                _originalRotation = _targetCamera.transform.localRotation;
            }

            if (combat != null)
            {
                combat.OnAttackExecuted += HandleAttackExecuted;
            }
        }

        private void HandleAttackExecuted(DamageResult damage)
        {
            if (damage.IsCritical)
            {
                AddTrauma(0.45f);
            }
            else if (damage.FinalDamage >= 25)
            {
                AddTrauma(0.25f);
            }
        }

        public void AddTrauma(float amount)
        {
            currentTrauma = Mathf.Clamp01(currentTrauma + amount);
        }

        public void Tick(float dt)
        {
            if (currentTrauma <= 0.001f)
            {
                currentTrauma = 0f;
                currentOffset = Vector3.zero;
                if (_targetCamera != null)
                {
                    _targetCamera.transform.localPosition = _originalPosition;
                    _targetCamera.transform.localRotation = _originalRotation;
                }
                return;
            }

            float shake = currentTrauma * currentTrauma;
            float offsetX = maxTranslation * shake * (Mathf.PerlinNoise(Time.time * 25f, 0f) * 2f - 1f);
            float offsetY = maxTranslation * shake * (Mathf.PerlinNoise(0f, Time.time * 25f) * 2f - 1f);
            float rotZ = maxRotation * shake * (Mathf.PerlinNoise(Time.time * 20f, Time.time * 20f) * 2f - 1f);

            currentOffset = new Vector3(offsetX, offsetY, 0f);

            if (_targetCamera != null)
            {
                _targetCamera.transform.localPosition = _originalPosition + currentOffset;
                _targetCamera.transform.localRotation = _originalRotation * Quaternion.Euler(0f, 0f, rotZ);
            }

            currentTrauma = Mathf.Max(0f, currentTrauma - traumaDecay * dt);
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }
    }
}
