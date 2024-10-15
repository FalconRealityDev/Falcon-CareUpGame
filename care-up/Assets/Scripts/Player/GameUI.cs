using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CareUp.Actions;
using System.Linq;
using UnityEngine.SceneManagement;
using CareUp.Localize;

public class GameUI : MonoBehaviour
{

    public GameObject subjectWindow;
    string autoPlayMoveWTGName = "";
    public AnimatedFingerHint animatedFinger;
    GameObject Player;
    public Animator Blink;
    public Animator IPadBlink;
    public bool BlinkState = false;
    public bool testValue;
    GameObject donePanel;
    GameObject closeButton;
    GameObject closeDialog;
    GameObject donePanelYesNo;
    GameObject WalkToGroupPanel;
    float autoExitTime = 900f;
    AutoPlayer autoPlayer;
    public GameObject activeTalkBobblePoint = null;

    public WalkToGroupButton LeftSideButton;
    public WalkToGroupButton RightSideButton;
    public Dictionary<string, WalkToGroupButton> WTGButtons;
    private Tutorial_Combining tutorialCombine;
    private Tutorial_UseOn tutorialUseOn;
    private HandsInventory handsInventory;
    private EndScoreManager scoreManager;
    private ActionManager actionManager;
    private Animator controller;
    private float startTimeOut = 0.5f;
    private bool timeOutEnded = false;
    PlayerPrefsManager prefs;
    public string debugSS = "";
    ObjectsIDsController objectsIDsController;
    bool practiceMode = true;
    public QuizTab quiz_tab;
    public RandomEventTab randomEventTab;

    public bool DropLeftBlink = false;
    public bool DropRightBlink = false;
    //bool generalButtonActive = false;
    public int stopAutoPlayOnStep = 0;
    GameObject AutoActionObject;

    public bool LevelEnded = false;
    public bool showNoTarget_right = false;

    GameObject MovementSideButtons;
    public List<string> reqPlaces = new List<string>();
    List<ActionManager.StepData> Current_SubTasks;
    float current_UpdateHintDelay = 0f;
    bool toDelayUpdateHint = false;
    GameObject gameLogic;
    public GameObject TalkBubble;
    GameObject DetailedHintPanel;
    public GameObject autoplayPanel;
    protected Toggle autoplayToggle;
    protected GameObject autoplayFrame;
    protected Text autoExitLabeb;

    public List<string> activeHighlighted = new List<string>();

    public GameObject IPad;
    public GameObject ItemControlPanel;
    public GameObject combineButton;
    public GameObject decombineButton;
    public GameObject decombineButton_right;
    public GameUI.ItemControlButtonType buttonToBlink;
    public GameUI.MoveControlButtonType moveButtonToBlink;
    public bool prescriptionButtonBlink;
    public bool recordsButtonBlink;
    public bool paperAndPenButtonblink;
    public GameObject patientInfo;

    public GameObject noTargetButton;
    public GameObject noTargetButton_right;

    private CameraMode cameraMode;

    public GameObject zoomButtonLeft;
    public GameObject zoomButtonRight;
    public GameObject SubStepsPanel;
    Text subStepsText;

    public bool currentAnimLock = false;
    public GameObject DropLeftButton;
    public GameObject DropRightButton;
    public GameObject BlockPopUp;
    public Text BlockTitle;
    public Text BlockMessage;

    float cooldownTime = 0;
    float lastCooldownTime = 0;
    int currentActionsCount = 0;

    public TheoryTab theoryTab;

    private Text pointsTextIpad;
    private Text percentageTextIpad;

    private bool startTimer = false;
    private float targetTime = 0.7f;
    public PersonObject PersonToTalk = null;
    bool currentItemControlPanelState = false;
    int currentLeft;
    int currentRight;
    string useOnNTtext;
    PlayerScript ps;
    bool ICPCurrentState = false;
    public bool allowObjectControlUI = true;
    public static bool encounterStarted = false;
    InputField AutoplayStopInput;
    public GameObject PointToObject = null;

    private Transform itemDescriptionGroup;

    public void SetItemDescriptionGroup(Transform itemDescriptionValue)
    {
        itemDescriptionGroup = itemDescriptionValue;
    }
    
    public void ChangeAutoStopValue(int value)
    {
        stopAutoPlayOnStep += value;
        AutoplayStopInput.text = stopAutoPlayOnStep.ToString();
    }
    public void SetRandomEventTab(RandomEventTab _randomEventTab)
    {
        randomEventTab = _randomEventTab;
    }
    public void AutoStopValueChanged()
    {
        stopAutoPlayOnStep = int.Parse(AutoplayStopInput.text);
    }

    public void TriggerRandomEvent()
    {
        PlayerScript.TriggerRandomEvent(0.1f);
        //randomEventTab.NextRandomEvent();
    }

    public void UpdateMovementButtons(WalkToGroup currentWTG)
    {
        foreach (WalkToGroupButton b in WalkToGroupPanel.GetComponentsInChildren<WalkToGroupButton>())
        {
            if (currentWTG != null)
                b.GetComponent<Button>().interactable = b.GetLinkedWalkToGroup() != currentWTG;
            else
                b.GetComponent<Button>().interactable = true;
        }
    }

    public bool AllowAutoPlay(bool forActions = true)
    {
        bool result = PlayerPrefsManager.simulatePlayerActions;
        if (result && forActions)
            result = !ps.away && !DropLeftBlink && !DropRightBlink && moveButtonToBlink == MoveControlButtonType.None;
        result = result && (stopAutoPlayOnStep <= 0 || stopAutoPlayOnStep > (actionManager.currentActionIndex));
        return result;
    }

    public enum ItemControlButtonType
    {
        None,
        Combine,
        DecombineLeft,
        DecombineRight,
        NoTargetLeft,
        NoTargetRight,
        ZoomLeft,
        ZoomRight,
        DropLeft,
        DropRight,
        MoveLeft,
        MoveRight,
        Records,
        Prescription,
        Ipad,
        General,
        PaperAndPen,
        GeneralBack,
        RecordsBack,
        PrescriptionBack,
        MessageTabBack,
        Close,
        TalkBubble,
    }

    public enum MoveControlButtonType
    {
        None,
        MoveA,
        MoveB,
        MoveC,
        MoveD
    }

    public void TestOutput()
    {
        Debug.LogWarning("removed due to new asset bundle manager");
        //AssetBundleManager.PrintLoadedBundles();
    }

    public void UseOn()
    {
        if (!(handsInventory.LeftHandEmpty() && handsInventory.RightHandEmpty()))
        {
            handsInventory.OnCombineAction();
        }
    }

    public void TalkButtonPressed()
    {
        if (PersonToTalk == null)
            return;
        PersonToTalk.CreateSelectionDialogue();
        activeTalkBobblePoint = null;
    }

    public void ShowBlockMessage(string Title, string Message)
    {
        if (objectsIDsController != null)
        {
            if (objectsIDsController.cheat)
                return;
        }
        if (PlayerAnimationManager.IsLongAnimation())
            return;
        if (GameObject.Find("Sequences") != null)
        {
            return;
        }
        if (Message == "")
            return;
        //@
        BlockTitle.text = LocalizationManager.GetValueIfKey(Title);
        //@
        BlockMessage.text = LocalizationManager.GetValueIfKey(Message);

        InGameLocalEditTool inGameLocalEditTool = PlayerPrefsManager.GetDevMode() ? GameObject.FindObjectOfType<InGameLocalEditTool>() : null;  
        if (inGameLocalEditTool != null)
        {
            //!
            inGameLocalEditTool.AddUILocalizationComponentToGO(BlockTitle.gameObject, Title);
            //!
            inGameLocalEditTool.AddUILocalizationComponentToGO(BlockMessage.gameObject, Message);
        }
        BlockPopUp.GetComponent<Animator>().SetTrigger("pop");
    }

    public void HideBlockMessage()
    {
        BlockPopUp.GetComponent<Animator>().SetTrigger("fold");
        if (prefs.practiceMode)
            DetailedHintPanel.SetActive(true);
    }

    public void UseOnNoTarget(bool leftHand = true)
    {
        GeneralAction generalAction = null;
        if (!leftHand)
        {
            if (!showNoTarget_right)
            {
                bool skipBlocks = false;
                if (GameObject.FindObjectOfType<PlayerPrefsManager>() != null)
                    skipBlocks = !GameObject.FindObjectOfType<PlayerPrefsManager>().practiceMode;
                generalAction = actionManager.CheckGeneralAction();
            }
        }

        if (generalAction == null)
        {
            if (tutorialUseOn != null && !tutorialUseOn.ventAllowed)
            {
                return;
            }

            if (leftHand && !handsInventory.LeftHandEmpty())
            {
                if (actionManager.CompareUseOnInfo(handsInventory.leftHandObject.name, ""))
                {
                    if (handsInventory.LeftHandObject.GetComponent<PickableObject>().Use(true, true))
                    {
                        UpdateWalkToGroupUI(false);
                    }

                    if (tutorialUseOn != null)
                    {
                        handsInventory.LeftHandObject.GetComponent<PickableObject>().tutorial_usedOn = true;
                    }
                    return;
                }
                else
                    actionManager.OnUseOnAction(handsInventory.leftHandObject.name, "");

            }
            if (!leftHand && !handsInventory.RightHandEmpty())
            {
                if (actionManager.CompareUseOnInfo(handsInventory.rightHandObject.name, ""))
                {
                    if (handsInventory.RightHandObject.GetComponent<PickableObject>().Use(false, true))
                    {
                        UpdateWalkToGroupUI(false);
                    }

                    if (tutorialUseOn != null)
                    {
                        handsInventory.RightHandObject.GetComponent<PickableObject>().tutorial_usedOn = true;
                    }
                }
                else
                    actionManager.OnUseOnAction(handsInventory.rightHandObject.name, "");
            }
        }
        else
            GeneralAction(generalAction);
    }

    public void OpenRobotUI()
    {
        if (cameraMode.camViewObject)
            return;

        if (PlayerAnimationManager.IsLongAnimation())
            return;

        if (Camera.main == null)
            return;

        RobotManager.UIElementsState[0] = false;

        Player.GetComponent<PlayerScript>().OpenRobotUI();


    }

    public void ToggleUsingOnMode()
    {
        Player.GetComponent<PlayerScript>().ToggleUsingOnMode(false);
    }

    public void CloseButtonPressed(bool value)
    {
        closeDialog.SetActive(value);
        // closeButton.SetActive(!value);
        if (value)
        {
            ps.robotUIopened = true;
        }
        else
        {
            ps.robotUIopened = false;
        }
    }

    public void CloseGameLevelSelection()
    {
        closeDialog.SetActive(false);
        GameObject.FindObjectOfType<MainMenuAutomationData>().toAutomate = true;
        bl_SceneLoaderUtils.GetLoader.LoadLevel("MainMenu");
    }
    public void CloseGame()
    {
        closeDialog.SetActive(false);
        GameObject.FindObjectOfType<MainMenuAutomationData>().toAutomate = false;
        bl_SceneLoaderUtils.GetLoader.LoadLevel("MainMenu");
    }

    public void ButtonBlink(bool ToBlink)
    {
        if (prefs != null)
            if (!prefs.practiceMode)
                return;
        if (BlinkState == ToBlink)
            return;
        BlinkState = ToBlink;
        if (transform.Find("Extra").gameObject.activeSelf)
        {
            Blink.SetTrigger("BlinkOnes");
            BlinkState = false;
        }
        else if (ToBlink)
        {
            if (Blink != null)
                Blink.SetTrigger("BlinkStart");
            RobotManager.UIElementsState[1] = true;
        }
        else
        {
            if (Blink != null)
                Blink.SetTrigger("BlinkStop");
            RobotManager.UIElementsState[1] = false;
        }
    }

    public void Examine(bool leftHand = true)
    {
        if (leftHand && handsInventory.LeftHandEmpty())
            return;
        if (!leftHand && handsInventory.RightHandEmpty())
            return;
        GameObject initedObject = null;
        if (leftHand)
            initedObject = handsInventory.leftHandObject.gameObject;
        else
            initedObject = handsInventory.rightHandObject.gameObject;

        cameraMode.selectedObject = initedObject.GetComponent<ExaminableObject>();
        if (cameraMode.selectedObject != null) // if there is a component
        {
            if (tutorialUseOn != null)
            {
                tutorialUseOn.examined = true;
            }
            cameraMode.selectedObject.OnExamine();
            //controls.ResetObject();
        }
    }

    public void GeneralAction(GeneralAction generalAction)
    {
        if (generalAction != null)
        {
            if (actionManager.CheckGeneralAction() == null)
            {
                ActionManager.WrongAction(true);
                actionManager.UpdatePoints(-1);
                return;
            }

            GameObject item = GameObject.Find(generalAction.Item);

            PlayerAnimationManager playerAnimationManager = FindObjectOfType<PlayerAnimationManager>();
            Animator animator;

            if (playerAnimationManager != null)
            {
                animator = playerAnimationManager.GetComponent<Animator>();

                if (animator)
                {
                    animator.SetTrigger(generalAction.Action);
                    animator.SetTrigger("S " + generalAction.Action);
                    actionManager.OnGeneralAction();
                }
            }
            else if (item != null)
            {
                animator = item.GetComponent<Animator>();

                if (animator)
                {
                    animator.SetTrigger(generalAction.Action);
                    actionManager.OnGeneralAction();
                }
            }
        }
    }

    public void UpdateWalkToGroupButtons()
    {
        if (WTGButtons == null)
            return;
        if (WTGButtons.Count == 0)
            return;

        foreach (string k in WTGButtons.Keys)
        {
            WTGButtons[k].gameObject.SetActive(false);
        }

        foreach (WalkToGroup w in GameObject.FindObjectsOfType<WalkToGroup>())
        {
            w.FindNeighbors();
        }
        // Find left WalkToGroup
        WalkToGroup leftWTG = null;

        foreach (WalkToGroup w in GameObject.FindObjectsOfType<WalkToGroup>())
        {
            if (w.LeftWalkToGroup == null)
            {
                leftWTG = w;
                break;
            }
        }
        List<WalkToGroup> WTGInOrder = new List<WalkToGroup>();
        WTGInOrder.Add(leftWTG);
        int n = 0;
        while(leftWTG != null && n < 10)
        {
            if (leftWTG.RightWalkToGroup != null)
            {
                WTGInOrder.Add(leftWTG.RightWalkToGroup);
            }
            leftWTG = leftWTG.RightWalkToGroup;
            n++;
        }

        string[] buttonNames = { "A", "B", "C", "D" };
        for(int i = 0; i < WTGInOrder.Count; i++)
        {
            if (i > buttonNames.Length)
                break;
            WTGButtons[buttonNames[i]].setWalkToGroup(WTGInOrder[i]);
            WTGButtons[buttonNames[i]].gameObject.SetActive(true);
        }
        UpdateMovementButtons(null);

    }

    //void ShowWalkToGroupPanel()
    //{
    //    WalkToGroupPanel.SetActive(ps.away);
    //}

    public void HideTheoryTab()
    {
        GameObject.Find("theoryPanel/panel/quizElements/Continue").gameObject.GetComponent<Button>().onClick.AddListener(
            () => GameObject.FindObjectOfType<PlayerScript>().CloseRobotUI());
        theoryTab.Show(false);
    }

    public void ShowSubjectWindow(bool toShow = true)
    {
        subjectWindow.SetActive(toShow);
        if (toShow)
        {
            subjectWindow.transform.parent.GetComponent<ActionSubjectWindow>().RefrashWindow();
        }
    }

    // Use this for initialization
    void Start()
    {
        DetailedHintPanel = GameObject.Find("DetailedHintPanel");

        if (PlayerPrefsManager.videoRecordingMode)
        {
            autoplayPanel.GetComponent<CanvasGroup>().alpha = 0f;
            if (GameObject.Find("ActionsPanel") != null)
                GameObject.Find("ActionsPanel").GetComponent<CanvasGroup>().alpha = 0f;
            if (GameObject.Find("AssetDebugPanel") != null)
                GameObject.Find("AssetDebugPanel").GetComponent<CanvasGroup>().alpha = 0f;
            if (!PlayerPrefsManager.videoRecordingWithTextMode)
                DetailedHintPanel.GetComponent<CanvasGroup>().alpha = 0f;
        }

        gameLogic = GameObject.Find("GameLogic");
        autoPlayer = GameObject.FindObjectOfType<AutoPlayer>();
        theoryTab = GameObject.FindObjectOfType<TheoryTab>();
        autoplayToggle = autoplayPanel.transform.Find("bottomPanel/Toggle").GetComponent<Toggle>();
        autoplayFrame = autoplayPanel.transform.Find("redRect").gameObject;
        autoExitLabeb = autoplayPanel.transform.Find("bottomPanel/exitLabel").GetComponent<Text>();
        autoplayToggle.isOn = PlayerPrefsManager.simulatePlayerActions;
        autoplayFrame.SetActive(PlayerPrefsManager.simulatePlayerActions);
        AutoplayStopInput = autoplayPanel.transform.Find("bottomPanel/InputField").GetComponent<InputField>();
        ChangeAutoStopValue(0);
        animatedFinger = GameObject.FindObjectOfType<AnimatedFingerHint>();
        objectsIDsController = GameObject.FindObjectOfType<ObjectsIDsController>();
        MovementSideButtons = GameObject.Find("MovementSideButtons");

        pointsTextIpad = GameObject.Find("TopBarUI").transform.Find("GeneralDynamicCanvas")
            .Find("Points").Find("PointsText").GetComponent<Text>();

        percentageTextIpad = GameObject.Find("TopBarUI").transform.Find("GeneralDynamicCanvas")
            .Find("Percentage").Find("PointsText").GetComponent<Text>();

        prefs = GameObject.FindObjectOfType<PlayerPrefsManager>();
        if (prefs != null)
            practiceMode = prefs.practiceMode;

        useOnNTtext = noTargetButton.transform.GetChild(0).GetComponent<Text>().text;
        ps = GameObject.FindObjectOfType<PlayerScript>();
        controller = GameObject.FindObjectOfType<PlayerAnimationManager>().GetComponent<Animator>();
        cameraMode = GameObject.Find("GameLogic").GetComponent<CameraMode>();
        handsInventory = GameObject.FindObjectOfType<HandsInventory>();
        tutorialCombine = GameObject.FindObjectOfType<Tutorial_Combining>();
        tutorialUseOn = GameObject.FindObjectOfType<Tutorial_UseOn>();
        actionManager = GameObject.FindObjectOfType<ActionManager>();
        scoreManager = GameObject.FindObjectOfType<EndScoreManager>();

        ActionManager.BuildRequirements();
        zoomButtonLeft.SetActive(false);
        zoomButtonRight.SetActive(false);
        combineButton.SetActive(false);
        decombineButton.SetActive(false);
        decombineButton_right.SetActive(false);
        noTargetButton.SetActive(false);
        ItemControlPanel.SetActive(false);
        noTargetButton_right.SetActive(false);
        DropRightButton.SetActive(false);
        DropLeftButton.SetActive(false);

        HideTheoryTab();

        IPad.GetComponent<Animator>().enabled = false;

        ActionManager.practiceMode = true;
        if (prefs != null)
        {
            ActionManager.practiceMode = prefs.practiceMode;
        }
        if (!PlayerPrefsManager.GetDevMode())
        {
            if(GameObject.Find("ActionsPanel") != null)
                GameObject.Find("ActionsPanel").SetActive(false);
            if(GameObject.Find("AssetDebugPanel") != null)
                GameObject.Find("AssetDebugPanel").SetActive(false);
            autoplayPanel.SetActive(false);
        }

        WalkToGroupPanel = GameObject.Find("MovementButtons");
        Player = GameObject.Find("Player");
        closeButton = transform.Find("CloseButtonPanel/CloseBtn").gameObject;
        closeDialog = transform.Find("CloseDialog").gameObject;
        closeDialog.SetActive(false);
        donePanel = transform.Find("DonePanel").gameObject;
        donePanel.SetActive(false);

        donePanelYesNo = transform.Find("DonePanelYesNo").gameObject;
        donePanelYesNo.SetActive(false);

        if (WalkToGroupPanel != null)
        {
            WTGButtons = new Dictionary<string, WalkToGroupButton>();

            foreach (WalkToGroupButton b in GameObject.FindObjectsOfType<WalkToGroupButton>())
            {
                if (b.name == "MoveLeft")
                {
                    LeftSideButton = b;
                }
                else if (b.name == "MoveRight")
                {
                    RightSideButton = b;
                }
                else
                {
                    WTGButtons.Add(b.name, b);
                }
            }
        }
        UpdateWalkToGroupButtons();
        UpdateWalkToGroupUI(true);

        foreach (InteractableObject o in Resources.FindObjectsOfTypeAll<InteractableObject>())
        {
            o.assetSource = InteractableObject.AssetSource.Included;
        }
        WalkToGroupPanel.SetActive(false);
        //Invoke("ShowWalkToGroupPanel", 0.5f);
    }

    public HighlightObject AddHighlight(Transform target, string prefix, HighlightObject.type hl_type = HighlightObject.type.NoChange, float startDelay = 0, float LifeTime = float.PositiveInfinity)
    {
        string hl_name = prefix + "_" + target.name;
        if (GameObject.Find(hl_name) != null)
            return null;

        // assets/resources/necessaryprefabs

        GameObject hl_obj = Instantiate(Resources.Load<GameObject>("NecessaryPrefabs/HighlightObject"), target.position, new Quaternion()) as GameObject;

        HighlightObject hl = hl_obj.GetComponent<HighlightObject>();
        hl.name = hl_name;
        hl.setTarget(target);
        if (hl_type != HighlightObject.type.NoChange)
            hl.setType(hl_type);
        hl.setTimer(LifeTime);
        if (startDelay > 0)
            hl.setStartDelay(startDelay);
        return hl_obj.GetComponent<HighlightObject>();
    }

    public void RemoveHighlight(string prefix, string _name)
    {
        string hl_name = prefix + "_" + _name;

        if (GameObject.Find(hl_name) != null)
        {
            if (GameObject.Find(hl_name).GetComponent<HighlightObject>())
                GameObject.Find(hl_name).GetComponent<HighlightObject>().Destroy();
        }
    }

    void AutoPlay()
    {
        if (!currentAnimLock)
        {
            Invoke("AutoPlay", 1f);
            return;
        }

        if (AutoActionObject != null)
        {
            if (AutoActionObject != null)
            {
                ps.AutoClick(AutoActionObject);
                AutoActionObject = null;
                activeTalkBobblePoint = null;
            }
        }
    }
   
    public void UpdateHelpHighlight()
    {
        PointToObject = null;
        bool practiceMode = true;

        if (prefs != null)
            practiceMode = prefs.practiceMode;
        if (!practiceMode)
            return;

        List<string> newHLObjects = new List<string>();

        string prefix = "helpHL";

        if (!handsInventory.LeftHandEmpty())
            RemoveHighlight(prefix, handsInventory.leftHandObject.name);

        if (!handsInventory.RightHandEmpty())
            RemoveHighlight(prefix, handsInventory.rightHandObject.name);
        bool autoObjectSelected = false;
        bool AlloweAutoAction = AllowAutoPlay();
        
        
        string AutoMoveTo = "";
        foreach (Action a in actionManager.IncompletedActions)
        {
            string[] ObjectNames = new string[0];
            a.ObjectNames(out ObjectNames);

            if (PlayerPrefsManager.simulatePlayerActions && ps.away)
            {
                if (a.info.placeRequirement != "")
                {
                    if (AutoMoveTo == "")
                        AutoMoveTo = a.info.placeRequirement;
                    else if (a.Type == ActionManager.ActionType.PersonTalk)
                        AutoMoveTo = a.info.placeRequirement;
                    
                }
            }
            if (AutoMoveTo != "")
                AlloweAutoAction = false;

            // if (a.Type == ActionManager.ActionType.PersonTalk)
            //     AlloweAutoAction = AllowAutoPlay(false);
            if (AlloweAutoAction && !autoObjectSelected)
            {
                if (ObjectNames.Length == 1)
                {
                    if (a.Type == ActionManager.ActionType.PersonTalk)
                    {
                        foreach (PersonObject po in GameObject.FindObjectsOfType<PersonObject>())
                        {
                            if (po.hasTopic(a.info.topic))
                            {
                                AutoActionObject = po.gameObject.GetComponentInChildren<PersonObjectPart>().gameObject;
                                autoObjectSelected = true;
                                Invoke("AutoPlay", 4f);
                            }
                        }
                    }
                    else if (GameObject.Find(ObjectNames[0]) != null)
                    {
                        AutoActionObject = GameObject.Find(ObjectNames[0]);
                        autoObjectSelected = true;
                        Invoke("AutoPlay", 1f);
                    }
                        
                }
                else
                {
                    if (ObjectNames[1] == "" && GameObject.Find(ObjectNames[0]) != null)
                    {
                        AutoActionObject = GameObject.Find(ObjectNames[0]);
                        autoObjectSelected = true;
                        Invoke("AutoPlay", 1f);
                    }

                }
            }
            foreach (string objectToUse in ObjectNames)
            {
                if (GameObject.Find(objectToUse) != null)
                {
                    if (AlloweAutoAction && !autoObjectSelected && a.Type != ActionManager.ActionType.PersonTalk)
                    {
                        if (!handsInventory.IsInHand(GameObject.Find(objectToUse)))
                        {
                            AutoActionObject = GameObject.Find(objectToUse);
                            autoObjectSelected = true;
                            Invoke("AutoPlay", 4f);
                        }
                    }
                    HighlightObject.type hl_type = HighlightObject.type.NoChange;
                    if (GameObject.Find(objectToUse).GetComponent<WorkField>() != null)
                    {
                        hl_type = HighlightObject.type.Hand;
                    }
                    if (handsInventory.IsInHand(GameObject.Find(objectToUse)))
                        continue;
                    HighlightObject h = AddHighlight(GameObject.Find(objectToUse).transform, prefix, hl_type, 2f + Random.Range(0f, 0.5f));
                    if (h != null)
                    {
                        h.setGold(true);
                    }
                    newHLObjects.Add(objectToUse);
                    if (PointToObject == null)
                        PointToObject = GameObject.Find(objectToUse);
                        if (PointToObject != null)
                        {
                            if (PointToObject.GetComponent<PersonObject>() != null)
                                PointToObject = null;
                        }
                }
                else
                {
                    GameObject usableHL = null;

                    foreach (UsableObject u in GameObject.FindObjectsOfType<UsableObject>())
                    {
                        if (u.PrefabToAppear == objectToUse && u.PrefabToAppear != "")
                        {
                            usableHL = u.gameObject;
                            break;
                        }
                    }
                    if (usableHL == null)
                    {
                        foreach (PickableObject p in GameObject.FindObjectsOfType<PickableObject>())
                        {
                            if (p.prefabInHands == objectToUse && p.prefabInHands != "")
                            {
                                if (AlloweAutoAction && !autoObjectSelected)
                                {
                                    AutoActionObject = p.gameObject;
                                    autoObjectSelected = true;
                                    Invoke("AutoPlay", 4f);
                                }
                                HighlightObject h = AddHighlight(p.transform, prefix, HighlightObject.type.NoChange, 2f + Random.Range(0f, 0.5f));
                                if (h != null)
                                {
                                    h.setGold(true);
                                }
                                newHLObjects.Add(p.name);
                            }
                        }
                    }
                    else
                    {
                        if (AlloweAutoAction && !autoObjectSelected)
                        {
                            AutoActionObject = usableHL;
                            autoObjectSelected = true;
                            Invoke("AutoPlay", 4f);
                        }
                        HighlightObject h = AddHighlight(usableHL.transform, prefix, HighlightObject.type.NoChange, 2f + Random.Range(0f, 0.5f));
                        if (h != null)
                        {
                            h.setGold(true);
                        }
                        newHLObjects.Add(usableHL.name);
                    }
                }
            }
        }
        if (PlayerPrefsManager.simulatePlayerActions && ps.away)
            if (AutoMoveTo != "")
            {
                autoPlayMoveWTGName = AutoMoveTo;
                Invoke("AutoPlayMove", 5f);
            }

        //clear highlights
        for (int i = 0; i < activeHighlighted.Count; i++)
        {
            if (!newHLObjects.Contains(activeHighlighted[i]))
            {
                RemoveHighlight(prefix, activeHighlighted[i]);
            }
        }
        activeHighlighted.Clear();
        foreach (string s in newHLObjects)
        {
            activeHighlighted.Add(s);
        }
    }

    void AutoPlayMove()
    {
        ps.WalkToGroup_(GameObject.Find(autoPlayMoveWTGName).GetComponent<WalkToGroup>());
    }

    public void ShowDonePanel(bool value)
    {
        if (PlayerPrefsManager.simulatePlayerActions)
        {
            if (GameObject.FindObjectOfType<AutoPlayer>() != null)
                if (GameObject.FindObjectOfType<AutoPlayer>().toStartAutoplaySession)
                    Invoke("AutoplayCloseGame", 5f);
        }
        donePanel.SetActive(value);
        LevelEnded = value;
    }


    void AutoplayCloseGame()
    {
        CloseGame();
    }

    public void EndScene()
    {
        if (GameObject.Find("Preferences") != null)
        {
            GameObject.Find("Preferences").GetComponent<EndScoreManager>().LoadEndScoreScene();
        }
        else
        {
            bl_SceneLoaderUtils.GetLoader.LoadLevel("UMenuPro");
        }

        donePanel.SetActive(false);
    }

    public void RestartScene()
    {
        closeDialog.SetActive(false);
        bl_SceneLoaderUtils.GetLoader.ReloadScene();
        //bl_SceneLoaderUtils.GetLoader.LoadLevel("MainMenu");
    }

    void OnGUI()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
       GUIStyle style = new GUIStyle();
       style.normal.textColor = new Color(1f, 0f, 0f);
       style.fontSize = 30;
       // ****Show FPS 
       GUI.Label(new Rect(0, 0, 100, 100), ((int)(1.0f / Time.smoothDeltaTime)).ToString(), style);
       if (objectsIDsController != null)
       {
           if (objectsIDsController.cheat)
               GUI.Label(new Rect(30, 0, 100, 100), "Cheat enabled", style);
       }
       //****Show FPS end.

       //debugSS = PlayerAnimationManager.animTimeout.ToString();
       GUI.Label(new Rect(0, 30, 1000, 100), debugSS, style);
#endif
   }
    
    public void DropFromHand(bool leftHand = true)
    {
        PickableObject item = null;
        if (leftHand && !handsInventory.LeftHandEmpty())
            item = handsInventory.leftHandObject;
        else if (!leftHand && !handsInventory.RightHandEmpty())
            item = handsInventory.rightHandObject;
        if (item != null)
        {
            GameObject ghost = null;
            List<PickableObject> ghosts = item.ghostObjects.OrderBy(x =>
                    Vector3.Distance(x.transform.position, ps.transform.position)).ToList();
            if (ghosts.Count > 0)
            {
                ghost = ghosts[0].gameObject;
                if (leftHand)
                    handsInventory.DropLeft(ghost);
                else
                    handsInventory.DropRight(ghost);
            }

            for (int i = item.ghostObjects.Count - 1; i >= 0; --i)
            {
                GameObject g = item.ghostObjects[i].gameObject;
                item.ghostObjects.RemoveAt(i);
                Destroy(g);
            }
        }
        ActionManager.UpdateRequirements();
    }

    public void UpdateButtonsBlink()
    {
        bool practiceMode = true;
        if (prefs != null)
            practiceMode = prefs.practiceMode;
        if (!practiceMode)
            return;

        foreach (ItemControlButton b in GameObject.FindObjectsOfType<ItemControlButton>())
        {
            b.UpdateBlinkState();
        }
        foreach (ButtonBlinking b in GameObject.FindObjectsOfType<ButtonBlinking>())
        {
            b.UpdateButtonState();
        }
    }

    public GameObject GetMovementButtonByType(GameUI.MoveControlButtonType buttonType)
    {
        if (buttonType == GameUI.MoveControlButtonType.None)
            return null;
        foreach (WalkToGroupButton b in WalkToGroupPanel.GetComponentsInChildren<WalkToGroupButton>())
        {
            if (b.moveControlButtonType == buttonType)
                return b.gameObject;
        }
        return null;
    }

    public WalkToGroupButton FindMovementButton(string neededWalkToGroup, WalkToGroup startWalkToGtoup)
    {
        //WalkToGroupPanel
        foreach(WalkToGroupButton b in WalkToGroupPanel.GetComponentsInChildren<WalkToGroupButton>())
        {
            if (b.GetLinkedWalkToGroup().name == neededWalkToGroup)
                return b;
        }
        return null;
    }

    public int FindDirection(string neededWalkToGroup, WalkToGroup startWalkToGtoup, int direction)
    {
        if (startWalkToGtoup.name == neededWalkToGroup)
            return 0;
        WalkToGroup l = null;
        WalkToGroup r = null;
        if (startWalkToGtoup.LeftWalkToGroup != null)
            l = startWalkToGtoup.LeftWalkToGroup;
        if (startWalkToGtoup.RightWalkToGroup != null)
            r = startWalkToGtoup.RightWalkToGroup;

        if (direction != 1 && l != null)
        {
            if (l.name == neededWalkToGroup)
                return -1;
            else
            {
                int a = FindDirection(neededWalkToGroup, l, -1);
                if (a != 0)
                    return a;
            }
        }

        if (direction != -1 && r != null)
        {
            if (r.name == neededWalkToGroup)
                return 1;
            else
            {
                int a = FindDirection(neededWalkToGroup, r, 1);
                if (a != 0)
                    return a;
            }
        }

        return 0;
    }

    public void ShowIpad(bool isSequence = false)
    {
        
        void ShowTheoryTab()
        {
            PlayerScript.actionsLocked = false;
            if (PlayerPrefsManager.videoRecordingMode)
                return;
            if (!GameObject.FindObjectOfType<QuizTab>())
            {
                if (!string.IsNullOrEmpty(actionManager.Message))
                {
                    if (!GameObject.FindObjectOfType<PlayerScript>().robotUIopened)
                    {
                        GameObject.FindObjectOfType<PlayerScript>().OpenRobotUI();
                        theoryTab.ShowTheory(actionManager.MessageTitle, actionManager.Message);
                    }
                    actionManager.Message = null;
                    actionManager.ShowTheory = false;
                }
            }
        }

        void ShowRandomQuizTab()
        {
            if (!GameObject.FindObjectOfType<PlayerScript>().robotUIopened)
            {
                quiz_tab.NextQuizQuestion(true);
                RandomQuiz.showQuestion = false;
            }
        }

        void ShowEncounter()
        {
            if (!GameObject.FindObjectOfType<PlayerScript>().robotUIopened)
            {
                bool randomValue = System.Convert.ToBoolean(Random.Range(0, 2));
                if (randomValue)
                    PlayerScript.TriggerQuizQuestion(QuizTab.encounterDelay, true);
                else
                    QuizTab.encounterDelay = -1f;

                Debug.Log("Encounter quiz with result: " + randomValue);
                encounterStarted = true;
            }
        }

        void SetTargetTime(float time)
        {
            startTimer = false;
            targetTime = time;
        }

        if (actionManager.currentRandomEventIndices.Count > 0 || actionManager.ShowTheory || RandomQuiz.showQuestion || (QuizTab.encounterDelay >= 0))
        {
            if (actionManager.currentRandomEventIndices.Count > 0 && isSequence)
                PlayerScript.TriggerRandomEvent();
            else
                startTimer = true;

        }

        if (startTimer)
        {
            targetTime -= Time.deltaTime;
        }

        if (targetTime <= 0.0f)
        {
            if (QuizTab.encounterDelay >= 0)
            {
                if (encounterStarted == false)
                {
                    ShowEncounter();
                    SetTargetTime(0.4f);
                }
            }
            else if (actionManager.Message != null)
            {
                StartCoroutine(MessageCoroutine(actionManager.MessageDelay));
                SetTargetTime(0.4f);
            }
            else if (RandomQuiz.showQuestion)
            {
                ShowRandomQuizTab();
                SetTargetTime(0.4f);
            }
            else if (actionManager.currentRandomEventIndices.Count > 0 && !PlayerScript.actionsLocked)
            {
                randomEventTab.NextRandomEvent();
                SetTargetTime(0.4f);
            }
        }
        else if (isSequence && (QuizTab.encounterDelay > 0))
        {
            ShowEncounter();
        }
        else if (isSequence && actionManager.ShowTheory)
        {
            StartCoroutine(MessageCoroutine(actionManager.MessageDelay));
            // actionManager.Message = null;
        }
        else if (isSequence && RandomQuiz.showQuestion)
        {
            ShowRandomQuizTab();
        }

        IEnumerator MessageCoroutine(float delay)
        {
            PlayerScript.actionsLocked = true;
            yield return new WaitForSeconds(delay);
            ShowTheoryTab();
        }
    }


    public RectTransform GetActiveTalkBubblePoint()
    {
        if (activeTalkBobblePoint != null)
            return activeTalkBobblePoint.GetComponent<RectTransform>();
        return null;
    }

    public void PlaceTalkBubble(GameObject person)
    {
        activeTalkBobblePoint = null;
        if (person == null)
            return;
        PersonObject personObject = person.GetComponent<PersonObject>();
        if (personObject == null)
            return;
        if (personObject.TalkBubbleAnchor == null)
            return;
        WalkToGroup near = ActionManager.NearestWalkToGroup(person);
        if (ps.away)
            return;
        if (ps.currentWalkPosition == near)
        {
            TalkBubble.SetActive(true);
            TalkBubble.GetComponent<TutorialHintsN>().WorldObject = personObject.TalkBubbleAnchor;
            TalkBubble.GetComponent<TutorialHintsN>().Update();
            PersonToTalk = personObject;
            activeTalkBobblePoint = TalkBubble.transform.Find("cloud").gameObject;
        }
        else
        {
            TalkBubble.SetActive(false);
        }
    }


    public void UITimeout(float waitTime = 0.5f)
    {
        timeOutEnded = false;
        startTimeOut = waitTime;
    }

    void Update()
    {
        if (itemDescriptionGroup != null && 
                (PlayerPrefsManager.GetDevMode() && !Input.GetKey(KeyCode.LeftControl)))
            itemDescriptionGroup.position = Input.mousePosition;
        if (!PlayerPrefsManager.videoRecordingWithTextMode)
            DetailedHintPanel.SetActive(true);      //to remove
        
        if (PlayerPrefsManager.GetDevMode())
        {
            if (PlayerPrefsManager.simulatePlayerActions)
            {
                if (autoPlayer != null)
                {
                    if (autoPlayer.toStartAutoplaySession)
                    {
                        autoExitTime -= Time.deltaTime;
                        if (autoExitTime < 0 && autoExitTime > -15f)
                        {
                            autoExitTime = -30;
                            Debug.LogError("!! Exited from the scene before completion !!");
                            CloseGame();
                        }
                        float t = autoExitTime;
                        if (autoExitTime < 0)
                            t = 0;
                        System.TimeSpan interval = System.TimeSpan.FromSeconds(t);
                        autoExitLabeb.text = "Automatic exit in: " + string.Format("{0:D2}:{1:D2}", interval.Minutes, interval.Seconds);
                    }
                }
            }
        }

        if (!timeOutEnded)
        {
            startTimeOut -= Time.deltaTime;
            if (startTimeOut < 0)
            {
                timeOutEnded = true;

                ActionManager.BuildRequirements();
                ActionManager.UpdateRequirements();
                UpdateHelpHighlight();
   
                if (actionManager.CheckGeneralAction() == null)
                    UpdateHelpHighlight();

                UpdateWalkToGroupUI(true);
            }
            else
            {
                return;
            }
        }
        if (toDelayUpdateHint)
        {
            if (current_UpdateHintDelay > 0)
            {
                current_UpdateHintDelay -= Time.deltaTime;
            }
            else
            {
                toDelayUpdateHint = false;
                UpdateHintPanel(null);
            }
        }

        //Don't show object control panel if animation is playing
        //if animation is longer than 0.2 (is not hold animation)
        bool animationUINotBlocke = true;

        if (allowObjectControlUI)
        {
            animationUINotBlocke = !PlayerAnimationManager.IsLongAnimation();
            if (Camera.main == null)
                animationUINotBlocke = false;
        }

        if (donePanelYesNo.activeSelf)
        {
            ItemControlPanel.SetActive(false);
            patientInfo.SetActive(false);
            return;
        }
        //to show object control panel if no animation block and action block
        bool showItemControlPanel = allowObjectControlUI && animationUINotBlocke;

        //if some object was added or removed to hands
        if (showItemControlPanel)
        {
            if (!PlayerPrefsManager.videoRecordingMode)
                ShowIpad();

            int lHash = 0;
            if (handsInventory.leftHandObject != null)
                lHash = handsInventory.leftHandObject.gameObject.GetHashCode();
            int rHash = 0;
            if (handsInventory.rightHandObject != null)
                rHash = handsInventory.rightHandObject.gameObject.GetHashCode();

            bool handsStateChanged = (currentLeft != lHash || currentRight != rHash
            || (ICPCurrentState != ItemControlPanel.activeSelf)
            || currentActionsCount != actionManager.actionsCount);

            if (handsStateChanged)
            {
                decombineButton.SetActive(false);
                decombineButton_right.SetActive(false);
                ActionManager.UpdateRequirements();
                UpdateHelpHighlight();
                currentActionsCount = actionManager.actionsCount;

                //hide panel for the first frame of hands state change
                //prevent quick blinking of buttons before animation starts
                showItemControlPanel = false;

                //Update current hands state 
                currentLeft = lHash;
                currentRight = rHash;

                bool LEmpty = handsInventory.LeftHandEmpty();
                bool REmpty = handsInventory.RightHandEmpty();
                bool showDecomb = (LEmpty && !REmpty) || (!LEmpty && REmpty);
                bool showCombin = !LEmpty && !REmpty;
                bool showZoomLeft = false;
                bool showZoomRight = false;

                bool showNoTarget = false;
                showNoTarget_right = false;

                if (!LEmpty)
                {
                    DropLeftButton.SetActive(true);
                    if (handsInventory.leftHandObject.GetComponent<ExaminableObject>() != null)
                        showZoomLeft = true;
                    if (actionManager.CompareUseOnInfo(handsInventory.leftHandObject.name, "", "", false))
                    {
                        showNoTarget = true;
                        noTargetButton.transform.GetChild(0).GetComponent<Text>().text =
                            actionManager.CurrentButtonText(handsInventory.leftHandObject.name, true);
                    }
                    if (REmpty)
                    {
                        bool show_decomb_left = actionManager.CompareCombineObjects(handsInventory.leftHandObject.name, "", true);

                        if (!practiceMode)
                            show_decomb_left = true;

                        if (objectsIDsController != null)
                        {
                            if (objectsIDsController.cheat)
                                show_decomb_left = true;
                        }

                        decombineButton.SetActive(show_decomb_left && !showNoTarget);
                        decombineButton.GetComponent<Animator>().SetTrigger("BlinkOn");
                        decombineButton.transform.GetChild(0).GetComponent<Text>().text =
                        (actionManager.CompareCombineObjects(handsInventory.leftHandObject.name, "", true)) ?
                            actionManager.CurrentDecombineButtonText(handsInventory.leftHandObject.name, true)
                            : "Openen";
                    }
                    else
                    {
                        decombineButton.SetActive(false);
                    }
                }
                else
                {
                    DropLeftButton.SetActive(false);
                }
                if (!REmpty)
                {
                    DropRightButton.SetActive(true);

                    if (handsInventory.rightHandObject.GetComponent<ExaminableObject>() != null)
                        showZoomRight = true;
                    if (actionManager.CompareUseOnInfo(handsInventory.rightHandObject.name, "", "", false))
                    {
                        showNoTarget_right = true;
                        noTargetButton_right.transform.GetChild(0).GetComponent<Text>().text =
                           actionManager.CurrentButtonText(handsInventory.rightHandObject.name, true);
                    }
                    if (LEmpty)
                    {
                        bool show_decomb_right = actionManager.CompareCombineObjects(handsInventory.rightHandObject.name, "", true);
                        if (!practiceMode)
                            show_decomb_right = true;
                        if (objectsIDsController != null)
                        {
                            if (objectsIDsController.cheat)
                                show_decomb_right = true;
                        }
                        decombineButton_right.SetActive(show_decomb_right && !showNoTarget_right);
                        decombineButton_right.transform.GetChild(0).GetComponent<Text>().text =
                        (actionManager.CompareCombineObjects("", handsInventory.rightHandObject.name, true)) ?
                            actionManager.CurrentDecombineButtonText(handsInventory.rightHandObject.name, true)
                            : "Openen";
                    }
                    else
                    {
                        decombineButton_right.SetActive(false);
                    }
                }
                else
                {
                    DropRightButton.SetActive(false);
                }
                zoomButtonLeft.SetActive(showZoomLeft);
                zoomButtonRight.SetActive(showZoomRight);
                noTargetButton.SetActive(showNoTarget);

                if (actionManager.CheckGeneralAction() == null && !decombineButton_right.activeSelf)
                    noTargetButton_right.SetActive(showNoTarget_right);
                else
                    noTargetButton_right.SetActive(!decombineButton_right.activeSelf);

                combineButton.SetActive(showCombin);
            }

            if (!currentItemControlPanelState && showItemControlPanel)
            {
                cooldownTime = 1.0f;
            }
            lastCooldownTime = cooldownTime;

            currentItemControlPanelState = showItemControlPanel;

            if (cooldownTime > 0)
            {
                cooldownTime -= Time.deltaTime;
                showItemControlPanel = false;
            }

            if (PlayerScript.actionsLocked)
                showItemControlPanel = false;

            ItemControlPanel.SetActive(showItemControlPanel);
            patientInfo.SetActive(showItemControlPanel);
            //MovementSideButtons.SetActive(showItemControlPanel);
            WalkToGroupPanel.SetActive(showItemControlPanel);
            animatedFinger.gameObject.SetActive(showItemControlPanel && !theoryTab.gameObject.activeSelf && 
                !quiz_tab.gameObject.activeSelf);
            ICPCurrentState = ItemControlPanel.activeSelf;
        }

        currentAnimLock = animationUINotBlocke;
        if (!showItemControlPanel)
            currentAnimLock = false;
    }

    public void ShowNoTargetButton(string buttonText = "")
    {
        if (decombineButton_right.activeSelf)
            return;
        if (!handsInventory.RightHandEmpty())
            if (actionManager.CompareUseOnInfo(handsInventory.rightHandObject.name, "", "", false))
                return;
        //if (noTargetButton_right.activeSelf)
        noTargetButton_right.SetActive(true);
        if (!string.IsNullOrEmpty(buttonText))
            noTargetButton_right.transform.GetChild(0).GetComponent<Text>().text = buttonText;
        else
            noTargetButton_right.transform.GetChild(0).GetComponent<Text>().text =
                actionManager.CurrentButtonText(null);
    }

    public void UpdateWalkToGroupUI(bool value)
    {
        allowObjectControlUI = value;
        if (!value)
            cooldownTime = 1.0f;
        MovementSideButtons.SetActive(false);
        WalkToGroupPanel.SetActive(false); //-------------------
        animatedFinger.gameObject.SetActive(false);
        if (!allowObjectControlUI && !LeftSideButton.gameObject.activeSelf && !RightSideButton.gameObject.activeSelf &&
            !WalkToGroupPanel.activeSelf && !ItemControlPanel.activeSelf)
        {
            return;
        }
        if (cameraMode != null)
            if (cameraMode.currentMode == CameraMode.Mode.ObjectPreview)
            {
                value = false;
            }
        if (!value)
        {
            LeftSideButton.gameObject.SetActive(false);
            RightSideButton.gameObject.SetActive(false);
            //WalkToGroupPanel.SetActive(false);
            ItemControlPanel.SetActive(false);
            patientInfo.SetActive(false);
        }
        else
        {
            //WalkToGroupPanel.SetActive(ps.away);
            if (!ps.away)
            {
                LeftSideButton.gameObject.SetActive(ps.currentWalkPosition.LeftWalkToGroup != null);
                RightSideButton.gameObject.SetActive(ps.currentWalkPosition.RightWalkToGroup != null);

                if (ps.currentWalkPosition.LeftWalkToGroup != null)
                {
                    LeftSideButton.setWalkToGroup(ps.currentWalkPosition.LeftWalkToGroup);
                }

                if (ps.currentWalkPosition.RightWalkToGroup != null)
                {
                    RightSideButton.setWalkToGroup(ps.currentWalkPosition.RightWalkToGroup);
                }
            }
            else
            {
                LeftSideButton.gameObject.SetActive(false);
                RightSideButton.gameObject.SetActive(false);
                foreach (WalkToGroupButton b in GameObject.FindObjectsOfType<WalkToGroupButton>())
                {
                    b.UpdateHint();
                }
            }
        }
        UpdateButtonsBlink();
    }


    public void OpenDonePanelYesNo()
    {
        donePanelYesNo.SetActive(true);
    }

    public void DonePanelYes()
    {
        actionManager.OnUseAction("PaperAndPen");
        donePanelYesNo.SetActive(false);
    }

    public void DonePanelNo()
    {
        donePanelYesNo.SetActive(false);
    }

    public void ClearHintPanel()
    {
        if (DetailedHintPanel.transform.Find("HintContainer") != null)
        {
            Transform panel = DetailedHintPanel.transform.Find("HintContainer").transform;
            for (int i = 0; i < panel.childCount; ++i)
            {
                Destroy(panel.GetChild(i).gameObject);
            }
        }
    }

    public void SetHintPanelAlpha(float alpha)
    {
        Color panelColor = DetailedHintPanel.GetComponent<Image>().color;
        panelColor.a = alpha * 0.7f;
        DetailedHintPanel.GetComponent<Image>().color = panelColor;
        foreach (Text t in DetailedHintPanel.GetComponentsInChildren<Text>())
        {
            Color c = t.color;
            c.a = alpha;
            t.GetComponent<Text>().color = c;
        }
    }

    public void UpdateHintPanel(List<ActionManager.StepData> subTasks, float UpdateHintDelay = 0f)
    {
        if (subTasks == null)
            subTasks = Current_SubTasks;
        else
        {
            //Current_SubTasks.Clear();
            Current_SubTasks = subTasks;
        }
        if (UpdateHintDelay > 0)
        {
            current_UpdateHintDelay = UpdateHintDelay;
            toDelayUpdateHint = true;
            return;
        }
        if (current_UpdateHintDelay > 0)
            return;

        ClearHintPanel();
        Text hintText;
        Text subTaskText;
        InGameLocalEditTool inGameLocalEditTool = PlayerPrefsManager.GetDevMode() ? GameObject.FindObjectOfType<InGameLocalEditTool>() : null;  

        for (int i = 0; i < actionManager.CurrentDescription.Count; i++)
        {
            GameObject currentHintPanel = null;

            currentHintPanel = Instantiate<GameObject>(Resources.Load<GameObject>("NecessaryPrefabs/UI/HintPanel"),
                DetailedHintPanel.transform.Find("HintContainer").transform);
            currentHintPanel.name = "HintPanel";
            hintText = currentHintPanel.transform.Find("Text").gameObject.GetComponent<Text>();
            string actionHintText = "[wat ga je426]";
            if (actionManager.CurrentActionType == ActionManager.ActionType.SequenceStep)
            {
                actionHintText = actionManager.CurrentDescription[i];
            }
            //@
            hintText.text = LocalizationManager.GetValueIfKey(actionHintText);

            if (inGameLocalEditTool != null)
            {
                //!
                inGameLocalEditTool.AddUILocalizationComponentToGO(hintText.gameObject, actionHintText);
            }
            

            for (int y = 0; y < subTasks.Count; y++)
            {
                if (subTasks[y] == null)
                    continue;
                if (subTasks[y].subindex == i)
                {
                    if (Resources.Load<GameObject>("NecessaryPrefabs/UI/SubtaskHints") != null)
                    {
                        if (!subTasks[y].completed)
                        {
                            GameObject subtaskPanel = Instantiate<GameObject>(Resources.Load<GameObject>("NecessaryPrefabs/UI/SubtaskHints"), currentHintPanel.transform);
                            subTaskText = subtaskPanel.transform.Find("Text").GetComponent<Text>();
                            // string textText = subTasks[y].requirement;
                            subTaskText.text = LocalizationManager.GetLocalizedWithMultiKey(subTasks[y].requirement);

                            if (inGameLocalEditTool != null)
                            {
                                //!
                                inGameLocalEditTool.AddUILocalizationComponentToGO(subTaskText.gameObject, subTasks[y].requirement);
                            }
                        }
                    }
                }
            }
            float alpha = DetailedHintPanel.GetComponent<Image>().color.a;
            SetHintPanelAlpha(alpha);
        }
    }

    public void AutoPlayModeChanged()
    {

        PlayerPrefsManager.simulatePlayerActions = autoplayToggle.isOn;
        if (autoPlayer != null)
        {
            autoplayFrame.SetActive(PlayerPrefsManager.simulatePlayerActions);
            autoExitLabeb.text = "";
            autoPlayer.toStartAutoplaySession = false;
        }
    }

    public void UpdateIpadInfo()
    {
        if (pointsTextIpad.gameObject.activeSelf)
        {
            pointsTextIpad.text = ActionManager.Points.ToString();
        }

        if (percentageTextIpad.gameObject.activeSelf)
        {
            if (scoreManager != null)
                percentageTextIpad.text = scoreManager.CalculatePercentage().ToString() + "%";
        }
    }
}
