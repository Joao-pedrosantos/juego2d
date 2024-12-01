using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentAudio : MonoBehaviour
{
    public static PersistentAudio instance;  // Singleton instance

    private AudioSource audioSource;

    public AudioClip mainMenuMusic;
    public AudioClip firstLevel;
    public AudioClip secondLevel;
    public AudioClip thirdLevel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Ensure proper cleanup of sceneLoaded event subscription
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.buildIndex);
    }

    private void PlayMusicForScene(int sceneIndex)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource is missing or destroyed. Attempting to recreate.");
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Logic for assigning music based on scene index
        if (sceneIndex == 0 || sceneIndex == 1)
        {
            if (audioSource.clip != mainMenuMusic)
            {
                audioSource.clip = mainMenuMusic;
                audioSource.Play();
            }
        }
        else if (sceneIndex == 2)
        {
            if (audioSource.clip != firstLevel)
            {
                audioSource.clip = firstLevel;
                audioSource.Play();
            }
        }
        else if (sceneIndex == 3)
        {
            if (audioSource.clip != secondLevel)
            {
                audioSource.clip = secondLevel;
                audioSource.Play();
            }
        }
        else if (sceneIndex == 4)
        {
            if (audioSource.clip != thirdLevel)
            {
                audioSource.clip = thirdLevel;
                audioSource.Play();
            }
        }
    }

    public void StopBackgroundMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    public void PlayBackgroundMusic()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}
