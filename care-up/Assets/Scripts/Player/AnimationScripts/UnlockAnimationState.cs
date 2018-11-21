﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockAnimationState : StateMachineBehaviour
{

    public string ObjectName = "";

    protected float frame = 0f;
    protected float prevFrame = 0f;
    public int unlock_frame;
    public int lock_frame;


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        lock_function(false);
        frame = 0f;
        prevFrame = 0f;
    }

    void lock_function(bool value)
    {
       
        if (GameObject.Find(ObjectName) != null)
        {
            foreach (GameObject gameObj in GameObject.FindObjectsOfType<GameObject>())
            {
                if (gameObj.name == ObjectName)
                {
                    bool is_sihlouette = false;
                    if (gameObj.GetComponent<PickableObject>() != null)
                    {
                        if (gameObj.GetComponent<PickableObject>().sihlouette)
                        {
                            is_sihlouette = false;
                        }
                    }

                    if (!is_sihlouette )
                    {
                        if (gameObj.GetComponent<ObjectStateManager>() != null)
                        {
                            gameObj.GetComponent<ObjectStateManager>().LockHoldState = value;
                        }
                        else if (gameObj.GetComponent<Syringe>() != null)
                        {
                            gameObj.GetComponent<Syringe>().updatePlunger = !value;
                        }
                    }
                }
            }
      
        }
    }


    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.speed != 0)
        {
            if (PlayerAnimationManager.CompareFrames(frame, prevFrame, unlock_frame))
            {
                lock_function(false);
            }

            if (PlayerAnimationManager.CompareFrames(frame, prevFrame, lock_frame))
            {
                lock_function(true);
            }
            prevFrame = frame;
            frame += Time.deltaTime;
        }
    }


    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

}
