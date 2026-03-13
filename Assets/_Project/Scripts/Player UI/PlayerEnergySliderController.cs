using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergySliderController : MonoBehaviour
{
    [SerializeField] private PlayerCore playerCore;

    private Slider _slider;

    private void Awake()
    {
        _slider =  GetComponent<Slider>();
    }

    void OnEnable()
    {
        _slider.maxValue = playerCore.GetPlayerData().energy;
        _slider.value = playerCore.GetPlayerData().energy;
        playerCore.OnEnergyChanged += UpdateSlider;
    }

    void OnDisable()
    {
        playerCore.OnEnergyChanged -= UpdateSlider;
    }
    
    void UpdateSlider(float current, float max)
    {
        _slider.value = current;
    }
}