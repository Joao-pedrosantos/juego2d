using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentAudio : MonoBehaviour
{
    public static PersistentAudio instance;  // Made public

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.buildIndex);
    }

    private void PlayMusicForScene(int sceneIndex)
    {
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

    // Add these methods
    public void StopBackgroundMusic()
    {
        audioSource.Stop();
    }

    public void PlayBackgroundMusic()
    {
        audioSource.Play();
    }
}
