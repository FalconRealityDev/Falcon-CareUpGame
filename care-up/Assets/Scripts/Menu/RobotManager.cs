﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RobotManager : MonoBehaviour {

    public bool top = true;

    private GameObject UI_object;
    private GameObject UI_trigger;
    
    private static RobotManager instance;
    
    private static Transform eyeL;
    private static Transform eyeLA;
    private static Transform eyeR;
    private static Transform eyeRA;
    private static Transform mouth;
    private static Transform mouthA;

    private static Material eyeLMat;
    private static Material eyeRMat;
    private static Material mouthMat;

    private static Material robotHandMat;
    private static Material robotFaceMat;

    void Start ()
    {
        instance = this;

        Transform face = transform.Find("robotArm").Find("main").Find("face");
        eyeL = face.Find("mEye").Find("eye.L");
        eyeR = face.Find("mEye").Find("eye.R");
        eyeLA = face.Find("mAnker").Find("anker.L");
        eyeRA = face.Find("mAnker").Find("anker.R");
        mouth = face.Find("mouth");
        mouthA = face.Find("mouthAnker");

        mouthMat = transform.Find("robot_mouth").GetComponent<Renderer>().material;
        eyeLMat = transform.Find("robot_eye.L").GetComponent<Renderer>().material;
        eyeRMat = transform.Find("robot_eye.R").GetComponent<Renderer>().material;

        UI_object = Camera.main.transform.Find("UI (1)").Find("RobotUI").gameObject;
        UI_object.SetActive(false);

        UI_trigger = Camera.main.transform.Find("UI").Find("RobotUITrigger").gameObject;
        UI_trigger.SetActive(true);

        EventTrigger.Entry event1 = new EventTrigger.Entry();
        event1.eventID = EventTriggerType.PointerEnter;
        event1.callback.AddListener((eventData) => { OnEnterHover(); });

        EventTrigger.Entry event2 = new EventTrigger.Entry();
        event2.eventID = EventTriggerType.PointerExit;
        event2.callback.AddListener((eventData) => { OnExitHover(); });

        EventTrigger.Entry event3 = new EventTrigger.Entry();
        event3.eventID = EventTriggerType.PointerClick;
        event3.callback.AddListener((eventData) => { OnExitHover(); });

        UI_trigger.AddComponent<EventTrigger>();
        UI_trigger.GetComponent<EventTrigger>().triggers.Add(event1);
        UI_trigger.GetComponent<EventTrigger>().triggers.Add(event2);
        UI_trigger.GetComponent<EventTrigger>().triggers.Add(event3);

        robotHandMat = transform.Find("robot").GetComponent<Renderer>().material;
        robotFaceMat = transform.Find("robot_face").GetComponent<Renderer>().material;
    }

    public void OnEnterHover()
    {
        Color color = new Color(0.0f, 0.831f, 1.0f);
        robotHandMat.color = color;
        robotFaceMat.color = color;
    }

    public void OnExitHover()
    {
        Color color = new Color(1.0f, 1.0f, 1.0f);
        robotHandMat.color = color;
        robotFaceMat.color = color;
    }

    void Update ()
    {
        UpdateTriggerPosition();
        UpdateFaceAnimations();
	}

    public void TriggerUI(bool value)
    {
        // play some kind of UI animation?
        UI_object.SetActive(value);

        if (!value)
        {
            GameObject.FindObjectOfType<PlayerScript>().ResetUIHover();
        }
    }

    private void UpdateTriggerPosition()
    {
        float x = top ? Screen.width - 182.9f : 182.9f;
        float y = Screen.height * 0.63f;
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 4.0f));

        transform.LookAt(Camera.main.transform);
        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            0.0f);

        Vector3 robotPosition = Camera.main.WorldToScreenPoint(transform.position);
        robotPosition += new Vector3(7.0f, 3.0f); // slight offset
        UI_trigger.transform.position = robotPosition;
    }

    public static void RobotCorrectAction()
    {
        instance.GetComponent<Animator>().SetTrigger("Yes");
    }

    public static void RobotWrongAction()
    {
        instance.GetComponent<Animator>().SetTrigger("No");
    }

    private void UpdateFaceAnimations()
    {
        eyeLMat.mainTextureOffset = new Vector2(
            (eyeL.parent.localPosition.x - eyeL.localPosition.x - eyeLA.parent.localPosition.x - eyeLA.localPosition.x - 0.1772721f) * 2,
            (eyeLA.parent.localPosition.y - eyeLA.localPosition.y - eyeL.parent.localPosition.y - eyeL.localPosition.y - 0.1221102f) * 2
            );

        eyeRMat.mainTextureOffset = new Vector2(
            (eyeR.parent.localPosition.x - eyeR.localPosition.x - eyeRA.parent.localPosition.x - eyeRA.localPosition.x + 0.1772721f) * 2,
            (eyeRA.parent.localPosition.y - eyeRA.localPosition.y - eyeR.parent.localPosition.y - eyeR.localPosition.y - 0.1221102f) * 2
            );
        mouthMat.mainTextureOffset = new Vector2(
            (mouth.localPosition.x - mouthA.localPosition.x) * 2f,
            (mouth.localPosition.y - mouthA.localPosition.y) * 2f * 1.11900882674f
            );
    } 

    public void ToggleTrigger(bool value)
    {
        UI_trigger.SetActive(value);
    }

    public void ToggleCloseBtn(bool value)
    {
        UI_object.transform.Find("CloseBtn").gameObject.SetActive(value);
    }
}
	 