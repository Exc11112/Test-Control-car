using UnityEngine;
using UnityEngine.UI;

public class DayDisplay : MonoBehaviour
{
    public GameObject day1Object; // Assign GameObject for Day 1
    public GameObject day2Object; // Assign GameObject for Day 2
    public GameObject day3Object; // Assign GameObject for Day 3

    void Start()
    {
        UpdateDayText();
    }

    public void UpdateDayText()
    {
        int day = PlayerPrefs.GetInt("day", 1); // Default to 1 if not set

        // Activate the correct GameObject and deactivate the others
        day1Object.SetActive(day == 1);
        day2Object.SetActive(day == 2);
        day3Object.SetActive(day == 3);
    }
}
