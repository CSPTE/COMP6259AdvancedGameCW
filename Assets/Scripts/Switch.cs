using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    private PlayerController playerController;
    private AIController aiController;

    void Awake()
    {
        // Get the control scripts from the same GameObject
        playerController = GetComponent<PlayerController>();
        aiController = GetComponent<AIController>();

        // Initial check to set the appropriate control mode
        SetupControlMode();
    }

    void SetupControlMode()
    {
        if (PersistentManager.Instance.GameMode == "Single")
        {
            // Single Player Mode: Enable AI control, disable player control
            if (aiController != null) aiController.enabled = true;
            if (playerController != null) playerController.enabled = false;
        }
        else if (PersistentManager.Instance.GameMode == "Multi")
        {
            // Multiplayer Mode: Disable AI control, enable player control
            if (aiController != null) aiController.enabled = false;
            if (playerController != null) playerController.enabled = true;
        }
    }
}
