using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    public Animator animator;
    private int randomIndex = 0;
    private int newRandomIndex = 0;

    public void TriggerBloodEffect()
    {
        animator.SetBool("BloodEffect1", false);
        animator.SetBool("BloodEffect2", false);
        animator.SetBool("BloodEffect3", false);
        animator.SetBool("BloodEffect4", false);
        animator.SetBool("BloodEffect5", false);
        animator.SetBool("BloodEffect6", false);
        while(randomIndex == newRandomIndex){
            newRandomIndex = Random.Range(1, 7);
        }
        randomIndex = newRandomIndex;
        switch (randomIndex)
        {
            case 1:
                animator.SetBool("BloodEffect1", true);
                break;
            case 2:
                animator.SetBool("BloodEffect2", true);
                break;
            case 3:
                animator.SetBool("BloodEffect3", true);
                break;
            case 4:
                animator.SetBool("BloodEffect4", true);
                break;
            case 5:
                animator.SetBool("BloodEffect5", true);
                break;
            case 6:
                animator.SetBool("BloodEffect6", true);
                break;
        }
    }

    private void OnEffectFinish() {
        switch (randomIndex)
        {
            case 1:
                animator.SetBool("BloodEffect1", false);
                break;
            case 2:
                animator.SetBool("BloodEffect2", false);
                break;
            case 3:
                animator.SetBool("BloodEffect3", false);
                break;
            case 4:
                animator.SetBool("BloodEffect4", false);
                break;
            case 5:
                animator.SetBool("BloodEffect5", false);
                break;
            case 6:
                animator.SetBool("BloodEffect6", false);
                break;
        }
    }
}
