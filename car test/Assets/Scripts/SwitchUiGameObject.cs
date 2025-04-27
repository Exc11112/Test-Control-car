using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchUiGameObject : MonoBehaviour
{
    public GameObject[] objects; // Assign your 6 GameObjects in the Inspector
    private int currentIndex = 0;

    private void Start()
    {
        // Make sure only the current active object is enabled
        UpdateActiveObject();
    }

    public void Next()
    {
        objects[currentIndex].SetActive(false);
        currentIndex = (currentIndex + 1) % objects.Length; // Move to next, loop back to 0
        objects[currentIndex].SetActive(true);
    }

    public void Previous()
    {
        objects[currentIndex].SetActive(false);
        currentIndex = (currentIndex - 1 + objects.Length) % objects.Length; // Move to previous, loop to last
        objects[currentIndex].SetActive(true);
    }

    private void UpdateActiveObject()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].SetActive(i == currentIndex);
        }
    }
}
