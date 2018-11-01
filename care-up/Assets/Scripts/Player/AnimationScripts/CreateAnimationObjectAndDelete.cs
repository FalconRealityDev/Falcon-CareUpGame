﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAnimationObjectAndDelete : StateMachineBehaviour
{
    public int addFrame;
    public int deleteFrame;
    public string objectName;
    PlayerAnimationManager.Hand hand;

    protected float frame;
    protected float prevFrame;

    HandsInventory inventory;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        inventory = GameObject.FindObjectOfType<HandsInventory>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (PlayerAnimationManager.CompareFrames(frame, prevFrame, addFrame))
        {
            inventory.CreateAnimationObject(objectName, hand);
        }

        if (PlayerAnimationManager.CompareFrames(frame, prevFrame, deleteFrame))
        {
            inventory.DeleteAnimationObject();
        }

        if (animator.speed != 0)
        {
            prevFrame = frame;
            frame += Time.deltaTime;
        }
    }
}
