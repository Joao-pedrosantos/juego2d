using UnityEngine;

public class FogParallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("Reference to the player's transform.")]
    public Transform player;  
    
    [Tooltip("Intensity of the parallax effect (e.g., 0.5 for slower movement).")]
    public float parallaxEffect = 0.5f;  
    
    [Header("Constant Movement Settings")]
    [Tooltip("Speed at which the fog moves to the left.")]
    public float constantLeftSpeed = 0.5f;  

    private float startPosX;

    void Start()
    {
        // Store the initial X position of the fog
        startPosX = transform.position.x;
    }

    void Update()
    {
        // Calculate parallax based on player's position
        float parallaxDistance = player.position.x * parallaxEffect;
        
        // Calculate constant leftward movement
        float leftMovement = constantLeftSpeed * Time.deltaTime;
        
        // Update the fog's position by combining parallax and leftward movement
        transform.position = new Vector3(startPosX + parallaxDistance - leftMovement, transform.position.y, transform.position.z);
    }
}
