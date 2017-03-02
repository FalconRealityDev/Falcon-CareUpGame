﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Handle animation changes of the player
/// </summary>
public class PlayerAnimationManager : MonoBehaviour {

    public static bool ikActive = false;

    private static GameObject LeftInteractObject;
    private static GameObject rightInteractObject;

    private static Animator animationController;

    void Start()
    {
        animationController = GetComponent<Animator>();
        if (animationController == null) Debug.LogError("Animator not found");
    }

    public static void PlayAnimation(string name, GameObject leftInteract = null, GameObject rightInteract = null)
    {
        animationController.SetTrigger(name);

        if (leftInteract || rightInteract)
        {
            LeftInteractObject = leftInteract;
            rightInteractObject = rightInteract;
        }
    }

    //a callback for calculating IK
    private void OnAnimatorIK(int layerIndex)
    {
        //if the IK is active, set the position and rotation directly to the goal. 
        if (ikActive)
        {
            // Set the right hand target position and rotation, if one has been assigned
            if (rightInteractObject != null)
            {
                animationController.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animationController.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animationController.SetIKPosition(AvatarIKGoal.RightHand, rightInteractObject.transform.position);
                animationController.SetIKRotation(AvatarIKGoal.RightHand, rightInteractObject.transform.rotation);
            }

            if (LeftInteractObject != null)
            {
                animationController.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animationController.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animationController.SetIKPosition(AvatarIKGoal.LeftHand, LeftInteractObject.transform.position);
                animationController.SetIKRotation(AvatarIKGoal.LeftHand, LeftInteractObject.transform.rotation);
            }
        }
        //if the IK is not active, set the position and rotation of the hand and head back to the original position
        else
        {
            animationController.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animationController.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            animationController.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animationController.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            animationController.SetLookAtWeight(0);
        }
    }
}
