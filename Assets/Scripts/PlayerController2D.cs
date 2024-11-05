using System.Collections;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    // Public variables
    public float walkSpeed = 4f; // Normal walk speed
    public float sprintSpeed = 9f; // Sprint speed
    public float slowWalkSpeed = 2f; // Slow walk speed
    public bool toggleSprint = false; // Whether sprinting is toggleable
    public bool toggleWalk = false; // Whether slow walking is toggleable
    public GameObject deathVFX; // First VFX prefab
    public AudioClip deathAudioClip; // Death audio clip
    public GameObject deathUI;  // UI to show after both VFX play

    // Footstep sound variables
    public AudioClip[] walkFootstepSounds; // Array of footstep sounds for walking
    public AudioClip[] sprintFootstepSounds; // Array of footstep sounds for sprinting
    public float walkFootstepInterval = 0.4f; // Interval for walking
    public float slowWalkFootstepInterval = 0.8f; // Interval for slow walking
    public float sprintFootstepInterval = 0.25f; // Interval for sprinting
    private float footstepTimer = 0f; // Timer to track footstep intervals

    // Footstep volume settings
    public float walkVolume = 0.5f;
    public float slowWalkVolume = 0.3f;
    public float sprintVolume = 0.7f;

    //Footstep noise that the a.i. can listen for
    public float walkNoiseRadius = 5f;
    public float sprintNoiseRadius = 10f;
    public float slowWalkNoiseRadius = 2f; // Noise radius for slow walking

    // Event that other scripts can listen to for noise
    public delegate void NoiseEvent(Vector2 position, float radius);
    public static event NoiseEvent OnNoiseMade;

    public int numKeys = 0; // Track keys collected


    // Private variables
    private Rigidbody2D rb; // Reference to the Rigidbody2D component attached to the player
    private Vector2 movement; // Stores the direction of player movement
    private float currentSpeed; // Current movement speed
    private bool isSprinting = false; // Tracks sprint state
    private bool isSlowWalking = false; // Tracks slow walk state
    private AudioSource audioSource; // Reference to the AudioSource component
    private Vector2 input;
    private bool isDead = false; //Player state

    //Animation
    private Animator anim;
    private Vector2 lastMoveDirection;
    private bool facingLeft = true; //Our sprite is facing left

    void Start()
    {
        // Initialize the Rigidbody2D and AudioSource components and Animation
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();

        // Prevent the player from rotating
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Set the default speed
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        if (isDead) return; // Stop further player actions if dead

        // Get player input from keyboard or controller
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        if (input != Vector2.zero)
        {
            lastMoveDirection = input;
        }

        // Handle sprint and walk toggles or holds
        HandleSpeedModifiers();

        // Set movement direction based on input
        movement = new Vector2(input.x, input.y);

        Animate();

        //Flip - check if facing other direction
        if (input.x < 0 && !facingLeft || input.x > 0 && facingLeft) { Flip(); }


        // Normalize the movement vector to prevent faster diagonal movement
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        // Optionally rotate the player based on movement direction
        //       RotatePlayer(input.x, input.y);

        // Play footstep sounds if the player is moving
        if (movement != Vector2.zero)
        {
            // Adjust footstep interval based on movement speed
            UpdateFootstepTimer();
        }

        // Emit noise based on the player's movement state
        if (isSprinting)
        {
            MakeNoise((Vector2)transform.position, sprintNoiseRadius);
        }
        else if (isSlowWalking)
        {
            MakeNoise((Vector2)transform.position, slowWalkNoiseRadius);
        }
        else if (movement.magnitude > 0)
        {
            MakeNoise((Vector2)transform.position, walkNoiseRadius);
        }
    }

    void FixedUpdate()
    {
        // Apply movement to the player in FixedUpdate for physics consistency
        rb.velocity = movement * currentSpeed;
    }


    void MakeNoise(Vector2 position, float radius)
    {
        OnNoiseMade?.Invoke(position, radius);
    }

    void Animate()
    {
        //Set our animator parameters
        anim.SetFloat("MoveX", input.x);
        anim.SetFloat("MoveY", input.y);
        anim.SetFloat("MoveMagnitude", movement.magnitude);
        // Use last movement direction for idle animation when not moving
        if (movement == Vector2.zero)
        {
            anim.SetFloat("LastMoveX", lastMoveDirection.x);
            anim.SetFloat("LastMoveY", lastMoveDirection.y);
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1; //Makes x negative >> flips sprite
        transform.localScale = scale;
        facingLeft = !facingLeft;
    }

    private void HandleSpeedModifiers()
    {
        // Check for sprinting
        if (toggleSprint)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift)) isSprinting = !isSprinting;
        }
        else
        {
            isSprinting = Input.GetKey(KeyCode.LeftShift);
        }

        // Check for slow walking
        if (toggleWalk)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl)) isSlowWalking = !isSlowWalking;
        }
        else
        {
            isSlowWalking = Input.GetKey(KeyCode.LeftControl);
        }

        // Set speed based on modifiers
        if (isSprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else if (isSlowWalking)
        {
            currentSpeed = slowWalkSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player collided with an object tagged as "Key"
        if (other.CompareTag("Collectible"))
        {
            numKeys++;
        }
    }


    private void UpdateFootstepTimer()
    {
        if (movement != Vector2.zero)
        {
            // Set the appropriate footstep interval based on the player's movement state
            if (isSprinting)
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f)
                {
                    PlayFootstepSound(sprintFootstepSounds, sprintVolume);
                    footstepTimer = sprintFootstepInterval;
                }
            }
            else if (isSlowWalking)
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f)
                {
                    PlayFootstepSound(walkFootstepSounds, slowWalkVolume);
                    footstepTimer = slowWalkFootstepInterval;
                }
            }
            else
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f)
                {
                    PlayFootstepSound(walkFootstepSounds, walkVolume);
                    footstepTimer = walkFootstepInterval;
                }
            }
        }
    }

    public void Die()
    {
        if (!isDead)
        {
            isDead = true; // Set the player to dead state
            StartCoroutine(DeathSequence());

            // Optional: Disable the player's renderer and collider for visual effect
            movement = Vector2.zero;
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;
        }
    }

    IEnumerator DeathSequence()
    {
        // Play the death audio clip
        if (deathAudioClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathAudioClip);
        }


        // Spawn the death VFX and wait for it to finish
        if (deathVFX != null)
        {
            GameObject vfx = Instantiate(deathVFX, transform.position, Quaternion.identity);
            var particleSystem = vfx.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                yield return new WaitForSeconds(particleSystem.main.duration);
            }
            Destroy(vfx);
        }

        // Activate the death UI after the VFX has played
        if (deathUI != null)
        {
            Instantiate(deathUI);
        }

    }

    private void PlayFootstepSound(AudioClip[] footstepSounds, float volume)
    {
        if (footstepSounds.Length > 0 && !audioSource.isPlaying)
        {
            // Randomly select a footstep sound from the array and play it with the specified volume
            AudioClip footstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
            audioSource.PlayOneShot(footstep, volume);
        }
    }

}
