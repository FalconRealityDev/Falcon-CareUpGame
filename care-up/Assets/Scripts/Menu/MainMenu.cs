using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MBS;
using System.Linq;
using System;
using System.Runtime.InteropServices;

public class MainMenu : MonoBehaviour
{
    private LoadingScreen loadingScreen;
    private PlayerPrefsManager prefs;
    public string eMail = "info@triplemotion.nl";

    public static string storeUrl;

    [SerializeField]
    private Text reward = default;

    [SerializeField]
    private GameObject rewardPanel = default;

    public GameObject UpdatesPanel;

    [System.Serializable]
    public class ResendingLock
    {
        public string sceneName;
        public int timeRemaining;

        public ResendingLock(string name, int time)
        {
            sceneName = name;
            timeRemaining = time;
        }
    };

    [UnityEngine.SerializeField]
    public List<ResendingLock> resendingLocks = new List<ResendingLock>();


#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            bl_SceneLoaderUtils.GetLoader.LoadLevel("Scenes_Character_Editor");
        }
    }

#endif  

    private void Start()
    {

        if (GameObject.Find("Preferences") != null)
        {
            loadingScreen = GameObject.Find("Preferences").GetComponent<LoadingScreen>();
            if (loadingScreen == null) Debug.LogError("No loading screen found");

            prefs = GameObject.Find("Preferences").GetComponent<PlayerPrefsManager>();
        }
        else
        {
            Debug.LogWarning("No 'preferences' found. Game needs to be started from first scene");
        }

        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            // Text text = GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/DialogTestPractice/Panel_UI/FreeDemoPlayCounter")
            //     .GetComponent<Text>();

            // if (!prefs.subscribed)
            // {
            //     WUData.FetchField("Plays_Number", "AccountStats", GetPlaysNumber, -1, ErrorHandle);
            //     text.text = "Je kunt nog " + (5 - prefs.plays) + " handelingen proberen.";
            // }
            // else
            // {
            //     text.text = "";
            // }

            //handle updates panel           

            if (EndScoreManager.showReward)
            {
                StoreViewModel.ShowRewardDialogue(reward);
                EndScoreManager.showReward = false;
            }

            if (CareUpNotification.showReward)
            {
                StoreViewModel.ShowRewardDialogue(reward, rewardPanel, 20);
                CareUpNotification.showReward = false;
            }

            bool updatesSeen = PlayerPrefs.GetInt("_updatesSeen") == 1;
            string versionSeen = PlayerPrefs.GetString("__version", "");
            string currentVersion = Application.version;
            if (PlayerPrefsManager.forceShowUpdatesPanel)
            {
                UpdatesPanel.SetActive(true);
                PlayerPrefs.SetInt("_updatesSeen", 0);
                PlayerPrefs.SetString("__version", "0.0.1");
            }
            else if (updatesSeen == false && versionSeen != currentVersion)
            {
               UpdatesPanel.SetActive(true);
               PlayerPrefs.SetInt("_updatesSeen", 1);
               PlayerPrefs.SetString("__version", currentVersion);
            }
            else
            {
               UpdatesPanel.SetActive(false);
            }

            // set up highscores something?
            string[][] highScores = DatabaseManager.FetchCategory("TestHighscores");
            if (highScores != null)
            {
                foreach (string[] score in highScores)
                {
                    // fetch date before formatting scene name back
                    string date = DatabaseManager.FetchField("CertificateDates", score[0]);
                    date = (date == "") ? "27052019" : date;
                    string sceneBaseName = prefs.GetSceneDisplayName(score[0]);
                    string sceneName = sceneBaseName.Replace("_", " ");

                    float fPercent = 0.0f;
                    float.TryParse(score[1].Replace(",", "."), out fPercent);
                    int percent = Mathf.FloorToInt(fPercent);

                    bool passed = percent > 70;

                    if (percent <= 0 || percent > 100)
                        continue; // don't show 0 percent scores as they are not completed even once

                    // FILTERing old scenes that should not be displayed anymore
                    if (sceneName == "Injecteren intramusculair (loodrechttechniek) - Flacon" ||
                       sceneName == "AED")
                        continue;

                    GameObject layoutGroup = GameObject.Find("/UMenuProManager/MenuCanvas/LayoutPanel/Tabs/Account_Scores/ContentPanel/Scenes/Account_Panel_UI/Panel/ProtocolList/ProtocolsHolder/Protocols/content");
                    GameObject scoreObject = Instantiate(Resources.Load<GameObject>("NecessaryPrefabs/UI/TestHighscore"), layoutGroup.transform);

                    if(sceneName == "Injecteren intramusculair (loodrechttechniek)")
                    {
                        sceneName = "Intramusculair injecteren (stretchtechniek)";
                    }

#if UNITY_EDITOR
                    Debug.Log($"Certificate is available for {sceneName}");
#endif
                    scoreObject.transform.Find("SceneName").GetComponent<Text>().text = sceneName;

                    scoreObject.transform.Find("Percent").GetComponent<Text>().text = percent.ToString() + "%";
                    scoreObject.transform.Find("Percent").GetComponent<Text>().color =
                        (passed ? Color.green : Color.red);

                    scoreObject.transform.Find("Button").GetComponent<Button>().interactable = passed;
                    scoreObject.transform.Find("Button").GetComponent<Button>().onClick.AddListener
                        (delegate { PlayerPrefsManager.__openCertificate(sceneName, date); });
                }
            }

            // shared field, will keep it outside DatabaseManager
            GameObject.FindObjectOfType<PlayerPrefsManager>().FetchLatestVersion();

            GameObject.Find("/UMenuProManager/MenuCanvas/LayoutPanel/Tabs/Account/TitlePanel/UserNamePanel/Text").GetComponent<Text>().text = MBS.WULogin.display_name;

            string bigNumber = GameObject.FindObjectOfType<PlayerPrefsManager>().bigNumber;
            string fullName = GameObject.FindObjectOfType<PlayerPrefsManager>().fullPlayerName;
            if (!string.IsNullOrEmpty(fullName))
            {
                GameObject.Find("/UMenuProManager/MenuCanvas/LayoutPanel/Tabs/Account/ContentPanel/Elements/NamePanel/NameHolder/InfoPanel/InfoHolder/UserName/Panel/Account_Username")
               .GetComponent<Text>().text = fullName;
            }

            GameObject.Find("/UMenuProManager/MenuCanvas/LayoutPanel/Tabs/Account/ContentPanel/Elements/NamePanel/NameHolder/InfoPanel/InfoHolder/UserNumber/BigNumber")
            .GetComponent<Text>().text = bigNumber;
        }
    }

    public void ResendCertificate(string scene, string date)
    {
        // check if can send
        bool flag = (resendingLocks.Where(x => x.sceneName == scene).Count() == 0);

        if (flag)
        {
            // send if so
            //PlayerPrefsManager.__sendCertificateToUserMail(scene, date);

            // show pop up that it's sent
            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/CertificatePopOp").SetActive(true);

            // add lock, time in seconds
            ResendingLock rLock = new ResendingLock(scene, 300);
            resendingLocks.Add(rLock);

            // set timer to unlock
            StartCoroutine(UnlockResending(rLock));
        }
        else
        {
            // can't send, show different pop up
            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/CertificateBlockedPopOp").SetActive(true);

            // set up time
            int timeLeft = resendingLocks.Where(x => x.sceneName == scene).First().timeRemaining;
            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/CertificateBlockedPopOp/Certificate/Remaining").
                GetComponent<Text>().text = "Resterende tijd: " + (timeLeft / 60) + "m " + (timeLeft % 60) + "s";
        }
    }

    IEnumerator UnlockResending(ResendingLock rLock)
    {
        while (rLock.timeRemaining > 0)
        {
            yield return new WaitForSeconds(1f);
            --rLock.timeRemaining;
        }

        resendingLocks.Remove(rLock);
        Debug.Log(rLock.sceneName + " scene certificate and be sent again.");
    }

    public void UpdateLatestVersionDev()
    {
        // makes current version - latest on database for further comparison
        CMLData data = new CMLData();
        data.Set("LatestVersion", Application.version);
        WUData.UpdateSharedCategory("GameInfo", data);
    }

    public void OnStartButtonClick()
    {
        if (prefs.tutorialCompleted || prefs.TutorialPopUpDeclined)
        {
            loadingScreen.LoadLevel("SceneSelection");
        }
        else
        {
            GameObject canvas = GameObject.Find("Canvas");

            canvas.transform.Find("MainMenu").gameObject.SetActive(false);
            //canvas.transform.Find("Logo").gameObject.SetActive(false);
            // canvas.transform.Find("OptionsBtn").gameObject.SetActive(false);

            canvas.transform.Find("TutorialPopUp").gameObject.SetActive(true);
        }
    }

    public void OnStartYes()
    {
        bl_SceneLoaderUtils.GetLoader.LoadLevel("Tutorial");
    }

    public void OnStartNo()
    {
        prefs.TutorialPopUpDeclined = true;
        bl_SceneLoaderUtils.GetLoader.LoadLevel("SceneSelection");
    }

    public void OnQuitButtonClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnMainMenuButtonClick()
    {
        bl_SceneLoaderUtils.GetLoader.LoadLevel("MainMenu");
    }

    public void OnTutorialButtonClick()
    {
        bl_SceneLoaderUtils.GetLoader.LoadLevel("Tutorial", "tutorial");
    }

    public void OnOptionsButtonClick()
    {
        GameObject canvas = GameObject.Find("Canvas");

        //canvas.transform.Find("MainMenu").gameObject.SetActive(false);
        canvas.transform.Find("MainMenu").gameObject.SetActive(false);
        //canvas.transform.Find("OptionsBtn").gameObject.SetActive(false);
        canvas.transform.Find("Opties").gameObject.SetActive(true);
    }

    public void OnOptionsBackButtonClick()
    {
        GameObject canvas = GameObject.Find("Canvas");

        //canvas.transform.Find("MainMenu").gameObject.SetActive(true);
        canvas.transform.Find("MainMenu").gameObject.SetActive(true);
        //canvas.transform.Find("OptionsBtn").gameObject.SetActive(true);
        canvas.transform.Find("Opties").gameObject.SetActive(false);
    }

    public void OnControlsButtonClick()
    {
        GameObject canvas = GameObject.Find("Canvas");

        //canvas.transform.Find("MainMenu").gameObject.SetActive(false);
        canvas.transform.Find("MainMenu").gameObject.SetActive(false);
        // canvas.transform.Find("OptionsBtn").gameObject.SetActive(false);
        canvas.transform.Find("ControlsUI").gameObject.SetActive(true);
    }

    public void OnControlsCloseButtonClick()
    {
        GameObject canvas = GameObject.Find("Canvas");

        //canvas.transform.Find("MainMenu").gameObject.SetActive(true);
        canvas.transform.Find("MainMenu").gameObject.SetActive(true);
        //canvas.transform.Find("OptionsBtn").gameObject.SetActive(true);
        canvas.transform.Find("ControlsUI").gameObject.SetActive(false);
    }

    public void OnBugReportButtonClick()
    {
        GameObject canvas = GameObject.Find("Canvas");

        //canvas.transform.Find("MainMenu").gameObject.SetActive(false);
        canvas.transform.Find("MainMenu").gameObject.SetActive(false);
        //canvas.transform.Find("OptionsBtn").gameObject.SetActive(false);
        canvas.transform.Find("BugReportUI").gameObject.SetActive(true);
    }

    public void OnBugReportCloseButtonClick()
    {
        GameObject canvas = GameObject.Find("Canvas");

        //canvas.transform.Find("MainMenu").gameObject.SetActive(true);
        canvas.transform.Find("MainMenu").gameObject.SetActive(true);
        //canvas.transform.Find("OptionsBtn").gameObject.SetActive(true);
        canvas.transform.Find("BugReportUI").gameObject.SetActive(false);

    }

    public void CloseUIBtn(GameObject ui)
    {
        ui.SetActive(false);
    }

    public void OnUpdatestCloseButtonClick()
    {
        //turning of the updates panel when button is clicked
        UpdatesPanel.SetActive(false);
    }

    public void OnSendEmail()
    {
        System.Diagnostics.Process.Start(("mailto:" + eMail + "?subject=" + "Fout melding Care-Up."
        + "&body="
        ));
        GameObject.Find("MessageWindow").GetComponent<TimedPopUp>().Set("Uw mailprogramma wordt geopend.");
    }

    public void ShowReward()
    {
        if (!StoreViewModel.ShowRewardDialogue(reward, rewardPanel))
        {
            OnRetryButtonClick();
        }
    }

    public void OnRetryButtonClick()
    {
        PlayerPrefsManager.AddOneToPlaysNumber();

        EndScoreManager manager = loadingScreen.GetComponent<EndScoreManager>();

        //if (prefs.practiceMode)
        //{
        //    PlayerPrefsManager.AddOneToPracticePlays(prefs.currentSceneVisualName);
        //}
        //else
        //{
        //    PlayerPrefsManager.AddOneToTestPlays(prefs.currentSceneVisualName);
        //}

        bl_SceneLoaderUtils.GetLoader.LoadLevel(manager.completedSceneName, manager.completedSceneBundle);
    }

    public void OnToggleAcceptTermsAndConditions(Button button)
    {
        button.interactable = !button.interactable;
    }

    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

    public void OpenUrl_NewWindow(string url)
    {
#if UNITY_WEBGL && ! UNITY_EDITOR
        openWindow(url);
#else
        OpenUrl(url);
#endif
    }

    public void OpenStoreUrl()
    {
        if (string.IsNullOrEmpty(storeUrl))
            storeUrl = "https://careup.online/cursussen-zorg/";

        OpenUrl_NewWindow(storeUrl);
    }

#if UNITY_WEBGL 

    [DllImport("__Internal")]
    private static extern void openWindow(string url);
#endif

    public void OnTutorialButtonClick_Interface()
    {
        string sceneName = "Tutorial_UI";
        string bundleName = "tutorial_ui";
        bl_SceneLoaderUtils.GetLoader.LoadLevel(sceneName, bundleName);
    }

    public void OnTutorialButtonClick_Movement()
    {
        string sceneName = "Tutorial_Movement";
        string bundleName = "tutorial_move";
        bl_SceneLoaderUtils.GetLoader.LoadLevel(sceneName, bundleName);
    }

    public void OnTutorialButtonClick_Picking()
    {
        string sceneName = "Tutorial_Picking";
        string bundleName = "tutorial_pick";
        bl_SceneLoaderUtils.GetLoader.LoadLevel(sceneName, bundleName);
    }

    public void OnTutorialButtonClick_Combining()
    {
        string sceneName = "Tutorial_Combining";
        string bundleName = "tutorial_combine";
        bl_SceneLoaderUtils.GetLoader.LoadLevel(sceneName, bundleName);
    }

    public void OnTutorialButtonClick_UsingOn()
    {
        string sceneName = "Tutorial_UseOn";
        string bundleName = "tutorial_useon";
        bl_SceneLoaderUtils.GetLoader.LoadLevel(sceneName, bundleName);
    }

    public void OnTutorialButtonClick_PersonDialogues()
    {
        string sceneName = "Tutorial_Talk";
        string bundleName = "tutorial_talking";
        bl_SceneLoaderUtils.GetLoader.LoadLevel(sceneName, bundleName);
    }

    public void OnTutorialButtonClick_Sequences()
    {
        string sceneName = "Tutorial_Sequence";
        string bundleName = "tutorial_sequences";
        bl_SceneLoaderUtils.GetLoader.LoadLevel(sceneName, bundleName);
    }

    public void OnTutorialButtonClick_Theory()
    {
        string sceneName = "Tutorial_Theory";
        string bundleName = "tutorial_theory";
        bl_SceneLoaderUtils.GetLoader.LoadLevel(sceneName, bundleName);
    }

    public void OnTutorialButtonClick_Full()
    {
        string sceneName = "Tutorial_Full";
        string bundleName = "tutorial_full";
        bl_SceneLoaderUtils.GetLoader.LoadLevel(sceneName, bundleName);
    }

    public void DownloadCertificate()
    {
        EndScoreManager endScoreManager = GameObject.FindObjectOfType<EndScoreManager>();
        if (endScoreManager != null)
            endScoreManager.OpenCertificateBtn();
    }
}
