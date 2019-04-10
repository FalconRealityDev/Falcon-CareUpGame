﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MBS;

public class MainMenu : MonoBehaviour {
    
    private LoadingScreen loadingScreen;
    private PlayerPrefsManager prefs;
	public string eMail="info@triplemotion.nl";

    public GameObject UpdatesPanel;

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
            Text text = GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/DialogTestPractice/Panel_UI/FreeDemoPlayCounter")
                .GetComponent<Text>();

            if (!prefs.subscribed)
            {
                WUData.FetchField("Plays_Number", "AccountStats", GetPlaysNumber, -1, ErrorHandle);
                text.text = "Je kunt nog " + (5 - prefs.plays) + " handelingen proberen.";
            }
            else
            {
                text.text = "";
            }

            //handle updates panel
            bool updatesSeen = PlayerPrefs.GetInt("_updatesSeen") == 1;
            string versionSeen = PlayerPrefs.GetString("__version", "");
            string currentVersion = Application.version;

            if (updatesSeen == false && versionSeen != currentVersion)
            {
                UpdatesPanel.SetActive(true);
                PlayerPrefs.SetInt("_updatesSeen", 1);
                PlayerPrefs.SetString("__version", currentVersion);
            }
            else
            {
                UpdatesPanel.SetActive(false);
            }

            GameObject.FindObjectOfType<PlayerPrefsManager>().FetchTestHighScores();
			
            GameObject.FindObjectOfType<PlayerPrefsManager>().FetchLatestVersion();

            GameObject.Find("UMenuProManager/MenuCanvas/Account/Account_Panel_UI/Account_Username")
                .GetComponent<Text>().text = MBS.WULogin.display_name;
        }
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
    public void OnUpdatestCloseButtonClick()
    {
        //turning of the updates panel when button is clicked
        UpdatesPanel.SetActive(false);
    }

    public void OnSendEmail()
	{
		System.Diagnostics.Process.Start (("mailto:" + eMail + "?subject=" + "Fout melding Care-Up."
		+ "&body="
		));
        GameObject.Find("MessageWindow").GetComponent<TimedPopUp>().Set("Uw mailprogramma wordt geopend.");
    }

    public void OnRetryButtonClick()
    {
        PlayerPrefsManager.AddOneToPlaysNumber();
        EndScoreManager manager = loadingScreen.GetComponent<EndScoreManager>();
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

    public void OnTutorialButtonClick_Full () {
        string sceneName = "Tutorial_Full";
        string bundleName = "tutorial_full";
        bl_SceneLoaderUtils.GetLoader.LoadLevel (sceneName, bundleName);
    }

    void GetPlaysNumber(CML response)
    {
        // we're here only if we got data
        int plays = response[1].Int("Plays_Number");
        bool result = plays < 5 ? true : false;
        AllowDenyContinue(result);
    }

    void ErrorHandle(CMLData response)
    {
        // we're here if we got error or no data which should be equal to 0 plays
        AllowDenyContinue((response["message"] == "WPServer error: Empty response. No data found"));
    }

    void AllowDenyContinue(bool allow)
    {
        allow |= FindObjectOfType<PlayerPrefsManager>().demoVersion;
        if (!allow)
        {
            // show pop up!
            //StatusMessage.Message = "Je hebt geen actief product";
            // or something more like
            GameObject.FindObjectOfType<UMP_Manager>().ShowDialog(5);
        }
    }
}
