using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenUI : MonoBehaviour
{
    // Method to load the game scene and play again
    public void PlayAgain()
    {
        // Replace "GameScene" with the exact name of your game scene
        SceneManager.LoadScene("GameScene");
    }

    // Method to load the main menu scene
    public void MainMenu()
    {
        // Replace "MainMenu" with the exact name of your main menu scene
        SceneManager.LoadScene("MainMenu");
    }

    // Method to quit the application
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Stop play mode in the editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the application in a build
        Application.Quit();
#endif
    }
}
