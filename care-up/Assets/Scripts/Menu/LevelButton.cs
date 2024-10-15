using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CareUp.Localize;

public class LevelButton : MonoBehaviour
{
    public int buttonIndex = -1;
    private static LoadingScreen loadingScreen;
    public List<string> inGroups = new List<string>();
    [HideInInspector]
    public string bundleName;
    [HideInInspector]
    public string sceneName;
    [HideInInspector]
    public string sceneDescription;
    [HideInInspector]
    public string inHouseBundleName = "";
    [HideInInspector]
    public string inHouseSceneName = "";
    [HideInInspector]
    public bool toLoadInhouse = false;
    [HideInInspector]
    public string url;
    public GameObject IsFreeIcon;
    bool demoMark = false;
    //int demoMarkType = 0; value never used
    bool PreviewIconChanged = false;
    public GameObject MarksPanel;
    public bool isFree = false;

    List<Animation> marksAnimations = new List<Animation>();

    [HideInInspector]
    public int dificultateLevel = -1; // 0 = Video; 1 = with hints; 2 = no hints; 3 = no hints + complications; 4 = test mode
    [HideInInspector]
    public bool multiple;
    [HideInInspector]
    public string sceneID;
    [HideInInspector] public string courseID;
    [HideInInspector]
    public string displayName;
    public Sprite image;
    public bool testDisabled;
    public bool validated;
    public string totalPoints;
    public string xPoints;
    bool started = false;
    public Text titleText;
    [HideInInspector]
    public string[] isInProducts = new string[0];
    Image LevelPreview;
    private static Transform sceneInfoPanel = default(Transform);
    private static PlayerPrefsManager manager;

    private static Transform leaderboard;
    private static Transform scores;
    private static Transform names;
    public bool demoLock = true;
    public Toggle AutoPlayToggle;
    public Toggle AutoPlayToggle2;
    public Text AutoPlayNum;
    public Text AutoPlayNum2;

    float scoreTimeout = 0.0f;
    public Text descriptionText;
    List<GameObject> frameElements = new List<GameObject>();

    List<bool> levelComplitionList = new List<bool>();
    // saving info
    public struct Info
    {
        public string bundleName;
        public string sceneName;
        public string displayName;
        public string description;
        public string result;
        public Sprite image;
        public bool testDisabled;
        public bool validated;
        public string totalPoints;
    };
    
    //--------------------------
    public void SetLockState(bool _lock)
    {
        demoLock = _lock;
        UpdateButtonLockState();
    }

    public void SetDemoMark(bool _mark)
    {
        demoMark = _mark;
    }

    public void UpdateButtonLockState()
    { 
        if (LevelPreview == null)
            LevelPreview = transform.Find("LevelPreview").GetComponent<Image>();
        bool toLock = demoLock;

        if (demoLock && isInProducts.Length > 0)
            if (PlayerPrefsManager.IsScenePurchased(isInProducts))
                toLock = false;

        if (toLock)
        {
            GetComponent<Image>().sprite = Resources.Load("Sprites/nUI/listElement_Base_gray", typeof(Sprite)) as Sprite;
            LevelPreview.sprite = Resources.Load("Sprites/btn_icon_lock", typeof(Sprite)) as Sprite;
            LevelPreview.gameObject.SetActive(true);
        }
        else
        {
            GetComponent<Image>().sprite = Resources.Load("Sprites/nUI/listElement_Base", typeof(Sprite)) as Sprite;
            if (!PreviewIconChanged)
                LevelPreview.gameObject.SetActive(false);
        }
     
    }

    public void ShowDemoMarks(int demoMarkType)
    {
        if (frameElements.Count == 0)
            ListFrameElements();
        for (int i = 0; i < frameElements.Count; i++)
        {
            frameElements[i].SetActive(demoMarkType == i);
        }
    }

    public void SetLevelPreviewIcon(bool iconToShow, Sprite newIcon)
    {
        if (LevelPreview == null)
            LevelPreview = transform.Find("LevelPreview").GetComponent<Image>();
        LevelPreview.gameObject.SetActive(LevelPreview);
        LevelPreview.sprite = newIcon;
        PreviewIconChanged = true;
    }

    public List<Info> variations = new List<Info>();

    public bool buy = false;


    void ListFrameElements()
    {
        frameElements.Add(transform.Find("GFrame_Full").gameObject);
        frameElements.Add(transform.Find("GFrame_Top").gameObject);
        frameElements.Add(transform.Find("GFrame_Mid").gameObject);
        frameElements.Add(transform.Find("GFrame_Bot").gameObject);
    }

    void OnEnable()
    {
        UpdateAutoPlayToggle();
        UpdateScoreMarks();
    }

    void UpdateScoreMarks()
    {
        if (marksAnimations.Count <= 0)
        {
            scoreTimeout = 0.3f;
            return;
        }
        levelComplitionList.Clear();
        for (int i = 0; i < 5; i++)
            levelComplitionList.Add(DatabaseManager.GetSceneCompletion(displayName, i));

        for (int i = 0; i < marksAnimations.Count; i++)
        { 
            Animation m = marksAnimations[i];
            
            if (levelComplitionList.Count > i)
            {
                if (levelComplitionList[i])
                    m.Play("twistOn");
                else
                    m.Play("twistOff");
            }
            else
            {
                m.Play("twistOff");
            }
        }
    }

    private void Start()
    {
        ListFrameElements();
        
        for (int i = 0; i < MarksPanel.transform.childCount; i++)
        {
            marksAnimations.Add(MarksPanel.transform.GetChild(i).GetComponent<Animation>());
        }
        scoreTimeout = 0.2f;

        LevelPreview = transform.Find("LevelPreview").GetComponent<Image>();
        if (!PlayerPrefsManager.simulatePlayerActions)
        {
            AutoPlayToggle.gameObject.SetActive(false);
            AutoPlayToggle2.gameObject.SetActive(false);
        }
        else
        {
            if(inHouseSceneName == "")
                AutoPlayToggle2.gameObject.SetActive(false);
        }
        if (!PlayerPrefsManager.GetDevMode())
        {
            AutoPlayToggle.gameObject.SetActive(false);
            AutoPlayToggle2.gameObject.SetActive(false);
        }


        if (GameObject.Find("Preferences") != null && loadingScreen == null)
        {
            loadingScreen = GameObject.Find("Preferences").GetComponent<LoadingScreen>();
            if (loadingScreen == null) Debug.LogError("No loading screen found");
        }

        if (manager == null)
        {
            if (GameObject.Find("Preferences") != null)
            {
                manager = GameObject.Find("Preferences").GetComponent<PlayerPrefsManager>();
                if (manager == null) Debug.LogWarning("No prefs manager ( start from 1st scene? )");
            }
            else
            {
                Debug.LogWarning("No prefs manager ( start from 1st scene? )");
            }
        }

        //@
        titleText.text = LocalizationManager.GetValueIfKey(displayName);
        //@
        descriptionText.text = LocalizationManager.GetValueIfKey(sceneDescription);

        InGameLocalEditTool inGameLocalEditTool = GameObject.FindAnyObjectByType<InGameLocalEditTool>();
        if (inGameLocalEditTool != null)
        {
            //!
            inGameLocalEditTool.AddUILocalizationComponentToGO(titleText.gameObject, displayName);
            //!
            inGameLocalEditTool.AddUILocalizationComponentToGO(descriptionText.gameObject, sceneDescription);

        }

        transform.Find("Points").GetComponent<Text>().text = xPoints;
        Text pointsLabel = transform.Find("PointsLabel").GetComponent<Text>();
        if (xPoints == "1")
            pointsLabel.text = "Point";
        else
            pointsLabel.text = "Points";
        started = true;
    }

    public void AutoPlayStateChanged(int locationID = 0)
    {
        if (started)
        {
            string _sceneName = sceneName;
            string _bundleName = bundleName;
            bool _toggle = AutoPlayToggle.isOn;

            if (locationID == 1)
            {
                _sceneName = inHouseSceneName;
                _bundleName = inHouseBundleName;
                _toggle = AutoPlayToggle2.isOn;
            }
            GameObject.FindObjectOfType<AutoPlayer>().AddSceneToList(_sceneName, _bundleName, _toggle);
            foreach(LevelButton levelButton in GameObject.FindObjectsOfType<LevelButton>())
            {
                levelButton.UpdateAutoPlayToggle();
            }
        }
    }

    public void OnHover()
    {
        if (PlayerPrefsManager.GetDevMode())
            GameObject.FindObjectOfType<LevelSelectionScene_UI>().debugSS = sceneName;
    }


    public void OnExit()
    {
        if (PlayerPrefsManager.GetDevMode())
            GameObject.FindObjectOfType<LevelSelectionScene_UI>().debugSS = "";
    }

    public void UpdateAutoPlayToggle()
    {
        AutoPlayNum.text = "";
        AutoPlayNum2.text = "";

        if (!PlayerPrefsManager.simulatePlayerActions)
        {
            AutoPlayToggle.gameObject.SetActive(false);
            AutoPlayToggle2.gameObject.SetActive(false);
        }
        else
        {
            AutoPlayToggle.gameObject.SetActive(true);
            if (inHouseSceneName != "")
                AutoPlayToggle2.gameObject.SetActive(true);

            int autoPlayNumValue = GameObject.FindObjectOfType<AutoPlayer>().IsSceneInList(sceneName);
            int autoPlayNumValue2 = GameObject.FindObjectOfType<AutoPlayer>().IsSceneInList(inHouseSceneName);

            AutoPlayToggle.isOn = autoPlayNumValue != -1;
            AutoPlayToggle2.isOn = autoPlayNumValue2 != -1;
            if (autoPlayNumValue >= 0)
                AutoPlayNum.text = (autoPlayNumValue + 1).ToString();
            if (autoPlayNumValue2 >= 0)
                AutoPlayNum2.text = (autoPlayNumValue2 + 1).ToString();
        }
    }


    public void OnLevelButtonClick()
    {
        MainMenu.storeUrl = url;
        GameObject.FindObjectOfType<MainMenuAutomationData>().SetCurrentLevelButtonID(buttonIndex);
        bool locked = (buy || demoLock);
        if (locked && isInProducts.Length > 0)
            if (PlayerPrefsManager.IsScenePurchased(isInProducts))
                locked = false;
        if (locked)
        {
            // show dialogue now instead
            GameObject.FindObjectOfType<UMP_Manager>().ShowDialog(5);

            // for apple, if scene is locked, setup the purchase panel to trigger the shop with correct scene ID
            if (manager != null)
            {
                manager.currentPEcourseID = sceneID;
                manager.currentPEAccreditationCourseID = courseID;
            }
        }
        else
        {
            LevelButton mainBtn = GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/DialogTestPractice/Panel_UI/Buttons/Start").GetComponent<LevelButton>();

            if (multiple)//??????????????????????????????????
            {
                // we need to fill info in the dialogue
                GameObject dialogue = GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/Dialog 1");

                // setting title i assume
                //@
                dialogue.transform.Find("Content/Panel_UI/Top/Title").GetComponent<Text>().text = 
                    LocalizationManager.GetValueIfKey(displayName);
                
                
                InGameLocalEditTool inGameLocalEditTool = GameObject.FindAnyObjectByType<InGameLocalEditTool>();
                if (inGameLocalEditTool != null)
                {
                    //!
                    inGameLocalEditTool.AddUILocalizationComponentToGO(
                        dialogue.transform.Find("Content/Panel_UI/Top/Title").gameObject, displayName);
                }

                if (manager != null)
                {
                    manager.currentSceneVisualName = displayName;
                    manager.currentPEcourseID = sceneID;
                    manager.currentPEAccreditationCourseID = courseID;
                    manager.validatedScene = validated;
                    manager.currentSceneXPoints = xPoints;

                }

                // filling up options
                for (int i = 0; i < variations.Count; ++i)
                {
                    LevelSelectionScene_UI_Option option =
                        dialogue.transform.Find("Content/Buttons/Option_" + (i + 1)).GetComponent<LevelSelectionScene_UI_Option>();
                    option.bundleName = variations[i].bundleName;
                    option.sceneName = variations[i].sceneName;
                    option.transform.GetComponentInChildren<Text>().text = LocalizationManager.GetValueIfKey(variations[i].displayName);
                    
                    if (inGameLocalEditTool != null)
                    {
                        inGameLocalEditTool.AddUILocalizationComponentToGO(
                            option.transform.GetComponentInChildren<Text>().gameObject, displayName);
                    }

                    if (i == 0)
                    {
                        // set 1st option as default
                        option.SetSelected();
                    }
                }

                // we need to show this dialogue only for scenes with variations
                GameObject.FindObjectOfType<UMP_Manager>().ShowDialog(0);
            }
            else
            {
                // filling info for loading
                mainBtn.bundleName = bundleName;
                mainBtn.sceneName = sceneName;

                mainBtn.inHouseBundleName = inHouseBundleName;
                mainBtn.inHouseSceneName = inHouseSceneName;
                mainBtn.toLoadInhouse = false;
                mainBtn.dificultateLevel = -1;
                // for single variation we can skip into practice/test dialogue
                //if (inHouseSceneName != "")
                //    GameObject.FindObjectOfType<UMP_Manager>().ShowDialog(11);
                //else
                //GameObject.FindObjectOfType<UMP_Manager>().ShowDialog(3);
                GameObject.FindObjectOfType<UMP_Manager>().ShowDialogByName("DialogLevelSelect");

                if (manager != null)
                {
                    manager.currentSceneVisualName = displayName;
                    manager.currentPEcourseID = sceneID;
                    manager.currentPEAccreditationCourseID = courseID;
                    manager.validatedScene = validated;
                    manager.currentSceneXPoints = xPoints;
                }
            }

            // maybe disable test button
            //GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/DialogTestPractice/Panel_UI/TestButton")
            //    .GetComponent<Button>().interactable = !testDisabled;

            //making button not interactable was not noticable (maybe change design), hiding instead
            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/DialogTestPractice/Panel_UI/Buttons/right/TestButton")
                .SetActive(!testDisabled);

            // now we can fetch practice plays number in order to know whethere to hide or show test button
            // making test button inactive from the beginning before fetching
            Button testBtn = GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
            "DialogTestPractice/Panel_UI/Buttons/right/TestButton").GetComponent<Button>();

            string formattedSceneName = PlayerPrefsManager.FormatSceneName(manager.GetSceneDatabaseName(manager.currentSceneVisualName));

            int practicePlays;
            int.TryParse(DatabaseManager.FetchField("PracticePlays", formattedSceneName), out practicePlays);
            manager.currentPracticePlays = practicePlays;
            DialogLevelSelect dls = GameObject.FindObjectOfType<DialogLevelSelect>();
            if (dls != null)
            {
                dls.timeoutValue = 0.5f;
            }
            //testBtn.interactable = practicePlays >= 1;

            //GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
            //        "DialogTestPractice/Panel_UI/Buttons/right/TestButton/contentlocked/practiceamount")
            //    .GetComponent<Text>().text = (1 - practicePlays).ToString() + " keer";

            if (testBtn.interactable)
            {
                float testHighscore;
                float.TryParse(DatabaseManager.FetchField("TestHighscores",
                    formattedSceneName).Replace(",", "."), out testHighscore);
                GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                    "DialogTestPractice/Panel_UI/Buttons/right/TestButton/contentunlocked/percentage")
                    .GetComponent<Text>().text = Mathf.RoundToInt(testHighscore).ToString() + "%";
            }

            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                "DialogTestPractice/Panel_UI/Buttons/right/TestButton/contentunlocked").SetActive(testBtn.interactable);
            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                "DialogTestPractice/Panel_UI/Buttons/right/TestButton/contentlocked").SetActive(!testBtn.interactable);

            int practiceHighscore, practiceStars;
            int.TryParse(DatabaseManager.FetchField("PracticeHighscores", "score_" + formattedSceneName), out practiceHighscore);
            int.TryParse(DatabaseManager.FetchField("PracticeHighscores", "stars_" + formattedSceneName), out practiceStars);

            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                "DialogTestPractice/Panel_UI/Buttons/left/PracticeButton/content/score").
            GetComponent<Text>().text = practiceHighscore.ToString();

            Sprite grey = Resources.Load<Sprite>("Sprites/Stars/star 1");
            Sprite gold = Resources.Load<Sprite>("Sprites/Stars/star_128x128px");

            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                    "DialogTestPractice/Panel_UI/Buttons/left/PracticeButton/content/Stars/Star1")
                .GetComponent<Image>().sprite = (practiceStars >= 1.0f) ? gold : grey;

            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                    "DialogTestPractice/Panel_UI/Buttons/left/PracticeButton/content/Stars/Star2")
                .GetComponent<Image>().sprite = (practiceStars >= 2.0f) ? gold : grey;

            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                    "DialogTestPractice/Panel_UI/Buttons/left/PracticeButton/content/Stars/Star3")
                .GetComponent<Image>().sprite = (practiceStars >= 3.0f) ? gold : grey;

            SetPointsAmount();
        }
    
    
    
    }

    public void OnStartButtonClick()
    {
        PlayerPrefsManager.AddOneToPlaysNumber();

        //if (manager.practiceMode)
        //{
        //    PlayerPrefsManager.AddOneToPracticePlays(manager.currentSceneVisualName);
        //}
        //else
        //{
        //    PlayerPrefsManager.AddOneToTestPlays(manager.currentSceneVisualName);
        //}
        if (toLoadInhouse)
            bl_SceneLoaderUtils.GetLoader.LoadLevel(inHouseSceneName, inHouseBundleName);
        else
            bl_SceneLoaderUtils.GetLoader.LoadLevel(sceneName, bundleName);

    }

    public void GetSceneDatabaseInfo_Success(string[] info)
    {
        if (info.Length > 1)
        {
            int iTime;
            string time = (int.TryParse(info[2], out iTime)) ? string.Format("Tijd: {0}m{1:00}s", iTime / 60, iTime % 60) : "";
            string text = " Score: " + info[1] + "  - " + time;
            sceneInfoPanel.Find("Result").GetComponent<Text>().text = text;
        }
        else
        {
            sceneInfoPanel.Find("Result").GetComponent<Text>().text = " Niet voltooid";
        }
    }

    public void UpdateHighScore()
    {
        manager.GetSceneDatabaseInfo(sceneName, GetSceneDatabaseInfo_Success);
    }

    public void SetPointsAmount()
    {
        Text points = GameObject.Find("/UMenuProManager/MenuCanvas/Dialogs/DialogLevelSelect/Panel_UI/PointsAmount/Text").GetComponent<Text>();
        points.text = "   ";
        //points.text = validated ? "Te behalen accreditatiepunten: " + totalPoints : "";
    }

    private void Update()
    {
        if (scoreTimeout > 0f)
        {
            scoreTimeout -= Time.deltaTime;
            if (scoreTimeout <= 0f)
            {
                UpdateScoreMarks();
            }
        }
    }
}
