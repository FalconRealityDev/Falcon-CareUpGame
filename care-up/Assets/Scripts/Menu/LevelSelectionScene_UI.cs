using UnityEngine;
using System.Xml;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using MBS;
using CareUpAvatar;
using CareUp.Localize;


public class SceneInfo
{
    public string sceneID = "";
    public string courseID = "";
    public string sceneType = "";
    public bool activated = true;
    public bool demoLock = false;
    public bool hidden = false;
    public string[] isInProducts = null;
    public string bundleName = "";
    public string displayName = "";
    public string description = "";
    public string sceneName = "";
    public string image = "";
    public bool validated = false;
    public string totalPoints = "0";
    public string mainScene = "";
    public bool multiple = false;
    public bool testDisabled = false;
    public string inHouseBundleName = "";
    public string inHouseSceneName = "";
    public string url = "";
    public bool freeCert = false;
    public string nameForDatabase = "";
    public bool hasVideoMode = false;
    public string videoURL = "";
    public string xPoints = "1";
    public List<string> inGroups = new List<string>();
}

/// <summary>
/// Handles Scene selection module
/// </summary>
/// 
public class LevelSelectionScene_UI : MonoBehaviour
{
    public string debugSS;
    public List<GameObject> rankButtons;
    public GameObject leaderBoardParticipatePanel;
    public InputField leaderBoardNameInput;
    public InputField leaderBoardNameInput2;

    public GameObject leaderBoardInfoButton;
    public GameObject leaderBoardNewNamePanel;
    private PlayerPrefsManager ppManager;
    public GameObject LeaderBoardInfoTabPanel;
    float initTime = 0f;
    bool sceneButtonsUpdated = false;
    Dictionary<string, int> sceneGroupNum = new Dictionary<string, int>();
    // leaderboard stuff
    public ScoreLine[] _Scores;
    public GameObject scoreLines;

    public List<Transform> variations;

    private Sprite completedSceneIcon;
    public List<GameObject> LBInfoTabs;
    public List<Button> LBInfoTabButtons;

    public void UpdateScenePurchases()
    {
        WPServer.RequestPurchases(WULogin.UID);
    }

    public void RefrashSceneSelectionButtons()
    {
        foreach(LevelButton levelButton in GameObject.FindObjectsOfType<LevelButton>())
        {
            levelButton.UpdateButtonLockState();
        }
    }

    public void SelectRank(int r)
    {
        for (int i = 0; i < rankButtons.Count; i++)
        {
            rankButtons[i].transform.Find("Button").GetComponent<Button>().interactable = i != r;
            rankButtons[i].transform.Find("WCircle").GetComponent<Image>().enabled = i == r;
        }
    }
    public int GetSceneGroupNum(string _groupID)
    {
        int value = 0;
        if (sceneGroupNum.ContainsKey(_groupID))
            value = sceneGroupNum[_groupID];
        return value;
    }

    public void ToggleLBTabs()
    {
        if (LeaderBoardInfoTabPanel.gameObject.activeSelf)
        {
            SwitchLBTab(-2);
        }
        else
        {
            SwitchLBTab(0);
        }
    }

    public void SwitchLBTab(int tabID)
    {
        if (tabID == -2)
        {
            LeaderBoardInfoTabPanel.SetActive(false);
            return;
        }

        LeaderBoardInfoTabPanel.SetActive(true);
        for (int i = 0; i < LBInfoTabs.Count(); i++)
        {
            LBInfoTabs[i].SetActive(tabID == i);
            LBInfoTabButtons[i].interactable = tabID != i;
        }
        if (tabID == 2)
        {
            leaderBoardNameInput2.text = leaderBoardNameInput.text = DatabaseManager.LeaderboardName;
        }

    }
    private void Awake()
    {
        Transform leaderPanel = GameObject.Find("UMenuProManager/MenuCanvas/LayoutPanel/Tabs/Leaderboard").transform;
        _Scores = scoreLines.GetComponentsInChildren<ScoreLine>();

        for (int i = 0; i < _Scores.Length; i++)
        {
            _Scores[i].SetScoreLine("", "", i);
        }

        // variations buttons should be disabled from the beginning
        //for (int i = 0; i < 3; ++i)
        //{
        //    Transform v = leaderPanel.parent.Find("InfoBar/menu/d" + (i + 1));
        //    variations.Add(v);
        //    v.gameObject.SetActive(false);
        //}
    }

    /// <summary>
    /// Load xml file, set all door variables.
    /// </summary>
    void Start()
    {
        initTime = Time.realtimeSinceStartup;
        ppManager = GameObject.FindObjectOfType<PlayerPrefsManager>();
        completedSceneIcon = Resources.Load<Sprite>("Sprites/check_circle_on");

        UpdateSceneUI();
    }

    private void Update()
    {
        if (!sceneButtonsUpdated)
        {
            if (Time.realtimeSinceStartup - initTime > 5f)
            {
                sceneButtonsUpdated = true;
                RefrashSceneSelectionButtons();
                Debug.Log("Updated menu");
            }
        }
    }

    void ClearUI()
    {
        Transform parent1 = GameObject.Find("UMenuProManager/MenuCanvas/Play/InfoHolder/ProtocolList/ProtocolsHolder/Protocols/content").transform;
        for (int i = 0; i < parent1.childCount; ++i)
        {
            Destroy(parent1.GetChild(i).gameObject);
        }

        Transform parent2 = GameObject.Find("UMenuProManager/MenuCanvas/Leaderboard/InfoHolder/ProtocolsHolder/Scroll View/Viewport/Content").transform;
        for (int i = 0; i < parent1.childCount; ++i)
        {
            Destroy(parent2.GetChild(i).gameObject);
        }
    }

    void OnGUI()
    {
        if (PlayerPrefsManager.GetDevMode())
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = new Color(0.3f, 1f, 0.3f);
            style.fontSize = 20;
            GUI.Label(new Rect(30, 10, 1000, 100), debugSS, style);
        }
    }

    public void ReinitializeUI()
    {      
        ClearUI();
        UpdateSceneUI();           
    }

    void UpdateSceneUI()
    {
       
        // load xml
        TextAsset textAsset;
        PlayerPrefsManager pp = GameObject.FindObjectOfType<PlayerPrefsManager>();

        textAsset = (TextAsset)Resources.Load("Xml/Scenes");

        XmlDocument xmlFile = new XmlDocument();
        xmlFile.LoadXml(textAsset.text);
        XmlNodeList xmlSceneList = xmlFile.FirstChild.NextSibling.FirstChild.ChildNodes;

        // leaderboard stuff
        //bool firstScene = true; value never used
        LeaderBoardSceneButton.buttons.Clear();
// FindObjectOfType<PlayerPrefsManager>().demoVersion

        Transform protocolsTransorm = GameObject.Find("/UMenuProManager/MenuCanvas/LayoutPanel/Tabs/PlayGroups/ContentPanel/Scenes/ContentPanel/PlayElements/ProtocolPanel/Panel/ProtocolList/ProtocolsHolder/Protocols/content").transform;
        pp.ClearFreeCertList();
        pp.ClearScenesInfo();
        Dictionary<string, SceneInfo> scenesInfo = new Dictionary<string, SceneInfo>();


        //Load data for scenes
        foreach (XmlNode xmlSceneNode in xmlSceneList)
        {
            SceneInfo sceneInfo = new SceneInfo();

            // bool activated = PlayerPrefs.GetInt(xmlSceneNode.Attributes["id"].Value + " activated") == 1;
            sceneInfo.activated = true;

            if (xmlSceneNode.Attributes["hasVideoMode"] != null)
                sceneInfo.hasVideoMode = xmlSceneNode.Attributes["hasVideoMode"].Value == "true";

            if (xmlSceneNode.Attributes["videoURL"] != null)
                sceneInfo.videoURL = xmlSceneNode.Attributes["videoURL"].Value;

            if (xmlSceneNode.Attributes["inGroups"] != null)
            {
                string[] _groups = xmlSceneNode.Attributes["inGroups"].Value.Split(',');
                if (_groups.Length > 0)
                {
                    foreach(string g in _groups)
                        sceneInfo.inGroups.Add(g);
                }
            }
            else
            {
                sceneInfo.inGroups.Add("o");
            }


            if (xmlSceneNode.Attributes["id"] != null)
                sceneInfo.sceneID = xmlSceneNode.Attributes["id"].Value;

            if (xmlSceneNode.Attributes["courseID"] != null)
                sceneInfo.courseID = xmlSceneNode.Attributes["courseID"].Value;

            if (xmlSceneNode.Attributes["type"] != null)
                sceneInfo.sceneType = xmlSceneNode.Attributes["type"].Value;

            sceneInfo.freeCert = false;
            if (xmlSceneNode.Attributes["freeCertificate"] != null)
                sceneInfo.freeCert = (xmlSceneNode.Attributes["freeCertificate"].Value == "true");
            if (xmlSceneNode.Attributes["name"] != null)
            {
                sceneInfo.displayName = xmlSceneNode.Attributes["name"].Value;
                if (sceneInfo.freeCert)
                    pp.AddFreeCertScene(sceneInfo.displayName);
            }

            sceneInfo.sceneName = xmlSceneNode.Attributes["sceneName"].Value;

            if (xmlSceneNode.Attributes["hidden"] != null)
                sceneInfo.hidden = xmlSceneNode.Attributes["hidden"].Value == "true";

            sceneInfo.demoLock = !(xmlSceneNode.Attributes["demo"] != null);
            if (!sceneInfo.demoLock)
            {
                sceneInfo.inGroups.Add("f");
            }

            if (xmlSceneNode.Attributes["isInProducts"] != null)
            {
                sceneInfo.isInProducts = xmlSceneNode.Attributes["isInProducts"].Value.Split('|');
            }
            if (xmlSceneNode.Attributes["nameForDatabase"] != null)
            {
                sceneInfo.nameForDatabase = xmlSceneNode.Attributes["nameForDatabase"].Value;
            }

            sceneInfo.bundleName = xmlSceneNode.Attributes["bundleName"].Value;

            if (xmlSceneNode.Attributes["description"] != null)
                sceneInfo.description = xmlSceneNode.Attributes["description"].Value;

            /* setting image
            if (xmlSceneNode.Attributes["image"] != null)
            {
                sceneUnit.image = Resources.Load<Sprite>("Sprites/ScenePreview/" + xmlSceneNode.Attributes["image"].Value);
                sceneUnit.transform.Find("LevelPreview").GetComponent<Image>().sprite = sceneUnit.image;
            } */

            if (xmlSceneNode.Attributes["validated"] != null)
                sceneInfo.validated = xmlSceneNode.Attributes["validated"].Value == "true";
            if (xmlSceneNode.Attributes["totalPoints"] != null)
                sceneInfo.totalPoints = xmlSceneNode.Attributes["totalPoints"].Value;

            if (xmlSceneNode.Attributes["test"] != null)
                sceneInfo.testDisabled = xmlSceneNode.Attributes["test"].Value == "disabled";

            if (xmlSceneNode.Attributes["mainScene"] != null)
                sceneInfo.mainScene = xmlSceneNode.Attributes["mainScene"].Value;
            string sceneType = "";
            if (xmlSceneNode.Attributes["type"] != null)
                sceneType = xmlSceneNode.Attributes["type"].Value;

            if (xmlSceneNode.Attributes["url"] != null)
                sceneInfo.url = xmlSceneNode.Attributes["url"].Value;

            if (!scenesInfo.ContainsKey(sceneInfo.sceneName) && sceneInfo.mainScene == "")
            {
                //Show only scenes and scene groups, that are available for current player
#if UNITY_IOS || UNITY_STANDALONE_OSX
                if (!(pp.subscribed || !sceneInfo.demoLock || PlayerPrefsManager.HasPurchasedScenes()))
                    continue;
#endif

                scenesInfo.Add(sceneInfo.sceneName, sceneInfo);
                
                foreach(string g in sceneInfo.inGroups)
                {
                    if (sceneGroupNum.ContainsKey(g))
                    {
                        sceneGroupNum[g] += 1;
                    }
                    else
                    {
                        sceneGroupNum[g] = 1;
                    }

                }
                // useless and spams the hell out of console
                //Debug.Log(sceneGroupNum);
            }

            if (sceneInfo.mainScene != "" && sceneType != "")
            {
                if(scenesInfo.ContainsKey(sceneInfo.mainScene))
                {
                    if (sceneType == "in_house")
                    {
                        scenesInfo[sceneInfo.mainScene].inHouseBundleName = sceneInfo.bundleName;
                        scenesInfo[sceneInfo.mainScene].inHouseSceneName = sceneInfo.sceneName;
                    }
                }
            }

            if (xmlSceneNode.Attributes["xPoints"] != null)
                sceneInfo.xPoints = xmlSceneNode.Attributes["xPoints"].Value;
        }


        //Creation of menu elements from loaded data

        Dictionary<string, SceneInfo> scenesInfoUnlocked = new Dictionary<string, SceneInfo>();
        Dictionary<string, SceneInfo> scenesInfoLocked = new Dictionary<string, SceneInfo>();
        Dictionary<string, SceneInfo> scenesInfoDemo = new Dictionary<string, SceneInfo>();

        foreach (string key in scenesInfo.Keys)
        {
            //Show only scenes and scene groups, that are available for current player
#if UNITY_IOS || UNITY_STANDALONE_OSX
            if (!(pp.subscribed || !scenesInfo[key].demoLock || PlayerPrefsManager.HasPurchasedScenes()))
                continue;
#endif

            pp.AddSceneInfo(scenesInfo[key]);
            SceneInfo sceneInfo = scenesInfo[key];

            if ((!sceneInfo.activated && sceneInfo.hidden) || sceneInfo.hidden)
            {
                // not activated and hidden scene should not even create a panel, so just end up here
                continue;
            }

            if (!sceneInfo.demoLock)
            {
                scenesInfoDemo.Add(key, scenesInfo[key]);
            }
            else
            {
                scenesInfoLocked.Add(key, scenesInfo[key]);
            }
        }

        Dictionary<string, SceneInfo> scenesInfoSorted = new Dictionary<string, SceneInfo>();
        foreach (string key in scenesInfoDemo.Keys)
            scenesInfoSorted.Add(key, scenesInfoDemo[key]);

        foreach (string key in scenesInfoLocked.Keys)
            scenesInfoSorted.Add(key, scenesInfoLocked[key]);

        MainMenuAutomation mainMenuAutomation = GameObject.FindObjectOfType<MainMenuAutomation>();
        mainMenuAutomation.levelButtons.Clear();
        for (int i = 0; i < scenesInfoSorted.Count; i++)
        {
            string key = scenesInfoSorted.Keys.ElementAt(i);

            SceneInfo sceneInfo = scenesInfoSorted[key];

            if ((!sceneInfo.activated && sceneInfo.hidden) || sceneInfo.hidden)
            {
                // not activated and hidden scene should not even create a panel, so just end up here
                continue;
            }
        

            GameObject sceneUnitObject = Instantiate(Resources.Load<GameObject>("NecessaryPrefabs/UI/SceneSelectionUnit"), protocolsTransorm);
            sceneUnitObject.name = "SceneSelectionUnit"; // i dont like that 'clone' word at the end, ugh

            LevelButton sceneUnit = sceneUnitObject.GetComponent<LevelButton>();
            sceneUnit.isFree = sceneInfo.demoLock;
            sceneUnit.sceneDescription = sceneInfo.description;
            sceneUnit.SetDemoMark(!sceneInfo.demoLock);
            sceneUnit.inHouseBundleName = sceneInfo.inHouseBundleName;
            sceneUnit.inHouseSceneName = sceneInfo.inHouseSceneName;
            sceneUnit.inGroups = sceneInfo.inGroups;
            sceneUnit.buttonIndex = i;
            mainMenuAutomation.levelButtons.Add(sceneUnit);
            if (!sceneInfo.activated && !sceneInfo.hidden)
            {
                // but if scene is not activated and NOT hidden either
                // just disable play button, but show the panel to the player
                //sceneUnit.transform.Find("BottonBar/Play").GetChild(0).GetComponent<Text>().text = "Bestellen";
                sceneUnit.buy = true;
                // grey out ?
                ColorBlock colorBlock = sceneUnit.GetComponent<Button>().colors;
                colorBlock.normalColor = Color.grey;
                colorBlock.highlightedColor = Color.grey;
                sceneUnit.GetComponent<Button>().colors = colorBlock;
            }
            if (sceneInfo.isInProducts != null)
                sceneUnit.isInProducts = sceneInfo.isInProducts;

            sceneUnit.sceneName = sceneInfo.sceneName;
            sceneUnit.bundleName = sceneInfo.bundleName;
            sceneUnit.url = sceneInfo.url;

            // setting scene title
            sceneUnit.displayName = sceneInfo.displayName;

            ppManager.currentSceneVisualName = sceneUnit.displayName;
            // setting id next to manager visual name
            sceneUnit.sceneID = sceneInfo.sceneID; // yes, duping value is important
            sceneUnit.courseID = sceneInfo.courseID;
            ppManager.currentPEcourseID = sceneUnit.sceneID;
            ppManager.currentPEAccreditationCourseID = sceneUnit.courseID;

            ppManager.CreateBlankHighscore(); // has a check inside for no DB info already

            // override image if scene is completed
            float fscore = 0.0f;
            float.TryParse(DatabaseManager.FetchField("TestHighscores",
                PlayerPrefsManager.FormatSceneName(pp.GetSceneDatabaseName(sceneUnit.displayName))).Replace(",", "."), out fscore);
            if (Mathf.FloorToInt(fscore) >= 70)
            {
                sceneUnit.image = completedSceneIcon;
                //----------------------------
                //sceneUnit.GetComponent<LevelButton>().SetLevelPreviewIcon(true, sceneUnit.image);
                //sceneUnit.transform.Find("LevelPreview").gameObject.SetActive(true);
                //sceneUnit.transform.Find("LevelPreview").GetComponent<Image>().sprite = sceneUnit.image;
            }
            sceneUnit.validated = sceneInfo.validated;
            sceneUnit.transform.Find("Validation").GetComponent<Text>().text = "";
            if (sceneUnit.validated)
            {
                //@
                sceneUnit.transform.Find("Validation").GetComponent<Text>().text = 
                    LocalizationManager.GetValueIfKey("[Geaccrediteerd]");
                
                InGameLocalEditTool inGameLocalEditTool = PlayerPrefsManager.GetDevMode() ? GameObject.FindObjectOfType<InGameLocalEditTool>() : null;  
                if (inGameLocalEditTool != null)
                {
                    //!
                    inGameLocalEditTool.AddUILocalizationComponentToGO(
                        sceneUnit.transform.Find("Validation").gameObject, "[Geaccrediteerd]"
                    );
                }
            }

            sceneUnit.totalPoints = sceneInfo.totalPoints;
            sceneUnit.xPoints = sceneInfo.xPoints;

            // leaderboard stuff
            if (pp.subscribed)
                sceneUnit.SetLockState(false);
            else
                sceneUnit.SetLockState(sceneInfo.demoLock);


            //GameObject button = Instantiate<GameObject>(Resources.Load<GameObject>("NecessaryPrefabs/UI/LeaderBoardItem"),
            //    GameObject.Find("/UMenuProManager/MenuCanvas/LayoutPanel/Tabs/Leaderboard/ContentPanel/Scenes/ProtocolPanel/Panel/ProtocolList/ProtocolsHolder/Protocols/content").transform);
            //LeaderBoardSceneButton buttonInfo = button.GetComponent<LeaderBoardSceneButton>();
            //button.transform.Find("Text").GetComponent<Text>().text = sceneUnit.displayName;

            //buttonInfo.sceneName = sceneUnit.sceneName;
            //buttonInfo.multiple = sceneUnit.multiple;

            //if (buttonInfo.multiple)
            //{
            //    foreach (LevelButton.Info v in sceneUnit.variations)
            //    {
            //        buttonInfo.sceneNames.Add(v.sceneName);
            //        buttonInfo.buttonNames.Add(v.displayName);
            //    }
            //}

            //if (firstScene)
            //{
            //    firstScene = false;
            //    buttonInfo.OnMainButtonClick();
            //}

            sceneUnit.testDisabled = sceneInfo.testDisabled;
            sceneUnit.UpdateAutoPlayToggle();
        }
        ScrollRect levelScroll =  GameObject.Find("/UMenuProManager/MenuCanvas/LayoutPanel/Tabs/Play/ContentPanel/PlayElements/ProtocolPanel/Panel/ProtocolList/ProtocolsHolder").GetComponent<ScrollRect>();
        
        levelScroll.verticalNormalizedPosition = ppManager.LevelScrollPosition;
        UpdateLeaderBoard();
    }

    public void LeaderBoardNewName(int nameScreenID = 0)
    {

        string newName = leaderBoardNameInput.text;
        if (nameScreenID == 1)
            newName = leaderBoardNameInput2.text;
        if (nameScreenID == 2)
            newName = "";
        if (newName != "" || nameScreenID >= 2)
        {
            DatabaseManager.SetLeaderboardName(newName);
            ShowLeaderBoardPopUp(0);
        }
        else
        {
            leaderBoardNameInput.GetComponent<Animation>().Play();
            leaderBoardNameInput2.GetComponent<Animation>().Play();
        }
    }
    public void ShowLeaderBoardPopUp(int index)
    {
        leaderBoardParticipatePanel.SetActive(index == 1);
        //if (index == 2)
        //{
        //    SwitchLBTab(2);
        //}
        leaderBoardNewNamePanel.SetActive(index == 2);
        leaderBoardInfoButton.SetActive(index == 0);
        leaderBoardNameInput.text = DatabaseManager.LeaderboardName;
        LeaderBoardInfoTabPanel.SetActive(false);
    }


    public void CheckAndShowLBParticipation()
    {
        if (CheckLeaderBoardParticipation())
            ShowLeaderBoardPopUp(1);
        else
            ShowLeaderBoardPopUp(0);
    }

    public bool CheckLeaderBoardParticipation()
    {
        return (DatabaseManager.LeaderboardName == "");
    }
    public void UpdateLeaderBoard()
    {
        int rank = 0;
        GameObject.FindObjectOfType<LeaderBoard>().CompelteAtLeastOneText.SetActive(LeaderboardDB.board.Count <= 0);
        if (LeaderboardDB.board.Count > 0)
        {
            rank = LeaderboardDB.board[0].Rank;
            for (int i = 0; i < LeaderboardDB.board.Count; i++)
            {
                GameObject leaderboardItem = Instantiate<GameObject>(Resources.Load<GameObject>("NecessaryPrefabs/UI/LeaderBoardItem"),
                    GameObject.Find("/UMenuProManager/MenuCanvas/LayoutPanel/Tabs/Leaderboard/ContentPanel/Scenes/ProtocolPanel/Panel/ProtocolList/ProtocolsHolder/Protocols/content").transform);
                LeaderBoardItem leaderboardItemInfo = leaderboardItem.GetComponent<LeaderBoardItem>();
                LeaderboardDB.LeaderboardLine lbLine = LeaderboardDB.board[i];
                leaderboardItemInfo.SetValues(lbLine.Name, lbLine.Rank, lbLine.Points, lbLine.UserID, i+1);
            }
        }
        float CSAlpha = 1.0f;
        if (LeaderboardDB.board.Count != 0)
            CSAlpha = 0.0f;
        GameObject.Find("/UMenuProManager/MenuCanvas/LayoutPanel/Tabs/Leaderboard/ContentPanel/Scenes/ProtocolPanel/Panel/ProtocolList/ProtocolsHolder/Protocols/CSPanel").GetComponent<CanvasGroup>().alpha = CSAlpha;
        SelectRank(rank);
    }
    public void LevelScrollChanged()
    {
        ScrollRect levelScroll =  GameObject.Find("/UMenuProManager/MenuCanvas/LayoutPanel/Tabs/Play/ContentPanel/PlayElements/ProtocolPanel/Panel/ProtocolList/ProtocolsHolder").GetComponent<ScrollRect>();
       
        ppManager.LevelScrollPosition = levelScroll.verticalNormalizedPosition;
    }

    public void UpdateLeaderBoard(string sceneName)
    {
        // Debug.Log("UpdateLeaderBoard:::" + sceneName);
        // let's clear current UI first, it might have some editor text or info from other scene we loaded before
        for (int i = 0; i < _Scores.Length; i++)
        {
            _Scores[i].SetScoreLine("", "", i);
        }

        // maybe launch loading icon or something? it isnt instant
        //ppManager.GetSceneLeaders(sceneName, 10, GetSceneLeaders_Success);

        MBS.WUScoring.onFetched = GetSceneLeaders_Success;

        // hashes are NOT a clean solution
        int hash = Mathf.Abs(sceneName.GetHashCode());
        MBS.WUScoring.FetchScores(0, hash);
    }

    public void GetSceneLeaders_Success(MBS.CML cml)
    {
        List<MBS.CMLData> entries = cml.AllNodesOfType("person");
        List<MBS.CMLData> sortedEntries = entries.OrderByDescending(x => int.Parse(x.String("score"))).ToList();

        LeaderBoard lb = GameObject.FindObjectOfType<LeaderBoard>();
        if (entries == null) return;

        for (int i = 0; i < 10; ++i)
        {
            string name = sortedEntries[i].String("dname");
            string score = sortedEntries[i].String("score");
            string uid = sortedEntries[i].String("uid");

            if (i < _Scores.Length)
                _Scores[i].SetScoreLine(name, score, i, uid);
        }

        // loading icon is shown
        if (lb.leftBar.activeSelf)
            lb.infoBar.SetActive(false);
        else
        {
            
            //GameObject.FindObjectOfType<UMP_Manager>().LeaderBoardSearchBar.gameObject.SetActive(false);
            lb.top.SetActive(false);
            lb.backButton.GetComponent<Button>().interactable = true;
            lb.topDescription.SetActive(false);
            lb.infoBar.SetActive(true);
            lb.description.GetComponent<Text>().text = LeaderBoardSceneButton.Descripton;
            lb.leaderboard.SetActive(false);
        }       
    }

    public void RequestCharacterInfoByUID(int uid)
    {
        // start loading animation?
        // actual load stuff
        WUData.FetchUserCategory(uid, "AccountStats", RequestCharacterInfoByUID_success);
    }

    public void RequestCharacterInfoByUID_success(CML response)
    {
        //loading done, stop loading animation, open UI
        string sex = response.Elements[1]["Sex"];
        string head = response.Elements[1]["Head"];
        string body = response.Elements[1]["Body"];
        string glasses = response.Elements[1]["Glasses"];
        string hat = response.Elements[1]["Hat"]; // didnt find info
        bool toShowPlayer = false;
        if (!string.IsNullOrEmpty(head))
        {
            PlayerAvatar mainAvatar = GameObject.Find("MainPlayerAvatar").GetComponent<PlayerAvatar>();
            PlayerAvatarData prevCharData = new PlayerAvatarData();
            if (sex == "Female")
                prevCharData.gender = Gender.Female;
            int.TryParse(head, out prevCharData.headType);
            if (prevCharData.headType < mainAvatar.GetMaxHeadNum(prevCharData.gender))
            {
                int.TryParse(body, out prevCharData.bodyType);
               
                int glassesType = -1;
                int.TryParse(glasses, out glassesType);
                if (glassesType < 3000000)
                    glassesType += 3000000;
                prevCharData.glassesType = glassesType;
                prevCharData.hat = hat;
                mainAvatar.avatarData = prevCharData;
                mainAvatar.UpdateCharacter();
                toShowPlayer = true;
            }
            else
                Debug.Log("No correct head");
        }
        else
            Debug.Log("No avatar data");
        GameObject.FindObjectOfType<HighscoreCharacterPanel>().HideContent(false, toShowPlayer);
    }

    public void OnAppleScenePurchaseButtonClick()
    {
        GameObject.Find("Preferences").GetComponent<IAPManager>().BuyProductID(ppManager.currentPEcourseID);
    }
}
