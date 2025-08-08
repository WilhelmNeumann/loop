using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class FreeCamController : MonoBehaviour
    {
        public float moveSpeed = 20f;
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

        private void Update()
        {
            HandleMovement();
            HandleZoom();
        }

        private void HandleMovement()
        {
            var input = Vector3.zero;

            if (Keyboard.current.wKey.isPressed) input += Vector3.forward;
            if (Keyboard.current.sKey.isPressed) input += Vector3.back;
            if (Keyboard.current.aKey.isPressed) input += Vector3.left;
            if (Keyboard.current.dKey.isPressed) input += Vector3.right;

            var move = Quaternion.Euler(0, _camTransform.eulerAngles.y, 0) * input;
            if (input != Vector3.zero)
            {
                _camTransform.position += move * (moveSpeed * Time.deltaTime);
            }
        }

        private void HandleZoom()
        {
            var scroll = Mouse.current.scroll.ReadValue().y;
            if (!(Mathf.Abs(scroll) > 0.01f)) return;
            var forward = _camTransform.forward;
            var zoomDelta = scroll * zoomSpeed * Time.deltaTime;

            var proposedPosition = _camTransform.position + forward * zoomDelta;
            var distance = Vector3.Distance(proposedPosition, Vector3.zero); // or use ground position if you have one

            if (distance >= minDistance && distance <= maxDistance)
            {
                _camTransform.position = proposedPosition;
            }
        }
        
        public void ShowTarget(Transform target, bool keepHeight = true)
        {
            if (target == null) return;

            var camPos   = _camTransform.position;
            var forward  = _camTransform.forward;

            // Vector from camera to target
            var toTarget = target.position - camPos;

            // Remove the component along forward → leaves the sideways (lateral) error
            var lateral  = toTarget - forward * Vector3.Dot(toTarget, forward);

            if (keepHeight)
                lateral.y = 0f; // only slide in XZ like WASD

            // Slide the camera without changing rotation
            _camTransform.position += lateral;
        }

    }
}
