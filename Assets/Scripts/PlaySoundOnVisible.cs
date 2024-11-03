using UnityEngine;

public class PlaySoundOnVisible : MonoBehaviour
{
    public AudioClip soundClip;
    private AudioSource audioSource;
    private Camera mainCamera;
    private bool hasPlayed = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.clip = soundClip;

        mainCamera = Camera.main;
    }

    void Update()
    {
        if (!hasPlayed && IsVisibleFrom(mainCamera))
        {
            audioSource.Play();
            hasPlayed = true;
        }
    }

    bool IsVisibleFrom(Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        Bounds bounds = GetComponent<Renderer>().bounds;
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }
}
