using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public DriftScore2[] driftScores;    // Array of drift score components
    public SpeedDisplay[] speedDisplays; // Array of speed display components
    public GameObject[] firstCharacterGameOverObjects;  // Array for Character 0
    public GameObject[] secondCharacterGameOverObjects; // Array for Character 1
    public GameObject[] thirdCharacterGameOverObjects;  // Array for Character 2
    public GameObject[] gameWinObjects;

    private bool gameEnded;
    private Dictionary<CarController2, float> carIn3DObjectTimes = new Dictionary<CarController2, float>();
    private List<DriftScore2> activeVictoryDrifts = new List<DriftScore2>(); // Tracks active victory conditions
    public AudioSource[] audioSources;  // Assign in Inspector
    public AudioClip victorySound;  // Assign your sound clip in inspector
    private int lastUsedSourceIndex = 0;
    private HashSet<CarController2> carsThatPlayedSound = new HashSet<CarController2>();

    void Update()
    {
        if (gameEnded) return;

        foreach (DriftScore2 driftScore in driftScores)
        {
            float totalBarsMax = driftScore.H1 + driftScore.H2 + driftScore.H3;
            if (driftScore.progressBar2To4 >= totalBarsMax && !activeVictoryDrifts.Contains(driftScore))
            {
                SetObjectsActive(driftScore.victory3DObjects, true);
                activeVictoryDrifts.Add(driftScore);
            }
        }

        foreach (DriftScore2 driftScore in activeVictoryDrifts)
        {
            CarController2 car = driftScore.car;
            if (car == null) continue;

            if (carIn3DObjectTimes.TryGetValue(car, out float entryTime))
            {
                float timeInside = Time.time - entryTime;

                if (timeInside >= 2f && !carsThatPlayedSound.Contains(car))
                {
                    PlayVictorySound();
                    carsThatPlayedSound.Add(car);
                }

                if (timeInside >= 2f)
                {
                    EndGame(true, driftScore);
                    return;
                }
            }
        }

        foreach (SpeedDisplay speedDisplay in speedDisplays)
        {
            if (speedDisplay.countdownTime <= 0)
            {
                PlayVictorySound();
                EndGame(false, null);
                return;
            }
        }
    }
    void PlayVictorySound()
    {
        if (victorySound == null || audioSources == null) return;

        // Find the first available audio source that's not playing
        foreach (AudioSource source in audioSources)
        {
                source.PlayOneShot(victorySound);
                Debug.Log("victorySound play");
                break; // Stop after finding first available
        }
    }

    public void OnCarEnter3DObject(CarController2 car, DriftScore2 driftScore)
    {
        if (activeVictoryDrifts.Contains(driftScore) && !carIn3DObjectTimes.ContainsKey(car))
        {
            carIn3DObjectTimes[car] = Time.time;
        }
    }

    public void OnCarExit3DObject(CarController2 car, DriftScore2 driftScore)
    {
        carIn3DObjectTimes.Remove(car);
        carsThatPlayedSound.Remove(car);
    }

    void EndGame(bool isVictory, DriftScore2 winningDriftScore)
    {
        gameEnded = true;
        Time.timeScale = 0.2f;
        Cursor.lockState = CursorLockMode.None;

        int selectedCharacter = SelectionData.SelectedCharacterIndex;

        if (isVictory && winningDriftScore != null)
        {
            // Get the index of the selected victory ending
            int endingIndex = winningDriftScore.currentUIIndex; // Should be 0, 1, or 2
            PlayerPrefs.SetInt("VictoryEndingIndex", endingIndex);
            PlayerPrefs.SetInt("LastSelectedCharacter", selectedCharacter);
            PlayerPrefs.SetInt("GameWon", 1);

            SetObjectsActive(winningDriftScore.gameWinObjects, true);
            SetObjectsActive(winningDriftScore.victory3DObjects, true);
            ReceiveVictoryIndex(winningDriftScore);
            SaveGalleryUnlockData(selectedCharacter, endingIndex);
        }
        else
        {
            // Set 5th image (index 4) for loss
            int lossIndex = 4;
            PlayerPrefs.SetInt("VictoryEndingIndex", lossIndex);
            PlayerPrefs.SetInt("LastSelectedCharacter", selectedCharacter);
            PlayerPrefs.SetInt("GameWon", 0);

            // Activate game-over objects
            if (selectedCharacter == 0)
            {
                SetObjectsActive(firstCharacterGameOverObjects, true);
            }
            else if (selectedCharacter == 1)
            {
                SetObjectsActive(secondCharacterGameOverObjects, true);
            }
            else if (selectedCharacter == 2)
            {
                SetObjectsActive(thirdCharacterGameOverObjects, true);
            }

            // Unlock the 5th image in gallery
            SaveGalleryUnlockData(selectedCharacter, lossIndex);
        }

        // Disable cars and speed displays
        foreach (DriftScore2 driftScore in driftScores)
        {
            if (driftScore.car != null) driftScore.car.enabled = false;
        }
        foreach (SpeedDisplay display in speedDisplays)
        {
            display.enabled = false;
        }

        PlayerPrefs.Save();
    }

    private void SaveGalleryUnlockData(int characterIndex, int endingIndex)
    {
        string key = characterIndex switch
        {
            0 => "Character1_Revealed",
            1 => "Character2_Revealed",
            2 => "Character3_Revealed",
            _ => throw new System.ArgumentOutOfRangeException()
        };

        string revealedData = PlayerPrefs.GetString(key, "00000");
        char[] dataArray = revealedData.ToCharArray();

        // Ensure endingIndex is within bounds (0-4)
        if (endingIndex >= 0 && endingIndex < dataArray.Length)
        {
            dataArray[endingIndex] = '1';
            PlayerPrefs.SetString(key, new string(dataArray));
        }
    }

    void SetObjectsActive(GameObject[] objects, bool state)
    {
        if (objects == null) return;
        foreach (GameObject obj in objects)
        {
            if (obj != null) obj.SetActive(state);
        }
    }

    public void ReceiveVictoryIndex(DriftScore2 winningDriftScore)
    {
        if (winningDriftScore == null || winningDriftScore.victoryUIObjects == null)
        {
            Debug.LogError("Winning DriftScore2 or its UI array is null.");
            return;
        }

        // Deactivate all UI elements for this DriftScore2
        foreach (GameObject obj in winningDriftScore.victoryUIObjects)
        {
            if (obj != null) obj.SetActive(false);
        }

        // Activate the UI element based on currentUIIndex
        int uiIndex = winningDriftScore.currentUIIndex;
        if (uiIndex >= 0 && uiIndex < winningDriftScore.victoryUIObjects.Length)
        {
            winningDriftScore.victoryUIObjects[uiIndex].SetActive(true);
            Debug.Log($"Activated UI index {uiIndex} for {winningDriftScore.name}");
        }
    }
}
