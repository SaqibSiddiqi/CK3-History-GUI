using UnityEngine;
using UnityEngine.EventSystems;

public class OnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse entered button area");
        // Add hover logic here (e.g., scale up)
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse left button area");
        // Add reset logic here
    }
}
