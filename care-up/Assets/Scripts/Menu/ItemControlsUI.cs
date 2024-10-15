using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CareUp.Actions;
using System;

public class ItemControlsUI : MonoBehaviour
{
    public bool oldInitDisabled = true;

    public GameObject initedObject;

    private Controls controls;
    private HandsInventory handsInventory;
    private CameraMode cameraMode;
    private ActionManager actionManager;
    private TutorialManager tutorial;
    private PlayerScript player;

    private GameObject closeButton;

    private GameObject pickButton;
    private GameObject examineButton;
    private GameObject useButton;
    private GameObject talkButton;
    private GameObject useOnButton;
    private GameObject useOnNTButton;
    private GameObject combineButton;
    private GameObject dropButton;
    private GameObject discardButton;
    private GeneralAction generalAction;

    public Vector2 cursorOffset;

    private string useOnNTtext;
    private string useText;

#pragma warning disable CS0414
    private bool UIhover;
#pragma warning restore CS0414

    private Tutorial_Combining tutorialCombine;
    private Tutorial_UseOn tutorialUseOn;

    public bool instantCloseFixFlag = false;

    private void OnEnterHover()
    {
        UIhover = true;
    }

    private void OnExitHover()
    {
        UIhover = false;
    }

    void Awake()
    {
        controls = GameObject.Find("GameLogic").GetComponent<Controls>();
        handsInventory = GameObject.Find("GameLogic").GetComponent<HandsInventory>();
        cameraMode = GameObject.Find("GameLogic").GetComponent<CameraMode>();
        actionManager = GameObject.Find("GameLogic").GetComponent<ActionManager>();
        tutorial = GameObject.Find("GameLogic").GetComponent<TutorialManager>();

        pickButton = transform.Find("PickButton").gameObject;
        examineButton = transform.Find("ExamineButton").gameObject;
        useButton = transform.Find("UseButton").gameObject;
        talkButton = transform.Find("TalkButton").gameObject;

        useOnButton = transform.Find("UseOnButton").gameObject;
        useOnNTButton = transform.Find("UseOnButton_noTarget").gameObject;
        combineButton = transform.Find("CombineButton").gameObject;

        dropButton = transform.Find("DropButton").gameObject;
        dropButton.SetActive(false);

        closeButton = transform.Find("CloseButton").gameObject;
        closeButton.SetActive(false);

        discardButton = transform.Find("DiscardButton").gameObject;

        useOnNTtext = useOnNTButton.transform.GetChild(0).GetComponent<Text>().text;
        useText = useButton.transform.GetChild(0).GetComponent<Text>().text;

        //------------

        EventTrigger.Entry event1 = new EventTrigger.Entry();
        event1.eventID = EventTriggerType.PointerEnter;
        event1.callback.AddListener((eventData) => { OnEnterHover(); });

        EventTrigger.Entry event2 = new EventTrigger.Entry();
        event2.eventID = EventTriggerType.PointerExit;
        event2.callback.AddListener((eventData) => { OnExitHover(); });

        EventTrigger.Entry event3 = new EventTrigger.Entry();
        event3.eventID = EventTriggerType.PointerClick;
        event3.callback.AddListener((eventData) => { OnExitHover(); });

        closeButton.AddComponent<EventTrigger>();
        closeButton.GetComponent<EventTrigger>().triggers.Add(event1);
        closeButton.GetComponent<EventTrigger>().triggers.Add(event2);
        closeButton.GetComponent<EventTrigger>().triggers.Add(event3);

        pickButton.AddComponent<EventTrigger>();
        pickButton.GetComponent<EventTrigger>().triggers.Add(event1);
        pickButton.GetComponent<EventTrigger>().triggers.Add(event2);
        pickButton.GetComponent<EventTrigger>().triggers.Add(event3);

        examineButton.AddComponent<EventTrigger>();
        examineButton.GetComponent<EventTrigger>().triggers.Add(event1);
        examineButton.GetComponent<EventTrigger>().triggers.Add(event2);
        examineButton.GetComponent<EventTrigger>().triggers.Add(event3);

        useButton.AddComponent<EventTrigger>();
        useButton.GetComponent<EventTrigger>().triggers.Add(event1);
        useButton.GetComponent<EventTrigger>().triggers.Add(event2);
        useButton.GetComponent<EventTrigger>().triggers.Add(event3);

        talkButton.AddComponent<EventTrigger>();
        talkButton.GetComponent<EventTrigger>().triggers.Add(event1);
        talkButton.GetComponent<EventTrigger>().triggers.Add(event2);
        talkButton.GetComponent<EventTrigger>().triggers.Add(event3);

        useOnButton.AddComponent<EventTrigger>();
        useOnButton.GetComponent<EventTrigger>().triggers.Add(event1);
        useOnButton.GetComponent<EventTrigger>().triggers.Add(event2);
        useOnButton.GetComponent<EventTrigger>().triggers.Add(event3);

        useOnNTButton.AddComponent<EventTrigger>();
        useOnNTButton.GetComponent<EventTrigger>().triggers.Add(event1);
        useOnNTButton.GetComponent<EventTrigger>().triggers.Add(event2);
        useOnNTButton.GetComponent<EventTrigger>().triggers.Add(event3);

        combineButton.AddComponent<EventTrigger>();
        combineButton.GetComponent<EventTrigger>().triggers.Add(event1);
        combineButton.GetComponent<EventTrigger>().triggers.Add(event2);
        combineButton.GetComponent<EventTrigger>().triggers.Add(event3);

        tutorialCombine = GameObject.FindObjectOfType<Tutorial_Combining>();
        tutorialUseOn = GameObject.FindObjectOfType<Tutorial_UseOn>();
    }

    public void Init(GameObject iObject)
    {
        if (cameraMode.CurrentMode != CameraMode.Mode.Free)
        {
            return;
        }

        if (player == null)
        {
            player = GameObject.FindObjectOfType<PlayerScript>();
        }

        if (PlayerAnimationManager.IsLongAnimation())
            return;
        initedObject = iObject;
        generalAction = actionManager.CheckGeneralAction();

        if (generalAction != null)
        {
            useOnNTButton.SetActive(true);
            useOnNTButton.transform.GetChild(0).GetComponent<Text>().text = generalAction.ButtonText;
        }

        if (initedObject != null && initedObject.GetComponent<InteractableObject>() != null)
        {
            if (player.itemControlsToInit == initedObject.name)
            {
                player.tutorial_itemControls = true;
            }

            cameraMode.ToggleCameraMode(CameraMode.Mode.ItemControlsUI);

            if (initedObject.GetComponent<PickableObject>() != null
                 && handsInventory.IsInHand(initedObject))
            {
                useOnButton.SetActive(true);
                useOnNTButton.SetActive(true);
                combineButton.SetActive(true);

                pickButton.SetActive(false);
                useButton.SetActive(false);
                talkButton.SetActive(false);

                examineButton.SetActive(initedObject.GetComponent<ExaminableObject>() != null);
            }
            else
            {
                if (initedObject.GetComponent<ExaminableObject>() != null)
                {
                    examineButton.SetActive(!initedObject.GetComponent<ExaminableObject>().animationExamine);
                }
                else
                {
                    examineButton.SetActive(false);
                }

                pickButton.SetActive(initedObject.GetComponent<PickableObject>() != null);
                useButton.SetActive(initedObject.GetComponent<UsableObject>() != null);

                talkButton.SetActive(initedObject.GetComponent<PersonObjectPart>() != null);
                if (talkButton.activeSelf)
                {
                    initedObject = initedObject.GetComponent<PersonObjectPart>().Person.gameObject;
                }

                useOnButton.SetActive(false);
                useOnNTButton.SetActive(false);
                combineButton.SetActive(false);
            }

            //talkin removed
            if (talkButton.activeSelf)
            {
                Talk();
            }
            else if ((pickButton.activeSelf && !examineButton.activeSelf)
                || (initedObject.GetComponent<PickableObject>() != null
                && initedObject.GetComponent<PickableObject>().sihlouette))
            {
                Pick();
            }
            else if (!pickButton.activeSelf && examineButton.activeSelf && !handsInventory.IsInHand(initedObject))
            {
                Examine();
            }
            else if (useButton.activeSelf)
            {
                Use();
            }
            else
            {

                transform.position = Input.mousePosition + new Vector3(cursorOffset.x, cursorOffset.y);
                gameObject.SetActive(true);

                GameObject descrGroup = GameObject.Find("ItemDescriptionGroup");

                float lowerBound = 330.0f;
                if (transform.position.y < lowerBound)
                {
                    float difference = lowerBound - transform.position.y;
                    transform.position += new Vector3(0.0f, difference, 0.0f);
                    if (descrGroup)
                    {
                        descrGroup.transform.position += new Vector3(0.0f, difference, 0.0f);
                    }
                }

                float rightBound = -125.0f;
                if (transform.position.x > Screen.width - rightBound)
                {
                    float difference = (Screen.width - rightBound) - transform.position.x;
                    transform.position += new Vector3(difference, 0.0f, 0.0f);
                    if (descrGroup)
                    {
                        descrGroup.transform.position += new Vector3(difference, 0.0f, 0.0f);
                    }
                }
                useOnNTButton.transform.GetChild(0).GetComponent<Text>().text =
                    (actionManager.CompareUseOnInfo(initedObject.name, "") ?
                    actionManager.CurrentButtonText(initedObject.name) : useOnNTtext);

                useButton.transform.GetChild(0).GetComponent<Text>().text =
                    (actionManager.CompareUseObject(initedObject.name)) ?
                    actionManager.CurrentButtonText(initedObject.name) : useText;

                useOnNTButton.SetActive(actionManager.CompareUseOnInfo(initedObject.name, ""));
                useButton.SetActive(actionManager.CompareUseObject(initedObject.name));

                discardButton.SetActive(false); // for now disable discard button

                initedObject.GetComponent<InteractableObject>().Reset();

                instantCloseFixFlag = true;
            }

            if (!pickButton.activeSelf &&
                !examineButton.activeSelf &&
                !useButton.activeSelf &&
                !talkButton.activeSelf &&
                !useOnButton.activeSelf &&
                !useOnNTButton.activeSelf &&
                !combineButton.activeSelf &&
                !discardButton.activeSelf)
            {
                Close();
            }

            if (oldInitDisabled)
            {
                // must be last, after all the checks in Init and with bypass=true
                Close(true);
            }
        }
    }

    public void Close(bool bypass = false)
    {
        if (instantCloseFixFlag && !bypass)
        {
            instantCloseFixFlag = false;
            return;
        }

        gameObject.SetActive(false);
        if (cameraMode.CurrentMode == CameraMode.Mode.ItemControlsUI)
        {
            cameraMode.ToggleCameraMode(CameraMode.Mode.Free);
        }
        UIhover = false;
    }

    public void Use()
    {
        if (initedObject.GetComponent<UsableObject>().PrefabToAppear != "")
        {
            initedObject.GetComponent<UsableObject>().Use();
        }
        else
        {
            if (cameraMode.CurrentMode == CameraMode.Mode.ItemControlsUI)
            {
                // specific case for "catheterisation" it doesnt need hands empty
                if ((handsInventory.Empty() || initedObject.name == "catheterisation") || initedObject.GetComponent<UsableObject>().UseWithObjectsInHands)
                {
                    initedObject.GetComponent<UsableObject>().Use();
                }
                else
                {
                    // no 'something in hands msg' for workfield if it's not a correct step
                    if (!(initedObject.name == "WorkField" && !actionManager.CompareUseObject("WorkField")))
                    {
                        string message = "Zorg ervoor dat alle materialen die je hebt gebruikt op het werkveld liggen. Maak je handen vrij door eventuele objecten terug te leggen op het werkveld.";
                        GameObject.FindObjectOfType<GameUI>().ShowBlockMessage("Materialen terugleggen.", message);
                    }
                }
            }
        }

        Close();
    }

    public void Examine()
    {
        if (cameraMode.CurrentMode == CameraMode.Mode.ItemControlsUI)
        {
            cameraMode.selectedObject = initedObject.GetComponent<ExaminableObject>();
            if (cameraMode.selectedObject != null) // if there is a component
            {
                if (tutorialUseOn != null)
                {
                    tutorialUseOn.examined = true;
                }
                cameraMode.selectedObject.OnExamine();
                controls.ResetObject();
            }
        }

        Close();
    }



    public void Pick()
    {
        if (cameraMode.CurrentMode == CameraMode.Mode.ItemControlsUI)
        {
            if (initedObject != null && player.robotSavedLeft != initedObject && player.robotSavedRight != initedObject)
            {
                PickableObject item = initedObject.GetComponent<PickableObject>();
                if (item != null)
                {
                    if (item.sihlouette == false)
                    {
                        if (tutorial == null ||
                            (tutorial != null &&
                                (item.name == tutorial.itemToPick ||
                                item.name == tutorial.itemToPick2)))
                        {
                            if (item.prefabInHands != "")
                            {
                                //item = ReplacePrefab(item, item.prefabInHands);
                            }

                            if (handsInventory.PickItem(item))
                            {
                                item.CreateGhostObject();
                            }
                        }
                    }
                    else if (item.sihlouette == true)
                    {
                        if (tutorial == null || (tutorial != null &&
                                (item.name == tutorial.itemToDrop ||
                                item.name == tutorial.itemToDrop2)))
                        {
                            GameObject ghost = item.gameObject;
                            initedObject = item.mainObject.gameObject;
                            Drop(ghost);
                        }
                    }
                    else
                    {
                        string message = "Volg de tips links bovenin het scherm om verder te gaan.";
                        GameObject.FindObjectOfType<GameUI>().ShowBlockMessage("Volg de tips!", message);
                    }

                    controls.ResetObject();
                }
            }
        }

        Close();
    }

    public void Talk()
    {
        ActionManager.personClicked = true;
        ActionManager.BuildRequirements();
        ActionManager.UpdateRequirements();

        if (cameraMode.CurrentMode == CameraMode.Mode.ItemControlsUI)
        {
            if (initedObject != null)
            {
                initedObject.GetComponent<PersonObject>().CreateSelectionDialogue();
                GameObject _itemDescription = GameObject.Find("ItemDescription");
                if (_itemDescription != null)
                    _itemDescription.SetActive(false);
            }
        }

        Close();
    }

    public void Combine()
    {
        if (tutorialCombine != null && !tutorialCombine.decombiningAllowed)
        {
            return;
        }

        if (tutorialUseOn != null && !tutorialUseOn.decombiningAllowed)
        {
            return;
        }

        if (handsInventory.LeftHandEmpty() ^ handsInventory.RightHandEmpty())
        {
            handsInventory.OnCombineAction();
        }

        Close();
    }

    public void Drop(GameObject ghost = null)
    {
        if (initedObject != null)
        {
            if (tutorial == null || (tutorial != null &&
            (tutorial.itemToDrop == initedObject.name ||
            tutorial.itemToDrop2 == initedObject.name)))
            {
                PickableObject item = initedObject.GetComponent<PickableObject>();

                if (handsInventory.LeftHandObject == initedObject)
                {
                    handsInventory.DropLeft(ghost);
                }
                else if (handsInventory.RightHandObject == initedObject)
                {
                    handsInventory.DropRight(ghost);
                }

                if (item != null)
                {
                    for (int i = item.ghostObjects.Count - 1; i >= 0; --i)
                    {
                        GameObject g = item.ghostObjects[i].gameObject;
                        item.ghostObjects.RemoveAt(i);
                        Destroy(g);
                    }
                }
            }
        }

        Close();
    }

    public void UseOn()
    {
        if (!(handsInventory.LeftHandEmpty() && handsInventory.RightHandEmpty()))
        {
            handsInventory.OnCombineAction();
        }

        Close();
    }  

    public void UseOnNoTarget()
    {
        if (tutorialUseOn != null && !tutorialUseOn.ventAllowed)
        {
            return;
        }

        if (initedObject == handsInventory.LeftHandObject)
        {
            handsInventory.LeftHandObject.GetComponent<PickableObject>().Use(true, true);

            if (tutorialUseOn != null)
            {
                handsInventory.LeftHandObject.GetComponent<PickableObject>().tutorial_usedOn = true;
            }
        }
        else
        {
            handsInventory.RightHandObject.GetComponent<PickableObject>().Use(false, true);

            if (tutorialUseOn != null)
            {
                handsInventory.RightHandObject.GetComponent<PickableObject>().tutorial_usedOn = true;
            }
        }

        Close();
    }
}
