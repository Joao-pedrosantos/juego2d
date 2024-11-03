using UnityEngine;

public class TriggerSound : MonoBehaviour
{
    public AudioClip soundClip; // Assign the sound clip in the Inspector
    private AudioSource audioSource;

    void Start()
    {
        // Get the AudioSource component attached to the same GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // If there's no AudioSource, add one
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Play the sound
            audioSource.PlayOneShot(soundClip);
        }
    }
}
