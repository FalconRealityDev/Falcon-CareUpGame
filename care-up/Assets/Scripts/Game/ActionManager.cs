using System.Xml;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CareUp.Actions;
using CareUp.Localize;
using System.Collections;
using NSubstitute.Core;

/// <summary>
/// GameLogic script. Everything about actions is managed by this script.
/// </summary>
public class ActionManager : MonoBehaviour
{
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
    private PlayerPrefsManager manager;
    private HandsInventory inventory;
    public RandomQuiz randomQuiz = new RandomQuiz();

    private List<Action> incompletedActions;
    private List<Action> completedActions;

    private List<Action> unlockedIncompletedActions;
    private List<string> unlockedBlocks = new List<string>();

    public string Message { get; set; } = null;
    public string MessageTitle { get; set; } = null;
    public float MessageDelay { get; set; }
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
            incompletedActions = actionList.Where(action => action.SubIndex == currentActionIndex && action.info.matched == false).ToList();
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
            completedActions = actionList.Where(action => action.info.matched == true).ToList();
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
            action.info.blockRequired.Count == 0 || unlockedBlocks.Intersect(action.info.blockRequired).Count() == action.info.blockRequired.Count()).ToList();

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
        General
    };

    static string MergeArticleWithName(string art, string descr)
    {
        if (descr[0] != '[')
            return art + " " + descr;
        
        return "[" + art + " " + descr.Replace("[", "");
    }

    // Will be refactored, someday
    public static void UpdateRequirements(float showDelay = 0f)
    {
        ActionManager actManager = GameObject.FindObjectOfType<ActionManager>();
        if (playerScript == null)
            playerScript = GameObject.FindObjectOfType<PlayerScript>();

        GeneralAction generalAction = actManager.CheckGeneralAction();
        if (generalAction != null && !playerScript.away)
        {
            actManager.NotTriggeredAction(generalAction);
        }

        if (!practiceMode)
        {
            return;
        }

        GameUI gameUI = GameObject.FindObjectOfType<GameUI>();

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

        foreach (Action a in actManager.IncompletedActions)
        {
            StepData placeData = null;
            StepData secondPlaceData = null;
            bool correctObjectsInHands = true;
            List<StepData> objectsData = new List<StepData>();

            bool dialog = false;

            if (a.info.placeRequirement == "PatientPos" || a.info.placeRequirement == "DoctorPos" || a.info.secondPlaceRequirement == "PatientPos" || a.info.secondPlaceRequirement == "DoctorPos")
                dialog = true;

            if (!string.IsNullOrEmpty(a.info.placeRequirement))
            {
                string placeName = a.info.placeRequirement;

                if (GameObject.Find(a.info.placeRequirement).GetComponent<WalkToGroup>().description != "")
                    placeName = GameObject.Find(a.info.placeRequirement).GetComponent<WalkToGroup>().description;

                bool completed = false;
                if (playerScript.currentWalkPosition != null)
                    completed = playerScript.currentWalkPosition.name == a.info.placeRequirement;


                placeData = new StepData(completed, "[-Ga naar] " + placeName + ".", i);
                if (completed)
                    anyCorrectPlace = true;

                if (a.Type == ActionType.PersonTalk && dialog)
                {
                    foreach (PersonObject po in GameObject.FindObjectsOfType<PersonObject>())
                    {
                        if (po.hasTopic(a.info.topic))
                        {
                            gameUI.PlaceTalkBubble(po.gameObject);
                        }
                    }

                    if (personClicked)
                        objectsData.Add(new StepData(false, "[-Kies wat je gaat doen.]", i));
                    else
                        objectsData.Add(new StepData(false,"[-Klik op] " + placeName + ".", i));
                }
            }

            if (!string.IsNullOrEmpty(a.info.secondPlaceRequirement))
            {
                string placeName = a.info.secondPlaceRequirement;

                if (GameObject.Find(a.info.secondPlaceRequirement).GetComponent<WalkToGroup>().description != "")
                    placeName = GameObject.Find(a.info.secondPlaceRequirement).GetComponent<WalkToGroup>().description;

                bool completed = false;
                if (playerScript.currentWalkPosition != null)
                    completed = playerScript.currentWalkPosition.name == a.info.secondPlaceRequirement;

                secondPlaceData = new StepData(completed, "[-Ga naar] " + placeName + ".", i);

                if (a.Type == ActionType.PersonTalk && dialog)
                {
                    foreach (PersonObject po in GameObject.FindObjectsOfType<PersonObject>())
                    {
                        if (po.hasTopic(a.info.topic))
                        {
                            gameUI.PlaceTalkBubble(po.gameObject);
                        }
                    }
                    if (personClicked)
                        objectsData.Add(new StepData(false, "[-Kies wat je gaat doen.]", i));
                    else
                        objectsData.Add(new StepData(false, "[-Klik op] " + placeName + ".", i));
                }
            }

            if (a.Type == ActionType.General)
            {
                objectsData.Add(new StepData(false, "[-Klik op de] " + "'" + actManager.CurrentButtonText() + 
                    "'" + "[knop.]", i));

                actManager.NotTriggeredAction();
                gameUI.buttonToBlink = GameUI.ItemControlButtonType.NoTargetRight;
                foundComplitedAction = true;
            }

            string[] actionHand = { a.info.leftHandRequirement, a.info.rightHandRequirement };
            GameObject leftR = null;
            GameObject rightR = null;
            string article = null;
            string currentLeftObject = null;
            string currentRightObject = null;
            bool objectCombinationCheck = false;

            bool iPad = a.info.leftHandRequirement == "PatientRecords" || a.info.leftHandRequirement == "PrescriptionForm" || a.info.leftHandRequirement == "PaperAndPen";

            if (iPad)
            {
                objectsData.Add(new StepData(false, "[-Klik op het informatie]", i));

                if (a.info.leftHandRequirement == "PatientRecords")
                {
                    gameUI.recordsButtonBlink = true;
                }
                else if (a.info.leftHandRequirement == "PrescriptionForm")
                {
                    gameUI.prescriptionButtonBlink = true;
                }
                else if (a.info.leftHandRequirement == "PaperAndPen")
                {
                    gameUI.paperAndPenButtonblink = true;
                }
                rightIncorrect = false;
                leftIncorrect = false;
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
                        else if(GameObject.FindObjectOfType<PrefabHolder>() != null)
                        {
                            PrefabHolder prefabHolder = GameObject.FindObjectOfType<PrefabHolder>();
                            GameObject handValuePrafeb = prefabHolder.GetPrefab(hand);
                            if (handValuePrafeb != null)
                            {
                                if (handValuePrafeb.GetComponent<InteractableObject>() != null)
                            {
                                    if (handValuePrafeb.GetComponent<InteractableObject>().description != "")
                                    {
                                        handValue = handValuePrafeb.GetComponent<InteractableObject>().description;
                                        article = handValuePrafeb.GetComponent<InteractableObject>().nameArticle;
                                        found = true;
                                        foundDescr = true;
                                    }
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
                            string localizeDescr = LocalizationManager.GetValueIfKey(inventory.leftHandObject.description);
                            if (!string.IsNullOrEmpty(localizeDescr))
                                currentLeftObject = localizeDescr.ToLower();

                            if (inventory.leftHandObject.name == hand || (a.info.objectsAllowedInHands != null &&
                                a.info.objectsAllowedInHands.Contains(inventory.leftHandObject.name)))
                            {
                                leftIncorrect = false;
                                completed = true;
                                leftR = inventory.leftHandObject.gameObject;
                            }
                        }

                        if (!inventory.RightHandEmpty())
                        {
                            string localizeDescr = LocalizationManager.GetValueIfKey(inventory.rightHandObject.description);

                            if (!string.IsNullOrEmpty(localizeDescr))
                                currentRightObject = localizeDescr.ToLower();

                            if (inventory.rightHandObject.name == hand || (a.info.objectsAllowedInHands != null &&
                                a.info.objectsAllowedInHands.Contains(inventory.rightHandObject.name)))
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
                        if (handValue != null)
                        {
                            if (handValue != "")
                            {
                                handValue = handValue.ToLower();
                            }
                        }
                        string keyWords = null;

                        if (leftR != null && rightR != null)
                        {
                            objectCombinationCheck = ((leftR.name == a.info.leftHandRequirement) && (rightR.name == a.info.rightHandRequirement)) || ((rightR.name == a.info.leftHandRequirement) && (leftR.name == a.info.rightHandRequirement));
                        }

                        if (objectCombinationCheck && a.Type == ActionType.ObjectCombine)
                        {
                            objectsData.Add(new StepData(false, "[-Klik op de Combineer]", i));
                            if (!foundComplitedAction)
                            {
                                foundComplitedAction = true;
                                gameUI.buttonToBlink = GameUI.ItemControlButtonType.Combine;
                            }
                        }

                        else if (leftR != null)
                        {
                            if (actManager.CompareUseOnInfo(inventory.leftHandObject.name, ""))
                            {
                                keyWords = actManager.CurrentButtonText(inventory.leftHandObject.name);

                                objectsData.Add(new StepData(false, "[-Klik op de] " + "'" + keyWords + "'" +
                                    "[knop.]", i));

                                if (!foundComplitedAction)
                                {
                                    foundComplitedAction = true;
                                    gameUI.buttonToBlink = GameUI.ItemControlButtonType.NoTargetLeft;
                                }
                            }
                            else if (a.Type == ActionType.ObjectDrop && a.info.leftHandRequirement == inventory.leftHandObject.name)
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
                                        string __newDescr = MergeArticleWithName(article, handValue);
                                        objectsData.Add(new StepData(false, "[-Leg] " + __newDescr + 
                                            "[neer.]", i));
                                    }
                                }
                                else
                                {
                                    gameUI.DropLeftBlink = true;
                                    string __newDescr = MergeArticleWithName(article, handValue);
                                    objectsData.Add(new StepData(false, "[-Leg] " + __newDescr + "[neer.]", i));
                                }
                            }
                            else if (a.Type == ActionType.ObjectExamine && inventory.leftHandObject.name == a.info.leftHandRequirement)
                            {
                                objectsData.Add(new StepData(false, "[-Klik op de Controleren]", i));
                                if (!foundComplitedAction)
                                {
                                    foundComplitedAction = true;
                                    gameUI.buttonToBlink = GameUI.ItemControlButtonType.ZoomLeft;
                                }
                            }

                            else if (!string.IsNullOrEmpty(actManager.CurrentDecombineButtonText(inventory.leftHandObject.name)))
                            {
                                keyWords = actManager.CurrentDecombineButtonText(inventory.leftHandObject.name);
                                objectsData.Add(new StepData(false, "[-Klik op de] " + "'" + 
                                    actManager.CurrentDecombineButtonText(inventory.leftHandObject.name) + "'" + 
                                    "[knop.]", i));
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
                            if (actManager.CompareUseOnInfo(inventory.rightHandObject.name, ""))
                            {
                                keyWords = actManager.CurrentButtonText(inventory.rightHandObject.name);

                                objectsData.Add(new StepData(false,"[-Klik op de] " + "'" + keyWords + "[knop.]", i));
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

                                        string __newDescr = MergeArticleWithName(article, handValue);
                                        
                                        objectsData.Add(new StepData(false, "[-Leg] " + __newDescr + 
                                            "[neer.]", i));
                                    }
                                }
                                else
                                {
                                    gameUI.DropRightBlink = true;
                                    string __newDescr = MergeArticleWithName(article, handValue);

                                    objectsData.Add(new StepData(false, "[-Leg] " + __newDescr + 
                                        "[neer.]", i));
                                }    
                            }
                            else if (a.Type == ActionType.ObjectExamine && inventory.rightHandObject.name == a.info.leftHandRequirement)
                            {
                                objectsData.Add(new StepData(false, "[-Klik op de Controleren]", i));
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
                            else if (!string.IsNullOrEmpty(actManager.CurrentDecombineButtonText(inventory.RightHandObject.name)))
                            {
                                keyWords = actManager.CurrentDecombineButtonText(inventory.rightHandObject.name);
                                objectsData.Add(new StepData(false, "[-Klik op de] " + "'" + 
                                    actManager.CurrentDecombineButtonText(inventory.rightHandObject.name) + "'" +
                                    "[knop.]", i));
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

                        keyWords = "[-Pak] ";

                        if (a.Type == ActionType.ObjectUse)
                        {
                            keyWords = "[-Klik op] ";
                        }
                        
                        string newDescr = MergeArticleWithName(article, handValue);
                        objectsData.Add(new StepData(completed, 
                            keyWords + newDescr + ".", i));
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
                        uncomplitedSecondPlace = a.info.secondPlaceRequirement;
                }
                else
                {
                    stepsList.Add(placeData);
                    placesReqList.Add(a.info.placeRequirement);
                }
            }

            foreach (StepData sd in objectsData)
            {
                stepsList.Add(sd);
            }

            if (objectsData.Count > 0)
                noObjectActions = false;

            gameUI.moveButtonToBlink = GameUI.MoveControlButtonType.None;

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

            if ((!anyCorrectPlace || uncomplitedSecondPlace != "") && !playerScript.away && placesReqList.Count > 0)
            {

                WalkToGroup currentWTG = playerScript.currentWalkPosition;

                foreach (string s in placesReqList)
                {
                    WalkToGroupButton b = gameUI.FindMovementButton(s, playerScript.currentWalkPosition);
                    if (b != null)
                    {
                        gameUI.moveButtonToBlink = b.moveControlButtonType;
                        break;
                    }

                    //int dir = gameUI.FindDirection(s, playerScript.currentWalkPosition, 0);
                    //if (dir == -1)
                    //{
                    //    gameUI.moveButtonToBlink = GameUI.ItemControlButtonType.MoveLeft;
                    //    break;
                    //}
                    //else if (dir == 1)
                    //{
                    //    gameUI.moveButtonToBlink = GameUI.ItemControlButtonType.MoveRight;
                    //    break;
                    //}
                }
            }

            i++;
        }

        if (gameUI.moveButtonToBlink != GameUI.MoveControlButtonType.None)
        {
            gameUI.buttonToBlink = GameUI.ItemControlButtonType.None;
            gameUI.DropRightBlink = false;
            gameUI.DropLeftBlink = false;
        }

        personClicked = false;

        GameObject.FindObjectOfType<GameUI>().UpdateHintPanel(stepsList, showDelay);

        if (leftIncorrect && !inventory.LeftHandEmpty() && !noObjectActions)
            gameUI.DropLeftBlink = true;
        if (rightIncorrect && !inventory.RightHandEmpty() && !noObjectActions)
            gameUI.DropRightBlink = true;
        GameObject.FindObjectOfType<GameUI>().UpdateHintPanel(stepsList);

        gameUI.UpdateButtonsBlink();
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
                result = currentAction.info.shortDescr;
                if (Ua && currentAction.info.commentUA != "")
                    result = currentAction.info.commentUA;
            }
            else
            {
                foreach (Action a in IncompletedActions)
                {
                    if (!Ua || a.info.commentUA == "")
                        actionsDescription.Add(a.info.shortDescr);
                    if (Ua)
                        actionsDescription.Add(a.info.commentUA);
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
                if (a.info.storedIndex < firstGeneral.info.storedIndex)
                    firstGeneral = a;
            }
            action = (GeneralAction)firstGeneral;
        }
        return action;
    }

    private Controls controls;

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
        manager = GameObject.FindObjectOfType<PlayerPrefsManager>();
        string sceneName = SceneManager.GetActiveScene().name;
        menuScene = sceneName == "Menu" || sceneName == "SceneSelection" || sceneName == "EndScore";
        particleHints = new List<GameObject>();
        Points = 0;

        controls = GameObject.Find("GameLogic").GetComponent<Controls>();
        if (controls == null) Debug.LogError("No controls found");

        TextAsset textAsset = (TextAsset)Resources.Load("Xml/Actions/" + actionListName);
        XmlDocument xmlFile = new XmlDocument();
        xmlFile.LoadXml(textAsset.text);

        XmlNodeList actions = xmlFile.FirstChild.NextSibling.ChildNodes;

        foreach (XmlNode action in actions)
        {
            if (action.Attributes["hidden"] != null)
                continue;

            ActionInfo info = new ActionInfo();

            int.TryParse(action.Attributes["index"].Value, out info.subindex);
            string type = action.Attributes["type"].Value;

            // info.shortDescr = LocalizationManager.GetValueIfKey(action.Attributes["description"].Value);
            //@
            info.shortDescr = action.Attributes["description"].Value;


            info.comment = "";
            if (action.Attributes["comment"] != null)
            {
                info.comment = action.Attributes["comment"].Value;
            }
            if (action.Attributes["objectsAllowedInHands"] != null && 
                    action.Attributes["objectsAllowedInHands"].Value != "")
            {
                info.objectsAllowedInHands = action.Attributes["objectsAllowedInHands"].Value.Split(',').ToList();
            }
            info.ignorePosition = action.Attributes["ignorePosition"] != null;

            info.secondPlaceRequirement = "";
            if (action.Attributes["secondPlace"] != null)
            {
                info.secondPlaceRequirement = action.Attributes["secondPlace"].Value;
            }

            info.placeRequirement = "";
            if (action.Attributes["place"] != null)
            {
                info.placeRequirement = action.Attributes["place"].Value;
            }

            info.commentUA = "";
            if (action.Attributes["commentUA"] != null)
            {
                info.commentUA = action.Attributes["commentUA"].Value;
            }

            info.subjectTitle = "";
            if (action.Attributes["subjectTitle"] != null)
            {
                info.subjectTitle = action.Attributes["subjectTitle"].Value;
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
                buttonText = info.shortDescr;
            }

            info.pointValue = 1;
            if (action.Attributes["points"] != null)
            {
                int.TryParse(action.Attributes["points"].Value, out info.pointValue);
            }

            info.UITimeout = 0f;
            if (action.Attributes["ui_timeout"] != null)
            {
                int intUITimeout = 0;
                int.TryParse(action.Attributes["ui_timeout"].Value, out intUITimeout);
                info.UITimeout = intUITimeout;
            }

            info.notMandatory = action.Attributes["optional"] != null;

            // try making all steps optional for test
            info.storedIndex = info.subindex;
            if (manager != null && !manager.practiceMode)
            {
                info.notMandatory = true;
                info.subindex = 0;
            }

            // quiz triggers
            info.quizTriggerTime = -1.0f;
            if (action.Attributes["quiz"] != null)
            {
                float.TryParse(action.Attributes["quiz"].Value, out info.quizTriggerTime);
                if (info.quizTriggerTime < 0.1f)
                    info.quizTriggerTime = 2.0f;
            }

            info.encounter = -1.0f;
            if (action.Attributes["encounter"] != null)
            {
                float.TryParse(action.Attributes["encounter"].Value, out info.encounter);
                if (info.encounter < 0.1f)
                    info.encounter = 2.0f;
            }

            info.messageTitle = "";
            if (action.Attributes["messageTitle"] != null)
            {
                info.messageTitle = action.Attributes["messageTitle"].Value;
            }

            info.messageContent = "";
            if (action.Attributes["messageContent"] != null)
            {
                info.messageContent = action.Attributes["messageContent"].Value;
            }

            info.messageDelay = 0f;
            if (action.Attributes["messageDelay"] != null)
            {
                int delayMS = 0;
                int.TryParse(action.Attributes["messageDelay"].Value, out delayMS);
                info.messageDelay = delayMS / 1000f; // milliseconds to seconds
            }

            if (action.Attributes["blockUnlock"] != null)
            {
                string[] sentence = action.Attributes["blockUnlock"].Value.Split(new string[] { ", ", "," }, System.StringSplitOptions.None);

                foreach (string word in sentence)
                {
                    info.blockUnlock.Add(word);
                }
            }

            if (action.Attributes["blockRequired"] != null)
            {
                string[] sentence = action.Attributes["blockRequired"].Value.Split(new string[] { ", ", "," }, System.StringSplitOptions.None);

                foreach (string word in sentence)
                {
                    info.blockRequired.Add(word);
                }
            }

            if (action.Attributes["blockLock"] != null)
            {
                string[] sentence = action.Attributes["blockLock"].Value.Split(new string[] { ", ", "," }, System.StringSplitOptions.None);

                foreach (string word in sentence)
                {
                    info.blockLock.Add(word);
                }
            }

            info.blockTitle = "";
            info.blockMessage = "";
            if (action.Attributes["blockTitle"] != null)
            {
                info.blockTitle = LocalizationManager.GetValueIfKey(action.Attributes["blockTitle"].Value);
            }

            if (action.Attributes["blockMessage"] != null)
            {
                info.blockMessage = LocalizationManager.GetValueIfKey(action.Attributes["blockMessage"].Value);
            }

            switch (type)
            {
                case "combine":
                    string left = action.Attributes["left"].Value;
                    string right = action.Attributes["right"].Value; 
                    string decombineText = "Openen";
                    if (action.Attributes["decombineText"] != null)
                    {
                        decombineText = LocalizationManager.GetValueIfKey(action.Attributes["decombineText"].Value);
                    }
                    actionList.Add(new CombineAction(left, right, decombineText, info));
                    break;
                case "use":
                    string use = action.Attributes["value"].Value;
                    actionList.Add(new UseAction(use, buttonText, info));
                    break;
                case "talk":
                    string topic = action.Attributes["topic"].Value;
                    actionList.Add(new TalkAction(topic, info));
                    break;
                case "useOn":
                    string target = action.Attributes["target"].Value;
                    actionList.Add(new UseOnAction(item, target, buttonText, info));
                    break;
                case "examine":
                    string expected = action.Attributes["expected"].Value;
                    actionList.Add(new ExamineAction(item, expected, info));
                    break;
                case "pickUp":
                    actionList.Add(new PickUpAction(item, info));
                    break;
                case "sequenceStep":
                    string stepName = action.Attributes["value"].Value;
                    actionList.Add(new SequenceStepAction(stepName, info));
                    break;
                case "drop":
                    string dropID = (action.Attributes["posID"] != null) ? action.Attributes["posID"].Value : "0";
                    actionList.Add(new ObjectDropAction(item, dropID, info));
                    break;
                case "movement":
                    string movement = action.Attributes["value"].Value;
                    actionList.Add(new MovementAction(movement, info));
                    break;
                case "general":
                    string actionValue = action.Attributes["action"].Value;
                    actionList.Add(new GeneralAction(item, actionValue, buttonText, info));

                    break;
                default:
                    Debug.LogError("No action type found: " + type);
                    break;
            }
            actionList[actionList.Count - 1].info.comment = info.comment;
            actionList[actionList.Count - 1].info.commentUA = info.commentUA;
            actionList[actionList.Count - 1].info.secondPlaceRequirement = info.secondPlaceRequirement;
            actionList[actionList.Count - 1].info.placeRequirement = info.placeRequirement;
            actionList[actionList.Count - 1].info.ignorePosition = info.ignorePosition;
            actionList[actionList.Count - 1].info.UITimeout = info.UITimeout;
            actionList[actionList.Count - 1].info.sequentialNumber = actionList.Count - 1;
            actionList[actionList.Count - 1].info.subjectTitle = LocalizationManager.GetValueIfKey(
                info.subjectTitle);
        }

        actionList.Last<Action>().info.sceneDoneTrigger = true;

        if (xmlFile.FirstChild.NextSibling.Attributes["points"] != null)
        {
            totalPoints = int.Parse(xmlFile.FirstChild.NextSibling.Attributes["points"].Value);
        }
        else
        {
            totalPoints = 0;
            foreach (Action a in actionList)
            {
                totalPoints += a.info.pointValue;
            }
        }

        foreach (Action a in actionList)
        {
            stepsList.Add(a.info.shortDescr);
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
    public void OnCombineAction(string leftHand, string rightHand, bool notWrongAction = false)
    {
        string[] info = { leftHand, rightHand };
        bool occured = Check(info, ActionType.ObjectCombine, notWrongAction);
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
        GameUI gameUI = FindObjectOfType<GameUI>();
        string buttonText = "";
        if (generalAction != null)
            buttonText = generalAction.ButtonText;
        gameUI.ShowNoTargetButton(buttonText);

        if (practiceMode)
            gameUI.buttonToBlink = GameUI.ItemControlButtonType.NoTargetRight;
    }

    public bool IsActionDoneWrong(Action action)
    {
        int actionIndex = actionList.IndexOf(action);
        if (wrongStepIndexes.Contains(actionIndex))
            return true;
        return false;
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
                    action.info.matched = true;
                    if (action.info.UITimeout > 0f)
                    {
                        GameObject.FindObjectOfType<GameUI>().UITimeout(action.info.UITimeout);
                    }
                    int index = actionList.IndexOf(action);

                    if (action.info.blockUnlock.Count() > 0)
                    {
                        foreach (string item in action.info.blockUnlock)
                        {
                            if (!unlockedBlocks.Contains(item))
                                unlockedBlocks.Add(item);
                        }
                    }

                    if (action.info.blockLock.Count > 0)
                    {
                        foreach (string item in action.info.blockLock)
                        {
                            unlockedBlocks.Remove(item);
                        }
                    }

                    if (action.info.quizTriggerTime >= 0.0f)
                    {
                        PlayerScript.TriggerQuizQuestion(action.info.quizTriggerTime);
                    }

                    //if (action.SubIndex == 1)
                    //{
                    //    ShowRandomEvent = true;
                    //}
                    BuildRandomEventList(action.info.sequentialNumber);
                    //--------------------------------------------------------------

                    if (action.info.encounter >= 0.0f)
                    {
                        QuizTab.encounterDelay = action.info.encounter;
                    }

                    if (action.info.messageContent != "" || action.info.messageTitle != "")
                    {
                        if (practiceMode)
                        {
                            ShowTheory = true;
                            Message = action.info.messageContent;
                            MessageTitle = action.info.messageTitle;
                            MessageDelay = action.info.messageDelay;
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
            GameObject.FindObjectOfType<GameUI>().ButtonBlink(false);
        }
        else if (manager != null && !manager.practiceMode) // test mode error, check for blocks
        {
            // make a flag that will become true if there is a step that is blocked
            // and could actually be performed if there would be no block
            bool foundBlockedStep = false;
            Action foundAction = null;

            if (IncompletedActions.Count != 0)
            {
                foreach (Action action in IncompletedActions)
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
                if (foundAction.info.blockTitle != "" && foundAction.info.blockMessage != "")
                {
                    title = foundAction.info.blockTitle;
                    message = foundAction.info.blockMessage;
                }
                else
                {
                    title = "Stap is niet mogelijk";
                    message = "Je kunt deze stap nog niet doen, het kan zijn dat je een stap vergeten bent.";
                }

                GameObject.FindObjectOfType<GameUI>().ShowBlockMessage(title, message);
            }
        }

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

            bool practice = (manager != null) ? manager.practiceMode : true;

            if (practice)
            {
                if (IncompletedActions.Count > 0)
                {
                    RobotUIMessageTab messageCenter = GameObject.FindObjectOfType<RobotUIMessageTab>();

                    if (type != ActionType.SequenceStep)
                        GameObject.FindObjectOfType<GameUI>().ShowBlockMessage("[Verkeerde handeling]", IncompletedActions[0].info.shortDescr);
                }
            }

            if (!notWtongAction)
                ActionManager.WrongAction(type != ActionType.SequenceStep);

            penalized = true;
        }
        else
        {
            currentPointAward = currentAction.info.pointValue;

            currentAction = IncompletedActions.Count > 0 ? IncompletedActions.First() : null;

            if ((manager == null || manager.practiceMode) && type != ActionType.Movement)
            {
                //now we have not mandatory actions, let's skip them and add to mistakes
                List<Action> skippableActions = IncompletedActions.Where(action => action.info.notMandatory == true).ToList();

                if (IncompletedActions.Count == skippableActions.Count) // all of them are skippable
                {
                    wrongStepIndexes.Add(actionList.IndexOf(currentAction));
                    currentActionIndex += 1;

                    // get next actions with new index
                    IncompletedActions = actionList.Where(action =>
                        action.SubIndex == currentActionIndex &&
                        action.info.matched == false).ToList();

                    currentAction = IncompletedActions.Count > 0 ? IncompletedActions.First() : null;
                }
            }
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

    void BuildRandomEventList(int actionSequentialNumber)
    {
//for testing random events without starting from main menu, comment those two lines
        if (manager == null)
            return;

        if (manager != null)
        { 
            if (manager.currentDifficultyLevel != 3)
                return;
        }
        for(int i = 0; i < randomEventBookmaks.Count; i++)
        {
            if (randomEventBookmaks[i].GetActionIndex() == actionSequentialNumber && !randomEventBookmaks[i].IsComplited())
            {
                currentRandomEventIndices.Add(i);
            }
        }
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
            if (actionList.Find(action => action.info.matched == true && action.info.sceneDoneTrigger == true) != null)
            {
                SceneCompletedRoutine();
                return true;
            }
            else return false;
        }
        else
        {
            if (actionList.Find(action => action.info.matched == false && action.info.notMandatory == false) == null)
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
                actionList[i].info.matched = items[i];
            }
        }
    }

    /// <summary>
    /// Rolls animation sequence back.
    /// </summary>
    public void RollSequenceBack()
    {
        Action lastAction = actionList.Last(x => x.info.matched == true);
        while (lastAction.Type == ActionType.SequenceStep)
        {
            lastAction.info.matched = false;
            lastAction = actionList.Last(x => x.info.matched == true);
        }
        lastAction.info.matched = false;
        currentActionIndex = lastAction.SubIndex;
        currentAction = lastAction;
        GameObject.FindObjectOfType<Cheat_CurrentAction>().UpdateAction();
    }

    public static void PlayAddPointSound()
    {
        if (!PlayerPrefsManager.videoRecordingMode)
            Narrator.PlaySound("PointScored", 0.1f);
        // todo move somewhere else
        if (GameObject.Find("_Dev") != null)
        {
            GameObject.Find("_Dev").GetComponent<Cheat_CurrentAction>().UpdateAction();
        }
    }

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

        if (PlayerPrefsManager.GetDevMode())
        {
            EndScoreManager endScoreManager = GameObject.FindObjectOfType<EndScoreManager>();
            if (endScoreManager != null)
                endScoreManager.CalculatePercentage();
        }
    }


    public static void CorrectAction()
    {
        GameObject.FindObjectOfType<ActionManager>().actionsCount++;
        RobotManager.RobotCorrectAction();
        ActionManager.PlayAddPointSound();

        ActionManager.BuildRequirements();
        ActionManager.UpdateRequirements(1.5f);
        GameObject.FindObjectOfType<ActionManager>().randomQuiz.NextRandomQuiz();
        if (PlayerPrefsManager.GetDevMode())
        {
            EndScoreManager endScoreManager = GameObject.FindObjectOfType<EndScoreManager>();
            if (endScoreManager != null)
                endScoreManager.CalculatePercentage();
        }
    }

    public static void BuildRequirements()
    {
        if (!practiceMode)
            return;
        GameUI gameUI = GameObject.FindObjectOfType<GameUI>();
        ActionManager am = GameObject.FindObjectOfType<ActionManager>();
        gameUI.TalkBubble.SetActive(false);
        gameUI.PersonToTalk = null;
        foreach (Action a in am.IncompletedActions)
        {
            string[] ObjectNames = new string[0];
            a.ObjectNames(out ObjectNames);

            if (a.info.ignorePosition)
                print("DDDDD");

            switch (a.Type)
            {
                case ActionType.PersonTalk:
                    foreach (PersonObject po in GameObject.FindObjectsOfType<PersonObject>())
                    {
                        if (po.hasTopic(a.info.topic))
                        {
                            a.info.placeRequirement = ActionManager.FindNearest(new string[] { po.name });
                        }
                    }
                    break;
                case ActionType.ObjectCombine:
                    a.info.leftHandRequirement = ObjectNames[0];
                    a.info.rightHandRequirement = ObjectNames[1];
                    if (a.info.placeRequirement == "")
                        a.info.placeRequirement = ActionManager.FindNearest(new string[] { ObjectNames[0], ObjectNames[1] });
                    break;
                case ActionType.ObjectUseOn:
                    a.info.leftHandRequirement = ObjectNames[0];
                    if (a.info.placeRequirement == "")
                        a.info.placeRequirement = ActionManager.FindNearest(new string[] { ObjectNames[0] });
                    break;
                case ActionType.ObjectExamine:
                    a.info.leftHandRequirement = ObjectNames[0];
                    if (a.info.placeRequirement == "")
                        a.info.placeRequirement = ActionManager.FindNearest(new string[] { ObjectNames[0] });
                    break;
                case ActionType.PickUp:
                    a.info.leftHandRequirement = ObjectNames[0];
                    if (a.info.placeRequirement == "")
                        a.info.placeRequirement = ActionManager.FindNearest(new string[] { ObjectNames[0] });
                    break;
                case ActionType.ObjectDrop:
                    a.info.leftHandRequirement = ObjectNames[0];
                    if (a.info.placeRequirement == "")
                        a.info.placeRequirement = ActionManager.FindNearest(new string[] { ObjectNames[0] });
                    break;
                case ActionType.ObjectUse:
                    a.info.leftHandRequirement = ObjectNames[0];
                    if (a.info.placeRequirement == "")
                        a.info.placeRequirement = ActionManager.FindNearest(new string[] { ObjectNames[0] });
                    break;
            }
            if (a.info.ignorePosition)
                a.info.placeRequirement = "";
        }

        if (PlayerPrefsManager.GetDevMode() && GameObject.FindObjectOfType<ActionsPanel>() != null)
        {
            GameObject.FindObjectOfType<ActionsPanel>().UpdatePanel();
        }
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
        if (obj.GetComponent<ExtraObjectOptions>() != null)
        {
            if (obj.GetComponent<ExtraObjectOptions>().nearestWalkToGroup != null)
                return obj.GetComponent<ExtraObjectOptions>().nearestWalkToGroup;
        }
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
}
