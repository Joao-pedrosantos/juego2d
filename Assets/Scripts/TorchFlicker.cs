using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchFlicker : MonoBehaviour
{
    [SerializeField] private Light2D torchLight;
    [SerializeField] private float minIntensity = 9f;      // Smaller range for smoother effect
    [SerializeField] private float maxIntensity = 10f;
    [SerializeField] private Vector2 minRadius = new Vector2(1.8f, 3.2f);  // Adjusted to match intensity smoothness
    [SerializeField] private Vector2 maxRadius = new Vector2(2.2f, 3.8f);
    [SerializeField] private float baseFlickerSpeed = 0.2f;  // Slower base speed for smoother flicker
    [SerializeField] private Color colorStart = new Color(1f, 0.6f, 0.2f);  // Subtle color change
    [SerializeField] private Color colorEnd = new Color(1f, 0.9f, 0.5f);

    private float randomOffset;  // Adds variation for each torch

    void Start()
    {
        // Randomize offset to prevent synchronization of multiple torches
        randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float time = Time.time + randomOffset;

        // Flicker Speed Using Perlin Noise for Smoother Variation
        float dynamicSpeed = baseFlickerSpeed * (0.75f + Mathf.PerlinNoise(time * 0.5f, 0) * 0.25f);

        // Smooth Flicker Intensity
        float flickerNoise = Mathf.PerlinNoise(time * dynamicSpeed, 0);
        float flickerIntensity = Mathf.Lerp(minIntensity, maxIntensity, flickerNoise);
        torchLight.intensity = flickerIntensity;

        // Smooth Radius Variation for a More Subtle Flicker
        float flickerRadiusX = Mathf.Lerp(minRadius.x, maxRadius.x, Mathf.PerlinNoise(time * (dynamicSpeed * 0.3f), 0));
        float flickerRadiusY = Mathf.Lerp(minRadius.y, maxRadius.y, Mathf.PerlinNoise(time * (dynamicSpeed * 0.3f + 0.5f), 0));

        torchLight.pointLightInnerRadius = Mathf.Min(flickerRadiusX, flickerRadiusY) / 2;
        torchLight.pointLightOuterRadius = Mathf.Max(flickerRadiusX, flickerRadiusY);

        // Apply non-circular shape to the light by adjusting the local scale
        torchLight.transform.localScale = new Vector3(flickerRadiusX, flickerRadiusY, 1);

        // Smooth Color Transition
        torchLight.color = Color.Lerp(colorStart, colorEnd, flickerNoise);
    }
}
