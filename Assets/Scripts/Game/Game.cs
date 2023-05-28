using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Game Instance;
    public enum GameState
    {
        Stop,
        Play,
        HappyEnding,
        BadEnding,
    }
    private GameState _gameState;
    private float Score = 0;

    public void ChangeGameState(GameState newState)
    {
        _gameState = newState;
    }
    public void AddScore(float score)
    {
        Score += score;
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
        }
        else if (_gameState == GameState.Stop)
        {

        }
        else
        {
            // Display the restart view
        }
    }
}
