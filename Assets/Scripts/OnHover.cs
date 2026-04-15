using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] string tooltipText;

    GameObject tooltipPanel;
    TMP_Text tooltipLabel;
    RectTransform rt;
    Canvas canvas;
    CanvasGroup canvasGroup;

    void Awake()
    {
        tooltipPanel = Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(g => g.name == "TooltipPanel");

        tooltipLabel = tooltipPanel.GetComponentInChildren<TMP_Text>();
        rt = tooltipPanel.GetComponent<RectTransform>();
        canvas = tooltipPanel.GetComponentInParent<Canvas>();

        // Add CanvasGroup if missing
        canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();

        tooltipPanel.SetActive(true);   // keep active
        canvasGroup.alpha = 0f;         // but invisible
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipLabel.text = tooltipText;
        StartCoroutine(ShowTooltip());
    }

    IEnumerator ShowTooltip()
    {
        // Hide first (prevents 0,0 flicker)
        canvasGroup.alpha = 0f;

        // Wait 1 frame for layout to settle
        yield return null;

        // Force layout AFTER text is set
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        // Position once immediately
        UpdateTooltipPosition();

        // Show
        canvasGroup.alpha = 1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (canvasGroup.alpha > 0f)
        {
            UpdateTooltipPosition();
        }
    }

    void UpdateTooltipPosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Pivot flip (stable)
        Vector2 pivot = new Vector2(
            mousePos.x < Screen.width * 0.5f ? 0 : 1,
            mousePos.y < Screen.height * 0.5f ? 0 : 1
        );

        rt.pivot = pivot;

        // Convert to canvas space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePos,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        // Offset so it doesn't overlap cursor
        Vector2 offset = new Vector2(18, -15);

        Vector2 finalPos = localPoint + new Vector2(
            pivot.x == 0 ? offset.x : -offset.x,
            pivot.y == 1 ? offset.y : -offset.y
        );

        rt.localPosition = finalPos;
    }
}