using UnityEngine;

public class AuraPulse : MonoBehaviour
{
    [Header("Light Settings")]
    public Light AuraLight;
    public float MinIntensity = 1f;
    public float MaxIntensity = 3f;
    public float PulseSpeed = 2f;

    [Header("Optional: Material Emission")]
    public Renderer AuraRenderer;
    public Color EmissionColor = Color.cyan;

    void Update()
    {
        float pulse = Mathf.PingPong(Time.time * PulseSpeed, 1f);
        float intensity = Mathf.Lerp(MinIntensity, MaxIntensity, pulse);

        if (AuraLight != null)
            AuraLight.intensity = intensity;

        if (AuraRenderer != null)
            AuraRenderer.material.SetColor("_EmissionColor", EmissionColor * intensity);
    }
}