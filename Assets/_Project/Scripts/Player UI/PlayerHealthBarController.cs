using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarController : MonoBehaviour
{
    [SerializeField] private PlayerCore playerCore;
    [SerializeField] private float lerpSpeed = 5f;

    private Slider _slider;
    private float _targetValue;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    void OnEnable()
    {
        _slider.maxValue = playerCore.GetPlayerData().health;
        _slider.value = playerCore.GetPlayerData().health;
        _targetValue = playerCore.GetPlayerData().health;
        playerCore.OnHealthChanged += UpdateSlider;
    }

    void OnDisable()
    {
        playerCore.OnHealthChanged -= UpdateSlider;
    }

    void Update()
    {
        _slider.value = Mathf.Lerp(_slider.value, _targetValue, lerpSpeed * Time.deltaTime);
    }

    void UpdateSlider(float current, float max)
    {
        _targetValue = current;
    }
}