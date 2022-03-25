using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ToolsPanel : MonoBehaviour
{
    [SerializeField] private Transform toolsHolder;
    [SerializeField] private GameObject toolButtonPrefab;

    [Inject] private ICanvasInputManager canvasInputManager;
    [Inject] private List<IDrawingTool> drawingTools;

    private IDrawingTool currentTool;

    private void Start()
    {
        InitializeButtons();
        SelectTool(drawingTools.First());
    }

    private void OnDestroy()
    {
        if (canvasInputManager != null)
            UnsubscribeCurrentTool();
    }

    private void InitializeButtons()
    {
        foreach (var tool in drawingTools)
        {
            var newToolButton = Instantiate(toolButtonPrefab, toolsHolder);
            newToolButton.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(tool.IconPath);
            newToolButton.GetComponent<Button>().onClick.AddListener(() => SelectTool(tool));
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) toolsHolder);
    }

    private void SelectTool(IDrawingTool tool)
    {
        UnsubscribeCurrentTool();
        currentTool = tool;
        SubscribeCurrentTool();
    }

    private void SubscribeCurrentTool()
    {
        if (currentTool == null) return;
        canvasInputManager.OnTouchBegin += currentTool.UseBegin;
        canvasInputManager.OnTouchDrag += currentTool.UseDrag;
        canvasInputManager.OnTouchEnd += currentTool.UseEnd;
    }

    private void UnsubscribeCurrentTool()
    {
        if (currentTool == null) return;
        canvasInputManager.OnTouchBegin -= currentTool.UseBegin;
        canvasInputManager.OnTouchDrag -= currentTool.UseDrag;
        canvasInputManager.OnTouchEnd -= currentTool.UseEnd;
    }
}