using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtonController : MonoBehaviour
{
    private Button _startButton;
    private Button _quitButton;

    private AudioClip _buttonClip;
    private AudioSource _audioSource;

    private GameObject _helpView;
    // Start is called before the first frame update
    void Start()
    {
        _buttonClip = Resources.Load<AudioClip>("Audio/Click");
        _audioSource = this.GetComponent<AudioSource>();

        _startButton = GameObject.Find("Canvas/StartGameButton").GetComponent<Button>();
        _startButton.onClick.AddListener(() =>
        {
            _audioSource.PlayOneShot(_buttonClip);
            _helpView.SetActive(true);
        });

        _quitButton = GameObject.Find("Canvas/QuitGameButton").GetComponent<Button>();
        _quitButton.onClick.AddListener(() =>
        {
            _audioSource.PlayOneShot(_buttonClip);
            Application.Quit();
        });

        _helpView = GameObject.Find("Canvas/Help");
        _helpView.SetActive(false);

    }
    private void Update()
    {
        if (_helpView.activeSelf == true && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            SceneManager.LoadScene("MainScene");
        }
    }
}
