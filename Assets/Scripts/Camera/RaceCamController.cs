using System;
using System.Linq;
using Feel.FeelDemos.SquashAndStretch.Scripts;
using Unity.Cinemachine;

namespace Camera
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    [RequireComponent(typeof(CinemachineCamera))]
    public class RaceCamController : MonoBehaviour
    {
        public float zoomSpeed = 100f;
        public float minDistance = 5f;
        public float maxDistance = 100f;

        private CinemachineCamera _cam;
        private Transform _camTransform;
        private float _currentDistance;

        private void Awake()
        {
            _cam = GetComponent<CinemachineCamera>();
            _camTransform = _cam.transform;

            // Estimate starting distance
            _currentDistance = Vector3.Distance(_camTransform.position, Vector3.zero);
        }

        private void OnEnable()
        {
            var cars = FindObjectsByType<CarController>(FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            if (cars.Length == 0)
            {
                Debug.Log("No active cars found");
            }

            _cam.Follow = cars.First().transform;
        }

        private void Update()
        {
            HandleZoom();
        }

        private void HandleZoom()
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 forward = _camTransform.forward;
                float zoomDelta = scroll * zoomSpeed * Time.deltaTime;

                Vector3 proposedPosition = _camTransform.position + forward * zoomDelta;
                float distance =
                    Vector3.Distance(proposedPosition, Vector3.zero); // or use ground position if you have one

                if (distance >= minDistance && distance <= maxDistance)
                {
                    _camTransform.position = proposedPosition;
                }
            }
        }
    }
}