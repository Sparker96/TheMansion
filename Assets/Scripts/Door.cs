using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private PlayerController2D player; // Reference to the player controller
    private int totalKeys; // Total number of keys in the scene
    private AudioSource audioSource; // Reference to the AudioSource component

    [Header("Audio Clips")]
    public AudioClip unlockClip; // The first clip to play when unlocking
    public AudioClip openClip; // The second clip to play when opening
    public AudioClip[] jiggleClips; // Array of jiggle sounds for when the door is locked

    void Start()
    {
        // Get components and setup
        audioSource = GetComponent<AudioSource>();
        player = FindObjectOfType<PlayerController2D>();

        // Count all collectible objects in the scene tagged as "Collectible"
        totalKeys = GameObject.FindGameObjectsWithTag("Collectible").Length;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player entered the door's trigger collider
        if (other.CompareTag("Player"))
        {
            // Check if the player has collected all the keys
            if (player.numKeys >= totalKeys)
            {
                StartCoroutine(OpenDoorSequence()); // Open the door with sound sequence
            }
            else
            {
                PlayJiggleSound(); // Play a random jiggle sound
                Debug.Log("You need more keys to open this door!");
            }
        }
    }

    IEnumerator OpenDoorSequence()
    {
        // Play the unlocking sound
        if (audioSource != null && unlockClip != null)
        {
            audioSource.PlayOneShot(unlockClip);
            yield return new WaitForSeconds(unlockClip.length); // Wait for the unlock sound to finish
        }

        // Play the opening sound
        if (audioSource != null && openClip != null)
        {
            audioSource.PlayOneShot(openClip);
            yield return new WaitForSeconds(openClip.length); // Wait for the open sound to finish
        }

        // Destroy the door after both sounds have played
        Destroy(gameObject);
    }

    void PlayJiggleSound()
    {
        if (jiggleClips.Length > 0)
        {
            // Choose a random clip from the jiggleClips array
            AudioClip jiggleClip = jiggleClips[Random.Range(0, jiggleClips.Length)];
            audioSource.PlayOneShot(jiggleClip); // Play the selected jiggle sound
        }
    }
}
