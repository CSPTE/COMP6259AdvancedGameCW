using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CooldownController : MonoBehaviour
{
    public Image cooldownImage;
    public float cooldownTime = 5f;

    void Start()
    {
        cooldownImage.fillAmount = 1;
    }

    public void StartCooldown()
    {
        cooldownImage.fillAmount = 0;
        StartCoroutine(CooldownAnimation(cooldownTime));
    }

    private IEnumerator CooldownAnimation(float cooldownTime)
    {
        float elapsedTime = 0;
        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime;
            cooldownImage.fillAmount = elapsedTime / cooldownTime;
            yield return null;
        }
        cooldownImage.fillAmount = 1; // Reset to full visibility
    }
}
