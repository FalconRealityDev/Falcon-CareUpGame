using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionHandler : MonoBehaviour
{
    private ActionManager actionManager;
    private GameUIVR gameUIVR;
  
    public bool TryExecuteAction(ActionManager.ActionType actionType, string leftHandObjectName, string rightHandObjectName)
    {
        if (actionManager == null)
            actionManager = GameObject.FindObjectOfType<ActionManager>();
        if (gameUIVR == null)
            gameUIVR = GameObject.FindObjectOfType<GameUIVR>();
        if (actionManager == null)
            return false;
        bool result = false;
        switch (actionType)
        {
            case ActionManager.ActionType.PersonTalk:
                result = actionManager.OnTalkAction(leftHandObjectName);
                break;
            case ActionManager.ActionType.ObjectUse:
                result = actionManager.OnUseAction(leftHandObjectName);
                break;
            case ActionManager.ActionType.ObjectCombine:
                result = actionManager.OnCombineAction(leftHandObjectName, rightHandObjectName);
                break;
            case ActionManager.ActionType.ObjectExamine:
                string expected = (rightHandObjectName == "") ? "good" : rightHandObjectName;
                result = actionManager.OnExamineAction(leftHandObjectName, expected);
                break;
            case ActionManager.ActionType.ObjectUseOn:
                result = actionManager.OnUseOnAction(leftHandObjectName, rightHandObjectName);
                break;
            case ActionManager.ActionType.SequenceStep:
                result = actionManager.OnSequenceStepAction(leftHandObjectName);
                break;
            default:
                Debug.LogWarning(actionType + " is not supported yet in ActionHandler. TODO");
                break;
        }
        if (gameUIVR != null)
            gameUIVR.UpdateHelpHighlight();

        return result;
    }

    public bool CheckAction(ActionManager.ActionType actionType, string leftHandObjectName, string rightHandObjectName)
    {
        string[] info = { leftHandObjectName, rightHandObjectName };
        return actionManager.Check(info, actionType);
    }
    
}
