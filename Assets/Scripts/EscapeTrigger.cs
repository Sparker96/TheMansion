using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player has entered the trigger
        if (other.CompareTag("Player"))
        {
            // Load the victory scene
            SceneManager.LoadScene("VictoryScene");
        }
    }
}
