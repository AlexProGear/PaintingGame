using UnityEngine;
using UnityEngine.UI;

public class DrawingManager : MonoBehaviour
{
    public Color SelectedColor { get; set; }
    public Image CanvasImage { get; private set; }

    private void Start()
    {
        CanvasImage = GetComponent<Image>();
        Texture2D originalTexture = CanvasImage.sprite.texture;
        Texture2D drawingTexture = new Texture2D(originalTexture.width, originalTexture.height);
        drawingTexture.SetPixels(originalTexture.GetPixels());
        drawingTexture.Apply();
        CanvasImage.sprite = Sprite.Create(drawingTexture, CanvasImage.sprite.rect, CanvasImage.sprite.pivot);
    }
}