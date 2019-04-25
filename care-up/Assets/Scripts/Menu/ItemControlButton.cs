﻿using UnityEngine;

public class ItemControlButton : MonoBehaviour
{
    public GameUI.ItemControlButtonType buttonType;
    GameUI gameUI;

    void Start()
    {
        gameUI = GameObject.FindObjectOfType<GameUI>();
    }


    public void updateBlinkState()
    {
        GetComponent<Animator>().ResetTrigger("BlinkOn");
        GetComponent<Animator>().ResetTrigger("BlinkOff");
        bool toBlink = false;
        if (gameUI == null)
        {
            gameUI = GameObject.FindObjectOfType<GameUI>();
        }

        if (buttonType == GameUI.ItemControlButtonType.DropLeft)
        {
            if (gameUI.DropLeftBlink)
            {
                GetComponent<Animator>().SetTrigger("BlinkOn");
                toBlink = true;
            }
        }
        else if (buttonType == GameUI.ItemControlButtonType.DropRight)
        {
            if (gameUI.DropRightBlink)
            {
                GetComponent<Animator>().SetTrigger("BlinkOn");
                toBlink = true;
            }
        }
        else if (buttonType == GameUI.ItemControlButtonType.MoveLeft || buttonType == GameUI.ItemControlButtonType.MoveRight)
        {
            if (gameUI.moveButtonToBlink == buttonType)
            {
                {
                    GetComponent<Animator>().SetTrigger("BlinkOn");
                    toBlink = true;
                }
            }
        }
        else if (gameUI.buttonToBlink == buttonType)
        {
            {
                GetComponent<Animator>().SetTrigger("BlinkOn");
                toBlink = true;
            }
        }

        if (!toBlink && GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("ItemBlink"))
            GetComponent<Animator>().SetTrigger("BlinkOff");
    }

    private void OnEnable()
    {
        updateBlinkState();
    }   
}
