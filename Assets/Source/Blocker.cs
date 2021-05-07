using Source.Networking;
using TMPro;
using UnityEngine;

public class Blocker : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private TextMeshProUGUI _textMeshProUgui;

    public string Text
    {
        get => _textMeshProUgui.text;
        set => _textMeshProUgui.text = value;
    }

    public void SetVisible(bool isVisible)
    {
        _canvasGroup.alpha = isVisible ? 1 : 0;
        _canvasGroup.blocksRaycasts = isVisible;
        _canvasGroup.interactable = isVisible;
    }

    void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _textMeshProUgui = GetComponentInChildren<TextMeshProUGUI>();

        var networking = FindObjectOfType<PaintSphereNetworking>();
        networking.OnResourceLoadStarted += NetworkingResourceLoadStarted;
        networking.OnResourceLoadEnded += NetworkingResourceLoadEnded;
    }

    private void NetworkingResourceLoadEnded()
    {
        SetVisible(false);
    }

    private void NetworkingResourceLoadStarted(float size)
    {
        Text = $"Please, wait...\n\nDownloading resources:\n{size:0.00} kbytes";
        SetVisible(true);
    }
}