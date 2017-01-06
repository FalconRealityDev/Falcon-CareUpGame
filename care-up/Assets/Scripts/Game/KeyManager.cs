﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Key {

    public abstract bool Pressed();
}

[Serializable]
public class KeyBoardKey : Key
{
    private KeyCode mainKey;
    private KeyCode helpKey;

    public KeyBoardKey(KeyCode main, KeyCode help = KeyCode.None)
    {
        mainKey = main;
        helpKey = help;
    }

    public override bool Pressed()
    {
        bool pressed = Input.GetKeyDown(mainKey);

        if (helpKey != KeyCode.None)
        {
            pressed = pressed && Input.GetKey(helpKey);
        }

        return pressed;
    }
}

[Serializable]
public class ControllerKey : Key
{
    private KeyCode mainKey;
    private KeyCode altKey;

    public ControllerKey(KeyCode main, KeyCode alt = KeyCode.None)
    {
        mainKey = main;
        altKey = alt;
    }

    public override bool Pressed()
    {
        bool pressed = Input.GetKeyDown(mainKey);

        if (altKey != KeyCode.None)
        {
            pressed = Input.GetKeyDown(altKey) || pressed;
        }

        return pressed;
    }
}

[Serializable]
public class ControllerAxisKey : Key
{
    private string axisName;
    private int value;

    public ControllerAxisKey(string axis, int v)
    {
        axisName = axis;
        value = v;
    }

    public override bool Pressed()
    {
        return Input.GetAxis(axisName) == value;
    }
}

[Serializable]
public class InputKey
{
    private List<Key> keyList;

    public InputKey(KeyBoardKey kkey = null, ControllerKey ckey = null, ControllerAxisKey xkey = null)
    {
        keyList = new List<Key>();

        if (kkey != null)
        {
            keyList.Add(kkey);
        }

        if (ckey != null)
        {
            keyList.Add(ckey);
        }

        if (xkey != null)
        {
            keyList.Add(xkey);
        }
    }

    public bool Pressed()
    {
        bool pressed = false;

        foreach (Key k in keyList)
        {
            if (k.Pressed())
            {
                pressed = true;
            }
        }

        if (pressed)
        {
            if (!Controls.keyUsed)
            {
                Controls.keyUsed = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        else return false;
    }
    
}