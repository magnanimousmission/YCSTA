using System;
using UnityEngine;

public class AuraLightController : MonoBehaviour
{
    [SerializeField] private AuraController auraController;
    [SerializeField] private float lightDimmer = 10f;
    private Light _auraLight;

    private void Awake()
    {
        _auraLight = GetComponent<Light>();
        _auraLight.intensity = 0f;
    }

    private void OnEnable() =>
        auraController.OnAuraChanged += UpdateLight;

    private void OnDisable() =>
        auraController.OnAuraChanged -= UpdateLight;

    private void UpdateLight(float aura) =>
        _auraLight.intensity = aura / lightDimmer;
}