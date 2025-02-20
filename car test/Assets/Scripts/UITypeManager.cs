using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITypeManager : MonoBehaviour
{
    public int UIType; // 1 = UI1, 2 = UI2, etc.
    public CarController2 car;
    public Rigidbody carRigidbody;

    // UI Elements Lists
    public List<Text> driftScoreTexts = new List<Text>();
    public List<Text> multiplierTexts = new List<Text>();
    public List<Slider> bar1List = new List<Slider>();
    public List<Slider> bar2List = new List<Slider>();
    public List<Slider> bar3List = new List<Slider>();
    public List<Slider> bar4List = new List<Slider>();

    // Animation
    public Animator[] carAnimators;
    public string fpointLayer = "fpoint";
    public string wallTag = "wall";

    // Progress Tracking
    private float driftScore = 0f;
    private float progressBar2To4 = 0f;
    private float bar1Progress = 0f;
    private float driftMultiplier = 1f;
    private float driftTime = 0f;
    private int currentMultiplier = 1;
    private const float multiplierIncreaseInterval = 2f;

    // Settings
    public float timeIncreaseRate = 10f;
    public float heartIncreaseRate = 50f;
    public float heartPlusIncrement = 50f;
    public float energyIncreaseRate = 15f;
    public float maxBar1 = 100f;
    public float maxBar2 = 500f;
    public float maxBar3 = 1000f;
    public float maxBar4 = 2500f;

    // Collision Handling
    public LayerMask wallLayer;
    public float timePenalty = 5f;
    public float scorePenalty = 50f;
    public float wallCooldown = 5f;
    private float lastWallHitTime = -Mathf.Infinity;

    // Audio
    public AudioClip timeSound;
    public AudioClip heartSound;
    [Range(0, 1)] public float audioVolume = 1f;
    private AudioSource audioSource;

    // State Tracking
    private bool wasDrifting = false;
    private bool bar2Triggered = false;
    private bool bar3Triggered = false;
    private bool bar4Triggered = false;
    private int pendingHits = 0;
    private Coroutine pendingPenaltyCoroutine;
    private float plusScore = 0f;
    private Coroutine plusScoreCoroutine;
    private SpeedDisplay speedDisplay;

    private void Start()
    {
        speedDisplay = FindObjectOfType<SpeedDisplay>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = audioVolume;

        foreach (var bar in bar1List) bar.maxValue = maxBar1;
        foreach (var bar in bar2List) bar.maxValue = maxBar2;
        foreach (var bar in bar3List) bar.maxValue = maxBar3;
        foreach (var bar in bar4List) bar.maxValue = maxBar4;

        ResetAllBars();
    }

    private void Update()
    {
        HandleDriftState();
        UpdateBar1();
        UpdateBar2To4();
    }

    private void HandleDriftState()
    {
        bool isDrifting = car.isDrifting;

        if (!wasDrifting && isDrifting)
        {
            pendingHits = 0;
            if (pendingPenaltyCoroutine != null) StopCoroutine(pendingPenaltyCoroutine);
        }

        if (wasDrifting && !isDrifting)
        {
            ApplyDriftResults();
        }

        wasDrifting = isDrifting;

        if (isDrifting) UpdateDriftScore();
    }

    public void UpdateDriftScore()
    {
        // Only calculate score while drifting
        if (!car.isDrifting) return;

        // Get current car speed from Rigidbody
        float speed = carRigidbody.velocity.magnitude;

        // Calculate base score increment
        float scoreIncrement = Time.deltaTime * speed * driftMultiplier;

        // Apply to total drift score
        driftScore += scoreIncrement;

        // Update multiplier timer
        driftTime += Time.deltaTime;

        // Handle multiplier increases
        if (driftTime >= multiplierIncreaseInterval && driftMultiplier < 5f)
        {
            currentMultiplier++;
            driftMultiplier++;
            driftTime = 0f;
            UpdateAllMultiplierTexts();
        }

        // Update UI elements
        UpdateAllScoreTexts();

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

    private void UpdateBar1()
    {
        if (!car.isDrifting) return;

        bar1Progress += Time.deltaTime * energyIncreaseRate;
        bar1Progress = Mathf.Clamp(bar1Progress, 0, maxBar1);

        foreach (var bar in bar1List)
        {
            bar.value = bar1Progress;
            if (bar.value >= maxBar1)
            {
                bar1Progress = 0;
                speedDisplay.countdownTime += timeIncreaseRate;
            }
        }
    }

    private void UpdateBar2To4()
    {
        if (!car.isDrifting) return;

        progressBar2To4 += Time.deltaTime * heartIncreaseRate;
        progressBar2To4 = Mathf.Clamp(progressBar2To4, 0, maxBar2 + maxBar3 + maxBar4);
        UpdateAllBarsVisual();
    }

    private void UpdateAllBarsVisual(float customProgress = -1)
    {
        float progress = customProgress >= 0 ? customProgress : progressBar2To4;

        foreach (var bar in bar2List) bar.value = Mathf.Clamp(progress, 0, maxBar2);
        foreach (var bar in bar3List) bar.value = Mathf.Clamp(progress - maxBar2, 0, maxBar3);
        foreach (var bar in bar4List) bar.value = Mathf.Clamp(progress - (maxBar2 + maxBar3), 0, maxBar4);

        HandleBarAnimations(progress);
    }

    private void HandleBarAnimations(float progress)
    {
        if (progress >= maxBar2 + maxBar3 + maxBar4)
        {
            TriggerAnimations("Ivy Like 2");
        }
        else if (progress >= maxBar2 + maxBar3)
        {
            TriggerAnimations("Ivy Like 1");
        }
        else if (progress >= maxBar2)
        {
            TriggerAnimations("Ivy Like");
        }
    }

    public void HandleCarCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag("TimePlus"))
        {
            foreach (var bar in bar1List) bar.value = Mathf.Min(bar.value + 10f, maxBar1);
            PlaySound(timeSound);
            StartCoroutine(ReactivateObject(collision.gameObject, 15f));
        }

        if (collision.gameObject.CompareTag("HeartPlus"))
        {
            plusScore += heartPlusIncrement;
            if (plusScoreCoroutine != null) StopCoroutine(plusScoreCoroutine);
            plusScoreCoroutine = StartCoroutine(ApplyDelayedScore());
            PlaySound(heartSound);
            StartCoroutine(ReactivateObject(collision.gameObject, 15f));
        }

        if (collision.gameObject.CompareTag(wallTag))
        {
            ApplyWallPenalty();
        }

        ResetMultiplier();
    }

    private IEnumerator ApplyDelayedScore()
    {
        yield return new WaitForSeconds(1f);
        progressBar2To4 = Mathf.Min(progressBar2To4 + plusScore, maxBar2 + maxBar3 + maxBar4);
        plusScore = 0f;
        UpdateAllBarsVisual();
    }

    public void ApplyWallPenalty()
    {
        if (Time.time < lastWallHitTime + wallCooldown) return;

        lastWallHitTime = Time.time;
        speedDisplay.countdownTime = Mathf.Max(0, speedDisplay.countdownTime - timePenalty);

        if (car.isDrifting)
        {
            bar1Progress = Mathf.Max(0, bar1Progress - scorePenalty);
        }
        else
        {
            pendingHits++;
            if (pendingPenaltyCoroutine != null) StopCoroutine(pendingPenaltyCoroutine);
            pendingPenaltyCoroutine = StartCoroutine(ApplyPendingPenalties());
        }
    }

    private IEnumerator ApplyPendingPenalties()
    {
        yield return new WaitForSeconds(1f);
        driftScore = Mathf.Max(0, driftScore - (pendingHits * scorePenalty));
        progressBar2To4 = Mathf.Max(0, progressBar2To4 - (pendingHits * scorePenalty));
        pendingHits = 0;
        UpdateAllBarsVisual();
        UpdateAllScoreTexts();
    }

    private void ResetAllBars()
    {
        foreach (var bar in bar1List) bar.value = 0;
        foreach (var bar in bar2List) bar.value = 0;
        foreach (var bar in bar3List) bar.value = 0;
        foreach (var bar in bar4List) bar.value = 0;
    }

    private void UpdateAllScoreTexts()
    {
        foreach (var text in driftScoreTexts)
            text.text = "Drift Score: " + Mathf.RoundToInt(driftScore);
    }

    private void UpdateAllMultiplierTexts()
    {
        foreach (var text in multiplierTexts)
            text.text = "x" + currentMultiplier;
    }

    private void ResetMultiplier()
    {
        currentMultiplier = 1;
        driftMultiplier = 1f;
        UpdateAllMultiplierTexts();
    }

    private void TriggerAnimations(string trigger)
    {
        foreach (var animator in carAnimators)
            animator.SetTrigger(trigger);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null) audioSource.PlayOneShot(clip, audioVolume);
    }

    private IEnumerator ReactivateObject(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(true);
    }

    private void ApplyDriftResults()
    {
        progressBar2To4 = Mathf.Min(progressBar2To4 + plusScore, maxBar2 + maxBar3 + maxBar4);
        plusScore = 0f;
        UpdateAllBarsVisual();
    }
    private void OnEnable()
    {
        CarController2.OnWallCollision += HandleWallCollisionUI;
    }

    private void OnDisable()
    {
        CarController2.OnWallCollision -= HandleWallCollisionUI;
    }
    private void HandleWallCollisionUI(Vector3 collisionDirection)
    {
        // Convert physics direction to animation triggers
        if (collisionDirection == Vector3.right)
            TriggerAnimations("Ivy Hit Right");
        else if (collisionDirection == Vector3.left)
            TriggerAnimations("Ivy Hit Left");
        else if (collisionDirection == Vector3.forward)
            TriggerAnimations("Ivy Hit Front");

        ApplyWallPenalty(); // Handle UI-specific penalties
    }
}