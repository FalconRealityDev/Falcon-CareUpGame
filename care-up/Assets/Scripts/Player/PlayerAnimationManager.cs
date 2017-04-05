﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Handle animation changes of the player
/// </summary>
public class PlayerAnimationManager : MonoBehaviour {

    public float ikWeight = 1.0f;

    public static bool ikActive = false;

    private static Transform leftInteractObject;
    private static Transform rightInteractObject;

    private static Animator animationController;
    private static CameraMode cameraMode;

    private static AnimationSequence animationSequence;

    void Start()
    {
        animationController = GetComponent<Animator>();
        if (animationController == null) Debug.LogError("Animator not found");

        cameraMode = GameObject.Find("GameLogic").GetComponent<CameraMode>();
        if (cameraMode == null) Debug.LogError("No camera mode");
    }
    
    public static void PlayAnimation(string name, Transform target = null)
    {
        animationController.SetTrigger(name);
        
        if (target)
        {
            cameraMode.SetCinematicMode(target);
        }
    }

    /// <summary>
    /// Sets the idle state of the hand, depending on object held.
    /// </summary>
    /// <param name="hand">True = left, False = right</param>
    /// <param name="item">Name of the item</param>
    public static void SetHandItem(bool hand, string item)
    {
        string handName = hand ? "LeftHandState" : "RightHandState";
        int itemID = 0;

        switch (item)
        {
            case "Alcohol":
                itemID = 1;
                break;
            case "NeedleCup":
                itemID = 2;
                break;
            case "AbsorptionNeedle":
            case "InjectionNeedle":
                itemID = 3;
                break;
            case "Syringe":
            case "SyringeWithAbsorptionNeedle":
            case "SyringeWithInjectionNeedle":
                itemID = 4;
                break;
            case "Medicine":
                itemID = 5;
                break;
            case "Cloth":
            case "DesinfectionCloth":
                itemID = 6;
                break;
            default:
                itemID = 0;
                break;
        }

        animationController.SetInteger(handName, itemID);
    }

    public static void PlayAnimationSequence(string name, Transform target)
    {
        animationSequence = new AnimationSequence(name);
        cameraMode.cinematicToggle = true; //before play animation
        PlayAnimation(name + "Sequence", target);
    }

    public static void NextSequenceStep(bool flag)
    {
        if (flag)
        {
            if (animationSequence != null)
            {
                animationSequence.NextStep();
            }
        }
        else
        { 
            animationController.speed = 1f;
        }
    }

    public static void ToggleAnimationSpeed()
    {
        animationController.speed = (animationController.speed == 0) ? 1f : 0f;
    }

    public static void AbortSequence()
    {
        animationController.SetTrigger("AbortSequence");
        animationController.speed = 1f;
    }
}
