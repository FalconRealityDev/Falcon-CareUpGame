﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationForceSync : StateMachineBehaviour
{
    public List<int> syncFrames = new List<int>();

    protected float frame;
    protected float prevFrame;

    public void SyncAnimations(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int targetLayer = (layerIndex == 0) ? 1 : 0;
        animator.Play(0, targetLayer, stateInfo.normalizedTime);
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        frame = 0f;
        prevFrame = 0f;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.speed != 0)
        {
            foreach (int syncFrame in syncFrames)
            {
                if (PlayerAnimationManager.CompareFrames(frame, prevFrame, syncFrame))
                {
                    SyncAnimations(animator, stateInfo, layerIndex);
                }
            }

            prevFrame = frame;
            frame += Time.deltaTime;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
