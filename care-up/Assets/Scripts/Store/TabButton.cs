﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{  
    [HideInInspector]
    public Image background;

    public UnityEvent onTabSelected;
    public UnityEvent onTabDeselected;

    private TabGroup tabGroup;

    public void OnPointerClick(PointerEventData eventData)
    {
        //tabGroup.OnTabSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //tabGroup.OnTabExit(this);
    }

    public void Select()
    {
        if (onTabSelected != null)
        {
            onTabSelected.Invoke();
        }
    }

    public void Deselect()
    {
        if (onTabDeselected != null)
        {
            onTabDeselected.Invoke();
        }
    }


    private void Awake()
    {
        tabGroup = transform.parent.gameObject.GetComponent<TabGroup>();
        //tabGroup.Subscribe(this);
        background = GetComponent<Image>();
    }    
}