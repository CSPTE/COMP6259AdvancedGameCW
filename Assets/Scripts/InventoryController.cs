using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;

    public GameObject NoWeapon;
    public GameObject NoAbility;
    public GameObject Tutorial;
    public AudioSource buttonAudio;

    private float sceneTransitionCooldown = 1.0f; // Cooldown time in seconds
    private float lastTransitionTime = -1.0f; // Time since last transition

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start(){
        Tutorial.SetActive(true);
    }

    public void CloseTutorial(){
        Tutorial.SetActive(false);
        buttonAudio.Play();
    }

    public void WeaponNotUnlocked(){
        if (!NoWeapon.activeSelf){
            NoWeapon.SetActive(true);
            if (NoAbility.activeSelf)
            {
                NoAbility.SetActive(false);
            }
        }
    }

    public void CloseWeaponNotUnlocked(){
        NoWeapon.SetActive(false);
        buttonAudio.Play();
    }

    public void AbilityNotUnlocked(){
        if (!NoAbility.activeSelf)
        {
            // Activate NoAbility and deactivate NoWeapon if it's active
            NoAbility.SetActive(true);
            if (NoWeapon.activeSelf)
            {
                NoWeapon.SetActive(false);
            }
        }
    }

    public void CloseAbilityNotUnlocked(){
        NoAbility.SetActive(false);
        buttonAudio.Play();
    }

    public void LoadSample(){
        if (CanTransitionScene())
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    private bool CanTransitionScene()
    {
        // Check if enough time has elapsed since the last transition
        if (Time.time >= lastTransitionTime + sceneTransitionCooldown)
        {
            lastTransitionTime = Time.time; // Reset the last transition time
            return true;
        }
        return false;
    }
}
