using PaintIn3D;
using UnityEngine;
using UnityEngine.UI;

public class ColorPalette : MonoBehaviour, IBrushColorProvider, IBrushSizeProvider
{
    [SerializeField] private Slider redSlider;
    [SerializeField] private Slider greenSlider;
    [SerializeField] private Slider blueSlider;
    [SerializeField] private Slider brushSizeSlider;

    [SerializeField] private P3dPaintSphere paintSphere;

    public Color Color => new Color(redSlider.value, greenSlider.value, blueSlider.value, 1);
    public float BrushSize => brushSizeSlider.value * 0.5f + 0.01f;

    private void Awake()
    {
        redSlider.onValueChanged.AddListener(f => paintSphere.Color = Color);
        greenSlider.onValueChanged.AddListener(f => paintSphere.Color = Color);
        blueSlider.onValueChanged.AddListener(f => paintSphere.Color = Color);
        brushSizeSlider.onValueChanged.AddListener(f => paintSphere.Radius = BrushSize);
        paintSphere.Color = Color;
        paintSphere.Radius = BrushSize;
    }
}