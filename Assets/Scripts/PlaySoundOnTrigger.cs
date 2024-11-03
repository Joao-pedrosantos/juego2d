using UnityEngine;

public class PlaySoundOnTrigger : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found on the object.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Object entered trigger: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger zone.");
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log("Playing sound.");
            }
            else
            {
                Debug.Log("Sound is already playing.");
            }
        }
        else
        {
            Debug.Log("Another object entered: " + other.name);
        }
    }
}
