using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PalettePanel : MonoBehaviour
{
    [SerializeField] private Transform paletteHolder;
    [SerializeField] private GameObject toolButtonPrefab;
    [SerializeField] private Color[] colors =
    {
        Color.black, Color.white, Color.red, Color.yellow, Color.green, Color.blue, Color.magenta
    };

    [Inject] private DrawingManager drawingManager;

    private void Start()
    {
        InitializeButtons();
        SelectColor(colors.First());
    }

    private void InitializeButtons()
    {
        foreach (var color in colors)
        {
            var newToolButton = Instantiate(toolButtonPrefab, paletteHolder);
            newToolButton.GetComponent<Image>().color = color;
            newToolButton.transform.GetChild(0).gameObject.SetActive(false);
            newToolButton.GetComponent<Button>().onClick.AddListener(() => SelectColor(color));
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) paletteHolder);
    }

    private void SelectColor(Color color)
    {
        drawingManager.SelectedColor = color;
    }
}