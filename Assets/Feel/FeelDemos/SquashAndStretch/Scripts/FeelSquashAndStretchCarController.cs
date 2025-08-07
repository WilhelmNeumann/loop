using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Feel;
using MoreMountains.Tools;
using UnityEngine;

namespace Feel.FeelDemos.SquashAndStretch.Scripts
{
    [AddComponentMenu("")]
    public class FeelSquashAndStretchCarController : MonoBehaviour
    {
        [Header("Car Settings")] public float Speed = 2f;
        public float RotationSpeed = 2f;

        public List<TrailRenderer> Trails;
        public MMFeedbacks TeleportFeedbacks;

        protected Vector2 _input;
        protected Vector3 _rotationAxis = Vector3.up;

        protected const string _horizontalAxis = "Horizontal";
        protected const string _verticalAxis = "Vertical";

        protected Vector3 _thisPosition;
        protected Vector3 _newPosition;
        protected float _trailTime = 0f;

        [SerializeField] private MeshRenderer _carRenderer;
        [SerializeField] private Light _light;

        protected virtual void Start()
        {
            TeleportFeedbacks?.Initialization();
            _trailTime = Trails[0].time;
        }

        protected virtual void HandleInput()
        {
            _input = FeelDemosInputHelper.GetDirectionAxis(ref _input);
        }

        protected virtual void Update()
        {
            HandleInput();
            MoveCar();
            HandleBounds();
        }

        protected virtual void MoveCar()
        {
            this.transform.Rotate(_rotationAxis, _input.x * Time.deltaTime * RotationSpeed);
            this.transform.Translate(this.transform.forward * _input.y * Speed * Time.deltaTime, Space.World);
        }

        protected virtual void HandleBounds()
        {
            _newPosition = _thisPosition = this.transform.position;

            if (_newPosition != _thisPosition)
            {
                StartCoroutine(TeleportSequence());
            }
        }

        protected virtual IEnumerator TeleportSequence()
        {
            TeleportFeedbacks?.PlayFeedbacks();
            SetTrails(false);
            yield return MMCoroutine.WaitForFrames(1);
            this.transform.position = _newPosition;
            TeleportFeedbacks?.PlayFeedbacks();
            SetTrails(true);
        }

        protected virtual void SetTrails(bool status)
        {
            foreach (var trail in Trails)
            {
                trail.Clear();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            TeleportFeedbacks?.PlayFeedbacks();
        }

        public void SetColor(Color color)
        {
            _light.color = color;

            var carMaterial = Instantiate(_carRenderer.sharedMaterials[0]);
            carMaterial.SetColor("_BaseColor", color); // URP Lit Shader
            carMaterial.SetColor("_Color", color);
            _carRenderer.sharedMaterials[0] = carMaterial;
        }
    }
}