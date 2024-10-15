using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.PostProcessing;
using CareUp.Localize;
public class PlayerScript : MonoBehaviour
{
    [HideInInspector]
    public bool tutorial_movementLock = false;
    [HideInInspector]
    public float tutorial_totalLookAround = 0.0f;
    [HideInInspector]
    public float tutorial_totalMoveAround = 0.0f;
    [HideInInspector]
    public bool tutorial_movedBack = false;
    [HideInInspector]
    public bool tutorial_movedTo = false;
    [HideInInspector]
    public bool tutorial_robotUI_opened = false;
    [HideInInspector]
    public bool tutorial_robotUI_closed = false;
    [HideInInspector]
    public bool tutorial_itemControls = false;
    [HideInInspector]
    public bool tutorial_UseOnControl = false;
    [HideInInspector]
    public string itemControlsToInit = "";

    public Camera cam;
    private float defaultFOV = 60f;
    Camera currentExtraCamera;
    GameObject flyHeloper;

    WalkToGroup ImmediateWTG;

    public MouseLook mouseLook = new MouseLook();
    public bool freeLook = false;
    GameUI gameUI;
    PlayerPrefsManager prefs;
    Controls controls;
    HandsInventory handsInv;
    CameraMode cameraMode;

    public bool away = true;
    private Vector3 savedPos;
    private Quaternion savedRot;
    private List<WalkToGroup> groups;
    public WalkToGroup currentWalkPosition;
    private bool AutoPlayClicked = false;
    private GameObject AutoPlayActionObject = null;
    private ActionManager actionManager = null;

    RobotManager robot;
    private Vector3 savedRobotPos;
    private Quaternion savedRobotRot;

    private bool fade;
    private float fadeTime = 1f;
    private float fadeTimer = 0.0f;
    Texture fadeTex;

    public ItemControlsUI itemControls;

    public bool usingOnMode = false;
    public bool usingOnHand;

    private GameObject usingOnText;
    private GameObject usingOnCancelButton;
    private bool onButtonHover = false;

    private GameObject closeButton;

    private float rotated = 0.0f;

    [HideInInspector]
    public static QuizTab quiz;
    public static RandomEventTab randomEvent;

    public static RandomEventTab randomEventTab;
    private static PlayerScript instance; // fix for coroutines

    public bool robotUIopened = false;
    private bool robotUINotOpenedYet = true;
    public GameObject robotSavedLeft;
    public GameObject robotSavedRight;

    GameObject devHintUI = null;
    GameObject biggerDevHintUI = null;
    GameObject tutorialCanvas;

    Tutorial_UI tutorial_UI;
    Tutorial_Theory tutorial_theory;

    bool devHintActiveForIpad = false;
    bool biggerDevHintActiveForIpad = false;

    GameObject extraButton;
    bool extraBtnActiveForIpad = false;
    GameObject extraPanel;
    bool extraPanelActiveForIpad = false;

    public static bool actionsLocked = false;
    float defaultInteractionDistance = -1;
    [HideInInspector]
    public GameObject joystickObject;
    //[HideInInspector]

    public WalkToGroup.GroupType momentaryJumpTo;

    public bool UIHover
    {
        get { return onButtonHover; }
    }

    public void ResetUIHover()
    {
        onButtonHover = false;
    }

    public void SwitchCamera(string cameraName)
    {
        if (flyHeloper == null)
            flyHeloper = GameObject.Find("flyHeloper");
        if (cameraName == "")
        {
            gameUI.GetComponent<CanvasGroup>().alpha = 1.0f;
            gameUI.GetComponent<CanvasGroup>().interactable = true;

            if (currentExtraCamera != null)
                currentExtraCamera.enabled = false;
            cam.enabled = true;
            flyHeloper.SetActive(true);
        }
        else
        {
            GameObject extraCamGO = GameObject.Find(cameraName);
            if (extraCamGO != null)
            {
                if (extraCamGO.GetComponent<Camera>() != null)
                {
                    currentExtraCamera = extraCamGO.GetComponent<Camera>();
                    cam.enabled = false;
                    currentExtraCamera.enabled = true;
                    gameUI.GetComponent<CanvasGroup>().alpha = 0.0f;
                    gameUI.GetComponent<CanvasGroup>().interactable = false;
                    flyHeloper.SetActive(false);
                }
            }
        }
    }

    private void Start()
    {
        instance = this;
        actionsLocked = false;
        gameUI = GameObject.FindObjectOfType<GameUI>();

        if (GameObject.Find("GameLogic") != null)
        {
            actionManager = GameObject.Find("GameLogic").GetComponent<ActionManager>();
        }

        mouseLook.Init(transform, cam.transform);

        if (GameObject.Find("Preferences") != null)
        {
            prefs = GameObject.Find("Preferences").GetComponent<PlayerPrefsManager>();
        }

        GetComponent<Crosshair>().enabled = (prefs == null) ? false : prefs.VR;

        controls = GameObject.Find("GameLogic").GetComponent<Controls>();

        groups = new List<WalkToGroup>(
            GameObject.FindObjectsOfType<WalkToGroup>());

        fadeTex = Resources.Load<Texture>("Sprites/Black");

        extraButton = GameObject.Find("ExtraButton");
        extraPanel = GameObject.Find("Extra");

        itemControls = GameObject.FindObjectOfType<ItemControlsUI>();
        itemControls.gameObject.SetActive(false);

        handsInv = GameObject.Find("GameLogic").GetComponent<HandsInventory>();
        cameraMode = GameObject.Find("GameLogic").GetComponent<CameraMode>();

        usingOnText = GameObject.Find("UsingOnModeText");
        usingOnCancelButton = GameObject.Find("CancelUseOnButton").gameObject;
        usingOnText.SetActive(false);

        quiz = gameUI.quiz_tab;
        randomEvent = gameUI.randomEventTab;

        EventTrigger.Entry event1 = new EventTrigger.Entry();
        event1.eventID = EventTriggerType.PointerEnter;
        event1.callback.AddListener((eventData) => { EnterHover(); });

        EventTrigger.Entry event2 = new EventTrigger.Entry();
        event2.eventID = EventTriggerType.PointerExit;
        event2.callback.AddListener((eventData) => { ExitHover(); });

        EventTrigger.Entry event3 = new EventTrigger.Entry();
        event3.eventID = EventTriggerType.PointerClick;
        event3.callback.AddListener((eventData) => { ExitHover(); });

        usingOnCancelButton.AddComponent<EventTrigger>();
        usingOnCancelButton.GetComponent<EventTrigger>().triggers.Add(event1);
        usingOnCancelButton.GetComponent<EventTrigger>().triggers.Add(event2);
        usingOnCancelButton.GetComponent<EventTrigger>().triggers.Add(event3);

        //GameObject robotUI = GameObject.Find("RobotUI");
        //robotUI.AddComponent<EventTrigger>();
        //robotUI.GetComponent<EventTrigger>().triggers.Add(event1);
        //robotUI.GetComponent<EventTrigger>().triggers.Add(event2);

        if (GameObject.Find("DetailedHintPanel") != null)
            devHintUI = GameObject.Find("DetailedHintPanel").gameObject;
        if (GameObject.Find("BiggerDevHint") != null)
            biggerDevHintUI = GameObject.Find("BiggerDevHint").gameObject;

        // warningPopUp.AddComponent<EventTrigger>();
        //warningPopUp.GetComponent<EventTrigger>().triggers.Add(event1);
        // warningPopUp.GetComponent<EventTrigger>().triggers.Add(event2);
        // warningPopUp.GetComponent<EventTrigger>().triggers.Add(event3);

        EventTrigger.Entry closePopUp = new EventTrigger.Entry();
        event3.eventID = EventTriggerType.PointerClick;
        event3.callback.AddListener((eventData) => { TimedPopUp.ForceHide(); });

        //warningPopUp.GetComponent<EventTrigger>().triggers.Add(closePopUp);

        if (GameObject.Find("GameLogic").GetComponent<TutorialManager>() != null)
        {
            GameObject tutorialEndUi = GameObject.Find("TutorialDonePanel");
            tutorialEndUi.AddComponent<EventTrigger>();
            tutorialEndUi.GetComponent<EventTrigger>().triggers.Add(event1);
            tutorialEndUi.GetComponent<EventTrigger>().triggers.Add(event2);
            tutorialEndUi.SetActive(false);
        }

        savedPos = transform.position;
        savedRot = transform.rotation;
        
        mouseLook.SaveRot(transform, cam.transform);

        robot = GameObject.FindObjectOfType<RobotManager>();
        savedRobotPos = robot.transform.position;
        savedRobotRot = robot.transform.rotation;

        tutorial_UI = GameObject.FindObjectOfType<Tutorial_UI>();
        tutorial_theory = GameObject.FindObjectOfType<Tutorial_Theory>();

        GameObject.Find("GameLogic").AddComponent<GestureControls>();
        if (momentaryJumpTo != WalkToGroup.GroupType.NotSet)
            Invoke("MomentaryJumpToGroup", 0.01f);

    }
    void MomentaryJumpToGroup()
    {
        foreach (WalkToGroup w in GameObject.FindObjectsOfType<WalkToGroup>())
        {
            if (momentaryJumpTo == w.WalkToGroupType)
            {
                WalkToGroup_(w);
            }
        }
    }

    public void EnterHover()
    {
        onButtonHover = true;
    }

    public void ExitHover()
    {
        onButtonHover = false;
    }

    public void FreeLookButton()
    {
        freeLook = !freeLook;
        rotated = 0.0f;

        mouseLook.Init(transform, cam.transform);

        if (freeLook)
        {
            foreach (WalkToGroup g in groups)
            {
                if (g != currentWalkPosition)
                {
                    g.HighlightGroup(false);
                    g.enabled = true;
                    g.GetComponent<Collider>().enabled = true;
                }
            }
        }
    }



    public void ResetTargetRot()
    {
        mouseLook.Init(transform, cam.transform);
    }

    public void LookRotationUpdate(Vector2 amount)
    {
        // OLD mouse look code, transfering to joystick
        if (freeLook && !robotUIopened && cameraMode.CurrentMode == CameraMode.Mode.Free)
        {
            rotated += mouseLook.LookRotation(transform, cam.transform, amount);
        }
    }

    private void Update()
    {

        if (prefs != null)
        {
            if (!prefs.VR)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (ImmediateWTG != null)
        {
            Immediate_WalkToGroup(ImmediateWTG);
            ImmediateWTG = null;
        }
        // OLD mouse look code, transfering to joystick
        /*if (freeLook && !robotUIopened && cameraMode.CurrentMode == CameraMode.Mode.Free)
        {
            rotated += mouseLook.LookRotation(transform, Camera.main.transform);
        }*/
        GameObject selectedObject = controls.SelectedObject;
        if (AutoPlayActionObject != null)
            selectedObject = AutoPlayActionObject;

        if (!freeLook && !robotUIopened &&
            ((Input.touchCount < 1 && controls.MouseClicked()) ||
            (Input.touchCount > 0 && Controls.MouseReleased()) || AutoPlayClicked))
        {
            if (!away && (selectedObject != null)
                && !itemControls.gameObject.activeSelf && !actionsLocked)
            {
                if (usingOnMode)
                {
                    if (usingOnHand)
                    {
                        handsInv.LeftHandUse();

                        ToggleUsingOnMode(false);
                    }
                    else
                    {
                        handsInv.RightHandUse();

                        ToggleUsingOnMode(false);
                    }
                }
                else
                {
                    itemControls.Init(selectedObject);
                }
            }
            else if (Input.touchCount > 0 && Controls.MouseReleased())
            {
                // catch falling touch here
                if (selectedObject != null &&
                    selectedObject.GetComponent<WalkToGroup>() && away)
                {
                    WalkToGroup_(selectedObject.GetComponent<WalkToGroup>());
                }
            }
        }
        else if (Input.GetMouseButtonDown(1) && usingOnMode)
        {
            ToggleUsingOnMode(false);
        }
        else if (Controls.MouseReleased())// && freeLook)
        {
            if (rotated < 3.0f && selectedObject != null &&
                selectedObject.GetComponent<WalkToGroup>() && away)
            {
                WalkToGroup_(selectedObject.GetComponent<WalkToGroup>());
            }
            else
            {
                rotated = 0.0f;
                freeLook = false;
                //FreeLookButton();
            }
        }

       
        AutoPlayClicked = false;
        AutoPlayActionObject = null;
    }

    public void ToggleUsingOnMode(bool value)
    {
        usingOnMode = value;
        if (value)
        {
            usingOnText.GetComponent<Text>().text = "Selecteer een object waarmee je " +
                (usingOnHand ?
                    (handsInv.LeftHandObject.GetComponent<InteractableObject>().description == ""
                    ? handsInv.LeftHandObject.name : LocalizationManager.GetValueIfKey(
                        handsInv.LeftHandObject.GetComponent<InteractableObject>().description))
                :
                    (handsInv.RightHandObject.GetComponent<InteractableObject>().description == ""
                    ? handsInv.RightHandObject.name : LocalizationManager.GetValueIfKey(
                        handsInv.RightHandObject.GetComponent<InteractableObject>().description))
                )
                + " wilt gebruiken.";
        }
        usingOnText.SetActive(value);

        if (!value)
        {
            onButtonHover = false;
        }
        else
        {
            tutorial_UseOnControl = true;
        }
    }

    void WalkToGroupAction(WalkToGroup fromGroup, WalkToGroup toGroup)
    {
        string fromName = "";
        if (fromGroup != null)
            fromName = fromGroup.name;
        if (GameObject.Find("PanoFlyCamera") != null)
        {
            GameObject panoFlyCamera = GameObject.Find("PanoFlyCamera");
            GameObject playerCamera = GameObject.Find("PlayerMainCamera");
            if (playerCamera != null)
            {
                if (playerCamera.GetComponent<PostProcessingBehaviour>() != null)
                {
                    if (panoFlyCamera.GetComponent<PostProcessingBehaviour>() != null)
                    {
                        panoFlyCamera.GetComponent<PostProcessingBehaviour>().enabled = 
                            playerCamera.GetComponent<PostProcessingBehaviour>().isActiveAndEnabled;
                    }
                }
            }
            if (panoFlyCamera.GetComponent<Animator>() != null)
            {
                panoFlyCamera.GetComponent<Animator>().SetTrigger("from_" + fromName + "_to_" + toGroup.name);
            }
        }
    }

    public void Immediate_WalkToGroup(WalkToGroup group, bool force = false)
    {
        if (robotUIopened && !force)
            return;
        ToggleAway();
        transform.position = group.Position;
        if (prefs == null || (prefs != null && !prefs.VR))
        {
            transform.rotation = Quaternion.Euler(0.0f, group.Rotation.y, 0.0f);
            cam.transform.localRotation = Quaternion.Euler(group.Rotation.x, 0.0f, 0.0f);
            mouseLook.SaveRot(transform, cam.transform);
        }
        currentWalkPosition = group;

        robot.transform.position = group.robotPosition;
        robot.transform.rotation = Quaternion.Euler(group.robotRotation);

        freeLook = false;

        foreach (WalkToGroup g in groups)
        {
            if (g != currentWalkPosition)
            {
                g.HighlightGroup(false);
                g.enabled = true;
                g.GetComponent<Collider>().enabled = true;
            }
        }

        actionManager.OnMovementAction(currentWalkPosition.name);
        gameUI.UpdateWalkToGroupUI(true);

        if (PlayerPrefsManager.simulatePlayerActions)
            gameUI.UpdateHelpHighlight();
        if (defaultInteractionDistance <= 0f)
        {
            defaultInteractionDistance = controls.interactionDistance;
        }
        if (group.interactionDistance > 0)
            controls.interactionDistance = group.interactionDistance;
        else
            controls.interactionDistance = defaultInteractionDistance;
    }


    public void ForceWalkToGroup(WalkToGroup group)
    {
        GameObject.FindObjectOfType<CameraMode>().ForceResetFromCinematic();
        WalkToGroup_(group);
        Immediate_WalkToGroup(group, true);
        ImmediateWTG = null;
        GameObject.FindObjectOfType<CameraMode>().ForceResetFromCinematic(true);


    }

    public void WalkToGroup_(WalkToGroup group)
    {
        if (group.playerCameraFOV > 0)
            cam.fieldOfView = group.playerCameraFOV;
        else
            cam.fieldOfView = defaultFOV;
        WalkToGroupAction(currentWalkPosition, group);
        ImmediateWTG = group;
        gameUI.UpdateMovementButtons(group);
    }

    private void ToggleAway(bool _away = false)
    {
        fade = true;
        away = _away;
        foreach (WalkToGroup g in groups)
        {
            g.HighlightGroup(false);
            g.enabled = away;
            g.GetComponent<Collider>().enabled = away;
        }

        itemControls.Close();

        if (away)
        {
            tutorial_movedBack = true;
        }
        else
        {
            tutorial_movedTo = true;
        }

        freeLook = false;
    }

    public void ResetFreeLook()
    {
        //transform.rotation = mouseLook.SavedCharRot;
        cam.transform.rotation = mouseLook.SavedCamRot;
    }

    private void OnGUI()
    {
        if (fade)
        {
            if (fadeTimer > fadeTime)
            {
                fadeTimer = 0.0f;
                fade = false;
            }
            else
            {
                GUI.color = new Color(0.0f, 0.0f, 0.0f, 1.0f -
                    Mathf.InverseLerp(0.0f, fadeTime, fadeTimer));
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTex);
                fadeTimer += Time.deltaTime;
            }
        }
    }

    public void OpenRobotUI()
    {
        if (robotUIopened)
            return;

        if ((tutorial_UI != null && tutorial_UI.expectedRobotUIstate == false) ||
            (tutorial_theory != null && tutorial_theory.expectedRobotUIstate == false))
        {
            return;
        }

        GameObject IPad = GameObject.FindObjectOfType<GameUI>().IPad;

        IPad.GetComponent<Animator>().enabled = true;
        IPad.GetComponent<Animator>().SetTrigger("start");

        IPad.GetComponent<CanvasGroup>().alpha = 1f;
        IPad.GetComponent<CanvasGroup>().blocksRaycasts = true;

        robotUIopened = true;

        gameUI.UpdateIpadInfo();

        if (devHintUI != null)
        {
            devHintActiveForIpad = devHintUI.activeSelf;
            devHintUI.SetActive(false);
        }

        if (biggerDevHintUI != null)
        {
            biggerDevHintActiveForIpad = biggerDevHintUI.activeSelf;
            biggerDevHintUI.SetActive(false);
        }

        if (extraButton != null)
        {
            extraBtnActiveForIpad = extraButton.activeSelf;
            extraButton.SetActive(false);
            gameUI.UpdateWalkToGroupUI(false);
        }

        if (extraPanel == null)
        {
            extraPanel = GameObject.Find("Extra");
        }

        RobotManager.SetUITriggerActive(false);

        tutorial_robotUI_opened = true;

        GameObject.FindObjectOfType<GameUI>().allowObjectControlUI = false;

        if (robotUINotOpenedYet)
        {
            robotUINotOpenedYet = false;
        }
        if (joystickObject != null)
            joystickObject.SetActive(!robotUIopened);

        itemControls.Close();
        //autoplay action
        if (PlayerPrefsManager.simulatePlayerActions)
            Invoke("CloseRobotUI", 5f);
    }

    public void CloseRobotUI()
    {
        if (GameObject.FindObjectOfType<QuizTab>() != null)
        {
            if (GameObject.FindObjectOfType<QuizTab>().quiz && gameUI.theoryTab.gameObject.activeSelf)
            {
                gameUI.HideTheoryTab();
                return;
            }
        }   

        GameObject.FindObjectOfType<GameUI>().IPad.GetComponent<Animator>().enabled = false;

        if (GameObject.FindObjectOfType<CameraMode>() != null)
        {
            if (GameObject.FindObjectOfType<CameraMode>().currentMode != CameraMode.Mode.SelectionDialogue)
            {
                RobotManager.SetUITriggerActive(true);
            }
        }

        QuizTab quizTab = GameObject.FindObjectOfType<QuizTab>();

        if (quizTab != null && quizTab.continueBtn)
        {
            GameObject.FindObjectOfType<QuizTab>().OnContinueButton();
        }

        GameObject.FindObjectOfType<GameUI>().allowObjectControlUI = false;

        GameObject.FindObjectOfType<GameUI>().IPad.GetComponent<CanvasGroup>().alpha = 0f;
        GameObject.FindObjectOfType<GameUI>().IPad.GetComponent<CanvasGroup>().blocksRaycasts = false;

        robotUIopened = false;

        if (GameObject.FindObjectOfType<TutorialManager>() == null
            || tutorial_UI != null || tutorial_theory != null)
        {
            if (devHintUI != null)
            {
                devHintUI.SetActive(devHintActiveForIpad);
            }

            if (biggerDevHintUI != null)
            {
                biggerDevHintUI.SetActive(biggerDevHintActiveForIpad);
            }

            if (extraButton != null)
            {
                extraButton.SetActive(extraBtnActiveForIpad);
                gameUI.UpdateWalkToGroupUI(extraBtnActiveForIpad);
            }

            if (extraPanel != null)
            {
                extraPanel.SetActive(extraPanelActiveForIpad);
            }
        }

        tutorial_robotUI_closed = true;

        GameObject.FindObjectOfType<GameUI>().allowObjectControlUI = true;

        if (joystickObject != null)
            joystickObject.SetActive(!robotUIopened);
    }

    public void PickItemsBackAfterRobotUI()
    {
        if (robotSavedLeft != null || robotSavedRight != null)
        {
            StartCoroutine(DelayedPickItemsAfterIpad(0.5f));
        }

        if (robotSavedLeft != null)
        {
            PlayerAnimationManager.SetHandItem(true, robotSavedLeft);
        }
        else
        {
            PlayerAnimationManager.SetHandItem(true, null);
        }

        if (robotSavedRight != null)
        {
            PlayerAnimationManager.SetHandItem(false, robotSavedRight);
        }
        else
        {
            PlayerAnimationManager.SetHandItem(false, null);
        }

        itemControls.Close();
    }

    IEnumerator DelayedPickItemsAfterIpad(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (robotSavedLeft != null)
        {
            handsInv.ForcePickItem(robotSavedLeft, PlayerAnimationManager.Hand.Left, true);
            robotSavedLeft = null; // reset
        }

        if (robotSavedRight != null)
        {
            handsInv.ForcePickItem(robotSavedRight, PlayerAnimationManager.Hand.Right, true);
            robotSavedRight = null; // reset
        }

        robotSavedLeft = robotSavedRight = null;
    }

    /// <summary>
    /// Function triggers next quiz question from xml file,
    /// that was set in PlayerSpawn object. 
    /// If there is no questions left - it will do nothing.
    /// </summary>
    /// <param name="delay">Delay before opening ipad.</param>
    public static void TriggerQuizQuestion(float delay = 0.0f, bool encounter = false)
    {
        // dont trigger quiz if a testing mode is on
#if UNITY_EDITOR
        //if (GameObject.FindObjectOfType<PlayerPrefsManager>() != null)
        //    if (GameObject.FindObjectOfType<PlayerPrefsManager>().testingMode)
        //        return;
        //if (GameObject.FindObjectOfType<ObjectsIDsController>() != null)
        //if (GameObject.FindObjectOfType<ObjectsIDsController>().testingMode)
        //return;
#endif
        // just dont trigger quiz if it's a tutorial for all cases
        if (GameObject.FindObjectOfType<TutorialManager>() != null)
            return;

        // lock actions so player does nothing to break until quiz triggers
        PlayerScript.actionsLocked = true;
        // close itemDescription if active, cuz we locked actions, so it's not updating
        GameObject itemDescription = GameObject.Find("ItemDescription");
        if (itemDescription)
        {
            itemDescription.SetActive(false);
        }
        // trigger quiz with delay
        instance.StartCoroutine(QuizCoroutine(delay, encounter));
    }

    private static IEnumerator QuizCoroutine(float delay, bool encounter)
    {
        yield return new WaitForSeconds(delay);
        quiz.NextQuizQuestion(false, encounter);
    }


    public static void TriggerRandomEvent(float delay = 0.0f)
    {
        if (GameObject.FindObjectOfType<TutorialManager>() != null)
            return;

        // lock actions so player does nothing to break until quiz triggers
        // close itemDescription if active, cuz we locked actions, so it's not updating
        GameObject itemDescription = GameObject.Find("ItemDescription");
        if (itemDescription)
        {
            itemDescription.SetActive(false);
        }
        // trigger quiz with delay
        instance.StartCoroutine(RandomEventCoroutine(delay));
    }
    private static IEnumerator RandomEventCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        randomEvent.NextRandomEvent();
    }

    public void AutoClick(GameObject obj)
    {
        AutoPlayClicked = true;
        AutoPlayActionObject = obj;
    }
}
