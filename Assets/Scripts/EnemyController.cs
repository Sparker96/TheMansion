using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Speed")]
    public float roamSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Senses")]
    public float detectionRange = 10f;
    public float fieldOfView = 45f;
    public Transform player;
    public LayerMask obstacleMask;

    [Header("Audio Clips")]
    public AudioClip[] roamSounds; // Sounds to play periodically while roaming
    public AudioClip[] detectedSounds;  // Sounds to play when enemy spots the player
    public AudioClip[] exitChaseSounds; // Sounds to play when enemy loses sight of the player
    public AudioClip[] chaseSounds; // Sounds to play periodically while chasing the player

    [Header("Timers")]
    public float roamTime = 3f; // Time to roam in one direction
    public float alertTime = 3f;

    [Header("Sound Intervals")]
    public float roamSoundInterval = 5f; // Reset interval for roam sounds
    public float minChaseSoundInterval = 0.5f; // Minimum interval for chase sounds
    public float maxChaseSoundInterval = 3f; // Maximum interval for chase sounds

    [Header("Sound Volume")]
    public float volume = 1f;  // Maximum volume when monster is closest
    public bool mute = false; // For my sanity



    private string state = "roam"; //This will control what state our monster is in Roaming, Alerted, or Chasing
    private Vector2 FOVDirection = Vector2.right; // Default direction, can be set to any initial direction

    private bool chased = false; //This is to determine if the monster was in a chase state at some point
    private float alertTimer;
    private float roamTimer;
    private AudioSource audioSource; //Container for our audioSource Component
    private Vector2 lastKnownPosition; //Var for where the monster heard a noise from or last saw player
    private Vector2 roamTarget; //Var for where our monster will head to next
    private bool detectedSoundPlayed = false; // Ensures chase sounds only start after alert
    private float roamSoundTimer = 5f; // Time interval to play roam sounds
    private float chaseSoundTimer = 0f; // Timer for chase sounds

    //Animation
    private Animator anim;
    private Vector2 lastMoveDirection;

    void OnEnable()
    {
        PlayerController2D.OnNoiseMade += OnNoiseHeard;
    }

    void OnDisable()
    {
        PlayerController2D.OnNoiseMade -= OnNoiseHeard;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        SetRoamTarget();
    }

    void Update()
    {
        if (state == "chase")
        {
            ChasePlayer();
        }
        else if (state == "alert")
        {
            MoveTowardsLastKnown();
            DetectPlayer();
        }
        else
        {
            Roam();
            DetectPlayer();
        }
    }

    void FixedUpdate()
    {
    }

    private void SetRoamTarget()
    {
        // Pick a random direction within a range, checking if it’s clear of obstacles
        Vector2 randomDirection;
        RaycastHit2D hit;

        do
        {
            randomDirection = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f)).normalized;
            hit = Physics2D.Raycast(transform.position, randomDirection, 1f, obstacleMask);
        }
        while (hit.collider != null); // Retry if direction is blocked

        roamTarget = (Vector2)transform.position + randomDirection * Random.Range(5f, 10f);
        roamTimer = roamTime;
    }

    private void Animate(Vector2 direction, float speed)
    {
        // Set movement parameters for Animator
        anim.SetFloat("MoveX", direction.x);
        anim.SetFloat("MoveY", direction.y);
        anim.SetFloat("MoveMagnitude", speed);

        // Use last movement direction for idle animation when not moving
        if (direction != Vector2.zero)
        {
            FOVDirection = direction.normalized; // Update FOVDirection
            lastMoveDirection = direction; // Update last move direction when moving
        }
        else
        {
            // Set last direction for idle animation
            anim.SetFloat("LastMoveX", lastMoveDirection.x);
            anim.SetFloat("LastMoveY", lastMoveDirection.y);
        }
    }

    private void Roam()
    {
        // Move towards the roam target
        Vector2 currentPosition = transform.position;
        Vector2 direction = (roamTarget - currentPosition).normalized;
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, roamTarget, roamSpeed * Time.deltaTime);
        transform.position = newPosition;

        //Play roaming sounds whilst roaming
        PlayRoamingSounds();

        // Raycast to check for obstacles If obstacle detected, set a new random roam target
        if (Physics2D.Raycast(transform.position, direction, 1f, obstacleMask)) { SetRoamTarget(); }
        else
        {
            // Move towards the target and animate the movement
            transform.position = newPosition;
            Animate(direction, roamSpeed);
        }

        // If the enemy is close to the target, pick a new one
        if (Vector2.Distance(currentPosition, roamTarget) < 1f || roamTimer <= 0) { SetRoamTarget(); }

        // Reduce the roam timer
        roamTimer -= Time.deltaTime;
    }

    private void DetectPlayer()
    {
        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Check if the player is within the detection range and within the field of view
        if (distanceToPlayer <= detectionRange && Vector2.Angle(FOVDirection, directionToPlayer) < fieldOfView / 2)
        {
            // Check if there are no obstacles blocking the view
            if (!Physics2D.Raycast(transform.position, directionToPlayer.normalized, distanceToPlayer, obstacleMask))
            {
                if (state != "chase")
                {
                    state = "chase";
                    chased = true;
                }
            }
        }
    }

    private void ChasePlayer()
    {
        Vector2 currentPosition = transform.position;
        Vector2 direction = ((Vector2)player.position - currentPosition).normalized; //Direction to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        //Play sounds
        PlayDetectedSound();
        PlayChaseSounds();

        // Raycast to check for obstacles and distance while chasing,If blocked or too far, change state alert and remeber last known
        if (distanceToPlayer <= detectionRange && !Physics2D.Raycast(transform.position, direction, distanceToPlayer, obstacleMask))
        {
            // Move towards the player if the path is clear
            transform.position = Vector2.MoveTowards(currentPosition, player.position, chaseSpeed * Time.deltaTime);
            Animate(direction, chaseSpeed);
        }
        else
        {
            state = "alert";
            lastKnownPosition = player.position;
            alertTimer = alertTime;
        }
    }

    private void OnNoiseHeard(Vector2 position, float radius)
    {
        // Check if the noise is within the detection range
        if (Vector2.Distance(transform.position, position) <= radius)
        {
            // Set the noise position and alert the enemy
            lastKnownPosition = position;
            if (state == "roam")
            {
                state = "alert";
                alertTimer = alertTime;
            }
            else if (state == "alert")
            {
                alertTimer = alertTime;
            }

        }
    }

    // This is monsters alert behavior.
    private void MoveTowardsLastKnown()
    {
        Vector2 currentPosition = transform.position;
        Vector2 direction = (lastKnownPosition - currentPosition).normalized;
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, lastKnownPosition, roamSpeed * Time.deltaTime);
        transform.position = newPosition;

        //move towards the last known position
        Animate(direction, roamSpeed);

        // Check if close to the last known position or if the alert timer has expired
        if (Vector2.Distance(transform.position, lastKnownPosition) < 1f || alertTimer <= 0f)
        {
            state = "roam";
            detectedSoundPlayed = false;
        }

        if (chased && state == "roam")
        {
            PlayExitChaseSound();
            chased = false;
        }

        // Decrease the alert timer
        alertTimer -= Time.deltaTime;
    }

    private void PlayRoamingSounds()
    {
        roamSoundTimer -= Time.deltaTime;
        if (roamSoundTimer <= 0f && roamSounds.Length > 0)
        {
            // Play a random roam sound
            AudioClip roamSound = roamSounds[Random.Range(0, roamSounds.Length)];
            //Debug.Log("Roam Sound Played");
            audioSource.PlayOneShot(roamSound, GetAdjustedVolume());

            // Reset the roam sound timer
            roamSoundTimer = roamSoundInterval;
        }
    }

    private void PlayDetectedSound()
    {
        if (detectedSounds.Length > 0 && !detectedSoundPlayed)
        {
            AudioClip detectedSound = detectedSounds[Random.Range(0, detectedSounds.Length)];
            //Debug.Log("Detect Sound Played");
            audioSource.PlayOneShot(detectedSound, GetAdjustedVolume());
            detectedSoundPlayed = true;
        }
    }

    private void PlayExitChaseSound()
    {
        if (exitChaseSounds.Length > 0)
        {
            AudioClip exitChaseSound = exitChaseSounds[Random.Range(0, exitChaseSounds.Length)];
            audioSource.PlayOneShot(exitChaseSound, GetAdjustedVolume());
            //Debug.Log("ExitChase Sound Played");
        }
    }

    private void PlayChaseSounds()
    {
        // Wait for the alert sound to finish before starting chase sounds
        if (!detectedSoundPlayed || audioSource.isPlaying)
        {
            chaseSoundTimer = 0f;
            return;
        }

        // Calculate interval based on distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float chaseSoundInterval = Mathf.Lerp(minChaseSoundInterval, maxChaseSoundInterval, distanceToPlayer / detectionRange);

        chaseSoundTimer -= Time.deltaTime;
        if (chaseSoundTimer <= 0f && chaseSounds.Length > 0)
        {
            AudioClip chaseSound = chaseSounds[Random.Range(0, chaseSounds.Length)];
            //Debug.Log("Chase Sound Played");
            audioSource.PlayOneShot(chaseSound, GetAdjustedVolume());
            chaseSoundTimer = chaseSoundInterval;
        }
    }

    private float GetAdjustedVolume()
    {
        // Calculate the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        float adjustedVolume = (1 - (distanceToPlayer / 35f)); //Get a percentage from how far the player is and inverse it to be appropraite values to pass as volume
        //Debug.Log(" Distance: " + distanceToPlayer + " adjusted vol: " + adjustedVolume);
        if (mute) return 0;

        return adjustedVolume * volume;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the enemy collided with the player
        if (other.CompareTag("Player"))
        {
            KillPlayer(other.gameObject);
        }
    }

    void KillPlayer(GameObject player)
    {
        // Get the PlayerController2D component
        PlayerController2D playerController = player.GetComponent<PlayerController2D>();
        if (playerController != null)
        {
            // Trigger the Die function on the player
            playerController.Die();
        }
        detectionRange = 0f;
        fieldOfView = 0f;
        state = "roam";

        // Trigger any game over actions here, such as:
        Debug.Log("KillPlayer");

        // You could implement a game over UI, reset the level, or reload the scene.
        // Example: Reload the current scene
        // UnityEngine.SceneManagement.SceneManager.LoadScene(
        //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }





    //END


}


