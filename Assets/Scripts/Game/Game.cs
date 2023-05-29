using System;
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
    private TMP_Text _scoreText;

    private GameState _gameState;
    private float _score = 0;

    private int _round = 0;

    public TMP_Text _roundText;


    private IEnumerator RoundRoutine(int round)
    {
        _roundText.text = $"Round {round}";
        float t = 0f;
        yield return Tween(0.5f, t =>
        {
            _roundText.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(0f, 1f, t));
        });
        yield return new WaitForSeconds(1f);
        yield return Tween(0.5f, t =>
        {
            _roundText.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, Mathf.SmoothStep(0f, 1f, t));
        });
    }

    private IEnumerator Tween(float duration, Action<float> onStep)
    {
        float t = 0f;
        while (!Mathf.Approximately(t, 1f))
        {
            t = Mathf.Clamp01(t + Time.deltaTime / duration);
            onStep(t);
            yield return null;
        }
    }


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

            EnemyManager.Instance.Reset();
            GridManager.Instance.Reset();
            ItemManager.Instance.Reset();

            this._score = 0;
        });
        _scoreText = GameObject.Find("Canvas/Score").GetComponent<TMP_Text>();
        _endingView.SetActive(false);

        _roundText = GameObject.Find("Canvas/Round").GetComponent<TMP_Text>();
        _roundText.transform.localScale = default;
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
            _roundText.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 40f) * 5f);
            // Do not display the restart view
            _endingView.SetActive(false);
            _scoreText.gameObject.SetActive(true);

            _scoreText.text = $"Your score£º{_score}";

            if (_round != EnemyManager.Instance.CurWave)
            {
                _round = EnemyManager.Instance.CurWave;
                StartCoroutine(RoundRoutine(_round));
            }
        }
        else if (_gameState == GameState.Stop)
        {

        }
        else
        {
            // Display the restart view
            _scoreText.gameObject.SetActive(false);
            _endingView.SetActive(true);
            string endingType = this._gameState == GameState.HappyEnding ? "Happy" : "Bad";
            this._endingHint.text = $"{endingType} Ending!";
            this._endingViewScore.text = $"Your score: {_score}";

            // Ban explosion 
            Weapon.Instance.LastExplodeTime = Time.time;
        }
    }
}
