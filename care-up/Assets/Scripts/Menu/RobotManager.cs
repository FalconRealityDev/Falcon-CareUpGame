﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotManager : MonoBehaviour {

    private GameObject UI_object;
    private GameObject UI_trigger;

    private Vector3 initialPosition;
    private float timer = 0.0f;
    private int direction = 1;
    
	void Start ()
    {
        UI_object = Camera.main.transform.Find("UI").Find("RobotUI").gameObject;
        UI_object.SetActive(false);

        UI_trigger = Camera.main.transform.Find("UI").Find("RobotUITrigger").gameObject;
        UI_trigger.SetActive(true);

        initialPosition = transform.localPosition;
	}
	
	void Update ()
    {
        UpdateAnimation();
        UpdateTriggerPosition();
	}

    public void TriggerUI()
    {
        // play some kind of UI animation?
        UI_object.SetActive(!UI_object.activeSelf);
        // also play some kind of robot animation?
    }

    private void UpdateAnimation()
    {
        timer += direction * Time.deltaTime;
        transform.localPosition = initialPosition + new Vector3(0.0f, timer * 0.05f, 0.0f);

        bool condition = (direction == 1) ? (timer >= 1.0f) : (timer <= -1.0f);
        if (condition) { direction *= -1; }
    }

    private void UpdateTriggerPosition()
    {
        Vector3 robotPosition = Camera.main.WorldToScreenPoint(transform.position);
        robotPosition += new Vector3(7.0f, 3.0f); // slight offset
        UI_trigger.transform.position = robotPosition;
    }
}
