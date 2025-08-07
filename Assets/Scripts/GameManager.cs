using System;
using System.Collections.Generic;
using Camera;
using Feel.FeelDemos.SquashAndStretch.Scripts;
using UI;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private enum GamePhase
    {
    }

    [SerializeField] private PlayerEditor _playerEditorPrefab;
    [SerializeField] private Transform _playerEditorContainer;
    [SerializeField] private GameObject _playerEditorMenu;
    [SerializeField] private FeelSquashAndStretchCarController _carPrefab;

    public static readonly List<Player> Players = new();

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

        for (var i = 0; i < Players.Count; i++)
        {
            var player = Players[i];
            var car = Instantiate(_carPrefab);
            car.transform.position = startAndFinish;
            car.SetColor(player.Color);
            player.Car = car.gameObject;
            car.gameObject.SetActive(false);
        }

        _playerEditorMenu.SetActive(false);
        CameraSwitcher.Instance.ActivateFreeCam(true);
        Debug.Log("start race");
    }

    public void Ready()
    {
        Players[_currentPlayerIndex].Car.gameObject.SetActive(true);
        CameraSwitcher.Instance.ActivateRaceCam(true);
    }
}