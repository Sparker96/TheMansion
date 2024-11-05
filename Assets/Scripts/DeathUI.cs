using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathUI : MonoBehaviour
{
    public void Retry()
    {
        // Reload the current scene
        SceneManager.LoadScene("GameScene");
    }

    public void MainMenu()
    {
        // Load the Main Menu scene (ensure it's in the build settings)
        SceneManager.LoadScene("MainMenu"); // Replace "MainMenu" with your scene's name
    }

}
