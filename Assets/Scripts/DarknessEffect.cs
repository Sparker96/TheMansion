using UnityEngine;

public class DarknessEffect : MonoBehaviour
{
    public Transform player;         // Reference to the player's Transform
    public Material darknessMaterial; // Reference to the material using DarknessShader
    public float radius = 0.2f;       // Radius of the transparent area around the player
    public float smoothness = 0.1f;   // Smoothness of the edge

    private Camera mainCamera;

    void Start()
    {
        // Cache reference to the main camera
        mainCamera = Camera.main;

        // Set initial properties of the darkness material
        darknessMaterial.SetFloat("_Radius", radius);
        darknessMaterial.SetFloat("_Smoothness", smoothness);
    }

    void Update()
    {
        if (player == null) return;

        // Convert the player's position to viewport space (0-1 range)
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(player.position);

        // Update shader properties with player's viewport position
        darknessMaterial.SetVector("_PlayerPos", new Vector4(viewportPos.x, viewportPos.y, 0, 0));
    }
}
