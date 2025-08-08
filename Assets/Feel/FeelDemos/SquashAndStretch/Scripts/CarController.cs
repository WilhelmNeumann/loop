using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Feel;
using MoreMountains.Tools;
using UnityEngine;

namespace Feel.FeelDemos.SquashAndStretch.Scripts
{
    [AddComponentMenu("")]
    public class CarController : MonoBehaviour
    {
        [Header("Car Settings")] public float Speed = 2f;
        public float RotationSpeed = 2f;

        public List<TrailRenderer> Trails;
        public MMFeedbacks TeleportFeedbacks;

        private Vector2 _input;
        private Vector3 _rotationAxis = Vector3.up;

        private Vector3 _thisPosition;
        private Vector3 _newPosition;
        protected float _trailTime = 0f;

        [SerializeField] private MeshRenderer _carRenderer;
        [SerializeField] private Light _light;
        [SerializeField] private GameObject _checkPointPrefab;
        [SerializeField] private AudioSource _audioSource;

        private bool _isFarFromStartEnoughSoThatWeCanEnableFinishTrigger = false;

        public GameObject CheckPoint { get; private set; }
        private Vector3 _previousPosition;

        private GameObject FinishTrigger;

        public bool IsFinished = false;
        
        public int PlayerIndex { get; private set; }

        public enum CarTurnState
        {
            Idling,
            Moving,
            Stopped,
            Crashed
        }

        public Action<CarController> OnCarStateChanged;
        public Action<int> OnFinished;

        private CarTurnState _state;

        public CarTurnState State
        {
            get => _state;
            private set
            {
                Debug.Log($"Car state changed {value}");
                _state = value;
                OnCarStateChanged?.Invoke(this);
            }
        }

        public void Initialize(Color color, int playerIndex, GameObject finishTrigger)
        {
            FinishTrigger = finishTrigger;
            PlayerIndex = playerIndex;

            _light.color = color;
            var carMaterial = Instantiate(_carRenderer.sharedMaterials[0]);
            carMaterial.SetColor("_BaseColor", color); // URP Lit Shader
            carMaterial.SetColor("_Color", color);
            _carRenderer.sharedMaterials[0] = carMaterial;

            CheckPoint = Instantiate(_checkPointPrefab);
            CheckPoint.transform.position = transform.position;
        }

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

        private void OnEnable()
        {
            if (FinishTrigger)
                FinishTrigger.GetComponent<SphereCollider>().enabled =
                    _isFarFromStartEnoughSoThatWeCanEnableFinishTrigger;
            State = CarTurnState.Idling;
        }

        protected virtual void MoveCar()
        {
            transform.Rotate(_rotationAxis, _input.x * Time.deltaTime * RotationSpeed);
            var previousPosition = transform.position;
            transform.Translate(transform.forward * (_input.y * Speed * Time.deltaTime), Space.World);
            var newPosition = transform.position;
            HandleState(previousPosition, newPosition);

            if (!_isFarFromStartEnoughSoThatWeCanEnableFinishTrigger)
            {
                var distanceFromStart = Vector3.Distance(transform.position, FinishTrigger.transform.position);
                if (distanceFromStart > 10)
                {
                    _isFarFromStartEnoughSoThatWeCanEnableFinishTrigger = true;
                }
            }

            if (_isFarFromStartEnoughSoThatWeCanEnableFinishTrigger)
            {
                FinishTrigger.GetComponent<SphereCollider>().enabled = true;
            }
        }

        private void HandleState(Vector3 previousPosition, Vector3 newPosition)
        {
            var isMoving = previousPosition != newPosition;
            if (State == CarTurnState.Idling && isMoving)
            {
                State = CarTurnState.Moving;
                _audioSource.Play();
            }

            if (State == CarTurnState.Moving && !isMoving)
            {
                State = CarTurnState.Stopped;
                _audioSource.Stop();
                CheckPoint.transform.position = transform.position;
            }
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

        private void OnTriggerEnter(Collider other)
        {
            IsFinished = true;
            OnFinished.Invoke(PlayerIndex);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!other.gameObject.CompareTag("Fence")) return;
            // CheckPoint.transform.position = other.contacts[0].point + other.contacts[0].normal * 1.5f;
            TeleportFeedbacks?.PlayFeedbacks();
            transform.position = CheckPoint.transform.position;
            _audioSource.Stop();
            State = CarTurnState.Crashed;
        }
    }
}