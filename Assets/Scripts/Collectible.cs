using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public GameObject onCollectEffect;
    public AudioClip collectSound; // The sound to play on collection
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Get an AudioSource component
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // Ensure the sound doesn't play on start


        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {


    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        // Check if the other object has a PlayerController2D component
        if (other.CompareTag("Player"))
        {
            // Play the collection sound
            audioSource.PlayOneShot(collectSound);

            // Disable the SpriteRenderer to hide the collectible immediately
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            // Destroy the collectible after the sound plays
            Destroy(gameObject, collectSound.length);

            // Instantiate the particle effect
            Instantiate(onCollectEffect, transform.position, transform.rotation);
        }


    }


}