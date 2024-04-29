using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    private bool paused = false;

    public GameObject DeathMenu;
    public GameObject PauseMenu;
    public GameObject Tutorial;
    public GameObject Tutorial2;
    public AudioSource buttonAudio;

    void Start()
    {
        GameManager.instance.OnPlayerDeath.AddListener(PlayerDied);
        GameManager.instance.OnFirstFinisher.AddListener(EnableTutorial2);
        Pause();
        Tutorial.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (paused) {
                UnPause();
            } else {
                Pause();
            }
        }
    }

    public void EnableTutorial2() {
        Pause();
        Tutorial2.SetActive(true);
    }

    public void CloseTutorial2(){
        UnPause();
        Tutorial2.SetActive(false);
    }

    public void CloseTutorial(){
        UnPause();
        Tutorial.SetActive(false);
    }

    public void Pause() {
        paused = true;
        Time.timeScale = 0f;
        PauseMenu.SetActive(true);
    }

    public void UnPause() {
        paused = false;
        Time.timeScale = 1f;
        PauseMenu.SetActive(false);
        buttonAudio.Play();
    }

    public void PlayerDied() {
        DeathMenu.SetActive(true);
    }

    public void MainMenu() {
        UnPause();
        SceneManager.LoadScene("MainMenu");
    }

    public void Inventory() {
        UnPause();
        SceneManager.LoadScene("Inventory");
    }
}
