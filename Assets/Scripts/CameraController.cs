using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float targetAspect = 2220f / 1080f;  // Your game's target aspect ratio

    void Start()
    {
        Camera camera = GetComponent<Camera>();
        // Calculate the current window's aspect ratio
        float windowAspect = (float)Screen.width / (float)Screen.height;
        // Calculate the scale height that would make the window aspect match the target aspect
        float scaleHeight = windowAspect / targetAspect;

        // Create a new Rect with the calculated dimensions
        Rect rect = camera.rect;

        if (scaleHeight < 1.0f)
        {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        camera.rect = rect;
    }
}
