﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour {

    private static LoadingScreen loadingScreen;

    public string bundleName;
    public string sceneName;

    public bool multiple;
    public string displayName;
    public Sprite image;
    public bool testDisabled;

    private static Transform sceneInfoPanel;
    private static PlayerPrefsManager manager;

    private static Transform leaderboard;
    private static Transform scores;
    private static Transform names;

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
    };
    
    public List<Info> variations = new List<Info>();

    public bool buy = false;

    private void Start()
    {
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
    }

    public void OnLevelButtonClick()
    {
        if (buy)
        {
            // show dialogue now instead
            GameObject.FindObjectOfType<UMP_Manager>().ShowDialog(5);
        }
        else
        {
            LevelButton mainBtn = GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/DialogTestPractice/Panel_UI/Start").GetComponent<LevelButton>();

            if (multiple)
            {
                // we need to fill info in the dialogue
                GameObject dialogue = GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/Dialog 1");

                // setting title i assume
                dialogue.transform.Find("Panel_UI/Top/Title").GetComponent<Text>().text = displayName;
                if (manager != null)
                {
                    manager.currentSceneVisualName = displayName;
                }

                // filling up options
                for (int i = 0; i < variations.Count; ++i)
                {
                    LevelSelectionScene_UI_Option option =
                        dialogue.transform.Find("Option_" + (i + 1)).GetComponent<LevelSelectionScene_UI_Option>();
                    option.bundleName = variations[i].bundleName;
                    option.sceneName = variations[i].sceneName;
                    option.transform.GetComponentInChildren<Text>().text = variations[i].displayName;

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

                // for single variation we can skip into practice/test dialogue
                GameObject.FindObjectOfType<UMP_Manager>().ShowDialog(3);

                if (manager != null)
                {
                    manager.currentSceneVisualName = displayName;
                }
            }

            // maybe disable test button
            //GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/DialogTestPractice/Panel_UI/TestButton")
            //    .GetComponent<Button>().interactable = !testDisabled;

            //making button not interactable was not noticable (maybe change design), hiding instead
            GameObject.Find("UMenuProManager/MenuCanvas/Dialogs/DialogTestPractice/Panel_UI/TestButton")
                .SetActive(!testDisabled);
        }
    }

    public void OnStartButtonClick()
    {
        PlayerPrefsManager.AddOneToPlaysNumber();
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
}
