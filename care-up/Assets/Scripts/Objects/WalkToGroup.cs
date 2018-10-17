﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WalkToGroup : MonoBehaviour
{
    public Vector3 position;
    public Vector3 rotation;
    
    public Vector3 robotPosition;
    public Vector3 robotRotation;

    private GameObject text;

    CameraMode cameraMode;
    Controls controls;

    GameObject gameLogic;
    ParticleSystem particles;

    private Transform target;

    public Vector3 Position
    {
        get { return (target == null) ? position : target.position; }
    }

    public Vector3 Rotation
    {
        get { return (target == null) ? rotation : target.rotation.eulerAngles; }
    }

    public void HighlightGroup(bool value)
    {
        if (SystemInfo.deviceType == DeviceType.Handheld)
            return;
        text.SetActive(value);

        text.transform.rotation = Camera.main.transform.rotation;

        if (particles != null)
        {
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = value;
        }
    }

    private void Start()
    {
        gameLogic = GameObject.Find("GameLogic");

        cameraMode = gameLogic.GetComponent<CameraMode>();
        controls = gameLogic.GetComponent<Controls>();

        text = transform.GetChild(0).gameObject;
        if (SystemInfo.deviceType != DeviceType.Handheld)
        {
            text.SetActive(false);
        }

        particles = GetComponent<ParticleSystem>();
        if (particles != null)
        {
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;
        }

        if (transform.Find("Target") != null)
        {
            target = transform.Find("Target").transform;
        }
        else
        {
            target = null;
        }
    }

    protected void Update()
    {
        if (cameraMode.CurrentMode == CameraMode.Mode.Free)
        {
            if (controls.SelectedObject == gameObject && !cameraMode.animating /*&& (player.away || player.freeLook)*/)
            {
                if (gameLogic.GetComponent<TutorialManager>() != null)
                    if (gameLogic.GetComponent<TutorialManager>().TutorialEnding)
                        return;
                HighlightGroup(true);
            }
            else
            {
                HighlightGroup(false);
            }
        }
    }
}