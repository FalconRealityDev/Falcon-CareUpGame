using System;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MBS;
using PaperPlaneTools;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using CareUp.Localize;
using System.Runtime.InteropServices;

/// <summary>
/// Handles quick access to saved data.
/// Volume | Tutorial completion | Scene completion + results
/// </summary>
public class PlayerPrefsManager : MonoBehaviour
{
    public class CANotifications
    {
        public string title;
        public string message;
        public string author;
        public bool isRead = false;
        public long createdTime;

        public CANotifications(string _title, string _message, string _author, bool _isRead, long _createdTime)
        {
            title = _title;
            message = _message;
            author = _author;
            isRead = _isRead;
            createdTime = _createdTime;
        }

        public string GetCreatedTimeString()
        {
            System.DateTime notifDate = UnixTimeStampToDate(createdTime);
            string sDate = notifDate.Day.ToString() + "." + notifDate.Month.ToString() + "." + notifDate.Year.ToString();
            return sDate;
        }
        public System.DateTime UnixTimeStampToDate(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

    };
    public string videoSceneName = "";
    public static Dictionary<int, CANotifications> Notifications = new Dictionary<int, CANotifications>();

    public static StoreManager storeManager = new StoreManager();

    public HatsPositioningDB hatsPositioning = new HatsPositioningDB();
    public static bool plusCoins = false;
    public static bool plusDiamonds = false;
    public static bool resetPurchases = false;
    public static string TestBundleIPAddr = "";
    public static bool UseDevBundleServer = false;
    public static bool toShowLocalMarkers = false;
    public static bool editCharacterOnStart = false;
    public static bool tutorialOnStart = false;
    public static bool simulatePlayerActions = false;
    public static bool videoRecordingMode = false;
    public static bool videoRecordingWithTextMode = false;
    public int currentPracticePlays = -1;
    public static bool CAKeyboardVisible = false;
    List<string> scenesWithFreeCert = new List<string>();
    public static bool forceShowUpdatesPanel = false;

    public bool VR = true;
    public bool practiceMode = true;
    public bool TextDebug = false;

    static bool masterModeEnabled = false;
    static List<string> purchasedScenes = new List<string>();
    static List<SceneInfo> ScenesInfo = new List<SceneInfo>();
    // store value here after getting from server
    public bool tutorialCompleted;
    public static bool firstStart = true;
    private static PlayerPrefsManager instance;

    public static string HighscorePlayerName = "";
    public static string HighscoreSceneName = "";

    private List<string> activatedScenes = new List<string>();

    // sets up after selecting scene in "scene selection"
    public string currentSceneVisualName;
    public string currentPEcourseID;
    public string currentPEAccreditationCourseID;
    public string currentSceneXPoints;
    public int currentDifficultyLevel;
    public bool validatedScene;
    public string currentSceneName;
    public string currentSceneBundleName;


    // post processing on camera
    public bool postProcessingEnabled = false;

    // is this game version is demo version
    public bool demoVersion = false;

    // indicates if game is played in testing mode (only editor mode)
    public bool testingMode = false;

    // save info about subscription
    // [HideInInspector]
    public bool subscribed = false;
    [HideInInspector]
    public static int plays = 0;

    public static int currentPracticeScore = 0;
    public static int currentPracticeStars = 0;

    private UMP_Manager manager;
    private MainMenu mainMenu;
    private Scene currentScene;

    public string fullPlayerName = "";
    public string bigNumber = "";

    public bool muteMusicForEffect = false;
    private bool muteMusic = false;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string GetStringParams();
#endif
    //public string currentLoginToken = "";
    //public string currentLoginName = "";
    //public string currentLoginPass = "";

    public static CANotifications GetNotificationByID(int _id)
    {
        if (Notifications.ContainsKey(_id))
        {
            return Notifications[_id];
        }
        return null;
    }

    public static bool GetDevMode()
    {
        if (GetMasterMode())
            return true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        return true;
#endif
        return false;
    }

    [System.Serializable]
    public class PurchasedScetesData
    {
        public string product_name;
    }

    public static void EnableMasterMode(bool toEnable)
    {
        masterModeEnabled = toEnable;
    }

    public static bool GetMasterMode()
    {
        return masterModeEnabled;
    }

    public void AddFreeCertScene(string sceneName)
    {
        if (!scenesWithFreeCert.Contains(sceneName))
            scenesWithFreeCert.Add(sceneName);
    }
    public void ClearFreeCertList()
    {
        scenesWithFreeCert.Clear();
    }
    public bool HasFreeCert(string sceneName)
    {
        return scenesWithFreeCert.Contains(sceneName);
    }

    public string ActivatedScenes
    {
        get
        {
            string result = "";
            foreach (string s in activatedScenes)
                result += s + " geactiveerd.\n";
            return result;
        }
    }

    static public bool IsScenePurchased(string[] SKUs)
    {
        foreach (string purchasedScene in purchasedScenes)
        {
            foreach (string sku in SKUs)
            {
                if (purchasedScene == sku)
                    return true;
            }
        }
        return false;
    }

    static public void AddSKU(string sku)
    {
        if (purchasedScenes.IndexOf(sku) < 0)
            purchasedScenes.Add(sku);
    }

    static public bool HasPurchasedScenes()
    {
        return purchasedScenes.Count > 0;
    }

    static public void ClearSKU()
    {
        purchasedScenes = new List<string>();
    }

    public void ClearScenesInfo()
    {
        ScenesInfo.Clear();
    }

    public void AddSceneInfo(SceneInfo sceleInfo)
    {
        ScenesInfo.Add(sceleInfo);
    }


    public string GetSceneDisplayName(string sceneName)
    {
        foreach(SceneInfo s in ScenesInfo)
        {
            if (s.nameForDatabase == sceneName)
                return s.displayName;
        }
        return sceneName;
    }

    public SceneInfo GetSceneInfoByName(string sceneName)
    {
        foreach (SceneInfo sceneInfo in ScenesInfo)
        {
            if (sceneInfo.sceneName == sceneName)
            {
                return sceneInfo;
            }
        }
        return null;
    }

    public string GetSceneDatabaseName(string sceneName)
    {
        string sceneDatabaseName = sceneName;
        foreach (SceneInfo sceneInfo in ScenesInfo)
        {
            if (sceneInfo.displayName == sceneName)
            {
                if (sceneInfo.nameForDatabase != "")
                {
                    sceneDatabaseName = sceneInfo.nameForDatabase;
                    break;
                }
            }
        }
        return sceneDatabaseName;
    }


    public bool IsScenePurchasedByName(string sceneName)
    {
        foreach(SceneInfo sceneInfo in ScenesInfo)
        {
            if (sceneInfo.sceneName == sceneName)
            {
                return IsScenePurchased(sceneInfo.isInProducts);
            }
        }
        return false;
    }

    public List<string> GetScenesInProduct(string SKU)
    {
        List<string> _scenes = new List<string>();
        foreach (SceneInfo sceneInfo in ScenesInfo)
        {
            if (sceneInfo.isInProducts != null)
            {
                foreach(string __sku in sceneInfo.isInProducts)
                {
                    if (SKU == __sku)
                    {
                        _scenes.Add(sceneInfo.displayName);
                        break;
                    }
                }
            }
        }
        return _scenes;
    }

    public void Update()
    {
        SetEscapeButtonLogic();
    }

    public void ToPlayMusic(bool value)
    {
        bool toPlay = value;
        if (muteMusicForEffect || muteMusic)
            toPlay = false;

        if (toPlay)
        {
            if (!GetComponent<AudioSource>().isPlaying)
                GetComponent<AudioSource>().Play();
        }
        else
        {
            GetComponent<AudioSource>().Stop();
        }
    }

    private void OnLoaded(Scene s, LoadSceneMode m)
    {
        transform.position = GameObject.FindObjectOfType<AudioListener>().transform.position;

        currentScene = s;

        if (!(s.name == "LoginMenu" ||
                s.name == "MainMenu" ||
                s.name == "SceneSelection" ||
                s.name == "Scenes_Character_Customisation" ||
                s.name == "Scenes_Tutorial" ))
        {
            // game scenes
            muteMusic = true;

            ToPlayMusic(!muteMusic);

            if (Camera.main != null)
            {
                if (Camera.main.GetComponent<PostProcessingBehaviour>() != null)
                {
                    Camera.main.GetComponent<PostProcessingBehaviour>().enabled = postProcessingEnabled;

                    if (postProcessingEnabled && Application.platform != RuntimePlatform.WebGLPlayer)
                    { 
                        Camera.main.GetComponent<PostProcessingBehaviour>().profile = Resources.Load("PostProcessing/Care_Up_PPS_Mobile") as PostProcessingProfile;
                    }
                    else
                    {
                        Camera.main.GetComponent<PostProcessingBehaviour>().profile = Resources.Load("PostProcessing/Care_Up_PPS") as PostProcessingProfile;
                    }
                    Debug.Log("*Curred PPS: " + Camera.main.GetComponent<PostProcessingBehaviour>().profile.ToString());
                }
            }
            GameObject panoFlyCamera = GameObject.Find("PanoFlyCamera");
            if (panoFlyCamera != null)
            {
                if (panoFlyCamera.GetComponent<PostProcessingBehaviour>() != null)
                {
                    panoFlyCamera.GetComponent<PostProcessingBehaviour>().enabled = postProcessingEnabled;
                    if (postProcessingEnabled && Application.platform != RuntimePlatform.WebGLPlayer)
                    {
                        panoFlyCamera.GetComponent<PostProcessingBehaviour>().profile = Resources.Load("PostProcessing/Care_Up_PPS_Mobile") as PostProcessingProfile;
                    }
                    else
                    {
                        panoFlyCamera.GetComponent<PostProcessingBehaviour>().profile = Resources.Load("PostProcessing/Care_Up_PPS") as PostProcessingProfile;
                    }
                }
            }
            GameObject playerMainCamera = GameObject.Find("PlayerMainCamera");
            if (playerMainCamera != null)
            {
                if (playerMainCamera.GetComponent<PostProcessingBehaviour>() != null)
                {
                    playerMainCamera.GetComponent<PostProcessingBehaviour>().enabled = postProcessingEnabled;
                    if (postProcessingEnabled && Application.platform != RuntimePlatform.WebGLPlayer)
                    {
                        playerMainCamera.GetComponent<PostProcessingBehaviour>().profile = Resources.Load("PostProcessing/Care_Up_PPS_Mobile") as PostProcessingProfile;
                    }
                    else
                    {
                        playerMainCamera.GetComponent<PostProcessingBehaviour>().profile = Resources.Load("PostProcessing/Care_Up_PPS") as PostProcessingProfile;
                    }
                }
            }
            
        }

        if (s.name == "EndScore" || (s.name == "MainMenu"))
        {
            muteMusic = false;
            ToPlayMusic(!muteMusic);
        }

        if (!Convert.ToBoolean(MenuAudio))
        {
            muteMusic = true;
            ToPlayMusic(!muteMusic);
        }

        if (s.name == "MainMenu")
        {
            GameObject.Find("/UMenuProManager/MenuCanvas/Opties/PanelUI/OptionsPanel/LeftSide/PostProcessingPanel/PostProcessingToggle").GetComponent<Toggle>().isOn = postProcessingEnabled;
        }

        // handle platform-dependant objects (deleting unnecesarry)
        bool windows = (Application.platform == RuntimePlatform.WindowsPlayer);
        windows |= (Application.platform == RuntimePlatform.WSAPlayerX86);
        windows |= (Application.platform == RuntimePlatform.WSAPlayerX64);
        windows |= (Application.platform == RuntimePlatform.WSAPlayerARM);

        if (s.name == "MainMenu")
        {
            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                Destroy(GameObject.Find("UMenuProManager/MenuCanvas/VersionUpdatePanel/Panel_Version_UI" +
                    "/NewVersionButtonGreenApple"));

                //Destroy(GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/WUSerialScreen" +
                //    "/RegisterArea/Buttons/MoreInfo_Apple"));
                //Destroy(GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/WUSerialScreen" +
                //    "/RegisterArea/Buttons/Purchase_Apple"));


                Destroy(GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/WUSerialScreen" +
                    "/RegisterArea/Buttons/PurchaseButton_Apple"));
            }

            if (Application.platform != RuntimePlatform.Android)
            {
                Destroy(GameObject.Find("UMenuProManager/MenuCanvas/VersionUpdatePanel/Panel_Version_UI" +
                    "/NewVersionButtonGreenAndroid"));
                Destroy(GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/WUSerialScreen" +
                    "/RegisterArea/Buttons/PurchaseButton_GoogleIAP"));
            }

            if (!windows)
            {
                Destroy(GameObject.Find("UMenuProManager/MenuCanvas/VersionUpdatePanel/Panel_Version_UI" +
                    "/NewVersionButtonGreenWindows"));
                Destroy(GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/WUSerialScreen" +
                    "/RegisterArea/Buttons/PurchaseButton_UWP"));
            }

            if ((Application.platform != RuntimePlatform.Android))
            {
                Destroy(GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/WUSerialScreen" +
                    "/RegisterArea/Buttons/PurchaseButton_AndroidWeb"));
            }

            if ((Application.platform != RuntimePlatform.WebGLPlayer))
            {
                Destroy(GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/WUSerialScreen" +
                    "/RegisterArea/Buttons/PurchaseButton_WebGL"));
            }
        }

        if (s.name == "LoginMenu")
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                Destroy(GameObject.Find("Canvas/WULoginPrefab/WUSerialScreen/RegisterArea/Purchase_Android_WebGL"));
            }

            if (!windows)
            {
                Destroy(GameObject.Find("Canvas/WULoginPrefab/WUSerialScreen/RegisterArea/Purchase_UWP"));
            }

            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                //Destroy(GameObject.Find("Canvas/WULoginPrefab/WUSerialScreen/RegisterArea/MoreInfo_Apple"));
                //Destroy(GameObject.Find("Canvas/WULoginPrefab/WUSerialScreen/RegisterArea/Purchase_Apple"));
            }
        }
    }

    void Awake()
    {

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if (!Application.isEditor)
        {
            testingMode = false;
        }
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        hatsPositioning.Init();

        // uncomment this, fill with correct info and start game
        // p.s. dont forget to comment this again and not push instead :)
        //PlayerPrefsManager.__dev__customCertificate("playerFullName", "sceneName", "06202019");

#if UNITY_WEBGL || UNITY_EDITOR
        HandleLoginToken();
#endif
        // stupid way to push new tokens 
        //CMLData data = new CMLData();
        //string testLogin = "test";
        //string testPass = "123";
        //byte[] encodeLogin = Encoding.UTF8.GetBytes(testLogin);
        //byte[] encodePass = Encoding.UTF8.GetBytes(testPass);
        //string authData = Convert.ToBase64String(encodeLogin) + " " + Convert.ToBase64String(encodePass);
        //data.Set("abcdefg123456", authData);
        //WUData.UpdateSharedCategory("LoginTokens", data);
    }

    public static bool HasNewNorifications()
    {
        bool hasNew = false;
        foreach (int _key in Notifications.Keys)
        {
            if (!Notifications[_key].isRead)
            {
                hasNew = true;
                break;
            }
        }
        return hasNew;
    }

    public void SerGraphicsLevel()
    {
        postProcessingEnabled = PlayerPrefs.GetInt("PostProcessing") == 1;
        int QualityLevel = 0;


        if (postProcessingEnabled)
        {
            QualityLevel = 7;//Set High Quality for mobile devices
#if UNITY_WEBGL

            QualityLevel = 5;//Set High Quality for WebGl 
#endif
        }


        QualitySettings.SetQualityLevel(QualityLevel, true);
        ColoredConsoleOutput.Print(RGBColor.Green, "*The quality level is: " + QualitySettings.names[QualityLevel].ToString());
        //Debug.Log ("PostProcessing is set to saved value: " + postProcessingEnabled);
    }

    void Start()
    {
        Application.targetFrameRate = 60; 
        SmartlookUnity.SetupOptionsBuilder builder = new SmartlookUnity.SetupOptionsBuilder("22f3cf28278dbff71183ef8e0fa90c90048b850d");
        builder.SetFps(2);
        builder.SetStartNewSession(true);
        builder.SetStartNewSessionAndUser(true);
        SmartlookUnity.Smartlook.SetupAndStartRecording(builder.Build());

        LocalizationManager.LoadAllDictionaries();
        SceneManager.sceneLoaded += OnLoaded;

        AudioListener.volume = Volume;
        //Debug.Log ("Volume is set to saved value: " + Volume);

        if (Convert.ToBoolean(MenuAudio))
        {
            GetComponent<AudioSource>().Play();
        }
        else
        {
            GetComponent<AudioSource>().Stop();
        }

        SerGraphicsLevel();

        // OnLoaded doesnt launch on initial scene? so force it in start function separately
        OnLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        manager = GameObject.FindObjectOfType<UMP_Manager>();

        // force NumberDecimalSeparator to be "."
        System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";

        System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
    }

    public float Volume
    {
        get { return PlayerPrefs.HasKey("Volume") ? PlayerPrefs.GetFloat("Volume") : 1.0f; }
        set { PlayerPrefs.SetFloat("Volume", value); }
    }

    public static int Lang
    {
        get { return PlayerPrefs.HasKey("Lang") ? PlayerPrefs.GetInt("Lang") : 0; }
        set { PlayerPrefs.SetInt("Lang", value); }
    }


    public int BundleVersion
    {
        get { return PlayerPrefs.HasKey("BundleVersion") ? PlayerPrefs.GetInt("BundleVersion") : 0; }
        set { PlayerPrefs.SetInt("BundleVersion", value); }
    }

    public int MenuAudio
    {
        get { return PlayerPrefs.HasKey("MenuAudio") ? PlayerPrefs.GetInt("MenuAudio") : 1; }
        set { PlayerPrefs.SetInt("MenuAudio", value); }
    }

    public int CarouselPosition
    {
        get { return PlayerPrefs.HasKey("CarouselPosition") ? PlayerPrefs.GetInt("CarouselPosition") : 1; }
        set { PlayerPrefs.SetInt("CarouselPosition", value); }
    }

    public float ScroopPosition
    {
        get { return PlayerPrefs.HasKey("ScroopPosition") ? PlayerPrefs.GetFloat("ScroopPosition") : 0f; }
        set { PlayerPrefs.SetFloat("ScroopPosition", value); }
    }

    public float LevelScrollPosition
    {
        get { return PlayerPrefs.HasKey("LevelScrollPosition") ? PlayerPrefs.GetFloat("LevelScrollPosition") : 1f; }
        set { PlayerPrefs.SetFloat("LevelScrollPosition", value); }
    }

    public int LevelPagePosition
    {
        get { return PlayerPrefs.HasKey("LevelPagePosition") ? PlayerPrefs.GetInt("LevelPagePosition") : 0; }
        set { PlayerPrefs.SetInt("LevelPagePosition", value); }
    }

    public void SetSceneActivated(string sceneName, bool value)
    {
        if (value) Debug.Log(sceneName + " activated");
        PlayerPrefs.SetInt(sceneName + " activated", value ? 1 : 0);
    }

    public void SetSceneCompletionData(string sceneName, int score, int time)
    {
        if (demoVersion) return;
        // hashes are NOT a clean solution
        int hash = Mathf.Abs(sceneName.GetHashCode());
        MBS.WUScoring.SubmitScore(score, hash);
    }

    public bool GetSceneCompleted(string sceneName)
    {
        return PlayerPrefs.GetInt(sceneName + "_completed") == 1;
    }

    public int GetSceneStars(string sceneName)
    {
        return PlayerPrefs.GetInt(sceneName + "_stars");
    }

    public string GetSceneTime(string sceneName)
    {
        return PlayerPrefs.GetString(sceneName + "_time");
    }

    public bool TutorialPopUpDeclined
    {
        get { return PlayerPrefs.GetInt("TutorialPopUpDeclined") == 1; }
        set { PlayerPrefs.SetInt("TutorialPopUpDeclined", value ? 1 : 0); }
    }

    public void SetSerial(string serial)
    {
        string[] data = new string[1];
        data[0] = serial;

        // LoginPro.Manager.ExecuteOnServer("SetSerial", SetSerialSuccess, Debug.LogError, data);
    }

    private void CheckSerial_Success(string[] datas)
    {
        activatedScenes.Clear();

        foreach (string data in datas)
        {
            if (data == "Received") break;
            string[] result;
            result = data.Split('|');

            SetSceneActivated(result[0], true);
            if (!activatedScenes.Contains(result[1]))
                activatedScenes.Add(result[1]);
        }

        // set new activated scenes in news
        GameObject.Find("UMenuProManager/MenuCanvas/Home/BannerArea/RegisterProtocols/News/Text")
            .GetComponent<Text>().text = ActivatedScenes;
    }

    private void CheckSerialAfterLogIn_Success(string[] datas)
    {
        activatedScenes.Clear();

        foreach (string data in datas)
        {
            if (data == "Received")
                break;

            string[] result;
            result = data.Split('|');

            SetSceneActivated(result[0], true);
            if (!activatedScenes.Contains(result[1]))
                activatedScenes.Add(result[1]);
        }

        // now after login
        // we check the serial for the scenes
        // and then load new menu scene
        //LoginPro_Security.Load("UMenuPro");
        bl_SceneLoaderUtils.GetLoader.LoadLevel("UMenuPro");
    }

    public void AfterLoginCheck()
    {
        // deactivate scenes
        //LoginPro.Manager.ExecuteOnServer("GetScenes", GetScenes_Success, Debug.LogError, null);

        // support for old key type
        if (PlayerPrefs.GetString("SerialKey") != "")
        {
            SetSerial(PlayerPrefs.GetString("SerialKey"));

            PlayerPrefs.SetString("SerialKey", ""); // clear this, so it happens once
        }

        // time for checking if tutorial is completed
        if (PlayerPrefs.HasKey("TutorialCompleted"))
        {
            if (PlayerPrefs.GetInt("TutorialCompleted") == 1)
            {
                // tutorial was completed, let's send to server and remove this info from PC
                // LoginPro.Manager.ExecuteOnServer("SetTutorialCompleted", Blank, Debug.LogError, null);
                tutorialCompleted = true; // store for current session
            }
            else
            {
                tutorialCompleted = false; // store for current session
            }

            PlayerPrefs.DeleteKey("TutorialCompleted"); // delete
        }
        else // when info is deleted from PC
        {
            // LoginPro.Manager.ExecuteOnServer("GetTutorialCompleted", GetTutorialCompleted_Success, Debug.LogError, null);
        }
    }

    public void GetTutorialCompleted_Success(string[] datas)
    {
        if (datas.Length > 0)
        {
            // getting numerical value, not boolean
            int value = 0;
            int.TryParse(datas[0], out value);

            // 0 = false, 1 = true
            tutorialCompleted = (value > 0);
        }
    }

    public void GetScenes_Success(string[] datas)
    {
        foreach (string data in datas)
        {
            SetSceneActivated(data, false);
        }

        // activate scenes corresponding to serials

    }

    public void GetSceneLeaders(string scene, int top, System.Action<string[]> method)
    {
        string[] datas = new string[2];
        datas[0] = scene;
        datas[1] = top.ToString();

        //LoginPro.Manager.ExecuteOnServer("GetSceneLeaders", method, Debug.LogError, datas);
    }

    public void GetSceneDatabaseInfo(string scene, System.Action<string[]> method)
    {
        string[] datas = new string[1];
        datas[0] = scene;
        Debug.Log(datas[0]);

        // LoginPro.Manager.ExecuteOnServer("GetSceneInfo", method, Debug.LogError, datas);
    }

    public void Blank(string[] s) { }

    public static void AddOneToPlaysNumber()
    {
        RateBox.Instance.IncrementCustomCounter();
        RateBox.Instance.Show();

        PlayerPrefsManager.plays += 1;

        DatabaseManager.UpdateField("AccountStats", "Plays_Number", PlayerPrefsManager.plays.ToString());
    }

    // keeping this just in case we need to send smth else
    public static void __sendMail(string topic, string message)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // apparently SMTP doesnt work with webgl
            Debug.LogError("Can't send email from webGL");
            return;
        }
        else
        {
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress("info@careup.online");
            mail.To.Add("info@careup.online");
            mail.Subject = topic;
            mail.Body = message;

            SmtpClient smtpServer = new SmtpClient("smtp.office365.com");
            smtpServer.Port = 587;
            smtpServer.Credentials = new System.Net.NetworkCredential("info@careup.online", "TripleMM3") as ICredentialsByHost;
            smtpServer.EnableSsl = true;

            ServicePointManager.ServerCertificateValidationCallback =
                delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            smtpServer.Send(mail);
        }
    }

    public static string MyEscapeURL(string url)
    {
#pragma warning disable
        return WWW.EscapeURL(url).Replace("+", "%20");
#pragma warning restore
    }

    /// <summary>
    /// Updates % highscore on database if new one is higher then old one.
    /// Also saves certificate date if there was no such previously.
    /// </summary>
    /// <param name="score"></param>
    public void UpdateTestHighscore(float score)
    {
        float currentTestScore = score;
        string currentTestScene = FormatSceneName(GetSceneDatabaseName(currentSceneVisualName));

        string highscoreStr = DatabaseManager.FetchField("TestHighscores", currentTestScene);
        float highscore = 0;
        float.TryParse(highscoreStr.Replace(",", "."), out highscore);

        if (highscore < currentTestScore)
        {
            DatabaseManager.UpdateField("TestHighscores", currentTestScene, currentTestScore.ToString());
        }

        // saving average score for analyzing
        //get current avg score
        string avgScoreStr = DatabaseManager.FetchField("TestAvgscores", currentTestScene);
        float avgScore = 0;
        float.TryParse(avgScoreStr.Replace(",", "."), out avgScore);
        //get total number of finishes
        string avgScorePlaysStr = DatabaseManager.FetchField("TestAvgscorePlays", currentTestScene);
        float avgScorePlays = 0;
        float.TryParse(avgScorePlaysStr.Replace(",", "."), out avgScorePlays);
        //new avg
        float newAvgScore = (avgScore * avgScorePlays + currentTestScore) / (avgScorePlays + 1);
        //push
        DatabaseManager.UpdateField("TestAvgscores", currentTestScene, newAvgScore.ToString());


        DatabaseManager.UpdateField("TestAvgscorePlays", currentTestScene, (avgScorePlays+1).ToString());

        // save certificate date here too
        string date = GetTodaysDateFormatted();
        DatabaseManager.UpdateField("CertificateDates", currentTestScene, date);
    }

    public void CreateBlankHighscore()
    {
        string currentTestScene = FormatSceneName(GetSceneDatabaseName(currentSceneVisualName));
        string highscoreStr = DatabaseManager.FetchField("TestHighscores", currentTestScene);
        if (highscoreStr == "") // returns empty string if field doesnt exist
        {
            DatabaseManager.UpdateField("TestHighscores", currentTestScene, (0.0f).ToString());
        }
    }

    public static void SetValueToSceneInCategory(string scene, string category, int value)
    {
        PlayerPrefsManager pp = GameObject.FindObjectOfType<PlayerPrefsManager>();
        string sceneName = FormatSceneName(pp.GetSceneDatabaseName(scene));
        int plays;
        int.TryParse(DatabaseManager.FetchField(category, sceneName), out plays);
        DatabaseManager.UpdateField(category, sceneName, (value).ToString());
    }

    public static void AddOneToSceneInCategory(string scene, string category)
    {
        PlayerPrefsManager pp = GameObject.FindObjectOfType<PlayerPrefsManager>();

        string sceneName = FormatSceneName(pp.GetSceneDatabaseName(scene));

        int plays;
        int.TryParse(DatabaseManager.FetchField(category, sceneName), out plays);
        DatabaseManager.UpdateField(category, sceneName, (plays + 1).ToString());
    }

    public static void AddOneToPracticePlays(string scene)
    {
        AddOneToSceneInCategory(scene, "PracticePlays");
    }

    public static void AddOneToTestPlays(string scene)
    {
        AddOneToSceneInCategory(scene, "TestPlays");
    }

    public static void AddOneToTestPassed(string scene)
    {
        AddOneToSceneInCategory(scene, "TestPassed");
    }

    public static void AddOneToTestFails(string scene)
    {
        AddOneToSceneInCategory(scene, "TestFails");
    }

    public void SetTutorialCompletedWU()
    {
        tutorialCompleted = true;

        CMLData data = new CMLData();
        data.Set("TutorialCompleted", "true");
        WUData.UpdateCategory("AccountStats", data);
    }

    static void GetTutorialCompleted(CML response)
    {
        //bool completed = response[1].Bool("TutorialCompleted");
        // do smth
    }

    static void GetTutorialCompleted_Error(CMLData response)
    {
        if ((response["message"] == "WPServer error: Empty response. No data found"))
        {
            CMLData data = new CMLData();
            data.Set("TutorialCompleted", "false");
            WUData.UpdateCategory("AccountStats", data);
        }
    }

    public void FetchLatestVersion()
    {
        WUData.FetchSharedField("LatestVersion", "GameInfo", GetLatestVersion, -1, GetLatestVersionError);
    }

    static void GetLatestVersion(CML response)
    {
        string currentVersion = Application.version;
        string latestVersion = response[1].String("LatestVersion");
        string[] cvSplit = currentVersion.Split('.');
        string[] lvSplit = latestVersion.Split('.');

        int currentVersionNum = int.Parse(cvSplit[0]) * 1000 + int.Parse(cvSplit[1]) * 100 + int.Parse(cvSplit[2]) * 10;
        int latestVersionNum = int.Parse(lvSplit[0]) * 1000 + int.Parse(lvSplit[1]) * 100 + int.Parse(lvSplit[2]) * 10;
        //print ("______Current: " + currentVersionNum.ToString () + " __latest: " + latestVersionNum.ToString ());
        if (currentVersionNum < latestVersionNum)
        {
            // player can download new version
            GameObject.Find("UMenuProManager/MenuCanvas/VersionUpdatePanel").SetActive(true);
        }
        else
        {
            GameObject versionUpdatePanel = GameObject.Find("UMenuProManager/MenuCanvas/VersionUpdatePanel");
            if (versionUpdatePanel != null)
                versionUpdatePanel.SetActive(false);
        }
    }

    public static string MD5 (string str)
    {
        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding ();
        byte[] bytes = encoding.GetBytes (str);      
        var sha = new System.Security.Cryptography.MD5CryptoServiceProvider();
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }

    public static bool CheckMasterPassword(string pass)
    {
        string correctPassHash = "jte8s/U2EidHtKjq50lk3w==";
        string passHash = MD5(pass);
        Debug.Log(passHash);
        if (passHash == correctPassHash)
            return true;
        return false;
    }


    static void GetLatestVersionError(CMLData response)
    {
        if ((response["message"] == "WPServer error: Empty response. No data found"))
        {
            GameObject.Find("UMenuProManager/MenuCanvas/VersionUpdatePanel").SetActive(false);
        }
    }

    public static string FormatSceneName(string sceneName)
    {
        string res = sceneName.Replace(" ", "_");
        res = res.Replace(".", "");
        return res;
    }

    public void UpdatePracticeHighscore(int score, int stars)
    {
        string practiceScene = FormatSceneName(GetSceneDatabaseName(currentSceneVisualName));

        int highscore;
        int.TryParse(DatabaseManager.FetchField("PracticeHighscores", "score_" + practiceScene), out highscore);
        if (highscore < score)
        {
            string[][] data = new string[][]
            {
                new string[] { "score_" + practiceScene, score.ToString() },
                new string[] { "stars_" + practiceScene, stars.ToString() }
            };
            DatabaseManager.UpdateCategory("PracticeHighscores", data);
        }
    }

    public static void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

    public static void OpenUrl_NewWindow(string url)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
            openWindow(url);
#else
            OpenUrl(url);
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void openWindow(string url);
#endif

    public static void __dev__customCertificate(string playerFullName, string sceneName, string date)
    {
        int keyValue = 192378; // salt
        keyValue += __sumString(playerFullName);
        keyValue += __sumString(sceneName);

        if (date == "")
        {
            date = GetTodaysDateFormatted();
        }
        keyValue += __sumString(date) * 13;

        string hexKey = Convert.ToString(keyValue, 16);
        hexKey = __trashFillString(hexKey);

        string link = "https://leren.careup.online/certificate.php";
        link += "?name=" + UnityWebRequest.EscapeURL(playerFullName);
        link += "&scene=" + UnityWebRequest.EscapeURL(sceneName);
        link += "&date=" + UnityWebRequest.EscapeURL(date);
        link += "&misc=" + hexKey;

        Debug.LogWarning("OPENING LINK " + link);
        Application.OpenURL(link);
    }

    public static string __getCertificateLinkParams(string scene, string date = "", bool mail = false)
    {
        PlayerPrefsManager manager = GameObject.FindObjectOfType<PlayerPrefsManager>();
        string name = manager.fullPlayerName;
        string email = WULogin.email;

        int keyValue = 192378; // salt
        keyValue += __sumString(name);
        keyValue += __sumString(scene);

        if (date == "")
        {
            date = GetTodaysDateFormatted();
        }
        keyValue += __sumString(date) * 13;

        string hexKey = Convert.ToString(keyValue, 16);
        hexKey = __trashFillString(hexKey);

        string link = "?";
        link += "name=" + UnityWebRequest.EscapeURL(name);
        link += "&scene=" + UnityWebRequest.EscapeURL(scene);
        link += "&date=" + UnityWebRequest.EscapeURL(date);
        link += "&misc=" + hexKey;
        if (mail)
        {
            link += "&mail=" + UnityWebRequest.EscapeURL(email);
        }

        return link;
    }

    /// <summary>
    /// Generates and opens link for certificate generation.
    /// Mirrors the safety measures of database script to generate safety key, included in the link.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="secondName"></param>
    /// <param name="scene"></param>
    /// <param name="score"></param>
    public static void __openCertificate(string scene, string date = "")
    {
        string link = "https://leren.careup.online/certificate.php";
        link += PlayerPrefsManager.__getCertificateLinkParams(scene, date);

        Debug.LogWarning("OPENING LINK " + link);
        
        OpenUrl_NewWindow(link.Replace(" ", "%20"));
    }

    public static void __sendCertificateToUserMail(string scene, string date = "")
    {
        string link = "https://leren.careup.online/Certificate_sendMail.php";
        link += PlayerPrefsManager.__getCertificateLinkParams(scene, date, true);

        Debug.LogWarning("Sending email with certificate to user.");
        UnityWebRequest unityWebRequest = new UnityWebRequest(link);
        unityWebRequest.SendWebRequest();
    }

    public static string GetTodaysDateFormatted()
    {
        string day = DateTime.Now.Day.ToString();
        if (day.Length == 1) day = "0" + day;
        string month = DateTime.Now.Month.ToString();
        if (month.Length == 1) month = "0" + month;

        string date = day + month + DateTime.Now.Year.ToString();

        return date;
    }

    static int __sumString(string str)
    {
        int sum = 0;
        for (int i = 0; i < str.Length; ++i)
        {
            sum += (byte)str[i];
        }
        return sum;
    }

    static string __trashFillString(string str)
    {
        string res = "";

        string allowedChars = "ghijkmnopqrstuvwxyzGHJKLMNOPQRSTUVWXYZ";
        System.Random random = new System.Random((int)DateTime.Now.Ticks);

        for (int i = 0; i < str.Length; ++i)
        {
            res += (random.Next(0, 1) > 0 ? str[i].ToString().ToUpper() : str[i].ToString().ToLower());
            res += allowedChars[random.Next(0, allowedChars.Length)];
        }

        return res;
    }

    public static void SetFullName(string fullName)
    {
        GameObject.FindObjectOfType<PlayerPrefsManager>().fullPlayerName = fullName;
        DatabaseManager.UpdateField("AccountStats", "FullName", fullName);
    }

    public static void SetBIGNumber(string number)
    {
        GameObject.FindObjectOfType<PlayerPrefsManager>().bigNumber = number;
        DatabaseManager.UpdateField("AccountStats", "BIG_number", number);
    }

    private void SetEscapeButtonLogic()
    {
        //if (startTimer)
        //{
        //    timeLeft -= Time.deltaTime;
        //    timeOut = timeLeft > 0f ? true : false;
        //}

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape button pressed");

            //if (timeOut == false)
            //{
            //    print("Are you sure you want to leave?");
            //    timeLeft = 3.0f;
            //    startTimer = true;
            //    return;
            //}

            manager = GameObject.FindObjectOfType<UMP_Manager>();

            // Escape button logic for login scene
            if (GameObject.Find("WULoginPrefab") != null)
            {
                mainMenu = GameObject.FindObjectOfType<MainMenu>();

                if (GameObject.Find("RegisterWindow") != null)
                {
                    GameObject.Find("LoginRegisterArea/RegisterArea")?.transform.GetChild(0)?.GetComponent<Button>().onClick.Invoke();
                }

                else if (GameObject.Find("TermsAndConditionScreen") != null && GameObject.Find("Terms_Condition_Screen") == null && GameObject.Find("Voorwaarden_Screen") == null)
                {
                    GameObject.Find("TermsAndConditionScreen/RegisterArea")?.transform.GetChild(3)?.GetComponent<Button>().onClick.Invoke();
                }

                else if (GameObject.Find("LoginWindow") != null)
                {
                    GameObject.Find("LoginRegisterArea/LoginArea")?.transform.GetChild(6)?.GetComponent<Button>().onClick.Invoke();
                }

                else if (GameObject.Find("PassResetScreen") != null)
                {
                    GameObject.Find("PassResetArea")?.transform.GetChild(2)?.GetComponent<Button>().onClick.Invoke();
                }
            }

            // Escape button logic for main menu scene
            else if (GameObject.Find("UMenuProManager") != null)
            {
                GameObject accauntAchievementWindow = manager.Windows[7];
                GameObject accauntScoresWindow = manager.Windows[8];

                if (GameObject.Find("Dialogs/DialogTestPractice") != null)
                {
                    GameObject.Find("DialogTestPractice/Panel_UI").transform.GetChild(2)?.GetComponent<Button>().onClick.Invoke();
                }

                else if (GameObject.Find("InfoBar") != null)
                {
                    GameObject.Find("InfoBar").transform.GetChild(0)?.GetComponent<Button>().onClick.Invoke();
                }

                else if (accauntAchievementWindow.activeSelf)
                {
                    accauntAchievementWindow.transform.GetChild(0)?.GetComponent<Button>().onClick.Invoke();
                    accauntAchievementWindow.transform.GetChild(0)?.GetComponent<UMP_ButtonGroup>().OnSelect();
                }

                else if (accauntScoresWindow.activeSelf)
                {
                    accauntScoresWindow.transform.GetChild(1)?.GetComponent<Button>().onClick.Invoke();
                    accauntScoresWindow.transform.GetChild(1)?.GetComponent<UMP_ButtonGroup>().OnSelect();
                }

                else if (manager.Windows[3].activeSelf)
                {
                    return;
                }

                else
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (manager.Windows[i].activeSelf)
                        {
                            GameObject quitWindowButton = GameObject.Find("Menu/slot (4)");

                            if (quitWindowButton != null)
                            {
                                quitWindowButton.GetComponent<Button>().onClick.Invoke();
                                quitWindowButton.GetComponent<Button>().GetComponent<UMP_ButtonGroup>().OnSelect();
                            }

                            break;
                        }
                    }
                }
            }

            // Escape button logic for protocol scenes
            else
            {
                if (GameObject.Find("UI/CloseBtn") != null)
                {
                    GameObject.Find("UI/CloseBtn").transform?.GetComponent<Button>().onClick.Invoke();
                }

                else if (currentScene.name == "Scenes_Character_Customisation" && WULogin.characterCreated)
                {
                    bl_SceneLoaderUtils.GetLoader.LoadLevel("MainMenu");
                }
            }

            //startTimer = false;
        }
    }

    public static string RandomString(int length)
    {
        System.Random random = new System.Random((int)DateTime.Now.Ticks);
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static void OpenShareOnFBWebPage(string sceneName = "")
    {
        if (sceneName == "")
            sceneName = GameObject.FindObjectOfType<PlayerPrefsManager>().currentSceneVisualName;

        string link = "https://www.facebook.com/sharer/sharer.php";  // main link
        link += "?u=https%3A%2F%2Fcareup.online";                         // u=link for link reference
        link += "&quote=I just passed a test for ";                  // quote=text for text quote
        link += sceneName + ". You can try it out yourself by going to https%3A%2F%2Fcareup.online";

        OpenUrl_NewWindow(link.Replace(" ", "%20"));
    }

    public static string GenerateAttendanceSXML(string BIG, string course, string id)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");

        XmlElement root = xmlDoc.CreateElement("Entry");
        xmlDoc.AppendChild(root);

#region Settings
        XmlElement settings = xmlDoc.CreateElement("Settings");
        root.AppendChild(settings);

        XmlElement userID = xmlDoc.CreateElement("userID");
        userID.InnerText = "24352";
        settings.AppendChild(userID);

        XmlElement userRole = xmlDoc.CreateElement("userRole");
        userRole.InnerText = "EDU";
        settings.AppendChild(userRole);

        XmlElement userKey = xmlDoc.CreateElement("userKey");
        userKey.InnerText = "2435202551361";
        settings.AppendChild(userKey);
        
        XmlElement orgID = xmlDoc.CreateElement("orgID");
        orgID.InnerText = "52";
        settings.AppendChild(orgID);

        XmlElement settingOutput = xmlDoc.CreateElement("settingOutput");
        settingOutput.InnerText = "1";
        settings.AppendChild(settingOutput);

        XmlElement emailOutput = xmlDoc.CreateElement("emailOutput");
        emailOutput.InnerText = "info@careup.online";
        settings.AppendChild(emailOutput);

        XmlElement languageID = xmlDoc.CreateElement("languageID");
        languageID.InnerText = "1";
        settings.AppendChild(languageID);

        XmlElement defaultLanguageID = xmlDoc.CreateElement("defaultLanguageID");
        defaultLanguageID.InnerText = "1";
        settings.AppendChild(defaultLanguageID);
#endregion

#region Attendance
        XmlElement attendance = xmlDoc.CreateElement("Attendance");
        root.AppendChild(attendance);
        
        XmlElement PECourseID = xmlDoc.CreateElement("PECourseID");
        PECourseID.InnerText = id;
        attendance.AppendChild(PECourseID);

        XmlElement externalModuleID = xmlDoc.CreateElement("externalmoduleID");
        externalModuleID.InnerText = course;
        attendance.AppendChild(externalModuleID);

        XmlElement externalPersonID = xmlDoc.CreateElement("externalPersonID");
        externalPersonID.InnerText = BIG;
        attendance.AppendChild(externalPersonID);
        
        XmlElement endDate = xmlDoc.CreateElement("endDate");
        endDate.InnerText = DateTime.Now.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK")
            .Replace("+", "%2B"); // "+" sign is operator in url, need to replace
        attendance.AppendChild(endDate);
#endregion

        return xmlDoc.OuterXml;
    }

    public void HandleLoginToken()
    {
        // get login token
#if UNITY_WEBGL && !UNITY_EDITOR
        string currentLoginToken;
        currentLoginToken = GetStringParams();
#endif
#if UNITY_EDITOR
        //currentLoginToken = "token12asudh"; // let's pretend we got it
        //currentLoginToken = "abcdefg123456"; // let's pretend we got it

        // fetch date from db, follows into next function
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
        WUData.FetchSharedField(currentLoginToken, "LoginTokens", CheckLoginToken_success, -1, CheckLoginToken_error);
#endif

    }

    public void CheckLoginToken_error(CMLData response)
    {
        Debug.Log(response.ToString());
        // most likely if we're here - no token found
    }

    public void CheckLoginToken_success(CML response)
    {
        string tokenValue;
        if (response.Elements.Count > 1 && response.Elements[1].Values.Length > 2)
        {
            tokenValue = response.Elements[1].Values[2];
            string[] split = tokenValue.Split(' ');
            if (split.Length > 1)
            {
                string base64LoginName = split[0];
                string base64LoginPass = split[1];
                if (base64LoginName != "" && base64LoginPass != "")
                {
                    byte[] loginData = Convert.FromBase64String(base64LoginName);
                    string decodedLogin = Encoding.UTF8.GetString(loginData);
                    byte[] passData = Convert.FromBase64String(base64LoginPass);
                    string decodedPass = Encoding.UTF8.GetString(passData);

                    GameObject.FindObjectOfType<WUUGLoginGUI>().DoAutoLogin(decodedLogin, decodedPass);
                }
            }
        }
    }
}