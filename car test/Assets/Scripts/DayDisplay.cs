using UnityEngine;
using UnityEngine.UI;

public class DayDisplay : MonoBehaviour
{
    public Text dayText; // Drag your UI text here in the Inspector

    void Start()
    {
        UpdateDayText();
    }

    public void UpdateDayText()
    {
        int day = PlayerPrefs.GetInt("day", 1); // Default to 1 if not set
        dayText.text = "Day: " + day;
    }
}
