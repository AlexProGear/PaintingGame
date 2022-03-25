using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public interface IDrawingTool
{
    public string IconPath { get; }
    public void UseBegin(Vector2 position);
    public void UseDrag(Vector2 position);
    public void UseEnd(Vector2 position);
}

public abstract class AbstractDrawingTool : IDrawingTool
{
    [Inject] protected DrawingManager drawingManager;
    public abstract string IconPath { get; }

    private static Texture2D texture;
    private static Color[] pixels;

    // Lazy initialization
    protected Texture2D Texture => texture ??= drawingManager.CanvasImage.sprite.texture;
    private Color[] Pixels => pixels ??= Texture.GetPixels();

    public virtual void UseBegin(Vector2 position)
    {
    }

    public virtual void UseDrag(Vector2 position)
    {
    }

    public virtual void UseEnd(Vector2 position)
    {
    }
    
    protected Color GetPixel(int x, int y)
    {
        return Pixels[x + y * Texture.width];
    }

    protected void PaintPixel(int x, int y)
    {
        Pixels[x + y * Texture.width] = drawingManager.SelectedColor;
    }

    protected void ApplyTextureChanges()
    {
        Texture.SetPixels(Pixels);
        Texture.Apply();
    }

    protected Vector2Int CalculateTextureCoordinate(Vector2 position)
    {
        Image image = drawingManager.CanvasImage;
        Texture2D texture = image.sprite.texture;
        Rect rect = image.rectTransform.rect;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            image.rectTransform, position, null, out Vector2 localPoint);

        int px = (int)Mathf.Clamp((localPoint.x - rect.x) * texture.width / rect.width, 0, texture.width);
        int py = (int)Mathf.Clamp(((localPoint.y - rect.y) * texture.height / rect.height), 0, texture.height);
        return new Vector2Int(px, py);
    }
}

public class PencilDrawingTool : AbstractDrawingTool
{
    public override string IconPath => "Sprites/ToolsIcons/pencil";
    private const int BrushWidth = 5;
    private const int BrushWidthSqr = BrushWidth * BrushWidth;

    private Vector2Int lastPosition;

    public override void UseBegin(Vector2 position)
    {
        lastPosition = CalculateTextureCoordinate(position);
        Draw(position);
    }

    public override void UseDrag(Vector2 position)
    {
        Draw(position);
    }

    private void Draw(Vector2 position)
    {
        Vector2Int newPosition = CalculateTextureCoordinate(position);
        BresenhamLine(newPosition, lastPosition);
        ApplyTextureChanges();
        lastPosition = newPosition;
    }

    private void BresenhamLine(Vector2Int start, Vector2Int end)
    {
        int x0 = start.x, y0 = start.y;
        int x1 = end.x, y1 = end.y;
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int error = dx + dy;

        while (true)
        {
            DrawCircleOnTexture(x0, y0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * error;
            if (e2 >= dy)
            {
                if (x0 == x1) break;
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                if (y0 == y1) break;
                error += dx;
                y0 += sy;
            }
        }
    }

    private void DrawCircleOnTexture(int x, int y)
    {
        int minX = Mathf.Clamp(x - BrushWidth, 0, Texture.width);
        int maxX = Mathf.Clamp(x + BrushWidth, 0, Texture.width - 1);
        int minY = Mathf.Clamp(y - BrushWidth, 0, Texture.height);
        int maxY = Mathf.Clamp(y + BrushWidth, 0, Texture.height - 1);
        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                int dx = i - x, dy = j - y;
                if (dx * dx + dy * dy < BrushWidthSqr)
                {
                    PaintPixel(i, j);
                }
            }
        }
    }
}

public class PaintBucketDrawingTool : AbstractDrawingTool
{
    public override string IconPath => "Sprites/ToolsIcons/paint_bucket";
    private const int IterationsLimit = 1000000;

    public override void UseBegin(Vector2 position)
    {
        FloodFill(position);
    }

    private void FloodFill(Vector2 position)
    {
        Vector2Int textureCoordinate = CalculateTextureCoordinate(position);
        Color targetColor = Texture.GetPixel(textureCoordinate.x, textureCoordinate.y);
        if (targetColor == drawingManager.SelectedColor)
            return;
        Queue<Vector2Int> toProcess = new Queue<Vector2Int>();
        toProcess.Enqueue(textureCoordinate);

        for (int i = 0; i < IterationsLimit && toProcess.Count > 0; i++)
        {
            Vector2Int current = toProcess.Dequeue();
            int x = current.x;
            int y = current.y;
            Color currentColor = GetPixel(x, y);
            if (currentColor == drawingManager.SelectedColor || currentColor != targetColor)
            {
                i--;
                continue;
            }

            PaintPixel(x, y);
            if (x - 1 >= 0)
                toProcess.Enqueue(new Vector2Int(x - 1, y));
            if (x + 1 < Texture.width)
                toProcess.Enqueue(new Vector2Int(x + 1, y));
            if (y - 1 >= 0)
                toProcess.Enqueue(new Vector2Int(x, y - 1));
            if (y + 1 < Texture.height)
                toProcess.Enqueue(new Vector2Int(x, y + 1));
        }

        if (toProcess.Count > 0)
            Debug.LogError($"Iterations limit reached! {toProcess.Count} elements left unprocessed!");
        ApplyTextureChanges();
    }
}