using UnityEngine;
using UnityEngine.UI;

public class ColorPalette : MonoBehaviour
{
    [SerializeField] private Slider redSlider;
    [SerializeField] private Slider greenSlider;
    [SerializeField] private Slider blueSlider;
    [SerializeField] private Slider brushSizeSlider;
    
    public Color Color => new Color(redSlider.value, greenSlider.value, blueSlider.value, 1);
    public float BrushSize => brushSizeSlider.value * 0.5f + 0.01f;
}