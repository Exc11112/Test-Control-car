using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public DriftScore2[] driftScores;    // Array of drift score components
    public SpeedDisplay[] speedDisplays; // Array of speed display components
    public GameObject[] gameOverObjects; // Common defeat objects

    private bool gameEnded;
    private Dictionary<CarController2, float> carIn3DObjectTimes = new Dictionary<CarController2, float>();
    private List<DriftScore2> activeVictoryDrifts = new List<DriftScore2>(); // Tracks active victory conditions

    void Update()
    {
        if (gameEnded) return;

        // 1. Activate 3D objects when bars are maxed (but don't end the game)
        foreach (DriftScore2 driftScore in driftScores)
        {
            float totalBarsMax = driftScore.maxBar2 + driftScore.maxBar3 + driftScore.maxBar4;
            if (driftScore.progressBar2To4 >= totalBarsMax && !activeVictoryDrifts.Contains(driftScore))
            {
                SetObjectsActive(driftScore.victory3DObjects, true); // Activate 3D objects
                activeVictoryDrifts.Add(driftScore); // Track this driftScore for victory checks
            }
        }

        // 2. Check for VICTORY: Car stays in its 3D object for 2 seconds
        foreach (DriftScore2 driftScore in activeVictoryDrifts)
        {
            CarController2 car = driftScore.car;
            if (car == null) continue;

            if (carIn3DObjectTimes.TryGetValue(car, out float entryTime) && Time.time - entryTime >= 2f)
            {
                EndGame(true, driftScore);
                return;
            }
        }

        // 3. Check for DEFEAT: Any speed display's countdown reaches 0
        foreach (SpeedDisplay speedDisplay in speedDisplays)
        {
            if (speedDisplay.countdownTime <= 0)
            {
                EndGame(false, null);
                return;
            }
        }
    }

    public void OnCarEnter3DObject(CarController2 car, DriftScore2 driftScore)
    {
        // Only track if the driftScore is in the active victory list
        if (activeVictoryDrifts.Contains(driftScore) && !carIn3DObjectTimes.ContainsKey(car))
        {
            carIn3DObjectTimes[car] = Time.time;
        }
    }

    public void OnCarExit3DObject(CarController2 car, DriftScore2 driftScore)
    {
        if (carIn3DObjectTimes.ContainsKey(car))
        {
            carIn3DObjectTimes.Remove(car);
        }
    }

    void EndGame(bool isVictory, DriftScore2 winningDriftScore)
    {
        gameEnded = true;
        Time.timeScale = 0f; // Pause the game
        Cursor.lockState = CursorLockMode.None;

        if (isVictory && winningDriftScore != null)
        {
            // Activate victory UI and 3D objects
            SetObjectsActive(winningDriftScore.victoryUIObjects, true);
            SetObjectsActive(winningDriftScore.victory3DObjects, true);
        }
        else
        {
            // Activate common defeat objects
            SetObjectsActive(gameOverObjects, true);
        }

        // Disable all cars and speed displays
        foreach (DriftScore2 driftScore in driftScores)
        {
            if (driftScore.car != null) driftScore.car.enabled = false;
        }
        foreach (SpeedDisplay display in speedDisplays)
        {
            display.enabled = false;
        }
    }

    void SetObjectsActive(GameObject[] objects, bool state)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null) obj.SetActive(state);
        }
    }
}