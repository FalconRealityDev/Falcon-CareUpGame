﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WalkToGroupButton : MonoBehaviour {

    WalkToGroup linkedWalkToGroup = null;
    Color ButtonColor = Color.white;
    bool mouse_over = false;
    //button components
    [HideInInspector]
    public GameObject blur;
    [HideInInspector]
    public GameObject bg;
    [HideInInspector]
    public GameObject bg_h;
    [HideInInspector]
    public GameObject _icon;

    public bool SideButton = false;

    void OnMouseOver()
    {
        //If your mouse hovers over the GameObject with the script attached, output this message
        Debug.Log("Mouse is over GameObject.  " + name);
    }
    
    void Start()
    {
        ButtonColor = GetComponent<Image>().color;
        if (transform.Find("blur"))
        {
            blur = transform.Find("blur").gameObject;
            Color b2 = ButtonColor;
            b2.a = 0.4f;
            blur.GetComponent<Image>().color = b2;
            blur.SetActive(false);
            bg = transform.Find("bg").gameObject;
            bg_h = transform.Find("bg_h").gameObject;
            bg_h.SetActive(false);
            _icon = transform.Find("icon").gameObject;
            if (!SideButton)
               _icon.GetComponent<Image>().sprite = Resources.Load("Sprites/WalkGroup_Icons/" + name, typeof(Sprite)) as Sprite;
        }
    }

    public void setWalkToGroup(WalkToGroup wtg)
    {
        linkedWalkToGroup = wtg;
        if (SideButton)
        {
            GameUI gameUI = GameObject.FindObjectOfType<GameUI>();
            if (_icon == null)
                _icon = transform.Find("icon").gameObject;
            switch (wtg.WalkToGroupType)
            {
                case WalkToGroup.GroupType.WorkField:
                    _icon.GetComponent<Image>().sprite = Resources.Load("Sprites/WalkGroup_Icons/MoveWorkfield" , typeof(Sprite)) as Sprite;
                    GetComponent<Image>().color = gameUI.WTGButtons["WorkField"].GetComponent<Image>().color;
                    break;
                case WalkToGroup.GroupType.Doctor:
                    _icon.GetComponent<Image>().sprite = Resources.Load("Sprites/WalkGroup_Icons/MoveCollegue", typeof(Sprite)) as Sprite;
                    GetComponent<Image>().color = gameUI.WTGButtons["Doctor"].GetComponent<Image>().color;
                    break;
                case WalkToGroup.GroupType.Patient:
                    _icon.GetComponent<Image>().sprite = Resources.Load("Sprites/WalkGroup_Icons/Movepatient", typeof(Sprite)) as Sprite;
                    GetComponent<Image>().color = gameUI.WTGButtons["Patient"].GetComponent<Image>().color;
                    break;
                case WalkToGroup.GroupType.Sink:
                    _icon.GetComponent<Image>().sprite = Resources.Load("Sprites/WalkGroup_Icons/MoveSink", typeof(Sprite)) as Sprite;
                    GetComponent<Image>().color = gameUI.WTGButtons["Sink"].GetComponent<Image>().color;
                    break;
            }
        }
    }


    public void HighlightGroup(bool value)
    {
        if (linkedWalkToGroup != null)
        linkedWalkToGroup.ButtonHovered = value;
   
        HighlightButton(value);
    }

    public void HighlightButton(bool value)
    {
        mouse_over = value;
        if (blur != null)
        {
            blur.SetActive(value);
            if (value)
            {
                if (!SideButton)
                    GetComponent<Image>().sprite = Resources.Load("Sprites/WalkGroup_Icons/button_ring", typeof(Sprite)) as Sprite;
                else
                    GetComponent<Image>().sprite = Resources.Load("Sprites/WalkGroup_Icons/sideButton_ring_h", typeof(Sprite)) as Sprite;

            }
            else
            {
                if (!SideButton)
                    GetComponent<Image>().sprite = Resources.Load("Sprites/WalkGroup_Icons/button_ring_small", typeof(Sprite)) as Sprite;
                else
                    GetComponent<Image>().sprite = Resources.Load("Sprites/WalkGroup_Icons/sideButton_ring", typeof(Sprite)) as Sprite;


            }
            bg_h.SetActive(value);
            bg.SetActive(!value);
        }

    }

    void OnEnable()
    {
        HighlightButton(false);
        PointerEventData pe = new PointerEventData(EventSystem.current);
        pe.position = Input.mousePosition;
        List<RaycastResult> hits = new List<RaycastResult>();
        if (EventSystem.current != null)
        {
            EventSystem.current.RaycastAll(pe, hits);
            foreach (RaycastResult h in hits)
            {
                if (h.gameObject == gameObject)
                    HighlightButton(true);
            }
        }
    }

    public void MoveToGroup()
    {       
        if (linkedWalkToGroup != null)
        {
            GameObject.FindObjectOfType<PlayerScript>().WalkToGroup(linkedWalkToGroup);
            linkedWalkToGroup.ButtonHovered = false;
            if (!SideButton)
                HighlightButton(false);
        }
    }
}
