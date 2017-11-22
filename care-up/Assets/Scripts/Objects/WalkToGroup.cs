﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WalkToGroup : MonoBehaviour
{

    public Vector3 position;
    public Vector3 rotation;

    private GameObject text;

    CameraMode cameraMode;
    Controls controls;

    public void HighlightGroup(bool value)
    {
        text.SetActive(value);
    }

    private void Start()
    {
        GameObject gameLogic = GameObject.Find("GameLogic");

        cameraMode = gameLogic.GetComponent<CameraMode>();
        controls = gameLogic.GetComponent<Controls>();

        text = transform.GetChild(0).gameObject;
        text.SetActive(false);
    }

    protected void Update()
    {
        if (cameraMode.CurrentMode == CameraMode.Mode.Free)
        {
            if (controls.SelectedObject == gameObject && !cameraMode.animating)
            {
                HighlightGroup(true);
            }
            else
            {
                HighlightGroup(false);
            }
        }
    }
}