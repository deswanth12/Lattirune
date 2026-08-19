using System;
using UnityEngine;

namespace Lattirune.Core
{
    /// <summary>
    /// Lightweight touch and mouse input controller.
    /// Provides unified pointer events for mobile touch and PC/Editor mouse testing.
    /// </summary>
    public class TouchController : MonoBehaviour
    {
        public static TouchController Instance { get; private set; }

        [SerializeField] private Camera targetCamera;

        public event Action<Vector2, Vector3> OnPointerDown; // (screenPos, worldPos)
        public event Action<Vector2, Vector3> OnPointerDrag; // (screenPos, worldPos)
        public event Action<Vector2, Vector3> OnPointerUp;   // (screenPos, worldPos)

        private bool _isPointerActive;
        private Vector2 _lastScreenPosition;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (targetCamera == null) return;

            // 1. Mobile Touch Input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 screenPos = touch.position;
                Vector3 worldPos = ScreenToWorld(screenPos);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _isPointerActive = true;
                        _lastScreenPosition = screenPos;
                        OnPointerDown?.Invoke(screenPos, worldPos);
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (_isPointerActive)
                        {
                            _lastScreenPosition = screenPos;
                            OnPointerDrag?.Invoke(screenPos, worldPos);
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (_isPointerActive)
                        {
                            _isPointerActive = false;
                            OnPointerUp?.Invoke(screenPos, worldPos);
                        }
                        break;
                }
                return;
            }

            // 2. Mouse Input Fallback (PC/Editor testing)
            if (Input.GetMouseButtonDown(0))
            {
                _isPointerActive = true;
                _lastScreenPosition = Input.mousePosition;
                OnPointerDown?.Invoke(_lastScreenPosition, ScreenToWorld(_lastScreenPosition));
            }
            else if (Input.GetMouseButton(0) && _isPointerActive)
            {
                _lastScreenPosition = Input.mousePosition;
                OnPointerDrag?.Invoke(_lastScreenPosition, ScreenToWorld(_lastScreenPosition));
            }
            else if (Input.GetMouseButtonUp(0) && _isPointerActive)
            {
                _isPointerActive = false;
                _lastScreenPosition = Input.mousePosition;
                OnPointerUp?.Invoke(_lastScreenPosition, ScreenToWorld(_lastScreenPosition));
            }
        }

        public Vector3 ScreenToWorld(Vector2 screenPosition)
        {
            if (targetCamera == null) return Vector3.zero;
            Vector3 screenPos3D = new Vector3(screenPosition.x, screenPosition.y, -targetCamera.transform.position.z);
            Vector3 worldPoint = targetCamera.ScreenToWorldPoint(screenPos3D);
            worldPoint.z = 0f; // Constrain to 2D plane
            return worldPoint;
        }
    }
}
