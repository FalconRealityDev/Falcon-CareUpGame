﻿using System.Xml;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CareUp.Actions;

/// <summary>
/// GameLogic script. Everything about actions is managed by this script.
/// </summary>
public class ActionManager : MonoBehaviour
{
    // tutorial variables - do not affect gameplay
    [HideInInspector]
    public bool tutorial_hintUsed = false;
    private bool currentStepHintUsed = false;
    private Text pointsText;
    private Text percentageText;
    public int actionsCount = 0;
    public static bool practiceMode = true;

    // list of types of actions
    public enum ActionType
    {
        ObjectCombine,
        ObjectUse,
        PersonTalk,
        ObjectUseOn,
        ObjectExamine,
        PickUp,
        SequenceStep,
        ObjectDrop,
        Movement,
    };

    // name of the xml file with actions
    public string actionListName;

    // actual list of actions
    public List<Action> actionList = new List<Action>();

    // list of descriptions of steps, player got penalty on
    private List<string> stepsList = new List<string>();
    private List<string> stepsDescriptionList = new List<string>();
    private List<int> wrongStepIndexes = new List<int>();
    private List<int> correctStepIndexes = new List<int>();

    private int totalPoints = 0;         // max points of scene
    private int points = 0;              // current points
    public int currentActionIndex = 0;  // index of current action
    private Action currentAction;        // current action instance
    private int currentPointAward = 1;
    private bool penalized = false;

    // GameObjects that show player next step when hint used
    private List<GameObject> particleHints;
    private bool menuScene;
    private bool uiSet = false;
    PlayerPrefsManager manager;
    HandsInventory inventory;
    static PlayerScript playerScript;

    private List<string> unlockedBlocks = new List<string>();

    public List<Action> ActionList
    {
        get { return actionList; }
    }

    /// <summary>
    /// List of wrong steps, merged into a string with line breaks.
    /// </summary>
    public List<string> StepsList
    {
        get { return stepsList; }
    }

    public List<string> StepsDescriptionList
    {
        get { return stepsDescriptionList; }
    }

    public List<int> WrongStepIndexes
    {
        get { return wrongStepIndexes; }
    }

    public List<int> CorrectStepIndexes
    {
        get { return correctStepIndexes; }
    }

    /// <summary>
    /// Current points during runtime.
    /// </summary>
    public int Points
    {
        get { return points; }
        set { points = value; }
    }

    /// <summary>
    /// Total max points player can get on the scene.
    /// </summary>
    public int TotalPoints
    {
        get { return totalPoints; }
    }

    public float PercentageDone
    {
        get
        {
            int cur = actionList.IndexOf(currentAction);
            int tot = actionList.Count;
            return 100.0f * cur / tot;
        }
    }

    /// <summary>
    /// Index of current action.
    /// </summary>
    public int CurrentActionIndex
    {
        get { return currentActionIndex; }
        set { currentActionIndex = value; }
    }

    // Will be refactored
    public static void UpdateRequirements()
    {
        if (!practiceMode)
            return;

        if (playerScript == null)
            playerScript = GameObject.FindObjectOfType<PlayerScript>();

        ActionManager manager = GameObject.FindObjectOfType<ActionManager>();
        GameUI gameUI = GameObject.FindObjectOfType<GameUI>();

        List<Action> sublist = manager.actionList.Where(action =>
               action.SubIndex == manager.currentActionIndex &&
               action.matched == false).ToList();

        List<StepData> stepsList = new List<StepData>();

        HandsInventory inventory = GameObject.FindObjectOfType<HandsInventory>();
        int i = 0;
        bool foundComplitedAction = false;
        gameUI.buttonToBlink = GameUI.ItemControlButtonType.None;
        gameUI.DropLeftBlink = false;
        gameUI.DropRightBlink = false;
        gameUI.reqPlaces.Clear();
        bool leftIncorrect = true;
        bool rightIncorrect = true;
        bool noObjectActions = true;
        bool anyCorrectPlace = false;
        List<string> placesReqList = new List<string>();
        string uncomplitedSecondPlace = "";
        gameUI.recordsButtonBlink = false;
        gameUI.prescriptionButtonBlink = false;
        foreach (Action a in sublist)
        {
            StepData placeData = null;
            StepData secondPlaceData = null;
            bool correctObjectsInHands = true;
            List<StepData> objectsData = new List<StepData>();

            bool dialog = false;

            if (a.placeRequirement == "PatientPos" || a.placeRequirement == "DoctorPos" || a.secondPlaceRequirement == "PatientPos" || a.secondPlaceRequirement == "DoctorPos")
                dialog = true;

            if (!string.IsNullOrEmpty(a.placeRequirement))
            {
                string placeName = a.placeRequirement;

                if (GameObject.Find(a.placeRequirement).GetComponent<WalkToGroup>().description != "")
                    placeName = GameObject.Find(a.placeRequirement).GetComponent<WalkToGroup>().description;

                bool completed = false;
                if (playerScript.currentWalkPosition != null)
                    completed = playerScript.currentWalkPosition.name == a.placeRequirement;

                placeData = new StepData(completed, $"- Ga naar {placeName}.", i);
                if (completed)
                    anyCorrectPlace = true;

                if (a.Type == ActionType.PersonTalk && dialog)
                {
                    objectsData.Add(new StepData(false, $"- Klik op {placeName}.", i));
                }
            }

            if (!string.IsNullOrEmpty(a.secondPlaceRequirement))
            {
                string placeName = a.secondPlaceRequirement;

                if (GameObject.Find(a.secondPlaceRequirement).GetComponent<WalkToGroup>().description != "")
                    placeName = GameObject.Find(a.secondPlaceRequirement).GetComponent<WalkToGroup>().description;

                bool completed = false;
                if (playerScript.currentWalkPosition != null)
                    completed = playerScript.currentWalkPosition.name == a.secondPlaceRequirement;

                secondPlaceData = new StepData(completed, $"- Ga naar {placeName}.", i);

                if (a.Type == ActionType.PersonTalk && dialog)
                {
                    objectsData.Add(new StepData(false, $"- Klik op {placeName}.", i));
                }
            }          

            string[] actionHand = { a.leftHandRequirement, a.rightHandRequirement };
            GameObject leftR = null;
            GameObject rightR = null;
            string article = null;
            string currentLeftObject = null;
            string currentRightObject = null;
            bool objectCombinationCheck = false;

            bool iPad = a.leftHandRequirement == "PatientRecords" || a.leftHandRequirement == "PrescriptionForm" || a.leftHandRequirement == "PaperAndPen";

            if (iPad)
            {
                objectsData.Add(new StepData(false, $"- Klik op het tablet icoon.", i));

                if(a.leftHandRequirement == "PatientRecords")
                {
                    gameUI.recordsButtonBlink = true;
                }
                else if (a.leftHandRequirement == "PrescriptionForm")
                {
                    gameUI.prescriptionButtonBlink = true;
                }
                else if (a.leftHandRequirement == "PaperAndPen")
                {
                    gameUI.paperAndPenButtonblink = true;
                }
            }
            else
            {
                foreach (string hand in actionHand)
                {
                    bool foundDescr = false;

                    if (!string.IsNullOrEmpty(hand))
                    {
                        string handValue = hand;
                        bool found = false;

                        if (GameObject.Find(hand) != null)
                        {
                            if (GameObject.Find(hand).GetComponent<InteractableObject>() != null)
                            {
                                if (GameObject.Find(hand).GetComponent<InteractableObject>().description != "")
                                {
                                    handValue = GameObject.Find(hand).GetComponent<InteractableObject>().description;
                                    article = GameObject.Find(hand).GetComponent<InteractableObject>().nameArticle;
                                    found = true;
                                    foundDescr = true;
                                }
                            }
                        }
                        if (GameObject.FindObjectOfType<ExtraObjectOptions>() != null && !found)
                        {
                            foreach (ExtraObjectOptions extraObject in GameObject.FindObjectsOfType<ExtraObjectOptions>())
                            {
                                string desc = extraObject.HasNeeded(hand);                                
                                if (desc != "")
                                {
                                    article = extraObject.HasNeededArticle(hand);
                                    handValue = desc;
                                    found = true;
                                    foundDescr = true;

                                }
                            }
                        }

                        if (GameObject.FindObjectOfType<WorkField>() != null && !found)
                        {
                            foreach (WorkField w in GameObject.FindObjectsOfType<WorkField>())
                            {
                                foreach (GameObject obj in w.objects)
                                {
                                    if (obj != null)
                                    {
                                        if (obj.name == hand)
                                        {
                                            article = obj.GetComponent<InteractableObject>().nameArticle;
                                            handValue = obj.GetComponent<InteractableObject>().description;
                                            foundDescr = true;
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                                if (found)
                                    break;
                            }
                        }
                        if (GameObject.FindObjectsOfType<CatheterPack>() != null && !found)
                        {
                            foreach (CatheterPack w in GameObject.FindObjectsOfType<CatheterPack>())
                            {
                                foreach (GameObject obj in w.objects)
                                {
                                    if (obj != null)
                                    {
                                        if (obj.name == hand)
                                        {
                                            handValue = obj.GetComponent<InteractableObject>().description;
                                            article = obj.GetComponent<InteractableObject>().nameArticle;
                                            foundDescr = true;
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                                if (found)
                                    break;
                            }
                        }

                        bool completed = false;

                        if (!inventory.LeftHandEmpty())
                        {
                            currentLeftObject = System.Char.ToLowerInvariant(inventory.leftHandObject.description[0]) + inventory.leftHandObject.description.Substring(1);

                            if (inventory.leftHandObject.name == hand)
                            {
                                leftIncorrect = false;
                                completed = true;
                                leftR = inventory.leftHandObject.gameObject;
                            }
                        }

                        if (!inventory.RightHandEmpty())
                        {
                            currentRightObject = System.Char.ToLowerInvariant(inventory.rightHandObject.description[0]) + inventory.rightHandObject.description.Substring(1);

                            if (inventory.rightHandObject.name == hand)
                            {
                                rightIncorrect = false;
                                completed = true;
                                rightR = inventory.rightHandObject.gameObject;
                            }
                        }
#if UNITY_EDITOR
                        if (!foundDescr)
                        {
                            if (GameObject.FindObjectOfType<NeededObjectsLister>() != null)
                            {
                                if (!GameObject.FindObjectOfType<NeededObjectsLister>().addNeeded(hand))
                                    print("______Add to extra______   " + hand);
                            }
                        }
#endif
                        
                        handValue = System.Char.ToLowerInvariant(handValue[0]) + handValue.Substring(1);

                        string keyWords = null;

                        if (leftR != null && rightR != null)
                        {
                            objectCombinationCheck = ((leftR.name == a.leftHandRequirement) && (rightR.name == a.rightHandRequirement)) || ((rightR.name == a.leftHandRequirement) && (leftR.name == a.rightHandRequirement));
                        }

                        if (objectCombinationCheck && a.Type == ActionType.ObjectCombine)
                        {
                            objectsData.Add(new StepData(false, "- Klik op de 'Combineer' knop.", i));
                            if (!foundComplitedAction)
                            {
                                foundComplitedAction = true;
                                gameUI.buttonToBlink = GameUI.ItemControlButtonType.Combine;
                            }
                        }

                        else if (leftR != null)
                        {
                            if (manager.CompareUseOnInfo(inventory.leftHandObject.name, ""))
                            {
                                keyWords = manager.CurrentButtonText(inventory.leftHandObject.name);

                                objectsData.Add(new StepData(false, $"- Klik op de '{keyWords}' knop.", i));
                                if (!foundComplitedAction)
                                {
                                    foundComplitedAction = true;
                                    gameUI.buttonToBlink = GameUI.ItemControlButtonType.NoTargetLeft;
                                }
                            }
                            else if (a.Type == ActionType.ObjectDrop && a.leftHandRequirement == inventory.leftHandObject.name)
                            {
                                if (!foundComplitedAction)
                                {
                                    foundComplitedAction = true;
                                    gameUI.buttonToBlink = GameUI.ItemControlButtonType.None;
                                }
                                if (secondPlaceData != null)
                                {
                                    if (secondPlaceData.completed)
                                    {
                                        gameUI.DropLeftBlink = true;
                                        
                                        objectsData.Add(new StepData(false, $"- Leg {article} {handValue} neer.", i));
                                    }
                                }
                                else
                                    objectsData.Add(new StepData(false, $"- Leg {article} {handValue} neer.", i));
                            }
                            else if (a.Type == ActionType.ObjectExamine && inventory.leftHandObject.name == a.leftHandRequirement)
                            {
                                objectsData.Add(new StepData(false, $"- Klik op de 'Controleren' knop.", i));
                                if (!foundComplitedAction)
                                {
                                    foundComplitedAction = true;
                                    gameUI.buttonToBlink = GameUI.ItemControlButtonType.ZoomLeft;
                                }
                            }

                            else if (!string.IsNullOrEmpty(manager.CurrentDecombineButtonText(inventory.leftHandObject.name)))
                            {
                                keyWords = manager.CurrentDecombineButtonText(inventory.leftHandObject.name);
                                objectsData.Add(new StepData(false, $"- Klik op de '{manager.CurrentDecombineButtonText(inventory.leftHandObject.name)}' knop.", i));
                                if (!foundComplitedAction)
                                {
                                    foundComplitedAction = true;
                                    gameUI.buttonToBlink = GameUI.ItemControlButtonType.DecombineLeft;
                                }
                            }
                            else if (!foundComplitedAction)
                                gameUI.buttonToBlink = GameUI.ItemControlButtonType.None;
                        }

                        else if (rightR != null)
                        {
                            if (manager.CompareUseOnInfo(inventory.rightHandObject.name, ""))
                            {
                                keyWords = manager.CurrentButtonText(inventory.rightHandObject.name);

                                objectsData.Add(new StepData(false, $"- Klik op de '{keyWords}' knop.", i));
                                if (!foundComplitedAction)
                                {
                                    foundComplitedAction = true;
                                    gameUI.buttonToBlink = GameUI.ItemControlButtonType.NoTargetRight;
                                }
                            }
                            else if (a.Type == ActionType.ObjectDrop)
                            {
                                if (!foundComplitedAction)
                                {
                                    foundComplitedAction = true;
                                    gameUI.buttonToBlink = GameUI.ItemControlButtonType.None;
                                }
                                if (secondPlaceData != null)
                                {
                                    if (secondPlaceData.completed)
                                    {
                                        gameUI.DropRightBlink = true;
                                        objectsData.Add(new StepData(false, $"- Leg {article} {handValue} neer.", i));
                                    }
                                }
                                else
                                    objectsData.Add(new StepData(false, $"- Leg {article} {handValue} neer.", i));
                            }
                            else if (a.Type == ActionType.ObjectExamine && inventory.rightHandObject.name == a.leftHandRequirement)
                            {
                                objectsData.Add(new StepData(false, $"- Klik op de 'Controleren' knop.", i));
                                if (!foundComplitedAction)
                                {
                                    foundComplitedAction = true;
                                    gameUI.buttonToBlink = GameUI.ItemControlButtonType.ZoomRight;
                                }
                            }
                            else if (a.Type == ActionType.PersonTalk)
                            {
                                if (!foundComplitedAction)
                                {
                                    gameUI.buttonToBlink = GameUI.ItemControlButtonType.None;
                                }
                            }
                            else if (!string.IsNullOrEmpty(manager.CurrentDecombineButtonText(inventory.RightHandObject.name)))
                            {
                                keyWords = manager.CurrentDecombineButtonText(inventory.rightHandObject.name);
                                objectsData.Add(new StepData(false, $"- Klik op de '{manager.CurrentDecombineButtonText(inventory.rightHandObject.name)}' knop.", i));
                                if (!foundComplitedAction)
                                {
                                    foundComplitedAction = true;
                                    gameUI.buttonToBlink = GameUI.ItemControlButtonType.DecombineRight;
                                }
                            }
                            else
                                gameUI.buttonToBlink = GameUI.ItemControlButtonType.None;
                        }
                        else if (!foundComplitedAction)
                            gameUI.buttonToBlink = GameUI.ItemControlButtonType.None;


                        if (!completed)
                            correctObjectsInHands = false;

                        keyWords = "- Pak";

                        if (a.Type == ActionType.ObjectUse)
                        {
                            keyWords = "- Klik op";
                        }                       

                        objectsData.Add(new StepData(completed, $"{keyWords} {article} {handValue}.", i));
                    }
                }
            }
            if (placeData != null)
            {
                if (secondPlaceData != null && correctObjectsInHands)
                {
                    stepsList.Add(secondPlaceData);
                    //placesReqList.Add(a.secondPlaceRequirement);
                    if (!secondPlaceData.completed)
                        uncomplitedSecondPlace = a.secondPlaceRequirement;
                }
                else
                {
                    stepsList.Add(placeData);
                    placesReqList.Add(a.placeRequirement);
                }
            }

            foreach (StepData sd in objectsData)
            {
                stepsList.Add(sd);
            }

            if (objectsData.Count > 0)
                noObjectActions = false;

            gameUI.moveButtonToBlink = GameUI.ItemControlButtonType.None;

            if (uncomplitedSecondPlace != "")
            {
                placesReqList.Clear();
                placesReqList.Add(uncomplitedSecondPlace);
            }

            string sss = "";
            foreach (string s in placesReqList)
            {
                sss += s + " | ";
                gameUI.reqPlaces.Add(s);
            }

            //gameUI.debugSS = anyCorrectPlace.ToString() + " " + sss;
            //if (!playerScript.away)
            //    gameUI.debugSS = gameUI.FindDirection("WorkFieldPos", playerScript.currentWalkPosition, 0).ToString();
            //else
            //gameUI.debugSS = "";

            //FindDirection(string neededWalkToGroup, WalkToGroup startWalkToGtoup, int direction)


            if ((!anyCorrectPlace || uncomplitedSecondPlace != "") && !playerScript.away && placesReqList.Count > 0)
            {
                WalkToGroup currentWTG = playerScript.currentWalkPosition;

                foreach (string s in placesReqList)
                {
                    int dir = gameUI.FindDirection(s, playerScript.currentWalkPosition, 0);
                    if (dir == -1)
                    {
                        gameUI.moveButtonToBlink = GameUI.ItemControlButtonType.MoveLeft;
                        break;
                    }
                    else if (dir == 1)
                    {
                        gameUI.moveButtonToBlink = GameUI.ItemControlButtonType.MoveRight;
                        break;
                    }
                }
            }

            i++;
        }
        if (gameUI.moveButtonToBlink != GameUI.ItemControlButtonType.None)
        {
            gameUI.buttonToBlink = GameUI.ItemControlButtonType.None;
            gameUI.DropRightBlink = false;
            gameUI.DropLeftBlink = false;
        }

        GameObject.FindObjectOfType<GameUI>().UpdateHintPanel(stepsList);

        if (leftIncorrect && !inventory.LeftHandEmpty() && !noObjectActions)
            gameUI.DropLeftBlink = true;
        if (rightIncorrect && !inventory.RightHandEmpty() && !noObjectActions)
            gameUI.DropRightBlink = true;
        GameObject.FindObjectOfType<GameUI>().UpdateHintPanel(stepsList);

        gameUI.updateButtonsBlink();
    }

    /// <summary>
    /// Description of the current action.
    /// Heavy function, use only once, never on update
    /// </summary>
    public List<string> CurrentDescription
    {
        get
        {
            List<string> actionsDescription = new List<string>();
            string result = "";
            bool Ua = false;

#if UNITY_EDITOR
            if (GameObject.FindObjectOfType<GameUI>() != null)
                Ua = GameObject.FindObjectOfType<ObjectsIDsController>().Ua;
#endif

            if (manager != null && !manager.practiceMode && currentAction != null)
            {
                result = currentAction.shortDescr;
                if (Ua && currentAction.commentUA != "")
                    result = currentAction.commentUA;
            }
            else
            {
                List<Action> sublist = actionList.Where(action =>
                    action.SubIndex == currentActionIndex &&
                    action.matched == false).ToList();

                foreach (Action a in sublist)
                {
                    if (!Ua || a.commentUA == "")
                        actionsDescription.Add(a.shortDescr);
                    if (Ua)
                        actionsDescription.Add(a.commentUA);
                }
            }
            return actionsDescription;
        }
    }
    
    /// <summary>
    /// Name of the file of audioHint of current action.
    /// </summary>
    public string CurrentAudioHint
    {
        get { return currentAction.audioHint; }
    }

    // new comparison looks for all actions of type withing current index
    public bool CompareUseObject(string name)
    {
        bool result = false;

        List<Action> sublist = actionList.Where(action =>
                action.SubIndex == currentActionIndex &&
                action.matched == false).ToList();
        sublist = sublist.Where(action =>
            action.blockRequired == "" ||
            unlockedBlocks.Contains(action.blockRequired)).ToList();

        foreach (Action a in sublist)
        {
            if (a.Type == ActionType.ObjectUse)
            {
                if (((UseAction)a).GetObjectName() == name)
                    result = true;
            }
        }

        return result;
    }

    public bool CompareCombineObjects(string left, string right)
    {
        bool result = false;

        List<Action> sublist = actionList.Where(action =>
                action.SubIndex == currentActionIndex &&
                action.matched == false).ToList();
        sublist = sublist.Where(action =>
            action.blockRequired == "" ||
            unlockedBlocks.Contains(action.blockRequired)).ToList();
        foreach (Action a in sublist)
        {
            if (a.Type == ActionType.ObjectCombine)
            {
                string _left, _right;
                ((CombineAction)a).GetObjects(out _left, out _right);
                if ((_left == left && _right == right) ||
                    (_right == left && _left == right))
                    result = true;
            }
        }

        return result;
    }

    public bool CompareUseOnInfo(string item, string target, string callerName = "")
    {
        bool result = false;

        if (callerName != "" && callerName != item)
            return result;

        List<Action> sublist = actionList.Where(action =>
                action.SubIndex == currentActionIndex &&
                action.matched == false).ToList();
        sublist = sublist.Where(action =>
            action.blockRequired == "" ||
            unlockedBlocks.Contains(action.blockRequired)).ToList();
        foreach (Action a in sublist)
        {
            if (a.Type == ActionType.ObjectUseOn)
            {
                string _item, _target;
                ((UseOnAction)a).GetInfo(out _item, out _target);
                if (_item == item && _target == target)
                    result = true;
            }
        }

        return result;
    }

    public bool CompareTopic(string t)
    {
        bool result = false;

        List<Action> sublist = actionList.Where(action =>
                action.SubIndex == currentActionIndex &&
                action.matched == false).ToList();
        sublist = sublist.Where(action =>
            action.blockRequired == "" ||
            unlockedBlocks.Contains(action.blockRequired)).ToList();
        foreach (Action a in sublist)
        {
            if (a.Type == ActionType.PersonTalk)
            {
                if (((TalkAction)a).Topic == t)
                    result = true;
            }
        }

        return result;
    }

    public string CurrentButtonText(string itemName)
    {
        List<Action> sublist = actionList.Where(action =>
               action.SubIndex == currentActionIndex &&
               action.matched == false).ToList();
        sublist = sublist.Where(action =>
            action.blockRequired == "" ||
            unlockedBlocks.Contains(action.blockRequired)).ToList();

        foreach (Action a in sublist)
        {
            if (a.Type == ActionType.ObjectUse)
            {
                UseAction useA = (UseAction)a;
                if (useA.GetObjectName() == itemName)
                {
                    return useA.buttonText;
                }
            }
            else if (a.Type == ActionType.ObjectUseOn)
            {
                UseOnAction useOnA = (UseOnAction)a;
                string i, t;
                useOnA.GetInfo(out i, out t);
                if (i == itemName)
                {
                    return useOnA.buttonText;
                }
            }
        }

        return "";
    }

    public string CurrentDecombineButtonText(string itemName)
    {
        List<Action> sublist = actionList.Where(action =>
               action.SubIndex == currentActionIndex &&
               action.matched == false).ToList();
        sublist = sublist.Where(action =>
            action.blockRequired == "" ||
            unlockedBlocks.Contains(action.blockRequired)).ToList();

        foreach (Action a in sublist)
        {
            if (a.Type == ActionType.ObjectCombine)
            {
                CombineAction useA = (CombineAction)a;
                string[] combineObjectNames;
                useA.ObjectNames(out combineObjectNames);
                if ((combineObjectNames[0] == itemName && combineObjectNames[1] == "")
                    || (combineObjectNames[1] == itemName && combineObjectNames[0] == ""))
                {
                    return useA.decombineText;
                }
            }
        }

        return "";
    }

    public bool CompareDropPos(string item, int pos)
    {
        bool result = false;

        List<Action> sublist = actionList.Where(action =>
                action.SubIndex == currentActionIndex &&
                action.matched == false).ToList();
        sublist = sublist.Where(action =>
            action.blockRequired == "" ||
            unlockedBlocks.Contains(action.blockRequired)).ToList();
        foreach (Action a in sublist)
        {
            if (a.Type == ActionType.ObjectDrop)
            {
                string[] o = new string[2];
                ((ObjectDropAction)a).ObjectNames(out o);
                if (o[0] == item && o[1] == pos.ToString())
                    result = true;
            }
        }

        return result;
    }

    public bool CompareMovementPosition(string position)
    {
        bool result = false;

        List<Action> sublist = actionList.Where(action =>
                action.SubIndex == currentActionIndex &&
                action.matched == false).ToList();
        sublist = sublist.Where(action =>
            action.blockRequired == "" ||
            unlockedBlocks.Contains(action.blockRequired)).ToList();

        foreach (Action a in sublist)
        {
            if (a.Type == ActionType.Movement)
            {
                if (((MovementAction)a).GetInfo() == position)
                    result = true;
            }
        }

        return result;
    }

    private Controls controls;

    /// <summary>
    /// Set some variables and load info from xml file.
    /// </summary>
    void Awake()
    {
        manager = GameObject.FindObjectOfType<PlayerPrefsManager>();

        string sceneName = SceneManager.GetActiveScene().name;
        menuScene = sceneName == "Menu" || sceneName == "SceneSelection" || sceneName == "EndScore";
        particleHints = new List<GameObject>();

        controls = GameObject.Find("GameLogic").GetComponent<Controls>();
        if (controls == null) Debug.LogError("No controls found");

        TextAsset textAsset = (TextAsset)Resources.Load("Xml/Actions/" + actionListName);
        XmlDocument xmlFile = new XmlDocument();
        xmlFile.LoadXml(textAsset.text);

        //totalPoints = int.Parse(xmlFile.FirstChild.NextSibling.Attributes["points"].Value);
        XmlNodeList actions = xmlFile.FirstChild.NextSibling.ChildNodes;

        foreach (XmlNode action in actions)
        {
            int index;
            int.TryParse(action.Attributes["index"].Value, out index);
            string type = action.Attributes["type"].Value;
            string descr = action.Attributes["description"].Value;


            string comment = "";
            if (action.Attributes["comment"] != null)
            {
                comment = action.Attributes["comment"].Value;
            }

            string secondPlace = "";
            if (action.Attributes["secondPlace"] != null)
            {
                secondPlace = action.Attributes["secondPlace"].Value;
            }

            string place = "";
            if (action.Attributes["place"] != null)
            {
                place = action.Attributes["place"].Value;
            }


            string commentUA = "";
            if (action.Attributes["commentUA"] != null)
            {
                commentUA = action.Attributes["commentUA"].Value;
            }

            string audio = "";
            if (action.Attributes["audioHint"] != null)
            {
                audio = action.Attributes["audioHint"].Value;
            }
            
            string extra = "";
            if (action.Attributes["extra"] != null)
            {
                extra = action.Attributes["extra"].Value;
            }

            string buttonText = "";
            if (action.Attributes["buttonText"] != null)
            {
                buttonText = action.Attributes["buttonText"].Value;
            }
            else
            {
                buttonText = descr;
            }

            int pointsValue = 1;
            if (action.Attributes["points"] != null)
            {
                int.TryParse(action.Attributes["points"].Value, out pointsValue);
            }

            bool notNeeded = action.Attributes["optional"] != null;

            // try making all steps optional for test
            if (manager != null && !manager.practiceMode)
            {
                notNeeded = true;
                index = 0;
            }

            // quiz triggers
            float quizTime = -1.0f;
            if (action.Attributes["quiz"] != null)
            {
                float.TryParse(action.Attributes["quiz"].Value, out quizTime);
                if (quizTime < 0.1f)
                    quizTime = 2.0f;
            }

            string messageTitle = "";
            if (action.Attributes["messageTitle"] != null)
            {
                messageTitle = action.Attributes["messageTitle"].Value;
            }

            string messageContent = "";
            if (action.Attributes["messageContent"] != null)
            {
                messageContent = action.Attributes["messageContent"].Value;
            }

            string blockUnlock = "";
            if (action.Attributes["blockUnlock"] != null)
            {
                blockUnlock = action.Attributes["blockUnlock"].Value;
            }

            string blockRequire = "";
            if (action.Attributes["blockRequired"] != null)
            {
                blockRequire = action.Attributes["blockRequired"].Value;
            }

            string blockLock = "";
            if (action.Attributes["blockLock"] != null)
            {
                blockLock = action.Attributes["blockLock"].Value;
            }

            string blockTitle = "";
            string blockMsg = "";
            if (action.Attributes["blockTitle"] != null)
            {
                blockTitle = action.Attributes["blockTitle"].Value;
            }

            if (action.Attributes["blockMessage"] != null)
            {
                blockMsg = action.Attributes["blockMessage"].Value;
            }

            string decombineText = "Openen";
            if (action.Attributes["decombineText"] != null)
            {
                decombineText = action.Attributes["decombineText"].Value;
            }

            switch (type)
            {
                case "combine":
                    string left = action.Attributes["left"].Value;
                    string right = action.Attributes["right"].Value;
                    actionList.Add(new CombineAction(left, right, index, descr, audio, extra,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg, decombineText));
                    break;
                case "use":
                    string use = action.Attributes["value"].Value;
                    actionList.Add(new UseAction(use, index, descr, audio, extra, buttonText,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg));
                    break;
                case "talk":
                    string topic = action.Attributes["topic"].Value;
                    actionList.Add(new TalkAction(topic, index, descr, audio, extra, pointsValue,
                        notNeeded, quizTime, messageTitle, messageContent, blockRequire, blockUnlock,
                        blockLock, blockTitle, blockMsg));
                    break;
                case "useOn":
                    string useItem = action.Attributes["item"].Value;
                    string target = action.Attributes["target"].Value;
                    actionList.Add(new UseOnAction(useItem, target, index, descr, audio, extra,
                        buttonText, pointsValue, notNeeded, quizTime, messageTitle, messageContent,
                        blockRequire, blockUnlock, blockLock, blockTitle, blockMsg));
                    break;
                case "examine":
                    string exItem = action.Attributes["item"].Value;
                    string expected = action.Attributes["expected"].Value;
                    actionList.Add(new ExamineAction(exItem, expected, index, descr, audio, extra,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg));
                    break;
                case "pickUp":
                    string itemPicked = action.Attributes["item"].Value;
                    actionList.Add(new PickUpAction(itemPicked, index, descr, audio, extra,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg));
                    break;
                case "sequenceStep":
                    string stepName = action.Attributes["value"].Value;
                    actionList.Add(new SequenceStepAction(stepName, index, descr, audio, extra,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg));
                    break;
                case "drop":
                    string dropItem = action.Attributes["item"].Value;
                    string dropID = (action.Attributes["posID"] != null) ? action.Attributes["posID"].Value : "0";
                    actionList.Add(new ObjectDropAction(dropItem, dropID, index, descr, audio, extra,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg));
                    break;
                case "movement":
                    string movement = action.Attributes["value"].Value;
                    actionList.Add(new MovementAction(movement, index, descr, audio, extra,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg));
                    break;
                default:
                    Debug.LogError("No action type found: " + type);
                    break;
            }
            actionList[actionList.Count - 1].comment = comment;
            actionList[actionList.Count - 1].commentUA = commentUA;
            actionList[actionList.Count - 1].secondPlaceRequirement = secondPlace;
            actionList[actionList.Count - 1].placeRequirement = place;



        }
        actionList.Last<Action>().sceneDoneTrigger = true;

        if (xmlFile.FirstChild.NextSibling.Attributes["points"] != null)
        {
            totalPoints = int.Parse(xmlFile.FirstChild.NextSibling.Attributes["points"].Value);
        }
        else
        {
            totalPoints = 0;
            foreach (Action a in actionList)
            {
                totalPoints += a.pointValue;
            }
        }

        foreach (Action a in actionList)
        {
            stepsList.Add(a.shortDescr);
            stepsDescriptionList.Add(a.extraDescr);
        }

        currentAction = actionList.First();
    }

    /// <summary>
    /// Handle pressing "Get Hint" key.
    /// Play audio hint, create particle hint, do penalty.
    /// </summary>
    void Update()
    {
        if (controls.keyPreferences.GetHintKey.Pressed())
        {
            if (Narrator.PlayHintSound(CurrentAudioHint)) // if sound played
            {
                string[] obj;
                currentAction.ObjectNames(out obj);
                GameObject parent;

                if (obj.Length > 0)
                {
                    parent = GameObject.Find(obj[0]);
                    if (parent != null)
                    {
                        CreateParticleHint(parent.transform);
                    }
                }

                if (obj.Length == 2)
                {
                    parent = GameObject.Find(obj[1]);
                    if (parent != null)
                    {
                        CreateParticleHint(parent.transform);
                    }
                }

                tutorial_hintUsed = true;
                if (!currentStepHintUsed)
                {
                    UpdatePoints(-1); // penalty for using hint
                    currentStepHintUsed = true;
                }
            }
        }

        if (!menuScene && uiSet)
        {
            if (pointsText.gameObject.activeSelf)
            {
                pointsText.text = points.ToString();// + " / " + totalPoints;
            }

            if (percentageText.gameObject.activeSelf)
            {
                percentageText.text = Mathf.RoundToInt(PercentageDone).ToString() + "%";
            }
        }
    }

    public void CreateParticleHint(Transform obj)
    {
        GameObject particles = Instantiate(Resources.Load<GameObject>("Prefabs/ParticleHint"),
            obj.position, Quaternion.Euler(-90f, 0f, 0f), obj);
        particles.transform.localScale = new Vector3(
            particles.transform.localScale.x / particles.transform.lossyScale.x,
             particles.transform.localScale.y / particles.transform.lossyScale.y,
              particles.transform.localScale.z / particles.transform.lossyScale.z);
        particles.name = "ParticleHint";
        particleHints.Add(particles);
    }

    /// <summary>
    /// Handle (trigger) Combine action.
    /// </summary>
    /// <param name="leftHand">Name of the object in left hand.</param>
    /// <param name="rightHand">Name of the object in right hand.</param>
    public void OnCombineAction(string leftHand, string rightHand)
    {
        string[] info = { leftHand, rightHand };
        bool occured = Check(info, ActionType.ObjectCombine);
        UpdatePoints(occured ? 1 : -1);

        Debug.Log("Combine " + leftHand + " and " + rightHand + " with result " + occured);

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();
    }

    /// <summary>
    /// Handle (trigger) UseAction.
    /// </summary>
    /// <param name="useObject">Name of the used object.</param>
    public void OnUseAction(string useObject)
    {
        string[] info = { useObject };
        bool occured = Check(info, ActionType.ObjectUse);
        UpdatePoints(occured ? 1 : -1);

        Debug.Log("Use " + useObject + " with result " + occured);

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();
    }

    /// <summary>
    /// Handle (trigger) Talk action.
    /// </summary>
    /// <param name="topic">Chosen topic</param>
    public void OnTalkAction(string topic)
    {
        string[] info = { topic };
        bool occured = Check(info, ActionType.PersonTalk);
        UpdatePoints(occured ? 1 : -1);

        Debug.Log("Say " + topic + " with result " + occured);

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();
    }

    /// <summary>
    /// Handle (trigger) UseOn action.
    /// </summary>
    /// <param name="item">Item that is used on target</param>
    /// <param name="target">Item that was targeted</param>
    public void OnUseOnAction(string item, string target)
    {
        string[] info = { item, target };
        bool occured = Check(info, ActionType.ObjectUseOn);
        UpdatePoints(occured ? 1 : (target == "" ? 0 : -1));

        Debug.Log("Use " + item + " on " + target + " with result " + occured);

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();
    }

    /// <summary>
    /// Handle (trigger) Examine aciton.
    /// </summary>
    /// <param name="item">Name of the examined item</param>
    /// <param name="expected">State of the examined item</param>
    public void OnExamineAction(string item, string expected)
    {
        string[] info = { item, expected };
        bool occured = Check(info, ActionType.ObjectExamine);
        UpdatePoints(occured ? 1 : 0); // no penalty

        Debug.Log("Examine " + item + " with state " + expected + " with result " + occured);

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();
    }

    /// <summary>
    /// Handle (trigger) picking up action.
    /// Is not penalised, but needs to be checked.
    /// </summary>
    /// <param name="item">Name of the picked item</param>
    public void OnPickUpAction(string item)
    {
        string[] info = { item };
        bool occured = Check(info, ActionType.PickUp);
        UpdatePoints(occured ? 1 : 0); // no penalty

        if (occured)
        {
            Debug.Log("Pick Up " + item);
        }

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();

        ActionManager.BuildRequirements();
        ActionManager.UpdateRequirements();
    }

    public void OnSequenceStepAction(string stepName)
    {
        string[] info = { stepName };
        bool occured = Check(info, ActionType.SequenceStep);
        UpdatePoints(occured ? 1 : -1);

        Debug.Log("Sequence step: " + stepName + " with result " + occured);

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();
    }

    public void OnDropDownAction(string item, int posId)
    {
        string[] info = { item, posId.ToString() };
        bool occured = Check(info, ActionType.ObjectDrop);
        UpdatePoints(occured ? 1 : 0); // no penalty

        if (occured)
        {
            Debug.Log("Dropped " + item + " on position #" + posId);
        }

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();
    }

    public void OnMovementAction(string position)
    {
        string[] info = { position };
        bool occured = Check(info, ActionType.Movement);

        Debug.Log($"Player moved to {position.Replace("Pos", "")} position with result {occured}");

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();

        UpdateRequirements();
    }

    public class StepData
    {
        public bool completed;
        public string requirement;
        public int subindex = 0;
        public string actionType;
        public bool disabled = false;

        public StepData(bool completedValue, string requirementValue, int index)
        {
            completed = completedValue;
            requirement = requirementValue;
            subindex = index;
        }
    }

    /// <summary>
    /// Checks if triggered action is correct ( expected to be done in action list ).
    /// Plays WrongAction sound from Narrator if wrong.
    /// </summary>
    /// <param name="info">Info passed from Handling functions.</param>
    /// <param name="type">Type of the action</param>
    /// <returns>True if action expected and correct. False otherwise.</returns>
    public bool Check(string[] info, ActionType type)
    {
        bool matched = false;

        // make a selection from all actions list
        // create a sublist with not completed actions of current action index only
        List<Action> sublist = actionList.Where(action =>
            action.SubIndex == currentActionIndex &&
            action.matched == false).ToList();

        // make sublist even smaller, leaving only actions that are not blocked by other steps
        sublist = sublist.Where(action =>
            action.blockRequired == "" ||
            unlockedBlocks.Contains(action.blockRequired)).ToList();

        int subcategoryLength = sublist.Count;

        // make a list from sublist with actions of performed action type only
        List<Action> subtypelist = sublist.Where(action => action.Type == type).ToList();

        if (sublist.Count != 0)
        {
            foreach (Action action in subtypelist)
            {
                if (action.Compare(info))
                {
                    matched = true;
                    action.matched = true;

                    int index = actionList.IndexOf(action);

                    //inserted checklist stuff
                    //RobotUITabChecklist.StrikeStep(index);

                    if (action.blockUnlock != "")
                    {
                        unlockedBlocks.Add(action.blockUnlock);
                    }

                    if (action.blockLock != "")
                    {
                        unlockedBlocks.Remove(action.blockLock);
                    }

                    if (action.quizTriggerTime >= 0.0f)
                    {
                        PlayerScript.TriggerQuizQuestion(action.quizTriggerTime);
                    }

                    if (action.messageContent != "" || action.messageTitle != "")
                    {
                        GameObject.FindObjectOfType<RobotUIMessageTab>().NewMessage(
                            action.messageTitle, action.messageContent, RobotUIMessageTab.Icon.Info);
                    }

                    if (type == ActionType.SequenceStep && penalized)
                    {
                        wrongStepIndexes.Add(index);
                    }
                    else
                    {
                        // end checklist
                        correctStepIndexes.Add(index);
                    }

                    // count only 1 step, some steps are identical
                    break;
                }
            }
        }

        if (matched)
        {
            currentStepHintUsed = false;
            GameObject.FindObjectOfType<GameUI>().ButtonBlink(false);
        }
        else if (manager != null && !manager.practiceMode) // test mode error, check for blocks
        {
            // action not matched this check, so list didnt change
            // so we can perform same selection as before
            List<Action> sublistWithoutBlocks = actionList.Where(action =>
                action.SubIndex == currentActionIndex &&
                action.matched == false && action.Type == type).ToList();

            // make a flag that will become true if there is a step that is blocked
            // and could actually be performed if there would be no block
            bool foundBlockedStep = false;
            Action foundAction = null;

            if (sublistWithoutBlocks.Count != 0)
            {
                foreach (Action action in sublistWithoutBlocks)
                {
                    if (action.Compare(info))
                    {
                        foundAction = action;
                        foundBlockedStep = true;
                        break; // found step, no need to continue
                    }
                }
            }

            // if there's actually such step, make  message
            if (foundBlockedStep)
            {
                string title, message;
                // found action will be assigned if foundBlockedStep == true
                if (foundAction.blockTitle != "" && foundAction.blockMessage != "")
                {
                    title = foundAction.blockTitle;
                    message = foundAction.blockMessage;
                }
                else
                {
                    title = "Step is blocked";
                    message = "Looks like this stepped cannot be performed YET. You need to do something before it.";
                }

                RobotUIMessageTab messageCenter = GameObject.FindObjectOfType<RobotUIMessageTab>();
                messageCenter.NewMessage(title, message, RobotUIMessageTab.Icon.Block);
            }
        }

        if (matched && subcategoryLength <= 1)
        {
            currentActionIndex += 1;
        }

        if (!matched && type != ActionType.ObjectExamine && type != ActionType.PickUp && type != ActionType.ObjectDrop && type != ActionType.Movement)
        {
            int index = actionList.IndexOf(currentAction);
            if (sublist.Count > 0 && !wrongStepIndexes.Contains(index))
            {
                wrongStepIndexes.Add(index);
            }

            bool practice = (manager != null) ? manager.practiceMode : true;

            if (practice)
            {
                if (sublist.Count > 0)
                {
                    RobotUIMessageTab messageCenter = GameObject.FindObjectOfType<RobotUIMessageTab>();
                    if (type == ActionType.SequenceStep)
                        messageCenter.NewMessage("Verkeerde handeling!", sublist[0].extraDescr, RobotUIMessageTab.Icon.Error);
                    else
                        messageCenter.NewMessage("Verkeerde handeling!", sublist[0].extraDescr, RobotUIMessageTab.Icon.Block);

                }
            }

            //--------------------------------------------------
            ActionManager.WrongAction(type != ActionType.SequenceStep);

            penalized = true;
        }
        else
        {
            currentPointAward = currentAction.pointValue;
            List<Action> actionsLeft = actionList.Where(action =>
                action.SubIndex == currentActionIndex &&
                action.matched == false).ToList();

            currentAction = actionsLeft.Count > 0 ? actionsLeft.First() : null;

            if ((manager == null || manager.practiceMode) && type != ActionType.Movement)
            {
                //now we have not mandatory actions, let's skip them and add to mistakes
                List<Action> skippableActions = actionsLeft.Where(action => action.notMandatory == true).ToList();
                if (actionsLeft.Count == skippableActions.Count) // all of them are skippable
                {
                    wrongStepIndexes.Add(actionList.IndexOf(currentAction));
                    currentActionIndex += 1;

                    // get next actions with new index
                    actionsLeft = actionList.Where(action =>
                        action.SubIndex == currentActionIndex &&
                        action.matched == false).ToList();

                    currentAction = actionsLeft.Count > 0 ? actionsLeft.First() : null;
                }
            }
        }

        return matched;
    }

    /// <summary>
    /// Check if every action from action list is done and scene is completed.
    /// If yes - go to EndScore scene.
    /// Runs after every action check and clears particle hints.
    /// </summary>
    private bool CheckScenarioCompleted()
    {
        // clear hints
        foreach (GameObject o in particleHints)
            Destroy(o);
        particleHints.Clear();

        if (manager != null && !manager.practiceMode)
        {
            if (actionList.Find(action => action.matched == true && action.sceneDoneTrigger == true) != null)
            {
                SceneCompletedRoutine();
                return true;
            }
            else return false;
        }
        else
        {
            if (actionList.Find(action => action.matched == false && action.notMandatory == false) == null)
            {
                SceneCompletedRoutine();
                return true;
            }
            else return false;
        }
    }

    private void SceneCompletedRoutine()
    {
        Narrator.PlaySound("LevelComplete", 0.1f);
        GameObject.FindObjectOfType<GameUI>().ShowDonePanel(true);

        /*//Loginpro is removed but these achievements can be used with new system later
        GameObject ach = GameObject.Find("FinishProtocol");
        if (ach != null)
        {
            ach.GetComponent<LoginProAsset.LoginPro_Achievement>().Unlock(100);
        }

        ach = GameObject.Find("FinishProtocol5min");
        if (ach != null && GameObject.FindObjectOfType<GameTimer>().CurrentTime < 300f)
        {
            ach.GetComponent<LoginProAsset.LoginPro_Achievement>().Unlock(100);
        }

        ach = GameObject.Find("FinishProtocol10min");
        if (ach != null && GameObject.FindObjectOfType<GameTimer>().CurrentTime < 600f)
        {
            ach.GetComponent<LoginProAsset.LoginPro_Achievement>().Unlock(100);
        }

        ach = GameObject.Find("Finish3Protocols");
        if (ach != null)
        {
            ach.GetComponent<LoginProAsset.LoginPro_Achievement>().Unlock(34);
        }

        ach = GameObject.Find("Finish5Protocols");
        if (ach != null)
        {
            ach.GetComponent<LoginProAsset.LoginPro_Achievement>().Unlock(20);
        }*/
    }

    /// <summary>
    /// Sets state of every action of the list.
    /// </summary>
    /// <param name="items">True = action completed</param>
    public void SetActionStatus(List<bool> items)
    {
        if (items.Count == actionList.Count)
        {
            for (int i = 0; i < actionList.Count; ++i)
            {
                actionList[i].matched = items[i];
            }
        }
    }

    /// <summary>
    /// Rolls animation sequence back.
    /// </summary>
    public void RollSequenceBack()
    {
        Action lastAction = actionList.Last(x => x.matched == true);
        while (lastAction.Type == ActionType.SequenceStep)
        {
            lastAction.matched = false;
            lastAction = actionList.Last(x => x.matched == true);
        }
        lastAction.matched = false;
        currentActionIndex = lastAction.SubIndex;
        currentAction = lastAction;
        GameObject.FindObjectOfType<Cheat_CurrentAction>().UpdateAction();
    }

    public static void PlayAddPointSound()
    {
        Narrator.PlaySound("PointScored", 0.1f);
        // todo move somewhere else
        if (GameObject.Find("_Dev") != null)
        {
            GameObject.Find("_Dev").GetComponent<Cheat_CurrentAction>().UpdateAction();
        }
    }

    /* not used
    public void OnGameOver()
    {
        Transform gameOver = GameObject.Find("UI").transform.Find("GameOver");
        gameOver.gameObject.SetActive(true);

        if (GameObject.Find("GameLogic") != null)
        {
            controls.keyPreferences.ToggleLock();
            GameObject.Find("GameLogic").GetComponent<GameTimer>().enabled = false;
        }

        PlayerScript player = GameObject.Find("Player").GetComponent<PlayerScript>();
        Crosshair crosshair = GameObject.Find("Player").GetComponent<Crosshair>();
        Animator animator = player.transform.GetChild(0).GetChild(0).GetComponent<Animator>();

        player.enabled = false;
        crosshair.enabled = false;

        animator.speed = 0.0f;
        Time.timeScale = 0f;

        AudioSource[] audio = GameObject.FindObjectsOfType<AudioSource>();
        foreach (AudioSource a in audio)
        {
            a.Pause();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    } */

    public void OnRetryButtonClick()
    {
        GameObject.Find("Preferences").GetComponent<LoadingScreen>().LoadLevel(
            SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuButtonClick()
    {
        GameObject.Find("Preferences").GetComponent<LoadingScreen>().LoadLevel("UMenuPro");
    }

    public static void WrongAction(bool headAnimation = true)
    {
        RobotManager.RobotWrongAction();
        Narrator.PlaySound("WrongAction");

        if (headAnimation)
        {
            PlayerAnimationManager.PlayAnimation("no");
        }
    }

    public static void CorrectAction()
    {
        GameObject.FindObjectOfType<ActionManager>().actionsCount++;
        RobotManager.RobotCorrectAction();
        ActionManager.PlayAddPointSound();

        ActionManager.BuildRequirements();
        ActionManager.UpdateRequirements();
    }

    public static void BuildRequirements()
    {
        if (!practiceMode)
            return;

        ActionManager am = GameObject.FindObjectOfType<ActionManager>();
        List<Action> sublist = am.actionList.Where(action =>
               action.SubIndex == am.currentActionIndex &&
               action.matched == false).ToList();

        foreach (Action a in sublist)
        {
            string[] ObjectNames = new string[0];
            a.ObjectNames(out ObjectNames);

            switch (a.Type)
            {
                case ActionType.PersonTalk:
                    foreach (PersonObject po in GameObject.FindObjectsOfType<PersonObject>())
                    {
                        if (po.hasTopic(a._topic))
                        {
                            a.placeRequirement = ActionManager.FindNearest(new string[] { po.name });
                        }
                    }
                    break;
                case ActionType.ObjectCombine:
                    a.leftHandRequirement = ObjectNames[0];
                    a.rightHandRequirement = ObjectNames[1];
                    if (a.placeRequirement == "")
                        a.placeRequirement = ActionManager.FindNearest(new string[] { ObjectNames[0], ObjectNames[1] });
                    break;
                case ActionType.ObjectUseOn:
                    a.leftHandRequirement = ObjectNames[0];
                    if (a.placeRequirement == "")
                        a.placeRequirement = ActionManager.FindNearest(new string[] { ObjectNames[0] });
                    break;
                case ActionType.ObjectExamine:
                    a.leftHandRequirement = ObjectNames[0];
                    if (a.placeRequirement == "")
                        a.placeRequirement = ActionManager.FindNearest(new string[] { ObjectNames[0] });
                    break;
                case ActionType.PickUp:
                    a.leftHandRequirement = ObjectNames[0];
                    if (a.placeRequirement == "")
                        a.placeRequirement = ActionManager.FindNearest(new string[] { ObjectNames[0] });
                    break;
                case ActionType.ObjectDrop:
                    a.leftHandRequirement = ObjectNames[0];
                    if (a.placeRequirement == "")
                        a.placeRequirement = ActionManager.FindNearest(new string[] { ObjectNames[0] });
                    break;
                case ActionType.ObjectUse:
                    a.leftHandRequirement = ObjectNames[0];
                    if (a.placeRequirement == "")
                        a.placeRequirement = ActionManager.FindNearest(new string[] { ObjectNames[0] });
                    break;
            }
        }
        if (GameObject.FindObjectOfType<ActionsPanel>() != null)
            GameObject.FindObjectOfType<ActionsPanel>().UpdatePanel();
    }

    public static List<GameObject> FindAnchers(string[] objectNames)
    {
        List<GameObject> anchors = new List<GameObject>();
        WorkField workField = GameObject.FindObjectOfType<WorkField>();
        CatheterPack catheterPack = GameObject.FindObjectOfType<CatheterPack>();

        foreach (string o in objectNames)
        {
            bool found = false;
            foreach (ExtraObjectOptions e in GameObject.FindObjectsOfType<ExtraObjectOptions>())
            {
                if (e.HasNeeded(o) != "")
                {
                    anchors.Add(e.gameObject);
                    found = true;
                    break;
                }
            }

            if (found)
                continue;

            if (workField != null)
            {
                foreach (GameObject workFieldObject in workField.objects)
                {
                    if (workFieldObject != null)
                    {
                        if (workFieldObject.name == o)
                        {
                            anchors.Add(workField.gameObject);
                            found = true;
                            break;
                        }
                    }
                }
            }
            if (found)
                continue;

            if (catheterPack != null)
            {
                foreach (GameObject catObject in catheterPack.objects)
                {
                    if (catObject != null)
                    {
                        if (catObject.name == o)
                        {
                            anchors.Add(workField.gameObject);
                            found = true;
                            break;
                        }
                    }
                }
            }
            if (found)
                continue;
            if (GameObject.Find(o) != null)
            {
                anchors.Add(GameObject.Find(o));
            }
        }
        return anchors;
    }

    public static WalkToGroup NearestWalkToGroup(GameObject obj)
    {
        WalkToGroup nearest = GameObject.FindObjectOfType<WalkToGroup>();
        float nearestDist = Vector3.Distance(nearest.transform.position, obj.transform.position);
        foreach (WalkToGroup w in GameObject.FindObjectsOfType<WalkToGroup>())
        {
            float dist = Vector3.Distance(w.transform.position, obj.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = w;
            }
        }
        return nearest;
    }

    public static string FindNearest(string[] objectNames)
    {
        List<WalkToGroup> nearestGroups = new List<WalkToGroup>();
        List<GameObject> anchors = ActionManager.FindAnchers(objectNames);
        if (anchors != null)
        {
            foreach (GameObject a in anchors)
            {
                nearestGroups.Add(ActionManager.NearestWalkToGroup(a));
            }
        }

        if (nearestGroups.Count > 0)
        {
            WalkToGroup ng = nearestGroups[0];
            foreach (WalkToGroup w in nearestGroups)
            {
                if (ng != w)
                    return "";
            }

            return ng.name;
        }

        return "";
    }

    public void UpdatePoints(int value)
    {
        if (value <= 0)
            return;

        value *= (penalized ? currentPointAward / 2 : currentPointAward);
        penalized = false;

        points += value;

        if (points < 0)
        {
            points = 0;
        }
    }

    public void UpdatePointsDirectly(int value)
    {
        points += (penalized ? value / 2 : value);
        penalized = false;

        if (points < 0)
        {
            points = 0;
        }
    }

    public void ActivatePenalty()
    {
        penalized = true;
    }

    public void SetUIObjects(Text points, Text percentage)
    {
        uiSet = true;

        pointsText = points;
        percentageText = percentage;
    }
}
