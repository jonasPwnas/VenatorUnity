using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnMouseHovering : MonoBehaviour, IPointerEnterHandler
{
    MenuAudio menuAudio;

    private void Awake()
    {
        menuAudio = FindAnyObjectByType<MenuAudio>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (menuAudio != null) 
        {
            menuAudio.PlayNavSound();
        }
    }
}
