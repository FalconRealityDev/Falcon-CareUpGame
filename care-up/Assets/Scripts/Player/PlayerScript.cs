﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
    
public class PlayerScript : MonoBehaviour {
    [HideInInspector]
    public bool tutorial_movementLock = false;
    [HideInInspector]
    public float tutorial_totalLookAround = 0.0f;
    [HideInInspector]
    public float tutorial_totalMoveAround = 0.0f;

    public Camera cam;
    public MouseLook mouseLook = new MouseLook();

    PlayerPrefsManager prefs;
    Controls controls;
    HandsInventory handsInv;

    public bool away = true;
    private Vector3 savedPos;
    private Quaternion savedRot;
    private List<WalkToGroup> groups;

    private bool fade;
    private float fadeTime = 1f;
    private float fadeTimer = 0.0f;
    Texture fadeTex;

    MoveBackButton moveBackButton;
    public ItemControlsUI itemControls;

    public bool usingOnMode = false;
    public bool usingOnHand;

    private GameObject usingOnText;

    public GameObject MoveBackButtonObject
    {
        get { return moveBackButton.gameObject; }
    }

    private void Start()
    {

        mouseLook.Init(transform, cam.transform);

        if (GameObject.Find("Preferences") != null)
        {
            prefs = GameObject.Find("Preferences").GetComponent<PlayerPrefsManager>();
        }

        
        GetComponent<Crosshair>().enabled = ( prefs == null ) ? false : prefs.VR;

        controls = GameObject.Find("GameLogic").GetComponent<Controls>();

        groups = new List<WalkToGroup>(
            GameObject.FindObjectsOfType<WalkToGroup>());

        fadeTex = Resources.Load<Texture>("Sprites/Black");

        moveBackButton = GameObject.Find("MoveBackButton").GetComponent<MoveBackButton>();
        moveBackButton.gameObject.SetActive(false);

        itemControls = GameObject.FindObjectOfType<ItemControlsUI>();
        itemControls.gameObject.SetActive(false);

        handsInv = GameObject.Find("GameLogic").GetComponent<HandsInventory>();

        usingOnText = GameObject.Find("UsingOnModeText");
        usingOnText.SetActive(false);
    }


    private void Update()
    {
        if (prefs != null)
        {
            if (!prefs.VR)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (controls.MouseClicked() && !moveBackButton.mouseOver)
        {
            if (away && controls.SelectedObject != null &&
                controls.SelectedObject.GetComponent<WalkToGroup>())
            {
                WalkToGroup(controls.SelectedObject.GetComponent<WalkToGroup>());
            }
            else if (!away && controls.SelectedObject != null
                && !itemControls.gameObject.activeSelf)
            {
                if (usingOnMode)
                {
                    if (usingOnHand)
                    {
                        if (handsInv.LeftHandObject)
                        {
                            handsInv.LeftHandObject.GetComponent<PickableObject>().Use(usingOnHand);
                        }

                        ToggleUsingOnMode(false);
                    } 
                    else
                    {
                        if (handsInv.RightHandObject)
                        {
                            handsInv.RightHandObject.GetComponent<PickableObject>().Use(usingOnHand);
                        }

                        ToggleUsingOnMode(false);
                    }
                }
                else
                {
                    itemControls.Init(controls.SelectedObject);
                }
            }
        }
        else if (Input.GetMouseButtonDown(1) && usingOnMode)
        {
            ToggleUsingOnMode(false);
        }
        
        moveBackButton.GetComponent<Button>().interactable = !tutorial_movementLock;
    }

    public void ToggleUsingOnMode(bool value)
    {
        usingOnMode = value;
        usingOnText.GetComponent<Text>().text = "Selecteer een object waarmee je " +
            (usingOnHand ?
                (handsInv.LeftHandObject.GetComponent<InteractableObject>().description == "" 
                ? handsInv.LeftHandObject.name : handsInv.LeftHandObject.GetComponent<InteractableObject>().description)
            :   
                (handsInv.RightHandObject.GetComponent<InteractableObject>().description == ""
                ? handsInv.RightHandObject.name : handsInv.RightHandObject.GetComponent<InteractableObject>().description)
            )
            + " wilt gebruiken (Druk op rechtermuisknop om te annuleren).";
        usingOnText.SetActive(value);
    }

    public void WalkToGroup(WalkToGroup group)
    {
        if (away)
        {
            ToggleAway();
            savedPos = transform.position;
            savedRot = transform.GetChild(0).GetChild(0).rotation;
            transform.position = group.position;
            if ( prefs == null || (prefs != null && !prefs.VR))
            {
                transform.GetChild(0).GetChild(0).rotation = Quaternion.Euler(group.rotation);
            }
        }
    }

    private void ToggleAway()
    {
        fade = true;
        away = !away;
        foreach (WalkToGroup g in groups)
        {
            g.HighlightGroup(false);
            g.enabled = away;
            g.GetComponent<Collider>().enabled = away;
        }
        moveBackButton.mouseOver = false;
        moveBackButton.gameObject.SetActive(!away);

        if (away)
        {
            itemControls.Close();
        }
    }

    private void OnGUI()
    {
        if (fade)
        {
            if (fadeTimer > fadeTime)
            {
                fadeTimer = 0.0f;
                fade = false;
            }
            else
            {
                GUI.color = new Color(0.0f, 0.0f, 0.0f, 1.0f -  
                    Mathf.InverseLerp(0.0f, fadeTime, fadeTimer));
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTex);
                fadeTimer += Time.deltaTime;
            }
        }
    }

    public void MoveBackButton()
    {
        if (!away)
        {
            ToggleAway();
            transform.position = savedPos;
            if (prefs == null || (prefs != null && !prefs.VR))
            {
                transform.GetChild(0).GetChild(0).rotation = savedRot;
            }
        }
    }
}
