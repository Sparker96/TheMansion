using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MaineMenuController : MonoBehaviour
{
    public GameObject MainMenuUI; // Reference to the MainMenuUI canvas
    public GameObject OptionsUI;  // Reference to the OptionsUI canvas
    public Slider volumeSlider;    // Reference to the Slider in OptionsUI

    private const string VolumePrefKey = "AmbientVolume"; // Key for saving volume

    void Start()
    {
        // Set slider to saved volume or default to max if it hasn't been set
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        volumeSlider.value = savedVolume;

        // Register listener to handle volume changes
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    // This method loads the Game Scene
    public void PlayGame()
    {
        // Replace "GameScene" with the exact name of your game scene
        SceneManager.LoadScene("GameScene");
    }

    public void OpenOptions()
    {
        // Show the OptionsUI canvas and hide the MainMenuUI canvas
        OptionsUI.SetActive(true);
        MainMenuUI.SetActive(false);
    }

    public void CloseOptions()
    {
        // Show the MainMenuUI canvas and hide the OptionsUI canvas
        MainMenuUI.SetActive(true);
        OptionsUI.SetActive(false);
    }

    // This method exits the application
    public void QuitGame()
    {
        // Logs a message when running in the editor (won't quit in the editor)
        Debug.Log("Game is exiting...");
        // If we're in the Unity Editor, stop playback
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Otherwise, quit the application
        Application.Quit();
#endif
    }

    public void ToggleMuteAmbientNoise()
    {
        bool isMuted = PlayerPrefs.GetInt("AmbientMuted", 0) == 1;
        PlayerPrefs.SetInt("AmbientMuted", isMuted ? 0 : 1); // Toggle mute setting
        PlayerPrefs.Save();
    }

    public void SetVolume(float volume)
    {
        // Save volume level to PlayerPrefs
        PlayerPrefs.SetFloat(VolumePrefKey, volume);
        PlayerPrefs.Save();
    }
}
