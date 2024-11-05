using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyUI : MonoBehaviour
{
    private TextMeshProUGUI keyText; // Reference to the TMP component
    private PlayerController2D player; // Reference to the player controller
    private int totalKeys; // Total number of keys in the scene

    void Start()
    {
        // Get the TextMeshPro component attached to this GameObject
        keyText = GetComponent<TextMeshProUGUI>();

        // Find the player object and get the PlayerController2D component
        player = FindObjectOfType<PlayerController2D>();

        // Count all key objects in the scene tagged as "Key" (make sure keys are tagged as "Key")
        totalKeys = GameObject.FindGameObjectsWithTag("Collectible").Length;

        // Initialize the text display
        UpdateKeyText();
    }

    void Update()
    {
        // Update the key count display each frame
        UpdateKeyText();
    }

    void UpdateKeyText()
    {
        // Update the TMP text to display "Keys: X/Y" with the current number of keys
        keyText.text = $"Keys: {player.numKeys}/{totalKeys}";
    }
}
