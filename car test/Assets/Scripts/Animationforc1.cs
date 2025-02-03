using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animationforc1 : MonoBehaviour
{
    public CarController2 carcontroller2;
    void HandleAnimation(bool rightHit, bool leftHit, bool frontHit)
    {
        foreach (Animator animator in carcontroller2.carAnimators)
        {
            animator.ResetTrigger("Ivy Hit Right");
            animator.ResetTrigger("Ivy Hit Left");
            animator.ResetTrigger("Ivy Idle");
            animator.ResetTrigger("Ivy Hit Front");

            if (rightHit)
            {
                Debug.Log("Setting Right Trigger");
                animator.SetTrigger("Ivy Hit Right");
            }
            else if (leftHit)
            {
                Debug.Log("Setting Left Trigger");
                animator.SetTrigger("Ivy Hit Left");
            }
            else if (frontHit)
            {
                animator.SetTrigger("Ivy Hit Front");
            }
            else
            {
                Debug.Log("Setting Idle Trigger");
                animator.SetTrigger("Ivy Idle");
            }
        }
    }
}
