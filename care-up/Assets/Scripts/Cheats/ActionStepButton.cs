﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CareUp.Actions;
using UnityEngine.UI;


public class ActionStepButton : MonoBehaviour {
    Action action;
    ActionsPanel.Mode lastMode;
    public Text main_text;
    public GameObject icons;
    ActionsPanel actionsPanel;
	void Start () {
        actionsPanel = GameObject.FindObjectOfType<ActionsPanel>();

    }

    public void setAction(Action a)
    {
        action = a;
        if (main_text != null)
            main_text.text = action.SubIndex.ToString() + " " + action.Type.ToString();
    }

    public void updateLook(int currentIndex)
    {
        if (action == null)
            return;
        if (lastMode != actionsPanel.mode)
        {
            icons.SetActive(false);
            lastMode = actionsPanel.mode;
            if (lastMode == ActionsPanel.Mode.ShortDescr)
            {
                main_text.text = action.SubIndex.ToString() + " " + action.Type.ToString();
            }
            else if (lastMode == ActionsPanel.Mode.Type)
            {
                string ss = action.SubIndex.ToString() + " " + action.Type.ToString();
                //ss += action.
                ss += "\n";
                string[] ObjectNames = new string[0];
                action.ObjectNames(out ObjectNames);
                foreach (string s in ObjectNames)
                {
                    ss += s + "  ";
                }
                main_text.text = ss;
            }
            else if (lastMode == ActionsPanel.Mode.Comment)
            {
                main_text.text = action.comment;
            }
            else if (lastMode == ActionsPanel.Mode.CommentUA)
            {
                main_text.text = action.commentUA;
            }
            else if (lastMode == ActionsPanel.Mode.Icons)
            {
                main_text.text = action.Type.ToString();
                icons.SetActive(true);
                string[] ObjectNames = new string[0];
                action.ObjectNames(out ObjectNames);
                Sprite x = Resources.Load("Sprites/Prefab_Icons/x", typeof(Sprite)) as Sprite;
                if (ObjectNames.Length >= 1)
                {
                    Sprite l = Resources.Load("Sprites/Prefab_Icons/" + ObjectNames[0], typeof(Sprite)) as Sprite;
                    if (l != null)
                        icons.transform.Find("left").GetComponent<Image>().sprite = l;
                    else
                        icons.transform.Find("left").GetComponent<Image>().sprite = x;
                }
                else
                {
                    icons.transform.Find("left").GetComponent<Image>().sprite = x;
                }
                if (ObjectNames.Length >= 2)
                {
                    Sprite r = Resources.Load("Sprites/Prefab_Icons/" + ObjectNames[1], typeof(Sprite)) as Sprite;

                    if (r != null)
                        icons.transform.Find("right").GetComponent<Image>().sprite = r;
                    else
                        icons.transform.Find("right").GetComponent<Image>().sprite = x;
                }
                else
                {
                    icons.transform.Find("right").GetComponent<Image>().sprite = x;
                }
            }
        }
        int index = action.SubIndex;
        if (currentIndex == index)
        {
            GetComponent<Image>().color = new Color(0f, 1f, 0f, 0.4f);
        }
        else if (index < currentIndex)
        {
            GetComponent<Image>().color = new Color(.5f, .5f, .5f, 0.4f);
        }
        else
        {
            GetComponent<Image>().color = new Color(1f, .6f, 0f, 0.4f);
        }
    }

}
