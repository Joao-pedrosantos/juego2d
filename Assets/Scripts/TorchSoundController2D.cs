using UnityEngine;

public class TorchSoundController2D : MonoBehaviour
{
    public AudioSource torchAudioSource;  // Assign the torch's AudioSource in the Inspector
    public Transform playerTransform;     // Assign the player's Transform in the Inspector
    public float maxVolume = 1.0f;        // Maximum volume of the torch sound
    public float maxDistance = 15f;  // Set maximum distance to 15 units

    private Camera mainCamera;

    void Start()
    {
        maxDistance = 15f;

        // If the AudioSource isn't assigned, get it from the current GameObject
        if (torchAudioSource == null)
        {
            torchAudioSource = GetComponent<AudioSource>();
        }

        // If the player's Transform isn't assigned, find the object tagged "Player"
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogError("Player Transform not assigned and no GameObject with tag 'Player' found.");
        }

        // Get the main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Please tag your camera as 'MainCamera'.");
        }

        torchAudioSource.volume = 0f;     // Start with the volume at zero
        torchAudioSource.loop = true;     // Ensure the sound loops continuously
        torchAudioSource.Play();          // Start playing the sound
    }

    void Update()
    {
        if (IsTorchVisible())
        {
            // Calculate the distance between the torch and the player
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (distance <= maxDistance)
            {
                // Adjust volume based on distance (closer = louder)
                float volume = Mathf.Lerp(maxVolume, 0f, distance / maxDistance);
                torchAudioSource.volume = volume;
            }
            else
            {
                torchAudioSource.volume = 0f;  // Mute if beyond maxDistance
            }
        }
        else
        {
            torchAudioSource.volume = 0f;      // Mute if not visible
        }
    }

    bool IsTorchVisible()
    {
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPoint.z > 0 &&
               viewportPoint.x > 0 && viewportPoint.x < 1 &&
               viewportPoint.y > 0 && viewportPoint.y < 1;
    }
}
