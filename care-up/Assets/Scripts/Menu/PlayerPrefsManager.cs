﻿using UnityEngine;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using LoginProAsset;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.PostProcessing;
using MBS;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using PaperPlaneTools;

/// <summary>
/// Handles quick access to saved data.
/// Volume | Tutorial completion | Scene completion + results
/// </summary>
public class PlayerPrefsManager : MonoBehaviour
{
    public bool VR = true;
    public bool practiceMode = true;

    // store value here after getting from server
    public bool tutorialCompleted;

    private static PlayerPrefsManager instance;

    private List<string> activatedScenes = new List<string>();

    // sets up after selecting scene in "scene selection"
    public string currentSceneVisualName;
    public bool validatedScene;

    // post processing on camera
    public bool postProcessingEnabled = false;

    // is this game version is demo version
    public bool demoVersion = false;

    // indicates if game is played in testing mode (only editor mode)
    public bool testingMode = false;

    // save info about subscription
    [HideInInspector]
    public bool subscribed = false;
    [HideInInspector]
    public int plays = 0;
    
    // used for storing scene name for test hightscore loading
    private static string currentTestScene = "";
    public static float currentTestScore = 0;

    public static int currentPracticeScore = 0;
    public static int currentPracticeStars = 0;

    private static string practiceScene = "";
    public static int practicePlays = 0;
    
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

    private void OnLoaded(Scene s, LoadSceneMode m)
    {
        transform.position =
        GameObject.FindObjectOfType<AudioListener>().transform.position;

        if (!(s.name == "Launch me 1" ||
              s.name == "MainMenu" ||
              s.name == "SceneSelection" ||
              s.name == "Scenes_Character_Customisation"))
        {
            // game scenes
            GetComponent<AudioSource>().Stop();
            if (Camera.main.GetComponent<PostProcessingBehaviour>() != null)
            {
                Camera.main.GetComponent<PostProcessingBehaviour>().enabled = postProcessingEnabled;
            }
        }

        if (s.name == "EndScore" ||
            (s.name == "MainMenu" && 
            !GetComponent<AudioSource>().isPlaying))
        {
            GetComponent<AudioSource>().Play();
        }

        if (s.name == "MainMenu")
        {
            GameObject.Find("UMenuProManager/MenuCanvas/Opties/Panel_UI/PostProcessingToggle").GetComponent<Toggle>().isOn = postProcessingEnabled;
        }
    }

    void Awake()
    {
        if(!Application.isEditor)
        {
            testingMode = false;
        }
        if ( instance )
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    void Start()
    {
        SceneManager.sceneLoaded += OnLoaded;

        AudioListener.volume = Volume;
        Debug.Log("Volume is set to saved value: " + Volume);

        postProcessingEnabled = PlayerPrefs.GetInt("PostProcessing") == 1;
        Debug.Log("PostProcessing is set to saved value: " + postProcessingEnabled);
    }

    public float Volume
    {
        get { return PlayerPrefs.HasKey("Volume") ? PlayerPrefs.GetFloat("Volume") : 1.0f; }
        set { PlayerPrefs.SetFloat("Volume", value); }
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

        foreach(string data in datas)
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
                tutorialCompleted = false;// store for current session
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
        foreach(string data in datas)
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
        WUData.FetchField("Plays_Number", "AccountStats", GetPlaysNumber, -1, GetPlaysNumber_Error);
    }
    
    static void GetPlaysNumber(CML response)
    {
        PlayerPrefsManager manager = GameObject.FindObjectOfType<PlayerPrefsManager>();
        manager.plays = response[1].Int("Plays_Number") + 1;
     
        RateBox.Instance.IncrementCustomCounter();
        RateBox.Instance.Show();

        // update +1
        CMLData data = new CMLData();
        data.Set("Plays_Number", manager.plays.ToString());
        WUData.UpdateCategory("AccountStats", data);
    }

    static void GetPlaysNumber_Error(CMLData response)
    {
        if ((response["message"] == "WPServer error: Empty response. No data found"))
        {
            // empty response, need to create field with 1 play
            PlayerPrefsManager manager = GameObject.FindObjectOfType<PlayerPrefsManager>();
            manager.plays = 1;
            CMLData data = new CMLData();
            data.Set("Plays_Number", "1");
            WUData.UpdateCategory("AccountStats", data);
        }
    }

    public static void __sendMail(string topic, string message)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {   // apparently SMTP doesnt work with webgl
            __sendMailApp(topic, message);
            return;
        }
        else
        {
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress("info@careup.nl");
            mail.To.Add("info@careup.nl");
            mail.Subject = topic;
            mail.Body = message;

            SmtpClient smtpServer = new SmtpClient("smtp.strato.de");
            smtpServer.Port = 587;
            smtpServer.Credentials = new System.Net.NetworkCredential("info@careup.nl", "TripleMotionMedia3") as ICredentialsByHost;
            smtpServer.EnableSsl = true;

            ServicePointManager.ServerCertificateValidationCallback =
                delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };

            smtpServer.Send(mail);
        }
    }

    public static string MyEscapeURL(string url)
    {
        return WWW.EscapeURL(url).Replace("+", "%20");
    }
    
    public static void __sendMailApp(string topic, string message)
    {
        topic = MyEscapeURL(topic);
        message = MyEscapeURL(message);
        Application.OpenURL("mailto:" + "info@careup.nl" + "?subject=" + topic + "&body=" + message);
    }

    public void UpdateTestHighscore(float score)
    {        
		currentTestScore = score * 100;
        currentTestScene = FormatSceneName(currentSceneVisualName);
        
        WUData.FetchField(currentTestScene, "TestHighscores", GetTestHighscore, -1, GetTestHighscore_Error);
    }

    static void GetTestHighscore(CML response)
    {        
		float highscore = response[1].Float(currentTestScene);
        if (highscore < currentTestScore)
        {
            CMLData data = new CMLData();            
			data.Set(currentTestScene, currentTestScore.ToString());
            WUData.UpdateCategory("TestHighscores", data);
        }
    }

    static void GetTestHighscore_Error(CMLData response)
    {
        if ((response["message"] == "WPServer error: Empty response. No data found"))
        {
            CMLData data = new CMLData();
            data.Set(currentTestScene, currentTestScore.ToString());
            WUData.UpdateCategory("TestHighscores", data);
        }
    }

    public static void AddOneToPracticePlays(string scene)
    {
        // pretty sure it is safe to use this variable again
        practiceScene = FormatSceneName(scene);

        WUData.FetchField(practiceScene, "PracticePlays", GetPracticePlays, -1, GetPracticePlays_Error);
    }

    static void GetPracticePlays(CML response)
    {
        int plays = response[1].Int(practiceScene);

        CMLData data = new CMLData();
        data.Set(practiceScene, (plays + 1).ToString());
        WUData.UpdateCategory("PracticePlays", data);
    }

    static void GetPracticePlays_Error(CMLData response)
    {
        if ((response["message"] == "WPServer error: Empty response. No data found"))
        {
            // if no data found when we're adding +1, create data with 1
            CMLData data = new CMLData();
            data.Set(practiceScene, "1");
            WUData.UpdateCategory("PracticePlays", data);
        }
    }

    public void FetchPracticePlays(string scene)
    {
        practiceScene = FormatSceneName(scene);

        WUData.FetchField(practiceScene, "PracticePlays", FetchPracticePlays_success, -1, FetchPracticePlays_Error);
    }
    
    static void FetchPracticePlays_success(CML response)
    {
        Debug.Log(response.ToString());
        if (GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" + "DialogTestPractice/Panel_UI/Buttons/TestButton") != null) 
        {
            Button testBtn = GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                "DialogTestPractice/Panel_UI/Buttons/TestButton").GetComponent<Button>();
            
            int plays = response[1].Int(practiceScene);
            testBtn.interactable = plays >= 3;
        }

        GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
            "DialogTestPractice/Panel_UI/Buttons/TestButton/contentlocked/practiceamount")
            .GetComponent<Text>().text = plays.ToString();

        if (testBtn.interactable)
        {
            GameObject.FindObjectOfType<PlayerPrefsManager>().FetchTestHighscore(practiceScene);
        }

        GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
            "DialogTestPractice/Panel_UI/Buttons/TestButton/contentunlocked").SetActive(testBtn.interactable);
        GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
            "DialogTestPractice/Panel_UI/Buttons/TestButton/contentlocked").SetActive(!testBtn.interactable);
    }

    static void FetchPracticePlays_Error(CMLData response)
    {
        Debug.Log(response.ToString());

        if ((response["message"] == "WPServer error: Empty response. No data found"))
        {
            // no data == 0 plays
            practicePlays = 0;
            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
            "DialogTestPractice/Panel_UI/Buttons/TestButton").GetComponent<Button>().interactable = false;

            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                "DialogTestPractice/Panel_UI/Buttons/TestButton/contentlocked/practiceamount")
                .GetComponent<Text>().text = "0";
        }
    }

    public void FetchTestHighScores()
    {
        WUData.FetchCategory("TestHighscores", GetAllHighScores);
    }

    static void GetAllHighScores(CML response)
    {
        //print(response.ToString());
        for(int i = 0; i < response.Elements[1].Keys.Length; ++i)
        {
            switch(response.Elements[1].Keys[i])
            {
                // we skip these keys cuz they hold no useful info about scenes
                case "id":
                case "category":
                case "woocommerce-login-nonce":
                case "_wpnonce":
                    continue;
                default:
                    // here we get actual scenes and values
                    string sceneName = response.Elements[1].Keys[i].Replace("_", " ");

                    float fPercent = 0.0f;
                    float.TryParse(response.Elements[1].Values[i], out fPercent);
                    int percent = Mathf.FloorToInt(fPercent);

                    bool passed = percent > 70;

                    if (percent <= 0 || percent > 100)
                        continue; // don't show 0 percent scores as they are not completed even once

                    GameObject layoutGroup = GameObject.Find("UMenuProManager/MenuCanvas/Account_Scores/Account_Panel_UI/ScoresHolder/Scores/LayoutGroup");
                    GameObject scoreObject = Instantiate(Resources.Load<GameObject>("Prefabs/UI/TestHighscore"), layoutGroup.transform);
                    scoreObject.transform.Find("SceneName").GetComponent<Text>().text = sceneName;
                    scoreObject.transform.Find("Percent").GetComponent<Text>().text = percent.ToString() + "%";
                    scoreObject.transform.Find("Passed").GetComponent<Text>().text = 
                        (passed ? "Voldoende" : "Onvoldoende");
                    
                    break;
            }
        }
    }

    public void SetTutorialCompletedWU()
    {
        tutorialCompleted = true;

        CMLData data = new CMLData();
        data.Set("TutorialCompleted", "true");
        WUData.UpdateCategory("AccountStats", data);
    }

    public void GetTutorialCompletedWU()
    {
        WUData.FetchField("TutorialCompleted", "AccountStats", GetTutorialCompleted, 
            -1, GetTutorialCompleted_Error);
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

        if (currentVersion != latestVersion)
        {
            // player can download new version
            GameObject.Find("UMenuProManager/MenuCanvas/VersionUpdatePanel").SetActive(true);
        }
        else
        {
            GameObject.Find("UMenuProManager/MenuCanvas/VersionUpdatePanel").SetActive(false);
        }
    }

    static void GetLatestVersionError(CMLData response)
    {
        if ((response["message"] == "WPServer error: Empty response. No data found"))
        {
            GameObject.Find("UMenuProManager/MenuCanvas/VersionUpdatePanel").SetActive(false);
        }
    }

    static string FormatSceneName(string sceneName)
    {
        string res = sceneName.Replace(" ", "_");
        res = res.Replace(".", "");
        return res;
    }

    public void UpdatePracticeHighscore(int score, int stars)
    {
        currentPracticeScore = score;
        currentPracticeStars = stars;
        practiceScene = FormatSceneName(currentSceneVisualName);

        WUData.FetchCategory("PracticeHighscores", GetPracticetHighscore, -1, GetPracticeHighscore_Error);
    }

    static void GetPracticetHighscore(CML response)
    {
        int highscore = response[1].Int("score_" + practiceScene);
        if (highscore < currentPracticeScore)
        {
            CMLData data = new CMLData();
            data.Set("score_" + practiceScene, currentPracticeScore.ToString());
            data.Set("stars_" + practiceScene, currentPracticeStars.ToString());
            WUData.UpdateCategory("PracticeHighscores", data);
        }
    }

    static void GetPracticeHighscore_Error(CMLData response)
    {
        if ((response["message"] == "WPServer error: Empty response. No data found"))
        {
            CMLData data = new CMLData();
            data.Set("score_" + practiceScene, currentPracticeScore.ToString());
            data.Set("stars_" + practiceScene, currentPracticeStars.ToString());
            WUData.UpdateCategory("PracticeHighscores", data);
        }
    }

    public void FetchPracticeHighscore(string scene)
    {
        practiceScene = FormatSceneName(scene);

        WUData.FetchCategory("PracticeHighscores", FetchPracticeHighscore, -1, FetchPracticeHighscore_Error);
    }

    static void FetchPracticeHighscore(CML response)
    {
        int highscore = response[1].Int("score_" + practiceScene);
        int stars = response[1].Int("stars_" + practiceScene);
        
        GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
            "DialogTestPractice/Panel_UI/Buttons/PracticeButton/content/score").
            GetComponent<Text>().text = highscore.ToString();
        
        GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
            "DialogTestPractice/Panel_UI/Buttons/PracticeButton/content/Stars/Star1")
            .SetActive(stars >= 1.0f);

        GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
            "DialogTestPractice/Panel_UI/Buttons/PracticeButton/content/Stars/Star2")
            .SetActive(stars >= 2.0f);

        GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
            "DialogTestPractice/Panel_UI/Buttons/PracticeButton/content/Stars/Star3")
            .SetActive(stars >= 3.0f);
    }

    static void FetchPracticeHighscore_Error(CMLData response)
    {
        if ((response["message"] == "WPServer error: Empty response. No data found"))
        {
            CMLData data = new CMLData();
            data.Set("score_" + practiceScene, currentPracticeScore.ToString());
            data.Set("stars_" + practiceScene, currentPracticeStars.ToString());
            WUData.UpdateCategory("PracticeHighscores", data);

            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                "DialogTestPractice/Panel_UI/Buttons/PracticeButton/content/score").
                GetComponent<Text>().text = "0";
            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                "DialogTestPractice/Panel_UI/Buttons/PracticeButton/content/Stars/Star1")
                .SetActive(false);
            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                "DialogTestPractice/Panel_UI/Buttons/PracticeButton/content/Stars/Star2")
                .SetActive(false);
            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                "DialogTestPractice/Panel_UI/Buttons/PracticeButton/content/Stars/Star3")
                .SetActive(false);
        }
    }

    public void FetchTestHighscore(string scene)
    {
        currentTestScene = FormatSceneName(scene);

        WUData.FetchField(currentTestScene, "TestHighscores", FetchTestHighscore, -1, FetchTestHighscore_Error);
    }

    static void FetchTestHighscore(CML response)
    {
        float highscore = response[1].Float(currentTestScene);

        GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/" +
                "DialogTestPractice/Panel_UI/Buttons/TestButton/contentunlocked/percentage")
                .GetComponent<Text>().text = Mathf.RoundToInt(highscore).ToString() + "%";
    }

    static void FetchTestHighscore_Error(CMLData response)
    {
        if ((response["message"] == "WPServer error: Empty response. No data found"))
        {
            CMLData data = new CMLData();
            data.Set(currentTestScene, currentTestScore.ToString());
            WUData.UpdateCategory("TestHighscores", data);
        }
    }
}
