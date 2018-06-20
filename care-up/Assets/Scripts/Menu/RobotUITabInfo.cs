﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotUITabInfo : RobotUITabs {

    private RectTransform selectedButton;
    private RectTransform deselectButton;

    private bool set = false;
    private float timer = 0.0f;

    private float initButtonWidth  = 0;
    private float initButtonHeight = 0;
	public GameObject ItemList;
	public GameObject ItemListButton;
    

    protected override void Start()
    {
        base.Start();

        //Transform t = transform.Find("InfoDynamicCanvas/ItemList/Scroll View/Viewport/Content").GetChild(0);
        //if ( t != null )
        //{
        //    OnItemButtonClick(t.GetComponent<RectTransform>());
        //}
		//ItemList.SetActive(false);


        // generate buttons
        // done in playerspawn
    }


	public void FullScreenPDF()
	{
		GameObject FullScreenPDF_UI = Instantiate(Resources.Load("Prefabs/PDFFullScreen")) as GameObject; 
	}

       
	public void ToggleItemList()
	{
		ItemListButton.GetComponent<Animator>().SetTrigger("BlinkStop");
		ItemList.SetActive(!ItemList.activeSelf);
	}

	public void SwitchItemList(bool value)
    {
		ItemList.SetActive(value);
    }


    private void Update()
    {
        if (set)
        {
            timer += Time.deltaTime;
            float maxTime = 0.3f;

            if (selectedButton != null)
            {
                float sX = (timer / maxTime) * 45.0f;
                selectedButton.sizeDelta = new Vector2(initButtonWidth + sX, initButtonHeight);
            }

            if (deselectButton != null)
            {
                float dX = ((maxTime - timer) / maxTime) * 45.0f;
                deselectButton.sizeDelta = new Vector2(initButtonWidth + dX, initButtonHeight);
            }

            if (timer >= maxTime)
            {
                timer = 0.0f;
                set = false;
            }
        }
    }

    public void OnItemButtonClick(RectTransform caller)
    {
        if (selectedButton != caller)
        {
            if (selectedButton != null)
            {
                deselectButton = selectedButton;
                deselectButton.GetComponent<RobotUIInfoButton>().Toggle(false);
            }

            selectedButton = caller;
            selectedButton.GetComponent<RobotUIInfoButton>().Toggle(true);

            set = true;

            initButtonWidth = selectedButton.sizeDelta.x;
            initButtonHeight = selectedButton.sizeDelta.y;
        }
		ToggleItemList();
    }

    protected override void SetTabActive(bool value)
    {
        base.SetTabActive(value);

        if (selectedButton != null)
        {
            selectedButton.GetComponent<RobotUIInfoButton>().Toggle(value);
        }
    }
}
