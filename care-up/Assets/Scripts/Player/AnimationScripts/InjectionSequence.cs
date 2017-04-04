﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InjectionSequence : AnimationSequenceState
{
    public int takeSyringeFrame;

    private HandsInventory inv;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        inv = GameObject.Find("GameLogic").GetComponent<HandsInventory>();
        inv.PutAllOnTable();
        inv.ForcePickItem("DesinfectionCloth", false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (frame == takeSyringeFrame)
        {
            inv.PutAllOnTable();
            inv.ForcePickItem("SyringeWithInjectionNeedle", false);
            inv.RightHandObject.GetComponent<Syringe>().updatePlunger = true;
        }

        if (keyFrame < keyFrames.Count)
        {
            if (animator.speed != 0)
            {
                if (++frame == keyFrames[keyFrame])
                {
                    PlayerAnimationManager.NextSequenceStep(true);
                    animator.speed = 0f;
                    ++keyFrame;
                }
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        inv.RightHandObject.GetComponent<Syringe>().updatePlunger = false;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
