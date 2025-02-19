using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UITypeManager : MonoBehaviour
{
    public int UIType; // 1 = UI1, 2 = UI2, etc.
    public CarController2 car;

    public List<Text> driftScoreTexts = new List<Text>();
    public List<Text> multiplierTexts = new List<Text>();
    public List<Slider> bar1List = new List<Slider>();
    public List<Slider> bar2List = new List<Slider>();
    public List<Slider> bar3List = new List<Slider>();
    public List<Slider> bar4List = new List<Slider>();

    public Animator[] carAnimator; // Fixed array declaration

    public Transform[] originTransformFront; // Fixed array declaration
    public Transform[] originTransformRight; // Fixed array declaration
    public Transform[] originTransformLeft; // Fixed array declaration

    private float driftScore = 0f;
    private float progressBar2To4 = 0f;
    private float bar1Progress = 0f;

    public float timeIncreaseRate = 10f;
    public float heartIncreaseRate = 50f;
    public float heartPlusIncrement = 50f;
    public float EnergyIncreaseRate = 15f;
    public float maxBar1 = 100f;
    public float maxBar2 = 500f;
    public float maxBar3 = 1000f;
    public float maxBar4 = 2500f;

    private bool bar2Triggered = false;
    private bool bar3Triggered = false;
    private bool bar4Triggered = false;
    private float wallRaycastDistance = 0.3f;
    private LayerMask wallLayer; // Fixed LayerMask

    private UIHandler uiHandler; // Reference to UIHandler instead of UITypeManager

    private void Start()
    {
        wallLayer = LayerMask.GetMask("wall"); // Fixed LayerMask assignment
        uiHandler = FindObjectOfType<UIHandler>(); // Get UIHandler instance
    }

    private void Update()
    {
        CheckWallRaycasts();
    }
    public void RegisterUI(Text scoreText, Text multiplierText, Slider bar1, Slider bar2, Slider bar3, Slider bar4)
    {
        if (!driftScoreTexts.Contains(scoreText)) driftScoreTexts.Add(scoreText);
        if (!multiplierTexts.Contains(multiplierText)) multiplierTexts.Add(multiplierText);
        if (!bar1List.Contains(bar1)) bar1List.Add(bar1);
        if (!bar2List.Contains(bar2)) bar2List.Add(bar2);
        if (!bar3List.Contains(bar3)) bar3List.Add(bar3);
        if (!bar4List.Contains(bar4)) bar4List.Add(bar4);
    }

    void CheckWallRaycasts()
    {
        bool hitFront = false, hitRight = false, hitLeft = false;

        foreach (Transform front in originTransformFront)
        {
            if (Physics.Raycast(front.position, transform.forward, wallRaycastDistance, wallLayer))
            {
                hitFront = true;
                break;
            }
        }

        foreach (Transform right in originTransformRight)
        {
            if (Physics.Raycast(right.position, transform.forward, wallRaycastDistance, wallLayer))
            {
                hitRight = true;
                break;
            }
        }

        foreach (Transform left in originTransformLeft)
        {
            if (Physics.Raycast(left.position, transform.forward, wallRaycastDistance, wallLayer))
            {
                hitLeft = true;
                break;
            }
        }

        if (hitFront)
            TriggerAnimation("Ivy Hit Front");
        else if (hitRight)
            TriggerAnimation("Ivy Hit Right");
        else if (hitLeft)
            TriggerAnimation("Ivy Hit Left");
    }

    public void UpdateDriftScore(float score)
    {
        driftScore += score;

        foreach (var text in driftScoreTexts)
            text.text = "Drift Score: " + Mathf.RoundToInt(driftScore);

        UpdateBar1();
        UpdateBar2To4();
    }

    void UpdateBar1()
    {
        if (car.isDrifting)
        {
            uiHandler.bar1.value += Time.deltaTime * EnergyIncreaseRate; // Fixed reference to UIHandler
        }

        uiHandler.bar1.value = Mathf.Min(uiHandler.bar1.value + 10f, maxBar1);
    }

    void UpdateBar2To4()
    {
        if (car.isDrifting)
        {
            progressBar2To4 += Time.deltaTime * heartIncreaseRate;
        }

        if (progressBar2To4 >= maxBar2 && !bar2Triggered)
        {
            bar2Triggered = true;
            TriggerAnimation("Ivy Like 0");
        }

        if (progressBar2To4 >= maxBar3 && !bar3Triggered)
        {
            bar3Triggered = true;
            TriggerAnimation("Ivy Like");
        }

        if (progressBar2To4 >= maxBar4 && !bar4Triggered)
        {
            bar4Triggered = true;
            TriggerAnimation("Ivy Like 1");
        }

        if (progressBar2To4 >= maxBar4)
        {
            progressBar2To4 = maxBar4;
            TriggerAnimation("Ivy Like 2");
        }
    }

    public void CollectHeartPlus()
    {
        if (progressBar2To4 < maxBar4)
        {
            progressBar2To4 += heartPlusIncrement;
            UpdateBar2To4();
        }
        else
        {
            TriggerAnimation("Ivy Like 2");
        }
    }

    private void UpdateBarsVisual()
    {
        float progress = progressBar2To4;

        foreach (var bar in bar2List) bar.value = Mathf.Clamp(progress, 0, maxBar2);
        progress -= maxBar2;
        foreach (var bar in bar3List) bar.value = Mathf.Clamp(progress, 0, maxBar3);
        progress -= maxBar3;
        foreach (var bar in bar4List) bar.value = Mathf.Clamp(progress, 0, maxBar4);

        PlayBarAnimation();
    }

    public void HandleCarCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag("TimePlus"))
        {
            foreach (var bar in bar1List)
            {
                bar.value = Mathf.Min(bar.value + 10f, maxBar1);
            }
            collision.gameObject.SetActive(false);
        }

        if (collision.gameObject.CompareTag("HeartPlus"))
        {
            progressBar2To4 = Mathf.Min(progressBar2To4 + 50f, maxBar2 + maxBar3 + maxBar4);
            UpdateBarsVisual();
            TriggerAnimation("HeartCollected");
            collision.gameObject.SetActive(false);
        }
    }

    private void TriggerAnimation(string animTrigger)
    {
        if (carAnimator != null)
        {
            foreach (var animator in carAnimator)
            {
                animator.SetTrigger(animTrigger);
                StartCoroutine(ResetToIdle(animator, animTrigger)); // Start coroutine to reset animation
            }
        }
    }

    // Coroutine to wait for animation to finish and reset to Ivy Idle
    private IEnumerator ResetToIdle(Animator animator, string animTrigger)
    {
        yield return null; // Ensure the next frame starts first

        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);
        while (animState.IsName(animTrigger) && animState.normalizedTime < 1.0f)
        {
            yield return null; // Wait until animation finishes
            animState = animator.GetCurrentAnimatorStateInfo(0);
        }

        animator.SetTrigger("Ivy Idle"); // Reset to Idle after animation finishes
    }


    private void PlayBarAnimation()
    {
        if (progressBar2To4 >= maxBar2 + maxBar3 + maxBar4)
        {
            TriggerAnimation("Ivy Like 2");
            TriggerAnimation("Iris Like 2");
        }
    }

    public void ApplyWallPenalty()
    {
        driftScore = Mathf.Max(0, driftScore - 50f); // Reduce drift score
        progressBar2To4 = Mathf.Max(0, progressBar2To4 - 50f); // Reduce progress bar
        UpdateBarsVisual();
    }
}
