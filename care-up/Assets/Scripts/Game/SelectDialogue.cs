using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using CareUp.Localize;
/// <summary>
/// Instance of dialogue with up to 4 options.
/// </summary>
public class SelectDialogue : MonoBehaviour
{
    public bool tutorial_lock = false;
    public bool cheated = false;
    int correctAnswerID = -1;
    GameUI gameUI;
    public GameObject selectionDialogueElements;
    public GameObject prevStepInfoElements;
    public Button prevStepInfoButton;
    DialogueOption prevStepInfo = null;

    public class DialogueOption
    {
        public delegate void OptionAction(string attr = "", List<DialogueOption> additionalOption = null, string question = null, string audio = "");

        public string text;
        public string attribute;
        public string question;
        public string audio;
        public OptionAction function;
        public OptionSide side;
        public List<DialogueOption> additional;

        public DialogueOption(string txt, OptionAction func, string attr, string aud, List<DialogueOption> additionalOptions = null, string questionText = null)
        {
            text = txt;
            function = func;
            attribute = attr;
            audio = aud;
            question = questionText;
            additional = additionalOptions;
        }
    };

    public void ShowPrevStepInfo(bool toShow = true)
    {
        selectionDialogueElements.SetActive(!toShow);
        prevStepInfoElements.SetActive(toShow);
    }
    public enum OptionSide
    {
        None,
        Top,
        Bottom,
        Right,
        Left
    };

    private List<DialogueOption> options = new List<DialogueOption>();
    //private OptionSide currentOption = OptionSide.None;
    private Color currentMaterial;
    public List<Button> sqButtons;
    private string text;
    private static CameraMode cameraMode;
    private static Controls controls;
    //private bool optionSelected = false;

    static DialogueOption optionWithAdditions = null;
    static string questionWithHint = null;

    void Start()
    {
        if (PlayerPrefsManager.videoRecordingMode)
            GetComponent<CanvasGroup>().alpha = 0f;
        if (cameraMode == null)
        {
            cameraMode = GameObject.Find("GameLogic").GetComponent<CameraMode>();
            if (cameraMode == null) Debug.LogError("No camera mode found");
        }
        if (controls == null)
        {
            controls = GameObject.Find("GameLogic").GetComponent<Controls>();
            if (controls == null) Debug.LogError("No controls found.");
        }
    }

    public void AddPrevStepInfo(List<DialogueOption> list)
    {
        foreach (DialogueOption o in list)
        {
            if (o.attribute != "" && o.attribute != "CM_Leave")
            {
                prevStepInfo = o;
            }
        }
        if (prevStepInfo != null)
        {
            //@
            prevStepInfoElements.transform.Find("Buttons/b0/Text").GetComponent<Text>().text = 
                LocalizationManager.GetValueIfKey(prevStepInfo.text);
            prevStepInfoButton.gameObject.SetActive(true);

            InGameLocalEditTool inGameLocalEditTool = PlayerPrefsManager.GetDevMode() ? GameObject.FindObjectOfType<InGameLocalEditTool>() : null;  
            if (inGameLocalEditTool != null)
            {
                //!
                inGameLocalEditTool.AddUILocalizationComponentToGO(
                    prevStepInfoElements.transform.Find("Buttons/b0/Text").gameObject,
                    prevStepInfo.text);
            }
        }
        else
            prevStepInfoButton.gameObject.SetActive(false);

    }

    public void AddOptions(List<DialogueOption> list, bool cheat = false)
    {
        foreach (DialogueOption item in list)
        {
            options.Add(item);
        }
        InitOptions();
        cheated = cheat;
    }

    /// <summary>
    /// Sets options sides based on how many are there.
    /// </summary>
    private void InitOptions()
    {
        if (gameUI == null)
            gameUI = GameObject.FindObjectOfType<GameUI>();
        if (options.Count == 0)
        {
            Debug.LogError("0 options inited.");
            return;
        }

        InGameLocalEditTool inGameLocalEditTool = PlayerPrefsManager.GetDevMode() ? GameObject.FindObjectOfType<InGameLocalEditTool>() : null;  

        for (int i = 0; i < 4; i++)
        {
            if (i < options.Count)
            {
                sqButtons[i].gameObject.SetActive(true);
                string cheatSimbol = "";

                if (PlayerPrefsManager.GetDevMode())
                {
                    ObjectsIDsController objectsIDsController = GameObject.FindAnyObjectByType<ObjectsIDsController>();
                    if (objectsIDsController != null && objectsIDsController.cheat)
                    {
                        if (options[i].attribute != "" && options[i].attribute != "CM_Leave")
                            cheatSimbol = "@";
                    }
                }
                //@
                sqButtons[i].transform.Find("Text").GetComponent<Text>().text = cheatSimbol + 
                    LocalizationManager.GetValueIfKey(options[i].text);

                if (inGameLocalEditTool != null)
                {
                    //!
                    inGameLocalEditTool.AddUILocalizationComponentToGO(
                        sqButtons[i].transform.Find("Text").gameObject,
                        options[i].text);
                }

                if (gameUI.AllowAutoPlay(false))
                    if (options[i].attribute != "" && options[i].attribute != "CM_Leave")
                    {
                        correctAnswerID = i;
                        Invoke("AutoPlay", 3f);
                    }
            }
            else
                sqButtons[i].gameObject.SetActive(false);
        }
     
        //ShowAnswer();
    }

    void AutoPlay()
    {
        if (correctAnswerID >= 0)
        {
            if (sqButtons[correctAnswerID].gameObject.activeSelf)
                sqButtons[correctAnswerID].GetComponent<Button>().onClick.Invoke();
        }
    }

    public void SetText(string t)
    {
        text = t;
    }

    public void ShowAnswer()
    {
        int i = 0;
        foreach (DialogueOption o in options)
        {
            //print(o.attribute);
            if (o.attribute != "" && o.attribute != "CM_Leave")
            {
                sqButtons[i].GetComponent<Image>().color = Color.yellow;
            }
            i++;
        }
    }

    public void ButtonClick(int num)
    {
        sqButtons[num].interactable = false;

        options[num].function(options[num].attribute, options[num].additional, options[num].question, options[num].audio);

        if (options[num].attribute != "")
        {
            Destroy(gameObject);
            cameraMode.ToggleCameraMode(CameraMode.Mode.Free);
        }
        else
        {
            ColorBlock newColorBlock = sqButtons[num].colors;
            newColorBlock.disabledColor = new Color(1f,0f,0f, 0.4f);
            sqButtons[num].colors = newColorBlock;

            GameObject.FindObjectOfType<ActionManager>().OnSequenceStepAction("");
            GameObject currentHintPanel = GameObject.Find("HintPanel");

            if (ActionManager.practiceMode && currentHintPanel != null)
            {
                string hintText = FindObjectOfType<ActionManager>().CurrentDescription[0];

                foreach (DialogueOption dialoqueOption in options)
                {
                    if (dialoqueOption.additional != null)
                    {
                        optionWithAdditions = dialoqueOption;
                        hintText = dialoqueOption.text;
                        break;
                    }
                }

                if (currentHintPanel.transform.Find("Text") != null)
                {
                    Text hint = currentHintPanel.transform.Find("Text").gameObject.GetComponent<Text>();

                    if (optionWithAdditions != null)
                    {
                        if (hint.text == optionWithAdditions.question || hint.text == questionWithHint)
                        {
                            questionWithHint = optionWithAdditions.question + " " + FindObjectOfType<ActionManager>().CurrentDescription[0];
                            hint.text = questionWithHint;
                        }
                        else if (options[num] == optionWithAdditions)
                            hint.text = optionWithAdditions.question;
                    }
                    else
                        hint.text = hintText;
                }
            }
        }
    }
}
