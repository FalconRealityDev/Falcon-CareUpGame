using System.Xml;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CareUp.Actions;
using CareUp.Localize;
using System.Collections;
using System.Security.Cryptography;

/// <summary>
/// GameLogic script. Everything about actions is managed by this script.
/// </summary>
public class ActionManager : MonoBehaviour
{
    private PlayerPrefsManager manager;

    public class RandomEventBookmak
    {
        public int actionIndex;
        public List<string> randomEventGroups;
        public bool complited = false;
        public RandomEventBookmak(int _actionIndex, List<string> _randomEventGroups)
        {
            actionIndex = _actionIndex;
            randomEventGroups = _randomEventGroups;
        }
        
        public int GetActionIndex()
        {
            return actionIndex;
        }
        public void SetActionIndex(int value)
        {
            actionIndex = value;
        }
        public List<string> GetEventGroups()
        {
            return randomEventGroups;
        }

        public bool IsComplited()
        {
            return complited;
        }

        public void Complite()
        {
            complited = true;
        }
    }
    public static int complitedRandomEvents = 0;

    public static List<RandomEventBookmak> randomEventBookmaks = new List<RandomEventBookmak>();

    //bool complitedSequenceStep = false; value never used
    public static bool practiceMode = true;
    public static bool personClicked = false;


    [HideInInspector]
    public bool tutorial_hintUsed = false;

    public int actionsCount = 0;
    public bool TextDebug = false;
    public int currentActionIndex = 0;  // index of current action

    // name of the xml file with actions
    public string actionListName;

    // actual list of actions
    public List<Action> actionList = new List<Action>();

    private Text pointsText;
    private Text percentageText;

    public Action currentAction;        // current action instance
    private int currentPointAward = 1;
    private bool penalized = false;

    private int totalPoints = 0;         // max points of scene
    private static int points = 0;              // current points  

    // list of descriptions of steps, player got penalty on
    private List<string> stepsList = new List<string>();
    //private List<string> stepsDescriptionList = new List<string>();
    private List<int> wrongStepIndexes = new List<int>();
    private List<int> correctStepIndexes = new List<int>();

    // GameObjects that show player next step when hint used
    private static PlayerScript playerScript;
    private List<GameObject> particleHints;
    private bool menuScene;
    //private bool uiSet = false;

    private List<Action> incompletedActions;
    private List<Action> completedActions;

    private List<Action> unlockedIncompletedActions;
    private List<string> unlockedBlocks = new List<string>();

    //sequence section
    static AnimationSequence currentSequence = null;

    public string Message { get; set; } = null;
    public string MessageTitle { get; set; } = null;
    public bool ShowTheory { get; set; } = false;
    public bool ShowRandomEvent { get; set; } = false;
    public List<int> currentRandomEventIndices = new List<int>();

    public static int percentage;

    public static void AddRandomEventData(int actionIndex, List<string> randomEventGroup)
    {
        randomEventBookmaks.Add(new RandomEventBookmak(actionIndex, randomEventGroup));
    }

    public static void FinalizeRandomEventData()
    {
        string ss = "Random event actions: ";
        int numberOfActions = GameObject.FindObjectOfType<ActionManager>().actionList.Count;
        for (int i = 0; i < randomEventBookmaks.Count; i++)
        {
            if (randomEventBookmaks[i].GetActionIndex() == -1)
            {
                int newActionIndex = Random.Range(0, numberOfActions - 2);
                randomEventBookmaks[i].SetActionIndex(newActionIndex);
            }
            ss += randomEventBookmaks[i].GetActionIndex().ToString() + ", ";
        }
        Debug.Log(ss);
    }


    public List<Action> ActionList
    {
        get { return actionList; }
    }

    public List<string> StepsList
    {
        get { return stepsList; }
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
    public static int Points
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

    /// <summary>
    /// Index of current action.
    /// </summary>
    public int CurrentActionIndex
    {
        get { return currentActionIndex; }
        set { currentActionIndex = value; }
    }

    public ActionType CurrentActionType
    {
        get { return currentAction.Type; }
    }

    /// <summary>
    /// A list of not completed actions of current action index only
    /// </summary>
    public List<Action> IncompletedActions
    {
        get
        {
            incompletedActions = actionList.Where(action => action.SubIndex == currentActionIndex && action.matched == false).ToList();
            return incompletedActions;
        }
        set
        {
            incompletedActions = value;
        }
    }

    public List<Action> CompletedActions
    {
        get
        {
            completedActions = actionList.Where(action => action.matched == true).ToList();
            return completedActions;
        }
        set
        {
            completedActions = value;
        }
    }
    /// <summary>
    /// A list of not completed actions of current action index only, that are not blocked by other steps
    /// </summary>
    public List<Action> UnlockedIncompletedActions
    {
        get
        {
            unlockedIncompletedActions = IncompletedActions.Where(action =>
            action.blockRequired.Count == 0 || unlockedBlocks.Intersect(action.blockRequired).Count() == action.blockRequired.Count()).ToList();

            return unlockedIncompletedActions;
        }
        set
        {
            unlockedIncompletedActions = value;
        }
    }

    [HideInInspector]
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
        General,
        None
    };


    public static ObjectDataHolder GetObjectDataByName(string oName, bool fromPrefabs = false)
    {
        if (fromPrefabs)
        {
            PrefabHolder prefabHolder = GameObject.FindObjectOfType<PrefabHolder>();
            if (prefabHolder != null)
            {
                foreach (GameObject o in prefabHolder.GetObjects())
                {
                    if (o.GetComponent<ObjectDataHolder>() != null)
                    {

                        if (o.GetComponent<ObjectDataHolder>().objectPrefabNames.Contains(oName))
                            return o.GetComponent<ObjectDataHolder>();
                    }
                }
            }
        }
        else
        {
            foreach(ObjectDataHolder o in GameObject.FindObjectsOfType<ObjectDataHolder>())
            {
                if (o.GetComponent<ObjectDataHolder>().objectPrefabNames.Contains(oName))
                    return o;
            }
        }
        return null;
    }

//----------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------
    public static void UpdateRequirements(float showDelay = 0f)
    {
        //Debug.Log("@UpdateRequirements:(float showDelay = 0f)");
        ActionManager actManager = GameObject.FindObjectOfType<ActionManager>();
        if (playerScript == null)
            playerScript = GameObject.FindObjectOfType<PlayerScript>();

        GeneralAction generalAction = actManager.CheckGeneralAction();
        if (generalAction != null && !playerScript.Away())
        {
            actManager.NotTriggeredAction(generalAction);
        }

        if (!practiceMode)
        {
            return;
        }

        GameUIVR gameUIVR = GameObject.FindObjectOfType<GameUIVR>();
        List<StepData> stepsList = new List<StepData>();
        HandsInventory inventory = GameObject.FindObjectOfType<HandsInventory>();

        int i = 0;
        bool foundComplitedAction = false;
        gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.None;
        gameUIVR.dropLeftBlink = false;
        gameUIVR.dropRightBlink = false;
        // gameUIVR.reqPlaces.Clear();
        bool leftIncorrect = true;
        bool rightIncorrect = true;
        bool noObjectActions = true;
        bool anyCorrectPlace = false;
        List<string> placesReqList = new List<string>();
        string uncomplitedSecondPlace = "";
        gameUIVR.recordsButtonBlink = false;
        gameUIVR.prescriptionButtonBlink = false;

        foreach (Action a in actManager.IncompletedActions)
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

                if (GameObject.Find(a.placeRequirement).GetComponent<WalkToGroupVR>().description != "")
                    placeName = GameObject.Find(a.placeRequirement).GetComponent<WalkToGroupVR>().description;

                bool completed = false;
                if (playerScript.currentWTGName != "")
                    completed = playerScript.currentWTGName == a.placeRequirement;

                placeData = new StepData(completed, $"- Ga naar {placeName}.", i);
                if (completed)
                    anyCorrectPlace = true;

                if (a.Type == ActionType.PersonTalk && dialog)
                {
                    // foreach (PersonObject po in GameObject.FindObjectsOfType<PersonObject>())
                    // {
                    //     if (po.hasTopic(a._topic))
                    //     {
                    //         gameUIVR.PlaceTalkBubble(po.gameObject);
                    //     }
                    // }

                    // if (personClicked)
                    //     objectsData.Add(new StepData(false, $"- Kies wat je gaat doen.", i));
                    // else
                    //     objectsData.Add(new StepData(false, $"- Klik op {placeName}.", i));
                }
            }

            if (!string.IsNullOrEmpty(a.secondPlaceRequirement))
            {
                string placeName = a.secondPlaceRequirement;

                if (GameObject.Find(a.secondPlaceRequirement).GetComponent<WalkToGroupVR>().description != "")
                    placeName = GameObject.Find(a.secondPlaceRequirement).GetComponent<WalkToGroupVR>().description;

                bool completed = false;
                if (playerScript.currentWTGName != "")
                    completed = playerScript.currentWTGName == a.secondPlaceRequirement;

                secondPlaceData = new StepData(completed, $"- Ga naar {placeName}.", i);

                if (a.Type == ActionType.PersonTalk && dialog)
                {
                    // foreach (PersonObject po in GameObject.FindObjectsOfType<PersonObject>())
                    // {
                    //     if (po.hasTopic(a._topic))
                    //     {
                    //         gameUIVR.PlaceTalkBubble(po.gameObject);
                    //     }
                    // }
                    // if (personClicked)
                    //     objectsData.Add(new StepData(false, $"- Kies wat je gaat doen.", i));
                    // else
                    //     objectsData.Add(new StepData(false, $"- Klik op {placeName}.", i));
                }
            }

            if (a.Type == ActionType.General)
            {
                objectsData.Add(new StepData(false, $"- Klik op de '{actManager.CurrentButtonText()}' knop.", i));
                actManager.NotTriggeredAction();
                gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.NoTargetRight;
                foundComplitedAction = true;
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
                objectsData.Add(new StepData(false, $"- Klik op het informatie icoon.", i));

                if (a.leftHandRequirement == "PatientRecords")
                {
                    gameUIVR.recordsButtonBlink = true;
                }
                else if (a.leftHandRequirement == "PrescriptionForm")
                {
                    gameUIVR.prescriptionButtonBlink = true;
                }
                else if (a.leftHandRequirement == "PaperAndPen")
                {
                    gameUIVR.paperAndPenButtonblink = true;
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
                        ObjectDataHolder obj = GetObjectDataByName(hand);
                        if (obj != null)
                        {
                            if (obj.description != "")
                            {
                                handValue = obj.description;
                                article = obj.nameArticle;
                                found = true;
                                foundDescr = true;
                            }
                        }
                        else if(GameObject.FindObjectOfType<PrefabHolder>() != null)
                        {
                            ObjectDataHolder prefObj = GetObjectDataByName(hand, true);
                            if (prefObj != null)
                            {
                                if (prefObj.description != "")
                                {
                                    handValue = prefObj.description;
                                    article = prefObj.nameArticle;
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
                        
                        // if (GameObject.FindObjectOfType<WorkField>() != null && !found)
                        // {
                        //     foreach (WorkField w in GameObject.FindObjectsOfType<WorkField>())
                        //     {
                        //         foreach (GameObject obj in w.objects)
                        //         {
                        //             if (obj != null)
                        //             {
                        //                 if (obj.name == hand)
                        //                 {
                        //                     article = obj.GetComponent<InteractableObject>().nameArticle;
                        //                     handValue = obj.GetComponent<InteractableObject>().description;
                        //                     foundDescr = true;
                        //                     found = true;
                        //                     break;
                        //                 }
                        //             }
                        //         }
                        //         if (found)
                        //             break;
                        //     }
                        // }
                        // if (GameObject.FindObjectsOfType<CatheterPack>() != null && !found)
                        // {
                        //     foreach (CatheterPack w in GameObject.FindObjectsOfType<CatheterPack>())
                        //     {
                        //         foreach (GameObject obj in w.objects)
                        //         {
                        //             if (obj != null)
                        //             {
                        //                 if (obj.name == hand)
                        //                 {
                        //                     handValue = obj.GetComponent<InteractableObject>().description;
                        //                     article = obj.GetComponent<InteractableObject>().nameArticle;
                        //                     foundDescr = true;
                        //                     found = true;
                        //                     break;
                        //                 }
                        //             }
                        //         }
                        //         if (found)
                        //             break;
                        //     }
                        // }

                        bool completed = false;
                        ObjectDataHolder leftObjectDataHolder = inventory.LeftHandObjectData();

                        if (leftObjectDataHolder != null)
                        {
                            if (leftObjectDataHolder != null && !string.IsNullOrEmpty(leftObjectDataHolder.description))
                                currentLeftObject = System.Char.ToLowerInvariant(leftObjectDataHolder.description[0]) + 
                                    leftObjectDataHolder.description.Substring(1);

                            if (leftObjectDataHolder.objectPrefabNames.Contains(hand))
                            {
                                leftIncorrect = false;
                                completed = true;
                                leftR = leftObjectDataHolder.gameObject;
                            }
                        }

                        ObjectDataHolder rightObjectDataHolder = inventory.RightHandObjectData();

                        if (rightObjectDataHolder != null)
                        {
                            if (rightObjectDataHolder != null && !string.IsNullOrEmpty(rightObjectDataHolder.description))
                                currentRightObject = System.Char.ToLowerInvariant(rightObjectDataHolder.description[0]) + 
                                    rightObjectDataHolder.description.Substring(1);

                            if (rightObjectDataHolder.objectPrefabNames.Contains(hand))
                            {
                                rightIncorrect = false;
                                completed = true;
                                rightR = rightObjectDataHolder.gameObject;
                            }
                        }
#if UNITY_EDITOR
                        // if (!foundDescr)
                        // {
                        //     if (GameObject.FindObjectOfType<NeededObjectsLister>() != null)
                        //     {
                        //         if (!GameObject.FindObjectOfType<NeededObjectsLister>().addNeeded(hand))
                        //             print("______Add to extra______   " + hand);
                        //     }
                        // }
#endif
                        if (handValue != null)
                        {
                            if (handValue != "")
                            {
                                handValue = System.Char.ToLowerInvariant(handValue[0]) + handValue.Substring(1);
                            }
                        }
                        string keyWords = null;

                        if (leftR != null && rightR != null)
                        {
                            objectCombinationCheck = ((leftR.name == a.leftHandRequirement) && 
                                (rightR.name == a.rightHandRequirement)) || 
                                ((rightR.name == a.leftHandRequirement) && 
                                (leftR.name == a.rightHandRequirement));
                        }

                        if (objectCombinationCheck && a.Type == ActionType.ObjectCombine)
                        {
                            objectsData.Add(new StepData(false, "- Klik op de 'Combineer' knop.", i));
                            if (!foundComplitedAction)
                            {
                                foundComplitedAction = true;
                                gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.Combine;
                            }
                        }

                        else if (leftR != null)
                        {

                            List<string> oPrefabNames = new List<string>();
                            ObjectDataHolder leftData = inventory.LeftHandObjectData();
                            if (leftData != null)
                                oPrefabNames = leftData.objectPrefabNames;
                            foreach (string lObjPrefabName in oPrefabNames)
                            {

                                if (actManager.CompareUseOnInfo(lObjPrefabName, ""))
                                {
                                    keyWords = actManager.CurrentButtonText(lObjPrefabName);

                                    objectsData.Add(new StepData(false, $"- Klik op de '{keyWords}' knop.", i));
                                    if (!foundComplitedAction)
                                    {
                                        foundComplitedAction = true;
                                        gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.NoTargetLeft;
                                    }
                                }
                                else if (a.Type == ActionType.ObjectDrop && a.leftHandRequirement == lObjPrefabName)
                                {
                                    if (!foundComplitedAction)
                                    {
                                        foundComplitedAction = true;
                                        gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.None;
                                    }
                                    if (secondPlaceData != null)
                                    {
                                        if (secondPlaceData.completed)
                                        {
                                            gameUIVR.dropLeftBlink = true;

                                            objectsData.Add(new StepData(false, $"- Leg {article} {handValue} neer.", i));
                                        }
                                    }
                                    else
                                    {
                                        gameUIVR.dropLeftBlink = true;
                                        objectsData.Add(new StepData(false, $"- Leg {article} {handValue} neer.", i));
                                    }
                                }
                                else if (a.Type == ActionType.ObjectExamine && lObjPrefabName == a.leftHandRequirement)
                                {
                                    objectsData.Add(new StepData(false, $"- Klik op de 'Controleren' knop.", i));
                                    if (!foundComplitedAction)
                                    {
                                        foundComplitedAction = true;
                                        gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.ZoomLeft;
                                    }
                                }

                                else if (!string.IsNullOrEmpty(actManager.CurrentDecombineButtonText(lObjPrefabName)))
                                {
                                    keyWords = actManager.CurrentDecombineButtonText(lObjPrefabName);
                                    objectsData.Add(new StepData(false, $"- Klik op de '{actManager.CurrentDecombineButtonText(lObjPrefabName)}' knop.", i));
                                    if (!foundComplitedAction)
                                    {
                                        foundComplitedAction = true;
                                        gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.DecombineLeft;
                                    }
                                }
                                else if (!foundComplitedAction)
                                    gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.None;

                            }
                        }

                        else if (rightR != null)
                        {
                            List<string> oPrefabNames = new List<string>();
                            ObjectDataHolder leftData = inventory.LeftHandObjectData();
                            if (leftData != null)
                                oPrefabNames = leftData.objectPrefabNames;
                            foreach (string rObjPrefabName in oPrefabNames)
                            {
                                if (actManager.CompareUseOnInfo(rObjPrefabName, ""))
                                {
                                    keyWords = actManager.CurrentButtonText(rObjPrefabName);

                                    objectsData.Add(new StepData(false, $"- Klik op de '{keyWords}' knop.", i));
                                    if (!foundComplitedAction)
                                    {
                                        foundComplitedAction = true;
                                        gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.NoTargetRight;
                                    }
                                }
                                else if (a.Type == ActionType.ObjectDrop)
                                {
                                    if (!foundComplitedAction)
                                    {
                                        foundComplitedAction = true;
                                        gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.None;
                                    }
                                    if (secondPlaceData != null)
                                    {
                                        if (secondPlaceData.completed)
                                        {
                                            gameUIVR.dropRightBlink = true;
                                            objectsData.Add(new StepData(false, $"- Leg {article} {handValue} neer.", i));
                                        }
                                    }
                                    else
                                    {
                                        gameUIVR.dropRightBlink = true;
                                        objectsData.Add(new StepData(false, $"- Leg {article} {handValue} neer.", i));
                                    }    
                                }
                                else if (a.Type == ActionType.ObjectExamine && rObjPrefabName == a.leftHandRequirement)
                                {
                                    objectsData.Add(new StepData(false, $"- Klik op de 'Controleren' knop.", i));
                                    if (!foundComplitedAction)
                                    {
                                        foundComplitedAction = true;
                                        gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.ZoomRight;
                                    }
                                }
                                else if (a.Type == ActionType.PersonTalk)
                                {
                                    if (!foundComplitedAction)
                                    {
                                        gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.None;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(actManager.CurrentDecombineButtonText(rObjPrefabName)))
                                {
                                    keyWords = actManager.CurrentDecombineButtonText(rObjPrefabName);
                                    objectsData.Add(new StepData(false, $"- Klik op de '{actManager.CurrentDecombineButtonText(rObjPrefabName)}' knop.", i));
                                    if (!foundComplitedAction)
                                    {
                                        foundComplitedAction = true;
                                        gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.DecombineRight;
                                    }
                                }
                                else
                                    gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.None;
                                }
                            }
                        
                        else if (!foundComplitedAction)
                            gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.None;


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
            if (!(placeData == null && secondPlaceData == null))
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

            gameUIVR.moveButtonToBlink = GameUIVR.MoveControlButtonType.None;

            if (uncomplitedSecondPlace != "")
            {
                placesReqList.Clear();
                placesReqList.Add(uncomplitedSecondPlace);
            }

            string sss = "";
            foreach (string s in placesReqList)
            {
                sss += s + " | ";
                gameUIVR.reqPlaces.Add(s);
            }

            if ((!anyCorrectPlace || uncomplitedSecondPlace != "") && !playerScript.Away() && placesReqList.Count > 0)
            {

                string currentWTGName = playerScript.currentWTGName;

                foreach (string s in placesReqList)
                {
                    // WalkToGroupButton b = gameUIVR.FindMovementButton(s, playerScript.currentWalkPosition);
                    // if (b != null)
                    // {
                    //     gameUIVR.moveButtonToBlink = b.moveControlButtonType;
                    //     break;
                    // }

                    //int dir = gameUIVR.FindDirection(s, playerScript.currentWalkPosition, 0);
                    //if (dir == -1)
                    //{
                    //    gameUIVR.moveButtonToBlink = GameUIVR.ItemControlButtonType.MoveLeft;
                    //    break;
                    //}
                    //else if (dir == 1)
                    //{
                    //    gameUIVR.moveButtonToBlink = GameUIVR.ItemControlButtonType.MoveRight;
                    //    break;
                    //}
                }
            }

            i++;
        }

        if (gameUIVR.moveButtonToBlink != GameUIVR.MoveControlButtonType.None)
        {
            gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.None;
            gameUIVR.dropRightBlink = false;
            gameUIVR.dropLeftBlink = false;
        }

        personClicked = false;

        gameUIVR.UpdateHintPanel(stepsList, showDelay);

        if (leftIncorrect && !inventory.LeftHandEmpty() && !noObjectActions)
            gameUIVR.dropLeftBlink = true;
        if (rightIncorrect && !inventory.RightHandEmpty() && !noObjectActions)
            gameUIVR.dropRightBlink = true;
        //gameUIVR.UpdateHintPanel(stepsList);

        gameUIVR.UpdateButtonsBlink();
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
            // if (GameObject.FindObjectOfType<GameUIVR>() != null)
            //     Ua = GameObject.FindObjectOfType<ObjectsIDsController>().Ua;
#endif

            if (manager != null && !manager.practiceMode && currentAction != null)
            {
                result = currentAction.shortDescr;
                if (Ua && currentAction.commentUA != "")
                    result = currentAction.commentUA;
            }
            else
            {
                foreach (Action a in IncompletedActions)
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

    // new comparison looks for all actions of type withing current index
    public bool CompareUseObject(string name, bool skipBlocks = false)
    {
        bool result = false;

        List<Action> list = !skipBlocks ? UnlockedIncompletedActions : IncompletedActions;

        foreach (Action a in list)
        {
            if (a.Type == ActionType.ObjectUse)
            {
                if (((UseAction)a).GetObjectName() == name)
                    result = true;
            }
        }

        return result;
    }

    public bool CompareCombineObjects(string left, string right, bool skipBlocks = false)
    {
        bool result = false;

        List<Action> list = !skipBlocks ? UnlockedIncompletedActions : IncompletedActions;

        foreach (Action a in list)
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

    public bool CompareUseOnInfo(string item, string target, string callerName = "", bool skipBlocks = false)
    {
        bool result = false;

        if (callerName != "" && callerName != item)
            return result;

        List<Action> list = !skipBlocks ? UnlockedIncompletedActions : IncompletedActions;

        foreach (Action a in list)
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

    public bool CompareTopic(string t, bool skipBlocks = false)
    {
        bool result = false;

        List<Action> list = !skipBlocks ? UnlockedIncompletedActions : IncompletedActions;

        foreach (Action a in list)
        {
            if (a.Type == ActionType.PersonTalk)
            {
                if (((TalkAction)a).Topic == t)
                    result = true;
            }
        }

        return result;
    }

    public string CurrentButtonText(string itemName = null, bool skipBlocks = false)
    {
        List<Action> list = !skipBlocks ? UnlockedIncompletedActions : IncompletedActions;
        foreach (Action a in list)
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
            else if (a.Type == ActionType.General && !skipBlocks)
            {
                GeneralAction action = (GeneralAction)a;
                return action.ButtonText;
            }
        }

        return "";
    }

    public string CurrentDecombineButtonText(string itemName, bool skipBlocks = false)
    {
        List<Action> list = !skipBlocks ? UnlockedIncompletedActions : IncompletedActions;

        foreach (Action a in list)
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

        foreach (Action a in UnlockedIncompletedActions)
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

        foreach (Action a in UnlockedIncompletedActions)
        {
            if (a.Type == ActionType.Movement)
            {
                if (((MovementAction)a).GetInfo() == position)
                    result = true;
            }
        }

        return result;
    }

    public GeneralAction CheckGeneralAction(bool skipBlocks = false)
    {
        GeneralAction action = null;
        List<Action> list = !skipBlocks ? UnlockedIncompletedActions : IncompletedActions;

        List<Action> generalList = new List<Action>();
        foreach (Action a in list)
        {
            if (a.Type == ActionType.General)
            {
                action = (GeneralAction)a;
                generalList.Add(action);
            }
        }
        if (generalList.Count > 1)
        {
            Action firstGeneral = generalList[0];
            foreach (Action a in generalList)
            {
                if (a.storedIndex < firstGeneral.storedIndex)
                    firstGeneral = a;
            }
            action = (GeneralAction)firstGeneral;
        }
        return action;
    }

    public bool IsNextActionExamineAction()
    {
        bool result = false;

        foreach (Action a in UnlockedIncompletedActions)
        {
            result |= a.Type == ActionType.ObjectExamine;
        }

        return result;
    }

    public bool IsNextActionSequenceStep()
    {
        bool result = false;

        foreach (Action a in UnlockedIncompletedActions)
        {
            result |= a.Type == ActionType.SequenceStep;
        }

        return result;
    }

    // private Controls controls;

    private IEnumerator ChangeFPS26()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            Application.targetFrameRate = 11;
        }
    }

    private IEnumerator ChangeFPS5()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            Application.targetFrameRate = 25;
        }
    }

    /// <summary>
    /// Set some variables and load info from xml file.
    /// </summary>
    void Awake()
    {
        // manager = GameObject.FindObjectOfType<PlayerPrefsManager>();
        string sceneName = SceneManager.GetActiveScene().name;
        menuScene = sceneName == "Menu" || sceneName == "SceneSelection" || sceneName == "EndScore";
        particleHints = new List<GameObject>();
        Points = 0;

        // controls = GameObject.Find("GameLogic").GetComponent<Controls>();
        // if (controls == null) Debug.LogError("No controls found");

        TextAsset textAsset = (TextAsset)Resources.Load("Xml/Actions/" + actionListName);
        XmlDocument xmlFile = new XmlDocument();
        xmlFile.LoadXml(textAsset.text);

        XmlNodeList actions = xmlFile.FirstChild.NextSibling.ChildNodes;

        foreach (XmlNode action in actions)
        {
            if (action.Attributes["hidden"] != null)
                continue;
            int index;
            int.TryParse(action.Attributes["index"].Value, out index);
            string type = action.Attributes["type"].Value;

            string descr = LocalizationManager.GetValueIfKey(action.Attributes["description"].Value);

            string comment = "";
            if (action.Attributes["comment"] != null)
            {
                comment = action.Attributes["comment"].Value;
            }

            bool ignorePosition = action.Attributes["ignorePosition"] != null;

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

            string item = "";
            if (action.Attributes["item"] != null)
            {
                item = action.Attributes["item"].Value;
            }

            string buttonText = "";
            if (action.Attributes["buttonText"] != null)
            {
                buttonText = LocalizationManager.GetValueIfKey(action.Attributes["buttonText"].Value);
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

            float UITimeout = 0f;
            if (action.Attributes["ui_timeout"] != null)
            {
                int intUITimeout = 0;
                int.TryParse(action.Attributes["ui_timeout"].Value, out intUITimeout);
                UITimeout = intUITimeout;
            }

            bool notNeeded = action.Attributes["optional"] != null;

            // try making all steps optional for test
            int storedIndex = index;
            // if (manager != null && !manager.practiceMode)
            // {
            //     notNeeded = true;
            //     index = 0;
            // }

            // quiz triggers
            float quizTime = -1.0f;
            if (action.Attributes["quiz"] != null)
            {
                float.TryParse(action.Attributes["quiz"].Value, out quizTime);
                if (quizTime < 0.1f)
                    quizTime = 2.0f;
            }

            float encounterTime = -1.0f;
            if (action.Attributes["encounter"] != null)
            {
                float.TryParse(action.Attributes["encounter"].Value, out encounterTime);
                if (encounterTime < 0.1f)
                    encounterTime = 2.0f;
            }

            string messageTitle = "";
            if (action.Attributes["messageTitle"] != null)
            {
                messageTitle = LocalizationManager.GetValueIfKey(action.Attributes["messageTitle"].Value);
            }

            string messageContent = "";
            if (action.Attributes["messageContent"] != null)
            {
                messageContent = LocalizationManager.GetValueIfKey(action.Attributes["messageContent"].Value);
            }

            List<string> blockUnlock = new List<string>();
            List<string> blockRequire = new List<string>();
            List<string> blockLock = new List<string>();

            if (action.Attributes["blockUnlock"] != null)
            {
                string[] sentence = action.Attributes["blockUnlock"].Value.Split(new string[] { ", ", "," }, System.StringSplitOptions.None);

                foreach (string word in sentence)
                {
                    blockUnlock.Add(word);
                }
            }

            if (action.Attributes["blockRequired"] != null)
            {
                string[] sentence = action.Attributes["blockRequired"].Value.Split(new string[] { ", ", "," }, System.StringSplitOptions.None);

                foreach (string word in sentence)
                {
                    blockRequire.Add(word);
                }
            }

            if (action.Attributes["blockLock"] != null)
            {
                string[] sentence = action.Attributes["blockLock"].Value.Split(new string[] { ", ", "," }, System.StringSplitOptions.None);

                foreach (string word in sentence)
                {
                    blockLock.Add(word);
                }
            }

            string blockTitle = "";
            string blockMsg = "";
            if (action.Attributes["blockTitle"] != null)
            {
                blockTitle = LocalizationManager.GetValueIfKey(action.Attributes["blockTitle"].Value);
            }

            if (action.Attributes["blockMessage"] != null)
            {
                blockMsg = LocalizationManager.GetValueIfKey(action.Attributes["blockMessage"].Value);
            }

            string decombineText = "Openen";
            if (action.Attributes["decombineText"] != null)
            {
                decombineText = LocalizationManager.GetValueIfKey(action.Attributes["decombineText"].Value);
            }

            switch (type)
            {
                case "combine":
                    string left = action.Attributes["left"].Value;
                    string right = action.Attributes["right"].Value;
                    actionList.Add(new CombineAction(left, right, index, descr,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg, decombineText, encounterTime));
                    break;
                case "use":
                    string use = action.Attributes["value"].Value;
                    actionList.Add(new UseAction(use, index, descr, buttonText,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg, encounterTime));
                    break;
                case "talk":
                    string topic = action.Attributes["topic"].Value;
                    actionList.Add(new TalkAction(topic, index, descr, pointsValue,
                        notNeeded, quizTime, messageTitle, messageContent, blockRequire, blockUnlock,
                        blockLock, blockTitle, blockMsg, encounterTime));
                    break;
                case "useOn":
                    string target = action.Attributes["target"].Value;
                    actionList.Add(new UseOnAction(item, target, index, descr,
                        buttonText, pointsValue, notNeeded, quizTime, messageTitle, messageContent,
                        blockRequire, blockUnlock, blockLock, blockTitle, blockMsg, encounterTime));
                    break;
                case "examine":
                    string expected = action.Attributes["expected"].Value;
                    actionList.Add(new ExamineAction(item, expected, index, descr,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg, encounterTime));
                    break;
                case "pickUp":
                    actionList.Add(new PickUpAction(item, index, descr,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg, encounterTime));
                    break;
                case "sequenceStep":
                    string stepName = action.Attributes["value"].Value;
                    actionList.Add(new SequenceStepAction(stepName, index, descr,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg, encounterTime));
                    break;
                case "drop":
                    string dropID = (action.Attributes["posID"] != null) ? action.Attributes["posID"].Value : "0";
                    actionList.Add(new ObjectDropAction(item, dropID, index, descr,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg, encounterTime));
                    break;
                case "movement":
                    string movement = action.Attributes["value"].Value;
                    actionList.Add(new MovementAction(movement, index, descr,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg, encounterTime));
                    break;
                case "general":
                    string actionValue = action.Attributes["action"].Value;
                    actionList.Add(new GeneralAction(item, actionValue, index, storedIndex, descr, buttonText,
                        pointsValue, notNeeded, quizTime, messageTitle, messageContent, blockRequire,
                        blockUnlock, blockLock, blockTitle, blockMsg, encounterTime));

                    break;
                default:
                    Debug.LogError("No action type found: " + type);
                    break;
            }
            actionList[actionList.Count - 1].comment = comment;
            actionList[actionList.Count - 1].commentUA = commentUA;
            actionList[actionList.Count - 1].secondPlaceRequirement = secondPlace;
            actionList[actionList.Count - 1].placeRequirement = place;
            actionList[actionList.Count - 1].ignorePosition = ignorePosition;
            actionList[actionList.Count - 1].UITimeout = UITimeout;
            actionList[actionList.Count - 1].sequentialNumber = actionList.Count - 1;

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
        }

        currentAction = actionList.First();
    }

    /// <summary>
    /// Handle pressing "Get Hint" key.
    /// Play audio hint, create particle hint, do penalty.
    /// </summary>
    public void CreateParticleHint(Transform obj)
    {
        GameObject particles = Instantiate(Resources.Load<GameObject>("NecessaryPrefabs/ParticleHint"),
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
    public bool OnCombineAction(string leftHand, string rightHand, bool notWrongAction = false)
    {
        string[] info = { leftHand, rightHand };
        bool occured = Check(info, ActionType.ObjectCombine, notWrongAction);
        UpdatePoints(occured ? 1 : -1);

        Debug.Log("Combine " + leftHand + " and " + rightHand + " with result " + occured);

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();
        return occured;
    }

    /// <summary>
    /// Handle (trigger) UseAction.
    /// </summary>
    /// <param name="useObject">Name of the used object.</param>
    public bool OnUseAction(string useObject)
    {
        string[] info = { useObject };
        bool occured = Check(info, ActionType.ObjectUse);
        UpdatePoints(occured ? 1 : -1);

        Debug.Log("Use " + useObject + " with result " + occured);

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();

        return occured;
    }

    /// <summary>
    /// Handle (trigger) Talk action.
    /// </summary>
    /// <param name="topic">Chosen topic</param>
    public bool OnTalkAction(string topic)
    {
        string[] info = { topic };
        bool occured = Check(info, ActionType.PersonTalk);
        UpdatePoints(occured ? 1 : -1);

        Debug.Log("Say " + topic + " with result " + occured);

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();
        return occured;

    }

    /// <summary>
    /// Handle (trigger) UseOn action.
    /// </summary>
    /// <param name="item">Item that is used on target</param>
    /// <param name="target">Item that was targeted</param>
    public bool OnUseOnAction(string item, string target)
    {
        string[] info = { item, target };
        bool occured = Check(info, ActionType.ObjectUseOn);
        UpdatePoints(occured ? 1 : (target == "" ? 0 : -1));

        Debug.Log("Use " + item + " on " + target + " with result " + occured);

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();
        return occured;
    }

    /// <summary>
    /// Handle (trigger) Examine aciton.
    /// </summary>
    /// <param name="item">Name of the examined item</param>
    /// <param name="expected">State of the examined item</param>
    public bool OnExamineAction(string item, string expected)
    {
        string[] info = { item, expected };
        bool occured = Check(info, ActionType.ObjectExamine);
        UpdatePoints(occured ? 1 : 0); // no penalty

        Debug.Log("Examine " + item + " with state " + expected + " with result " + occured);

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();

        return occured;
    }

    /// <summary>
    /// Handle (trigger) picking up action.
    /// Is not penalised, but needs to be checked.
    /// </summary>
    /// <param name="item">Name of the picked item</param>
    public bool OnPickUpAction(string item)
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
        return occured;
    }

    public bool OnSequenceStepAction(string stepName)
    {
        string[] info = { stepName };
        bool occured = Check(info, ActionType.SequenceStep);
        UpdatePoints(occured ? 1 : -1);

        Debug.Log("Sequence step: " + stepName + " with result " + occured);

        if (!CheckScenarioCompleted() && occured)
        {
            ActionManager.CorrectAction();
            playerScript.SetAnimationSpeed(1f);
        }
        return occured;
    }

    public bool OnDropDownAction(string item, int posId)
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
        return occured;
    }

    public bool OnMovementAction(string position)
    {
        string[] info = { position };
        bool occured = Check(info, ActionType.Movement);

        Debug.Log($"Player moved to {position.Replace("Pos", "")} position with result {occured}");

        if (!CheckScenarioCompleted() && occured)
            ActionManager.CorrectAction();

        UpdateRequirements();
        return occured;
    }

    public void OnGeneralAction()
    {
        bool occured = Check(null, ActionType.General);

        if (occured)
        {
            Debug.Log($"General action with result: {occured}");
        }

        if (!CheckScenarioCompleted() && occured)
        {
            ActionManager.CorrectAction();
        }
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

    public void NotTriggeredAction(GeneralAction generalAction = null)
    {
        // GameUIVR gameUIVR = FindObjectOfType<GameUIVR>();
        // string buttonText = "";
        // if (generalAction != null)
        //     buttonText = generalAction.ButtonText;
        // gameUIVR.ShowNoTargetButton(buttonText);

        // if (practiceMode)
        //     gameUIVR.buttonToBlink = GameUIVR.ItemControlButtonType.NoTargetRight;
    }

    /// <summary>
    /// Checks if triggered action is correct ( expected to be done in action list ).
    /// Plays WrongAction sound from Narrator if wrong.
    /// </summary>
    /// <param name="info">Info passed from Handling functions.</param>
    /// <param name="type">Type of the action</param>
    /// <returns>True if action expected and correct. False otherwise.</returns>
    public bool Check(string[] info, ActionType type, bool notWtongAction = false)
    {
        //if (info == null)
        //{
        //    return false;
        //}
        bool matched = false;

        int subcategoryLength = IncompletedActions.Count;

        // make a list from sublist with actions of performed action type only
        List<Action> subtypelist = UnlockedIncompletedActions.Where(action => action.Type == type).ToList();

        // Ugly temporary fix for catheterisation scene
        if (type == ActionType.ObjectDrop && info.Length > 1)
        {
            if (info[0] == "PlasticTrashbucket" && info[1] == "1")
            {
                if (unlockedBlocks.Contains("WaterUnpacked") && unlockedBlocks.Contains("LubUnpacked"))
                {
                    print("catch");
                    info[1] = "2";
                }
            }
        }

        if (IncompletedActions.Count != 0)
        {
            foreach (Action action in subtypelist)
            {
                if (action.Compare(info) || (action.Type == ActionType.General))
                {
                    matched = true;
                    action.matched = true;
                    if (action.UITimeout > 0f)
                    {
                        // GameObject.FindObjectOfType<GameUIVR>().UITimeout(action.UITimeout);
                    }
                    int index = actionList.IndexOf(action);

                    if (action.blockUnlock.Count() > 0)
                    {
                        foreach (string item in action.blockUnlock)
                        {
                            if (!unlockedBlocks.Contains(item))
                                unlockedBlocks.Add(item);
                        }
                    }

                    if (action.blockLock.Count > 0)
                    {
                        foreach (string item in action.blockLock)
                        {
                            unlockedBlocks.Remove(item);
                        }
                    }

                    if (action.quizTriggerTime >= 0.0f)
                    {
                        // PlayerScript.TriggerQuizQuestion(action.quizTriggerTime);
                    }

                    //if (action.SubIndex == 1)
                    //{
                    //    ShowRandomEvent = true;
                    //}
                    // BuildRandomEventList(action.sequentialNumber);
                    //--------------------------------------------------------------

                    if (action.encounter >= 0.0f)
                    {
                        // QuizTab.encounterDelay = action.encounter;
                    }

                    if (action.messageContent != "" || action.messageTitle != "")
                    {
                        if (practiceMode)
                        {
                            ShowTheory = true;
                            Message = action.messageContent;
                            MessageTitle = action.messageTitle;
                        }
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
            //currentStepHintUsed = false;
            // GameObject.FindObjectOfType<GameUIVR>().ButtonBlink(false);
        }
        // else if (manager != null && !manager.practiceMode) // test mode error, check for blocks
        // {
        //     // make a flag that will become true if there is a step that is blocked
        //     // and could actually be performed if there would be no block
        //     bool foundBlockedStep = false;
        //     Action foundAction = null;

        //     if (IncompletedActions.Count != 0)
        //     {
        //         foreach (Action action in IncompletedActions)
        //         {
        //             if (action.Compare(info))
        //             {
        //                 foundAction = action;
        //                 foundBlockedStep = true;
        //                 break; // found step, no need to continue
        //             }
        //         }
        //     }

        //     // if there's actually such step, make  message
        //     if (foundBlockedStep)
        //     {
        //         string title, message;

        //         // found action will be assigned if foundBlockedStep == true
        //         if (foundAction.blockTitle != "" && foundAction.blockMessage != "")
        //         {
        //             title = foundAction.blockTitle;
        //             message = foundAction.blockMessage;
        //         }
        //         else
        //         {
        //             title = "Stap is niet mogelijk";
        //             message = "Je kunt deze stap nog niet doen, het kan zijn dat je een stap vergeten bent.";
        //         }

        //         GameObject.FindObjectOfType<GameUIVR>().ShowBlockMessage(title, message);
        //     }
        // }


        if (matched && subcategoryLength <= 1)
        {
            currentActionIndex += 1;
        }

        if (!matched && type != ActionType.ObjectExamine && type != ActionType.PickUp && type != ActionType.ObjectDrop && type != ActionType.Movement)
        {
            int index = actionList.IndexOf(currentAction);

            if (IncompletedActions.Count > 0 && !wrongStepIndexes.Contains(index))
            {
                wrongStepIndexes.Add(index);
            }

            bool practice = false;//(manager != null) ? manager.practiceMode : true;

            if (practice)
            {
                if (IncompletedActions.Count > 0)
                {
                    // RobotUIMessageTab messageCenter = GameObject.FindObjectOfType<RobotUIMessageTab>();

                    // if (type != ActionType.SequenceStep)
                    //     GameObject.FindObjectOfType<GameUIVR>().ShowBlockMessage("Verkeerde handeling!", IncompletedActions[0].shortDescr);
                }
            }

            if (!notWtongAction)
            {
                ActionManager.WrongAction(type != ActionType.SequenceStep);
            }

            penalized = true;
        }
        else
        {
            currentPointAward = currentAction.pointValue;

            currentAction = IncompletedActions.Count > 0 ? IncompletedActions.First() : null;

            // if ((manager == null || manager.practiceMode) && type != ActionType.Movement)
            // {
            //     //now we have not mandatory actions, let's skip them and add to mistakes
            //     List<Action> skippableActions = IncompletedActions.Where(action => action.notMandatory == true).ToList();

            //     if (IncompletedActions.Count == skippableActions.Count) // all of them are skippable
            //     {
            //         wrongStepIndexes.Add(actionList.IndexOf(currentAction));
            //         currentActionIndex += 1;

            //         // get next actions with new index
            //         IncompletedActions = actionList.Where(action =>
            //             action.SubIndex == currentActionIndex &&
            //             action.matched == false).ToList();

            //         currentAction = IncompletedActions.Count > 0 ? IncompletedActions.First() : null;
            //     }
            // }
        }

        return matched;
    }

    public int GetComplitedRandomEventsCount()
    {
        int ev = 0;
        for (int i = 0; i < currentRandomEventIndices.Count; i++)
        {
            if (randomEventBookmaks[currentRandomEventIndices[i]].complited)
                ev++;
        }
        return ev;
    }

    public void RemoveRandomEventIndex(int index)
    {
        if (index == -1)
        {
            for (int i = 0; i < currentRandomEventIndices.Count; i++)
            {
                randomEventBookmaks[currentRandomEventIndices[i]].Complite();
            }
            currentRandomEventIndices.Clear();
        }
        else
        {
            randomEventBookmaks[currentRandomEventIndices[index]].Complite();
            currentRandomEventIndices.Remove(currentRandomEventIndices[index]);
        }
    }

//     void BuildRandomEventList(int actionSequentialNumber)
//     {
// //for testing random events without starting from main menu, comment those two lines
//         if (manager == null)
//             return;

//         if (manager != null)
//         { 
//             if (manager.currentDifficultyLevel != 3)
//                 return;
//         }
//         for(int i = 0; i < randomEventBookmaks.Count; i++)
//         {
//             if (randomEventBookmaks[i].GetActionIndex() == actionSequentialNumber && !randomEventBookmaks[i].IsComplited())
//             {
//                 currentRandomEventIndices.Add(i);
//             }
//         }
//     }
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

        // if (manager != null && !manager.practiceMode)
        // {
        //     if (actionList.Find(action => action.matched == true && action.sceneDoneTrigger == true) != null)
        //     {
        //         SceneCompletedRoutine();
        //         return true;
        //     }
        //     else return false;
        // }
        // else
        // {
        //     if (actionList.Find(action => action.matched == false && action.notMandatory == false) == null)
        //     {
        //         SceneCompletedRoutine();
        //         return true;
        //     }
        //     else return false;
        // }
        return false; //--------
    }

    private void SceneCompletedRoutine()
    {
        Narrator.PlaySound("LevelComplete", 0.1f);
        // GameObject.FindObjectOfType<GameUIVR>().ShowDonePanel(true);
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
        // GameObject.FindObjectOfType<Cheat_CurrentAction>().UpdateAction();
    }

    public static void PlayAddPointSound()
    {
        // if (!PlayerPrefsManager.videoRecordingMode)
        Narrator.PlaySound("PointScored", 0.1f);
        // // todo move somewhere else
        // if (GameObject.Find("_Dev") != null)
        // {
        //     GameObject.Find("_Dev").GetComponent<Cheat_CurrentAction>().UpdateAction();
        // }
    }

    public void OnRetryButtonClick()
    {
        // GameObject.Find("Preferences").GetComponent<LoadingScreen>().LoadLevel(
        //     SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuButtonClick()
    {
        // GameObject.Find("Preferences").GetComponent<LoadingScreen>().LoadLevel("UMenuPro");
    }

    public static void WrongAction(bool headAnimation = true)
    {
        RobotManager.RobotWrongAction();
        Narrator.PlaySound("WrongAction");

        if (headAnimation)
        {
            // PlayerAnimationManager.PlayAnimation("no");
        }
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)

        // EndScoreManager endScoreManager = GameObject.FindObjectOfType<EndScoreManager>();
        // if (endScoreManager != null)
        //     endScoreManager.CalculatePercentage();
#endif
    }


    public static void CorrectAction()
    {
        GameObject.FindObjectOfType<ActionManager>().actionsCount++;
        RobotManager.RobotCorrectAction();
        ActionManager.PlayAddPointSound();

        ActionManager.BuildRequirements();
        ActionManager.UpdateRequirements(1.5f);
        // GameObject.FindObjectOfType<ActionManager>().randomQuiz.NextRandomQuiz();
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)

        // EndScoreManager endScoreManager = GameObject.FindObjectOfType<EndScoreManager>();
        // if (endScoreManager != null)
        //     endScoreManager.CalculatePercentage();
#endif
    }

    public static void BuildRequirements()
    {
        if (!practiceMode)
            return;
        GameUIVR gameUIVR = GameObject.FindObjectOfType<GameUIVR>();
        ActionManager am = GameObject.FindObjectOfType<ActionManager>();
        // gameUIVR.TalkBubble.SetActive(false);
        // gameUIVR.PersonToTalk = null;
        foreach (Action a in am.IncompletedActions)
        {
            string[] ObjectNames = new string[0];
            a.ObjectNames(out ObjectNames);

            if (a.ignorePosition)
                print("DDDDD");

            switch (a.Type)
            {
                case ActionType.PersonTalk:
                    // foreach (PersonObject po in GameObject.FindObjectsOfType<PersonObject>())
                    // {
                    //     if (po.hasTopic(a._topic))
                    //     {
                    //         a.placeRequirement = ActionManager.FindNearest(new string[] { po.name });
                    //     }
                    // }
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
            if (a.ignorePosition)
                a.placeRequirement = "";
        }

// #if (UNITY_EDITOR || DEVELOPMENT_BUILD)
//         if (GameObject.FindObjectOfType<ActionsPanel>() != null)
//             GameObject.FindObjectOfType<ActionsPanel>().UpdatePanel();
// #endif

    }



    public static List<GameObject> FindAnchers(string[] objectNames)
    {
        return null;
        List<GameObject> anchors = new List<GameObject>();
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

            foreach (ShowHideObjects sh in GameObject.FindObjectsOfType<ShowHideObjects>())
            {
                if (sh.HasNeeded(o))
                {
                    anchors.Add(sh.gameObject);
                    found = true;
                    break;
                }
            }
            if (found)
                continue;

           
            if (found)
                continue;
            if (GameObject.Find(o) != null)
            {
                anchors.Add(GameObject.Find(o));
            }
        }
        return anchors;
    }

    public static WalkToGroupVR NearestWalkToGroup(GameObject obj)
    {
        WalkToGroupVR nearest = GameObject.FindObjectOfType<WalkToGroupVR>();
        // if (obj.GetComponent<ExtraObjectOptions>() != null)
        // {
        //     if (obj.GetComponent<ExtraObjectOptions>().nearestWalkToGroup != null)
        //         return obj.GetComponent<ExtraObjectOptions>().nearestWalkToGroup;
        // }
        float nearestDist = Vector3.Distance(nearest.transform.position, obj.transform.position);
        foreach (WalkToGroupVR w in GameObject.FindObjectsOfType<WalkToGroupVR>())
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
        List<WalkToGroupVR> nearestGroups = new List<WalkToGroupVR>();
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
            WalkToGroupVR ng = nearestGroups[0];
            foreach (WalkToGroupVR w in nearestGroups)
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

    public static void TriggerAnimationSequence(string name)
    {
        currentSequence = new AnimationSequence(name);
        // decided against adding NextStep() inside AnimationSequence c-tor
        // this way we have more flexibility when does the first dialogue selector pops up
        // currentSequence.NextStep(); 
    }
    
    public static void AnimationSequence_TriggerNextStep()
    {
        if (currentSequence != null)
        {
            Debug.Log("@ __Current Sequence:" + currentSequence.Completed.ToString() + " " +
                Random.Range(0, 999).ToString());
            if (!currentSequence.Completed)
            {
                currentSequence.NextStep();
            }
            else
            {
                Debug.LogWarning("Trying to trigger next step of Completed AnimationSequence");
            }
        }
    }
}
