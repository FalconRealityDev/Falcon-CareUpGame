﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CareUp.Actions;
using System.Linq;
public class GameUI : MonoBehaviour
{
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
    public WalkToGroupButton LeftSideButton;
    public WalkToGroupButton RightSideButton;
    public Dictionary<string, WalkToGroupButton> WTGButtons;
    WalkToGroup prevWalkToGroup = null;
    private Tutorial_Combining tutorialCombine;
    private Tutorial_UseOn tutorialUseOn;
    private HandsInventory handsInventory;
    private ActionManager actionManager;
    private Animator controller;
    private float startTimeOut = 0.5f;
    private bool timeOutEnded = false;
    PlayerPrefsManager prefs;
    public string debugSS = "";
    ObjectsIDsController objectsIDsController;

    public bool DropLeftBlink = false;
    public bool DropRightBlink = false;

    GameObject DetailedHintPanel;

    public List<string> activeHighlighted = new List<string>();

    public GameObject ItemControlPanel;
    public GameObject combineButton;
    public GameObject decombineButton;
    public GameObject decombineButton_right;
    public GameUI.ItemControlButtonType buttonToBlink;
    public GameUI.ItemControlButtonType moveButtonToBlink;
    public bool prescriptionButtonBlink;
    public bool recordsButtonBlink;
    public bool paperAndPenButtonblink;

    public GameObject noTargetButton;
    public GameObject noTargetButton_right;

    private CameraMode cameraMode;

    public GameObject zoomButtonLeft;
    public GameObject zoomButtonRight;
    public GameObject SubStepsPanel;
    Text SubStepsText;

    public bool currentAnimLock = false;
    public GameObject DropLeftButton;
    public GameObject DropRightButton;

    float cooldownTime = 0;
    float lastCooldownTime = 0;
    int currentActionsCount = 0;

    bool currentItemControlPanelState = false;
    int currentLeft;
    int currentRight;
    string useOnNTtext;
    PlayerScript ps;
    bool ICPCurrentState = false;
    public bool allowObjectControlUI = true;
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
        PaperAndPen
    }

    public void UseOn()
    {
        if (!(handsInventory.LeftHandEmpty() && handsInventory.RightHandEmpty()))
        {
            handsInventory.OnCombineAction();

        }
    }

    public void UseOnNoTarget()
    {
        if (tutorialUseOn != null && !tutorialUseOn.ventAllowed)
        {
            return;
        }

        if (!handsInventory.LeftHandEmpty())
        {
            if (actionManager.CompareUseOnInfo(handsInventory.leftHandObject.name, ""))
            {
                if (handsInventory.LeftHandObject.GetComponent<PickableObject>().Use(true, true))
                {
                    UpdateWalkToGtoupUI(false);
                }

                if (tutorialUseOn != null)
                {
                    handsInventory.LeftHandObject.GetComponent<PickableObject>().tutorial_usedOn = true;
                }
                return;
            }
        }
        if (!handsInventory.RightHandEmpty())
        {
            if (actionManager.CompareUseOnInfo(handsInventory.rightHandObject.name, ""))
            {
                if (handsInventory.RightHandObject.GetComponent<PickableObject>().Use(false, true))
                {
                    UpdateWalkToGtoupUI(false);
                }

                if (tutorialUseOn != null)
                {
                    handsInventory.RightHandObject.GetComponent<PickableObject>().tutorial_usedOn = true;
                }
            }
        }
    }

    public void OpenRobotUI()
    {
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
        closeButton.SetActive(!value);

        if (value)
        {
            ps.robotUIopened = true;
        }
        else
        {
            ps.robotUIopened = false;
        }
    }

    public void CloseGame()
    {
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

        int activeGroupButtons = 0;
        foreach (WalkToGroup g in GameObject.FindObjectsOfType<WalkToGroup>())
        {
            switch (g.WalkToGroupType)
            {
                case WalkToGroup.GroupType.WorkField:
                    WTGButtons["WorkField"].setWalkToGroup(g);
                    WTGButtons["WorkField"].gameObject.SetActive(true);
                    activeGroupButtons++;
                    break;
                case WalkToGroup.GroupType.Doctor:
                    WTGButtons["Doctor"].setWalkToGroup(g);
                    WTGButtons["Doctor"].gameObject.SetActive(true);
                    activeGroupButtons++;
                    break;
                case WalkToGroup.GroupType.Patient:
                    WTGButtons["Patient"].setWalkToGroup(g);
                    WTGButtons["Patient"].gameObject.SetActive(true);
                    break;
                case WalkToGroup.GroupType.Sink:
                    WTGButtons["Sink"].setWalkToGroup(g);
                    WTGButtons["Sink"].gameObject.SetActive(true);
                    activeGroupButtons++;
                    break;
            }
        }
        if ((!WTGButtons["Sink"].gameObject.activeSelf || activeGroupButtons <= 2) && WalkToGroupPanel.transform.Find("spacer0") != null)
            WalkToGroupPanel.transform.Find("spacer0").gameObject.SetActive(false);
        if ((!WTGButtons["Patient"].gameObject.activeSelf || activeGroupButtons < 2) && WalkToGroupPanel.transform.Find("spacer2") != null)
            WalkToGroupPanel.transform.Find("spacer2").gameObject.SetActive(false);
        if ((activeGroupButtons < 2) && WalkToGroupPanel.transform.Find("spacer1") != null)
            WalkToGroupPanel.transform.Find("spacer1").gameObject.SetActive(false);
        foreach (WalkToGroup w in GameObject.FindObjectsOfType<WalkToGroup>())
        {
            w.FindNeighbors();
        }
    }

    // Use this for initialization
    void Start()
    {
        objectsIDsController = GameObject.FindObjectOfType<ObjectsIDsController>();
        prefs = GameObject.FindObjectOfType<PlayerPrefsManager>();
        DetailedHintPanel = GameObject.Find("DetailedHintPanel");
        useOnNTtext = noTargetButton.transform.GetChild(0).GetComponent<Text>().text;
        ps = GameObject.FindObjectOfType<PlayerScript>();
        controller = GameObject.FindObjectOfType<PlayerAnimationManager>().GetComponent<Animator>();
        cameraMode = GameObject.Find("GameLogic").GetComponent<CameraMode>();
        handsInventory = GameObject.FindObjectOfType<HandsInventory>();
        tutorialCombine = GameObject.FindObjectOfType<Tutorial_Combining>();
        tutorialUseOn = GameObject.FindObjectOfType<Tutorial_UseOn>();
        actionManager = GameObject.FindObjectOfType<ActionManager>();
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

        ActionManager.practiceMode = true;
        if (prefs != null)
        {
            ActionManager.practiceMode = prefs.practiceMode;
        }

#if !UNITY_EDITOR
        if(GameObject.Find("ActionsPanel") != null)
            GameObject.Find("ActionsPanel").SetActive(false);
#endif
        WalkToGroupPanel = GameObject.Find("MovementButtons");
        Player = GameObject.Find("Player");
        closeButton = transform.Find("CloseBtn").gameObject;
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
                    string key = "";
                    switch (b.name)
                    {
                        case "MoveWorkfield":
                            key = "WorkField";
                            break;
                        case "Movepatient":
                            key = "Patient";
                            break;
                        case "MoveCollegue":
                            key = "Doctor";
                            break;
                        case "MoveSink":
                            key = "Sink";
                            break;
                    }
                    if (key != "")
                    {
                        WTGButtons.Add(key, b);
                    }
                }
            }
        }
        UpdateWalkToGroupButtons();
        UpdateWalkToGtoupUI(true);
    }

    public HighlightObject AddHighlight(Transform target, string prefix, HighlightObject.type hl_type = HighlightObject.type.NoChange, float startDelay = 0, float LifeTime = float.PositiveInfinity)
    {
        string hl_name = prefix + "_" + target.name;
        if (GameObject.Find(hl_name) != null)
            return null;
        GameObject hl_obj = Instantiate(Resources.Load<GameObject>("Prefabs\\HighlightObject"), target.position, new Quaternion()) as GameObject;

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

    //----------------------------------------------------------------------------------------------------------
    public void UpdateHelpHighlight()
    {
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

        List<Action> sublist = actionManager.actionList.Where(action =>
           action.SubIndex == actionManager.currentActionIndex &&
           action.matched == false).ToList();

        foreach (Action a in sublist)
        {
            string[] ObjectNames = new string[0];
            a.ObjectNames(out ObjectNames);

            //if (a.Type == ActionManager.ActionType.ObjectUse ||
            //a.Type == ActionManager.ActionType.ObjectDrop)
            foreach (string objectToUse in ObjectNames)
            {
                if (GameObject.Find(objectToUse) != null)
                {
                    HighlightObject.type hl_type = HighlightObject.type.NoChange;
                    if (GameObject.Find(objectToUse).GetComponent<WorkField>() != null)
                    {
                        hl_type = HighlightObject.type.Hand;
                    }
                    if (handsInventory.IsInHand(GameObject.Find(objectToUse)))
                        continue;
                    HighlightObject h = AddHighlight(GameObject.Find(objectToUse).transform, prefix, hl_type, 2f + Random.Range(0f, 0.5f));
                    if (h != null)
                        h.setGold(true);
                    newHLObjects.Add(objectToUse);
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
                    if (usableHL != null)
                    {
                        HighlightObject h = AddHighlight(usableHL.transform, prefix, HighlightObject.type.NoChange, 2f + Random.Range(0f, 0.5f));
                        if (h != null)
                            h.setGold(true);
                        newHLObjects.Add(usableHL.name);
                    }
                    else
                    {
                        foreach (PickableObject p in GameObject.FindObjectsOfType<PickableObject>())
                        {
                            if (p.prefabInHands == objectToUse && p.prefabInHands != "")
                            {
                                HighlightObject h = AddHighlight(p.transform, prefix, HighlightObject.type.NoChange, 2f + Random.Range(0f, 0.5f));
                                if (h != null)
                                    h.setGold(true);
                                newHLObjects.Add(p.name);
                            }
                        }
                    }
                }
            }
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

    public void ShowDonePanel(bool value)
    {
        donePanel.SetActive(value);
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

    void OnGUI()
    {
#if UNITY_EDITOR
        GUI.Label(new Rect(0, 0, 100, 100), ((int)(1.0f / Time.smoothDeltaTime)).ToString());
        if (objectsIDsController != null)
        {
            if (objectsIDsController.cheat)
                GUI.Label(new Rect(30, 0, 100, 100), "Cheat enabled");
        }

        GUI.Label(new Rect(20, 700, 1000, 100), debugSS);
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

            ghost = ghosts[0].gameObject;
            if (leftHand)
                handsInventory.DropLeft(ghost);
            else
                handsInventory.DropRight(ghost);

            for (int i = item.ghostObjects.Count - 1; i >= 0; --i)
            {
                GameObject g = item.ghostObjects[i].gameObject;
                item.ghostObjects.RemoveAt(i);
                Destroy(g);
            }
        }
    }

    public void updateButtonsBlink()
    {
        bool practiceMode = true;
        if (prefs != null)
            practiceMode = prefs.practiceMode;
        if (!practiceMode)
            return;

        foreach (ItemControlButton b in GameObject.FindObjectsOfType<ItemControlButton>())
        {
            b.updateBlinkState();
        }
        foreach (ButtonBlinking b in GameObject.FindObjectsOfType<ButtonBlinking>())
        {
            b.UpdateButtonState();
        }
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

    // Update is called once per frame
    void Update()
    {
        if (!timeOutEnded)
        {
            startTimeOut -= Time.deltaTime;
            if (startTimeOut < 0)
            {
                timeOutEnded = true;

                ActionManager.BuildRequirements();
                ActionManager.UpdateRequirements();

                UpdateHelpHighlight();
            }
        }

        //Don't show object control panel if animation is playing
        //if animation is longer than 0.2 (is not hold animation)
        bool animationUiBlock = true;
        if (allowObjectControlUI)
        {
            animationUiBlock = !PlayerAnimationManager.IsLongAnimation();
        }

        //to show object control panel if no animation block and action block
        bool showItemControlPanel = allowObjectControlUI && animationUiBlock;

        //if some object was added or removed to hands
        if (showItemControlPanel)
        {
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
                bool showNoTarget_right = false;

                if (!LEmpty)
                {
                    DropLeftButton.SetActive(true);
                    if (handsInventory.leftHandObject.GetComponent<ExaminableObject>() != null)
                        showZoomLeft = true;
                    if (actionManager.CompareUseOnInfo(handsInventory.leftHandObject.name, ""))
                    {
                        showNoTarget = true;
                        noTargetButton.transform.GetChild(0).GetComponent<Text>().text =
                            actionManager.CurrentButtonText(handsInventory.leftHandObject.name);
                    }
                    if (REmpty)
                    {
                        bool show_decomb_left = actionManager.CompareCombineObjects(handsInventory.leftHandObject.name, "");
                        decombineButton.SetActive(show_decomb_left);
                        decombineButton.GetComponent<Animator>().SetTrigger("BlinkOn");
                        decombineButton.transform.GetChild(0).GetComponent<Text>().text =
                        (actionManager.CompareCombineObjects(handsInventory.leftHandObject.name, "")) ?
                            actionManager.CurrentDecombineButtonText(handsInventory.leftHandObject.name)
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
                    if (actionManager.CompareUseOnInfo(handsInventory.rightHandObject.name, ""))
                    {
                        showNoTarget_right = true;
                        noTargetButton_right.transform.GetChild(0).GetComponent<Text>().text =
                           actionManager.CurrentButtonText(handsInventory.rightHandObject.name);
                    }
                    if (LEmpty)
                    {
                        bool show_decomb_right = actionManager.CompareCombineObjects(handsInventory.rightHandObject.name, "");
                        decombineButton_right.SetActive(show_decomb_right);
                        decombineButton_right.transform.GetChild(0).GetComponent<Text>().text =
                        (actionManager.CompareCombineObjects("", handsInventory.rightHandObject.name)) ?
                            actionManager.CurrentDecombineButtonText(handsInventory.rightHandObject.name)
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
                noTargetButton_right.SetActive(showNoTarget_right);

                //decombineButton.SetActive(showDecomb && REmpty && !showNoTarget);
                //if(decombineButton.activeSelf)
                //decombineButton.GetComponent<Animator>().SetTrigger("BlinkOn");

                //decombineButton_right.SetActive(showDecomb && LEmpty && !showNoTarget_right);
                combineButton.SetActive(showCombin);
            }


            if (!currentItemControlPanelState && showItemControlPanel)
            {
                cooldownTime = 0.4f;
            }
            lastCooldownTime = cooldownTime;
            if (cooldownTime > 0)
                cooldownTime -= Time.deltaTime;
            currentItemControlPanelState = showItemControlPanel;
            if (cooldownTime > 0)
                showItemControlPanel = false;
            if (PlayerScript.actionsLocked)
                showItemControlPanel = false;
            ItemControlPanel.SetActive(showItemControlPanel);

            ICPCurrentState = ItemControlPanel.activeSelf;
        }

        currentAnimLock = animationUiBlock;
        if (!showItemControlPanel)
            currentAnimLock = false;
    }

    public void UpdateWalkToGtoupUI(bool value)
    {
        allowObjectControlUI = value;
        //print("++++++++++++++++ " + cameraMode.currentMode.ToString());
        if (cameraMode != null)
            if (cameraMode.currentMode == CameraMode.Mode.ObjectPreview)
            {
                value = false;
            }
        if (!value)
        {
            LeftSideButton.gameObject.SetActive(false);
            RightSideButton.gameObject.SetActive(false);
            WalkToGroupPanel.SetActive(false);
            ItemControlPanel.SetActive(false);
        }
        else
        {
            WalkToGroupPanel.SetActive(ps.away);
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
            }
        }
        updateButtonsBlink();
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
        panelColor.a = alpha;
        DetailedHintPanel.GetComponent<Image>().color = panelColor;
        foreach (Text t in DetailedHintPanel.GetComponentsInChildren<Text>())
        {
            Color c = t.color;
            c.a = alpha;
            t.GetComponent<Text>().color = c;
        }
    }

    public void UpdateHintPanel(List<ActionManager.StepData> subTasks)
    {
        ClearHintPanel();
        Text hintText;
        Text subTaskText;

        for (int i = 0; i < actionManager.CurrentDescription.Count; i++)
        {
            GameObject currentHintPanel = null;

            currentHintPanel = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI/HintPanel"), DetailedHintPanel.transform.Find("HintContainer").transform);
            hintText = currentHintPanel.transform.Find("Text").gameObject.GetComponent<Text>();
            hintText.text = actionManager.CurrentDescription[i];

            for (int y = 0; y < subTasks.Count; y++)
            {

                if (subTasks[y].subindex == i)
                {
                    if (Resources.Load<GameObject>("Prefabs/UI/SubtaskHints") != null)
                    {
                        if (!subTasks[y].completed)
                        {
                            GameObject subtaskPanel = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI/SubtaskHints"), currentHintPanel.transform);
                            subTaskText = subtaskPanel.transform.Find("Text").GetComponent<Text>();
                            subTaskText.text = subTasks[y].requirement;
                        }
                    }
                }
            }
            float alpha = DetailedHintPanel.GetComponent<Image>().color.a;
            SetHintPanelAlpha(alpha);
        }
    }
}
