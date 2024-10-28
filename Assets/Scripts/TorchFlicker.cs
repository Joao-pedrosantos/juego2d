using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchFlicker : MonoBehaviour
{
    public Light2D torchLight;
    public float minIntensity = 6f;
    public float maxIntensity = 10f;
    public Vector2 minRadius = new Vector2(1.5f, 2f);
    public Vector2 maxRadius = new Vector2(2.5f, 3f);
    public float baseFlickerSpeed = 2f;
    public Color colorStart = new Color(1f, 0.5f, 0f);  // Bright Orange
    public Color colorEnd = new Color(1f, 1f, 0.6f);    // Light Yellow

    void Update()
    {
        // Vary flicker speed over time
        float dynamicSpeed = baseFlickerSpeed * (0.5f + Mathf.PerlinNoise(Time.time, 0) * 0.5f);

        // Flicker Intensity
        float flickerIntensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PingPong(Time.time * dynamicSpeed, 1));
        torchLight.intensity = flickerIntensity;

        // Non-uniform Radius for Dancing Effect
        float flickerRadiusX = Mathf.Lerp(minRadius.x, maxRadius.x, Mathf.PingPong(Time.time * (dynamicSpeed / 2), 1));
        float flickerRadiusY = Mathf.Lerp(minRadius.y, maxRadius.y, Mathf.PingPong(Time.time * (dynamicSpeed / 2 + 0.5f), 1));

        torchLight.pointLightInnerRadius = Mathf.Min(flickerRadiusX, flickerRadiusY) / 2;
        torchLight.pointLightOuterRadius = Mathf.Max(flickerRadiusX, flickerRadiusY);

        // Apply non-circular shape to the light by adjusting the local scale
        torchLight.transform.localScale = new Vector3(flickerRadiusX, flickerRadiusY, 1);

        // Fade Color between Orange and Yellow
        torchLight.color = Color.Lerp(colorStart, colorEnd, Mathf.PingPong(Time.time * dynamicSpeed, 1));
    }
}
