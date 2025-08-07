using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

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
        Vector3 input = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) input += Vector3.forward;
        if (Keyboard.current.sKey.isPressed) input += Vector3.back;
        if (Keyboard.current.aKey.isPressed) input += Vector3.left;
        if (Keyboard.current.dKey.isPressed) input += Vector3.right;

        if (input != Vector3.zero)
        {
            Vector3 move = Quaternion.Euler(0, _camTransform.eulerAngles.y, 0) * input;
            _camTransform.position += move * moveSpeed * Time.deltaTime;
        }
    }

    private void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 forward = _camTransform.forward;
            float zoomDelta = scroll * zoomSpeed * Time.deltaTime;

            Vector3 proposedPosition = _camTransform.position + forward * zoomDelta;
            float distance = Vector3.Distance(proposedPosition, Vector3.zero); // or use ground position if you have one

            if (distance >= minDistance && distance <= maxDistance)
            {
                _camTransform.position = proposedPosition;
            }
        }
    }
}
