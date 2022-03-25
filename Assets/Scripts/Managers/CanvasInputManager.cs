using System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ICanvasInputManager
{
    public event Action<Vector2> OnTouchBegin;
    public event Action<Vector2> OnTouchEnd;
    public event Action<Vector2> OnTouchDrag;
}

public class CanvasInputManager : MonoBehaviour, ICanvasInputManager, IPointerDownHandler, IPointerUpHandler
{
    public event Action<Vector2> OnTouchBegin;
    public event Action<Vector2> OnTouchEnd;
    public event Action<Vector2> OnTouchDrag;

    private bool pressed;

    private void Update()
    {
        if (!pressed) return;
        Vector2 position;
        if (Input.touchCount > 0)
        {
            position = Input.touches[0].position;
        }
        else
        {
            position = Input.mousePosition;
        }

        OnTouchDrag?.Invoke(position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
        OnTouchBegin?.Invoke(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
        OnTouchEnd?.Invoke(eventData.position);
    }
}