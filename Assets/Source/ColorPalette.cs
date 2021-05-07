using System;
using PaintIn3D;
using UnityEngine;
using UnityEngine.UI;

public class ColorPalette : MonoBehaviour
{
    [SerializeField] private Slider redSlider;
    [SerializeField] private Slider greenSlider;
    [SerializeField] private Slider blueSlider;
    [SerializeField] private Slider brushSizeSlider;

    [SerializeField] private Image redSliderBackground;
    [SerializeField] private Image greenSliderBackground;
    [SerializeField] private Image blueSliderBackground;

    [SerializeField] private Image resultColor;

    [SerializeField] private P3dPaintSphere paintSphere;

    private static readonly int Color1 = Shader.PropertyToID("_Color");
    private static readonly int Color2 = Shader.PropertyToID("_Color2");

    private Color _color;

    public float BrushSize => brushSizeSlider.value * 0.5f + 0.01f;

    private void Awake()
    {
        redSlider.onValueChanged.AddListener(f => OnColorChanged());
        greenSlider.onValueChanged.AddListener(f => OnColorChanged());
        blueSlider.onValueChanged.AddListener(f => OnColorChanged());
        brushSizeSlider.onValueChanged.AddListener(f => OnColorChanged());
        paintSphere.Color = _color;
        paintSphere.Radius = BrushSize;
    }

    private void OnColorChanged()
    {
        _color = new Color(redSlider.value, greenSlider.value, blueSlider.value, 1);

        redSliderBackground.material.SetColor(Color1, new Color(0, greenSlider.value, blueSlider.value));
        redSliderBackground.material.SetColor(Color2, new Color(1, greenSlider.value, blueSlider.value));
        greenSliderBackground.material.SetColor(Color1, new Color(redSlider.value, 0, blueSlider.value));
        greenSliderBackground.material.SetColor(Color2, new Color(redSlider.value, 1, blueSlider.value));
        blueSliderBackground.material.SetColor(Color1, new Color(redSlider.value, greenSlider.value, 0));
        blueSliderBackground.material.SetColor(Color2, new Color(redSlider.value, greenSlider.value, 1));

        resultColor.color = _color;
        paintSphere.Color = _color;
    }
}