using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Camera;
using Feel.FeelDemos.SquashAndStretch.Scripts;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private PlayerEditor _playerEditorPrefab;
    [SerializeField] private Transform _playerEditorContainer;
    [SerializeField] private GameObject _playerEditorMenu;
    [SerializeField] private CarController _carPrefab;

    public static readonly List<Player> Players = new();
    private static readonly int Tint = Shader.PropertyToID("_Tint");

    private int _currentPlayerIndex = 0;

    private void Start()
    {
        AddPlayer();
        AddPlayer();
    }

    public void AddPlayer()
    {
        if (Players.Count >= 5) return;
        var newPlayerEditor = Instantiate(_playerEditorPrefab, _playerEditorContainer);
        var playerCount = Players.Count;
        var playerName = $"Player {playerCount}";
        var playerColor = ColorPicker.GetColor(playerCount);
        var player = new Player(playerName, playerColor, playerCount);
        newPlayerEditor.Initialize(player);
        Players.Add(player);
    }

    public void StartRace()
    {
        if (Players.Count < 2)
        {
            return;
        }

        var startAndFinish = ProceduralTrackGenerator.Instance.GenerateTrack();
        var finishPosition = startAndFinish.transform.position;
        for (var i = 0; i < Players.Count; i++)
        {
            var player = Players[i];
            var car = Instantiate(_carPrefab);
            car.transform.position = finishPosition;
            car.Initialize(player.Color, i, startAndFinish);
            player.Car = car.gameObject.GetComponent<CarController>();
            car.gameObject.SetActive(false);
            car.CheckPoint.SetActive(false);
            car.OnCarStateChanged += OnCarStateChanged;
            car.OnFinished += Finish;
            SetCheckpointColor(player.Color, car.CheckPoint);
        }

        _playerEditorMenu.SetActive(false);

        Prepare();
    }

    private void SetCheckpointColor(Color color, GameObject checkpoint)
    {
        var flag = checkpoint.GetComponentsInChildren<MeshRenderer>().First(x => x.gameObject.name == "Flag");
        var mat = Instantiate(flag.sharedMaterial);
        mat.SetColor(Tint, color);
        flag.sharedMaterial = mat;
    }

    private void Prepare()
    {
        ProceduralTrackGenerator.Instance.trackMeshFilter.GetComponent<MeshRenderer>().enabled = true;
        ProceduralTrackGenerator.Instance.leftFenceMeshFilter.GetComponent<MeshRenderer>().enabled = true;
        ProceduralTrackGenerator.Instance.rightFenceMeshFilter.GetComponent<MeshRenderer>().enabled = true;

        var currentPlayer = Players[_currentPlayerIndex];
        TurnAnnounce.Instance.Announce($"{currentPlayer.Name}'s turn");

        for (var i = 0; i < Players.Count; i++)
        {
            var player = Players[i];
            player.Car.gameObject.SetActive(false);

            var carController = player.Car;
            carController.CheckPoint.SetActive(!carController.IsFinished);
        }

        var currentPlayerCheckpointTransform = currentPlayer.Car.CheckPoint.transform;
        CameraSwitcher.Instance.ActivateFreeCam(currentPlayerCheckpointTransform);
    }

    public void Ready()
    {
        ProceduralTrackGenerator.Instance.trackMeshFilter.GetComponent<MeshRenderer>().enabled = false;
        ProceduralTrackGenerator.Instance.leftFenceMeshFilter.GetComponent<MeshRenderer>().enabled = false;
        ProceduralTrackGenerator.Instance.rightFenceMeshFilter.GetComponent<MeshRenderer>().enabled = false;

        for (var i = 0; i < Players.Count; i++)
        {
            var player = Players[i];
            player.Car.gameObject.SetActive(_currentPlayerIndex == i);
            player.Car.CheckPoint.SetActive(_currentPlayerIndex == i);
        }

        CameraSwitcher.Instance.ActivateRaceCam();
    }

    public void Finish(int playerIndex)
    {
        TurnAnnounce.Instance.Announce(Players[playerIndex].Name + " wins!");

        var playersLeft = PlayersLeft();
        if (playersLeft <= 1)
        {
            StartCoroutine(StartAgain());
        }
        
        _currentPlayerIndex = CalculateNextPlayerIndex();
        Prepare();
    }

    private void OnCarStateChanged(CarController car)
    {
        if (car.State is CarController.CarTurnState.Crashed or CarController.CarTurnState.Stopped)
        {
            _currentPlayerIndex = CalculateNextPlayerIndex();
            Prepare();
        }
    }

    private int CalculateNextPlayerIndex()
    {
        var index = _currentPlayerIndex;
        do
        {
            index = (index + 1) % Players.Count;
        } while (Players[index].Car.IsFinished);

        return index;
    }

    private int GetNumberOfFinishedPlayers() => Players.Count(p => p.Car.IsFinished);
    private int PlayersLeft() => Players.Count - GetNumberOfFinishedPlayers();
    
    private IEnumerator StartAgain()
    {
        Players.ForEach(x =>
        {
            Destroy(x.Car.CheckPoint);
            Destroy(x.Car);
        });
        Players.Clear();

        CameraSwitcher.Instance.DeactivateAll();
        var index = SceneManager.GetActiveScene().buildIndex;

        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(index);
        yield return null;
    }
}