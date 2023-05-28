using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public static Game Instance;
    public enum GameState
    {
        Stop,
        Play,
        HappyEnding,
        BadEnding,
    }

    /// <summary>
    /// Death View
    /// </summary>
    private GameObject _endingView;
    private TMP_Text _endingHint;
    private TMP_Text _endingViewScore;
    private Button _restartButton;
    private Player _player => Player.Instance;


    private GameState _gameState;
    private float _score = 0;

    public void ChangeGameState(GameState newState)
    {
        if (_gameState != newState)
        {
            switch (newState)
            {
                default:
                    break;
            }
            _gameState = newState;
        }
    }
    public void AddScore(float score)
    {
        _score += score;
    }

    private void Start()
    {
        _endingView = GameObject.Find("Canvas/EndingView");
        _endingHint = GameObject.Find("Canvas/EndingView/Hint").GetComponent<TMP_Text>();
        _endingViewScore = GameObject.Find("Canvas/EndingView/Score").GetComponent<TMP_Text>();
        _restartButton = GameObject.Find("Canvas/EndingView/RestartButton").GetComponent<Button>();
        _restartButton.onClick.AddListener(() =>
        {
            ChangeGameState(GameState.Play);
            _player.Revive();
        });

        _endingView.SetActive(false);

    }
    private void Awake()
    {
        _gameState = GameState.Play;
        Instance = this;
    }
    private void Update()
    {
        if (_gameState == GameState.Play)
        {
            // Do not display the restart view
            EnemyManager.Instance.StopAll();
            _endingView.SetActive(false);
        }
        else if (_gameState == GameState.Stop)
        {

        }
        else
        {
            // Display the restart view
            _endingView.SetActive(true);
            string endingType = this._gameState == GameState.HappyEnding ? "Happy" : "Bad";
            this._endingHint.text = $"{endingType} Ending!";
            this._endingViewScore.text = $"Your score: {_score}";
        }
    }
}
