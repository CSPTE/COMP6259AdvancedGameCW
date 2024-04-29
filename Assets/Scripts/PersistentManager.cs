using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class PersistentManager : MonoBehaviour
{
    public static PersistentManager Instance { get; private set; }
    [SerializeField] private string gameMode;
    public string GameMode
    {
        get { return gameMode; }
        set { gameMode = value; }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to the sceneLoaded event
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            // Re-enable the EventSystem and Input System UI Input Module when MainMenu is loaded
            GetComponent<EventSystem>().enabled = true;
            GetComponent<InputSystemUIInputModule>().enabled = true;
        }
    }

    public void InventorySinglePlayer() {
        GameMode = "Single";
        DisableEventSystem();
        SceneManager.LoadScene("Inventory");
    }

    public void InventoryMultiPlayer() {
        GameMode = "Multi";
        DisableEventSystem();
        SceneManager.LoadScene("Inventory");
    }

    private void DisableEventSystem() {
        EventSystem eventSystem = GetComponent<EventSystem>();
        var inputModule = GetComponent<InputSystemUIInputModule>();
        
        if (eventSystem != null) {
            eventSystem.enabled = false;
        }
        if (inputModule != null){
            inputModule.enabled = false;
        }
    }

    public void ExitGame() {
        Debug.Log("Quitting");
        Application.Quit();
    }
}
