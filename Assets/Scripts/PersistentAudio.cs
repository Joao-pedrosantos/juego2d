using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentAudio : MonoBehaviour
{
    private static PersistentAudio instance;

    // Add an AudioSource to play the audio clips
    private AudioSource audioSource;

    // Create an array to store audio clips
    public AudioClip mainMenuMusic;   // Music for both scene 0 and scene 1
    public AudioClip levelMusic;      // Music for scene 2 (first level)

    private void Awake()
    {
        // Check if there is already an instance of PersistentAudio
        if (instance == null)
        {
            instance = this;  // Set this as the active instance
            DontDestroyOnLoad(gameObject);  // Prevent destruction on scene load

            // Get the AudioSource component or add one if it doesn't exist
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Register for scene change events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this)
        {
            Destroy(gameObject);  // Destroy duplicate instances
        }
    }

    // This method is called whenever a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Play the appropriate audio for the new scene
        PlayMusicForScene(scene.buildIndex);
    }

    // Method to play different music based on the scene index
    private void PlayMusicForScene(int sceneIndex)
    {
        // If the scene is either 0 (Main Menu) or 1 (Settings Menu), play the main menu music
        if (sceneIndex == 0 || sceneIndex == 1)
        {
            if (audioSource.clip != mainMenuMusic)
            {
                audioSource.clip = mainMenuMusic;
                audioSource.Play();
            }
        }
        // If the scene is 2 (First Level), play the level music
        else if (sceneIndex == 2)
        {
            if (audioSource.clip != levelMusic)
            {
                audioSource.clip = levelMusic;
                audioSource.Play();
            }
        }
    }
}
