using UnityEngine;

public class PersistentAudio : MonoBehaviour
{
    private static PersistentAudio instance;

    private void Awake()
    {
        // Check if there is already an instance of PersistentAudio
        if (instance == null)
        {
            instance = this;  // Set this as the active instance
            DontDestroyOnLoad(gameObject);  // Prevent destruction on scene load
        }
        else if (instance != this)
        {
            Destroy(gameObject);  // Destroy duplicate instances
        }
    }
}
