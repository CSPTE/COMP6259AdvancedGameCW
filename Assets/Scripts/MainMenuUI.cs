using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Button singlePlayerButton;
    public Button multiPlayerButton;
    public Button exitButton;

    private void Start()
    {
        if (PersistentManager.Instance != null)
        {
            SetupButtonListeners();
        }
        else
        {
            Debug.LogWarning("PersistentManager is not initialized yet. Listeners will be set up later.");
        }
    }

    private void SetupButtonListeners() {
        singlePlayerButton.onClick.AddListener(() => PersistentManager.Instance.InventorySinglePlayer());
        multiPlayerButton.onClick.AddListener(() => PersistentManager.Instance.InventoryMultiPlayer());
        exitButton.onClick.AddListener(PersistentManager.Instance.ExitGame);
    }
}
