using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DriftScore2 : MonoBehaviour
{
    // Existing variables
    public SpeedDisplay speedDisplay;
    private float driftScore = 0f;
    public CarController2 car;
    public Rigidbody carRigidbody;

    public Text driftScoreText;
    public Text multiplierText;

    public string fpointLayer = "fpoint";
    public string wallTag = "wall";

    public Slider bar1;
    public Slider bar2; // Combined bar for H1, H2, H3

    public float maxBar1 = 100f;
    public float H1 = 500f; // Formerly maxBar2
    public float H2 = 1000f; // Formerly maxBar3
    public float H3 = 2500f; // Formerly maxBar4

    public float progressBar2To4 = 0f;
    public float Plustime = 10f;
    public float EnergyIncreaseRate = 10f;
    public float HeartIncreaseRate = 50f;

    public float timePenalty = 5f;
    public float scorePenalty = 50f;
    public float wallCooldown = 5f;

    public float timePlusIncrement = 10f;
    public float heartPlusIncrement = 50f;

    private float lastWallHitTime = -Mathf.Infinity;
    private float driftMultiplier = 1f;
    private float driftTime = 0f;
    private float multiplierIncreaseInterval = 2f;
    private int currentMultiplier = 1;

    [Header("Wall Detection")]
    public float wallRaycastDistance = 1f;
    public LayerMask wallLayer;
    private float wallRaycastCooldown = 0.1f;
    private float lastWallRaycastTime;

    [Header("UI Configuration")]
    public int currentUIIndex = 0; // Default to the first UI element

    [Header("Victory Configuration")]
    public GameObject[] victoryUIObjects;
    public GameObject[] victory3DObjects;
    public GameObject[] gameWinObjects;
    public bool activateAllVictoryObjects = true;
    public Animator carAnimator;

    private bool h1Triggered = false;
    private bool h2Triggered = false;
    private bool h3Triggered = false;

    private bool wasDrifting = false;
    private bool wasAboveSpeedThreshold = false;

    public AudioClip TimeSound;
    public AudioClip HeartSound;
    [Range(0, 1)] public float bothVolume = 1f;
    public AudioSource audioSource;

    // Existing variables remain the same...
    [Header("HeartPlus Configuration")]
    public float heartPlusDelay = 1f; // Time to wait before applying PlusScore
    private float plusScore = 0f;      // Temporary score buffer
    private Coroutine plusScoreCoroutine; // Reference to our delay coroutine

    [Header("Wall Penalty Configuration")]
    public float penaltyDelay = 1f;
    private int pendingHits = 0;
    private Coroutine pendingPenaltyCoroutine;

    // Add these new variables with the existing ones
    private float currentBarProgress = 0f;
    private float currentDriftScore = 0f;
    private int wallHitsDuringDrift = 0;

    private void Start()
    {
        bar1.maxValue = maxBar1;
        bar2.maxValue = H1 + H2 + H3; // Set bar2's max to total of H1+H2+H3

        bar1.value = 0f;
        bar2.value = 0f;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = bothVolume;

        if (car == null) car = FindObjectOfType<CarController2>();
        if (speedDisplay == null) speedDisplay = FindObjectOfType<SpeedDisplay>();

        if (car == null || speedDisplay == null)
        {
            Debug.LogError("Car or SpeedDisplay references are missing in DriftScore2.");
        }
    }

    void Update()
    {
        bool isCurrentlyDrifting = car.isDrifting;
        bool isCurrentlyAboveSpeedThreshold = car.currentSpeed >= car.driftThresholdSpeed;

        // Handle drift start
        if (!wasDrifting && isCurrentlyDrifting)
        {
            // Transfer any pending hits to drift penalties
            wallHitsDuringDrift += pendingHits;
            pendingHits = 0;

            // Cancel pending penalty application
            if (pendingPenaltyCoroutine != null)
            {
                StopCoroutine(pendingPenaltyCoroutine);
                pendingPenaltyCoroutine = null;
            }
        }

        // Handle drift end
        if (wasDrifting && !isCurrentlyDrifting)
        {
            PlayBarAnimation();
            ApplyDriftResults();
        }

        // Animation triggers
        if (!wasAboveSpeedThreshold && isCurrentlyAboveSpeedThreshold)
        {
            PlayBarAnimation();
        }
        if (!wasDrifting && isCurrentlyDrifting)
        {
            h1Triggered = false;
            h2Triggered = false;
            h3Triggered = false;
        }

        wasDrifting = isCurrentlyDrifting;
        wasAboveSpeedThreshold = isCurrentlyAboveSpeedThreshold;

        // Drift score accumulation
        if (car.isDrifting)
        {
            float speed = carRigidbody.velocity.magnitude;
            currentDriftScore += Time.deltaTime * speed * driftMultiplier;
            driftTime += Time.deltaTime;

            if (driftTime >= multiplierIncreaseInterval && driftMultiplier < 5f)
            {
                currentMultiplier++;
                driftMultiplier++;
                driftTime = 0f;
                UpdateMultiplierText();
            }
        }
        else
        {
            driftTime = 0f;
        }

        UpdateBar1();
        UpdateBar2To4();

        if (Time.time - lastWallRaycastTime > wallRaycastCooldown)
        {
            CheckWallRaycasts();
            lastWallRaycastTime = Time.time;
        }
    }

    private void ApplyDriftResults()
    {
        float totalToAdd = currentBarProgress + plusScore;
        progressBar2To4 = Mathf.Min(progressBar2To4 + totalToAdd, H1 + H2 + H3);
        plusScore = 0f;
        currentBarProgress = 0f;

        // Cancel any pending delayed application
        if (plusScoreCoroutine != null) StopCoroutine(plusScoreCoroutine);

        UpdateBarsVisual();

        // Reset accumulators
        currentDriftScore = 0f;
        currentBarProgress = 0f;
        wallHitsDuringDrift = 0;
    }

    void CheckWallRaycasts()
    {
        if (car == null) return;

        foreach (Transform rayOrigin in car.frontRayOrigins)
        {
            if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, wallRaycastDistance, wallLayer))
            {
                ApplyWallPenalty();
                break;
            }
        }

        foreach (Transform rayOrigin in car.backRayOrigins)
        {
            if (Physics.Raycast(rayOrigin.position, -rayOrigin.forward, wallRaycastDistance, wallLayer))
            {
                ApplyWallPenalty();
                break;
            }
        }
        foreach (Transform rightRay in car.RightRayOrigins)
        {
            if (Physics.Raycast(rightRay.position, rightRay.forward, out RaycastHit hit, wallRaycastDistance, wallLayer))
            {
                ApplyWallPenalty();
                break;
            }
        }
        foreach (Transform leftRay in car.LeftRayOrigins)
        {
            if (Physics.Raycast(leftRay.position, leftRay.forward, out RaycastHit hit, wallRaycastDistance, wallLayer))
            {
                ApplyWallPenalty();
                break;
            }
        }
    }

    public void HandleCarCollision(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(fpointLayer))
        {
            driftScoreText.text = "Final Drift Score: " + Mathf.RoundToInt(driftScore).ToString();
        }

        if (collision.gameObject.CompareTag(wallTag))
        {
            ApplyWallPenalty();
        }

        if (collision.gameObject.CompareTag("TimePlus"))
        {
            bar1.value = Mathf.Min(bar1.value + timePlusIncrement, maxBar1);
            if (bar1.value >= maxBar1)
            {
                bar1.value = 0f;
                if (speedDisplay != null) speedDisplay.countdownTime += timePlusIncrement;
            }

            if (TimeSound != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(TimeSound, bothVolume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(TimeSound, transform.position, bothVolume);
                }
            }

            StartReactivateCoroutine(collision.gameObject, Random.Range(15f, 20f));
            collision.gameObject.SetActive(false);
        }

        if (collision.gameObject.CompareTag("HeartPlus"))
        {
            // Add to temporary buffer instead of direct progress
            plusScore += heartPlusIncrement;

            // Restart the delay timer
            if (plusScoreCoroutine != null) StopCoroutine(plusScoreCoroutine);
            SafeStartCoroutine(ApplyPlusScoreDelayed(), ref plusScoreCoroutine);

            // Visual feedback without changing real progress
            SafeStartCoroutine(TemporaryVisualFeedback(), ref plusScoreCoroutine);

            if (HeartSound != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(HeartSound, bothVolume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(HeartSound, transform.position, bothVolume);
                }
            }

            StartReactivateCoroutine(collision.gameObject, Random.Range(15f, 20f));
            collision.gameObject.SetActive(false);
        }

        currentMultiplier = 1;
        driftTime = 0f;
        UpdateMultiplierText();
    }
    private IEnumerator ApplyPlusScoreDelayed()
    {
        yield return new WaitForSeconds(heartPlusDelay);
        if (!gameObject.activeInHierarchy) yield break;
        // Apply the buffered score to real progress
        progressBar2To4 = Mathf.Min(progressBar2To4 + plusScore, H1 + H2 + H3);
        plusScore = 0f;
        UpdateBarsVisual();
    }
    private IEnumerator TemporaryVisualFeedback()
    {
        if (!gameObject.activeInHierarchy) yield break;
        float originalProgress = progressBar2To4;
        float targetProgress = originalProgress + plusScore;

        while (plusScore > 0)
        {
            // Show temporary progress preview
            float displayProgress = Mathf.Min(originalProgress + plusScore, H1 + H2 + H3);
            UpdateBarsVisual(displayProgress);
            yield return null;
        }

        // Restore actual progress display
        UpdateBarsVisual();
    }
    private void SafeStartCoroutine(IEnumerator routine, ref Coroutine coroutineReference)
    {
        if (gameObject.activeInHierarchy)
        {
            if (coroutineReference != null)
            {
                StopCoroutine(coroutineReference);
            }
            coroutineReference = StartCoroutine(routine);
        }
    }


    private void StartReactivateCoroutine(GameObject obj, float delay)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ReactivateGameObject(obj, delay));
        }
    }

    private IEnumerator ReactivateGameObject(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(true);
    }

    private void UpdateBar1()
    {
        if (!car.isDrifting) return;

        bar1.value += Time.deltaTime * EnergyIncreaseRate;
        if (bar1.value >= maxBar1)
        {
            bar1.value = 0f;
            if (speedDisplay != null)
            {
                speedDisplay.countdownTime += Plustime;
            }
        }
    }

    private void UpdateBar2To4()
    {
        if (!car.isDrifting) return;

        currentBarProgress += Time.deltaTime * HeartIncreaseRate;
        currentBarProgress = Mathf.Min(currentBarProgress,
            (H1 + H2 + H3) - progressBar2To4 - plusScore);

        UpdateBarsVisual();
    }

    private void UpdateBarsVisual(float customProgress = -1)
    {
        float progressToShow = customProgress >= 0 ? customProgress :
                             car.isDrifting ? progressBar2To4 + currentBarProgress :
                             progressBar2To4;

        bar2.value = progressToShow;

        // Reset triggers if below thresholds
        h1Triggered = progressToShow >= H1;
        h2Triggered = progressToShow >= H1 + H2;
        h3Triggered = progressToShow >= H1 + H2 + H3;

        // Trigger animations when thresholds are crossed
        if (progressToShow >= H1 + H2 + H3 && !h3Triggered)
        {
            TriggerAnimation("Ivy Like 2");
            TriggerAnimation("Iris Like 2");
            h3Triggered = true;
        }
        else if (progressToShow >= H1 + H2 && !h2Triggered)
        {
            TriggerAnimation("Ivy Like 1");
            TriggerAnimation("Iris Like 1");
            h2Triggered = true;
        }
        else if (progressToShow >= H1 && !h1Triggered)
        {
            TriggerAnimation("Ivy Like");
            TriggerAnimation("Iris Like");
            h1Triggered = true;
        }
        else if (progressToShow < H1)
        {
            if (h1Triggered || h2Triggered || h3Triggered)
            {
                TriggerAnimation("Ivy Like 0");
                TriggerAnimation("Iris Like 0");
                h1Triggered = false;
                h2Triggered = false;
                h3Triggered = false;
            }
        }
    }

    private void ApplyWallPenalty()
    {
        if (Time.time < lastWallHitTime + wallCooldown) return;

        lastWallHitTime = Time.time;

        // Always apply time penalty immediately
        if (speedDisplay != null)
        {
            speedDisplay.countdownTime = Mathf.Max(0, speedDisplay.countdownTime - timePenalty);
        }

        if (car.isDrifting)
        {
            // Existing drift penalty handling
            wallHitsDuringDrift++;
            bar1.value = Mathf.Max(0, bar1.value - scorePenalty);
        }
        else
        {
            // New delayed penalty system
            pendingHits++;

            // Restart delay timer
            if (pendingPenaltyCoroutine != null) StopCoroutine(pendingPenaltyCoroutine);
            SafeStartCoroutine(ApplyPendingPenalty(), ref pendingPenaltyCoroutine);

            // Show temporary visual feedback
            SafeStartCoroutine(TemporaryPenaltyPreview(), ref pendingPenaltyCoroutine);
        }
    }
    private IEnumerator ApplyPendingPenalty()
    {
        yield return new WaitForSeconds(penaltyDelay);
        if (!gameObject.activeInHierarchy) yield break;

        // Apply accumulated penalties
        int hitsToApply = pendingHits;
        pendingHits = 0;

        driftScore = Mathf.Max(0, driftScore - (hitsToApply * scorePenalty));
        progressBar2To4 = Mathf.Max(0, progressBar2To4 - (hitsToApply * scorePenalty));
        UpdateBarsVisual();

        driftScoreText.text = "Drift Score: " + Mathf.RoundToInt(driftScore).ToString();
    }
    private IEnumerator TemporaryPenaltyPreview()
    {
        if (!gameObject.activeInHierarchy) yield break;
        float originalScore = driftScore;
        float originalProgress = progressBar2To4;

        while (pendingHits > 0)
        {
            // Show preview of penalty
            float previewScore = Mathf.Max(originalScore - (pendingHits * scorePenalty), 0);
            float previewProgress = Mathf.Max(originalProgress - (pendingHits * scorePenalty), 0);

            driftScoreText.text = "Drift Score: " + Mathf.RoundToInt(previewScore).ToString();
            UpdateBarsVisual(previewProgress);

            yield return null;
        }

        // Restore actual values
        driftScoreText.text = "Drift Score: " + Mathf.RoundToInt(driftScore).ToString();
        UpdateBarsVisual();
    }

    private void TriggerAnimation(string animTrigger)
    {
        if (carAnimator != null)
        {
            carAnimator.SetTrigger(animTrigger);
        }
    }

    private void PlayBarAnimation()
    {
        float currentProgress = progressBar2To4 + currentBarProgress;

        if (currentProgress >= H1 + H2 + H3)
        {
            TriggerAnimation("Ivy Like 2");
            TriggerAnimation("Iris Like 2");
        }
        else if (currentProgress >= H1 + H2)
        {
            TriggerAnimation("Ivy Like 1");
            TriggerAnimation("Iris Like 1");
        }
        else if (currentProgress >= H1)
        {
            TriggerAnimation("Ivy Like");
            TriggerAnimation("Iris Like");
        }
        else
        {
            TriggerAnimation("Ivy Like 0");
            TriggerAnimation("Iris Like 0");
        }
    }

    private void UpdateMultiplierText()
    {
        multiplierText.text = "x" + currentMultiplier;
    }

    public void AddPoints(int amount)
    {
        driftScore += amount;
        if (driftScoreText != null)
        {
            driftScoreText.text = "Drift Score: " + Mathf.RoundToInt(driftScore).ToString();
        }
    }
}