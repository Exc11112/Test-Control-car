using UnityEngine;

public class GalleryManager : MonoBehaviour
{
    // Character 0 (First Character) Cover Images
    public GameObject[] character1CoverImages; // 3 victory, 1 lose, 1 game end
    public GameObject[] character2CoverImages;
    public GameObject[] character3CoverImages;

    void Start()
    {
        UpdateGalleryDisplay();
    }

    public void UpdateGalleryDisplay()
    {
        EnableAllCovers(); // Ensure all covers are initially enabled

        // Check PlayerPrefs for previously uncovered images
        RevealStoredImages(character1CoverImages, "Character1_Revealed");
        RevealStoredImages(character2CoverImages, "Character2_Revealed");
        RevealStoredImages(character3CoverImages, "Character3_Revealed");
    }

    private void EnableAllCovers()
    {
        // Enable all covers before checking which ones should be disabled
        foreach (var obj in character1CoverImages) obj.SetActive(true);
        foreach (var obj in character2CoverImages) obj.SetActive(true);
        foreach (var obj in character3CoverImages) obj.SetActive(true);
    }

    private void RevealStoredImages(GameObject[] coverImages, string key)
    {
        string revealedData = PlayerPrefs.GetString(key, "00000"); // Default to all covered (5 images)

        for (int i = 0; i < coverImages.Length; i++)
        {
            if (revealedData[i] == '1')
                coverImages[i].SetActive(false); // Uncover saved images
        }
    }

    public void UncoverImage(int characterIndex, int endingIndex)
    {
        string key = characterIndex == 0 ? "Character1_Revealed" :
                     characterIndex == 1 ? "Character2_Revealed" : "Character3_Revealed";

        string revealedData = PlayerPrefs.GetString(key, "00000");
        char[] revealedArray = revealedData.ToCharArray();
        revealedArray[endingIndex] = '1'; // Mark this image as uncovered

        PlayerPrefs.SetString(key, new string(revealedArray));
        PlayerPrefs.Save();
    }
}
