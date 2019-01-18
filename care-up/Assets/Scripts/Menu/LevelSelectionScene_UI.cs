﻿using UnityEngine;
using System.Collections;
using System.Xml;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;

/// <summary>
/// Handles Scene selection module
/// </summary>
public class LevelSelectionScene_UI : MonoBehaviour
{
    private PlayerPrefsManager ppManager;

    // leaderboard stuff
    public ScoreLine[] _Scores;

    public List<Transform> variations;

    private void Awake()
    {
        Transform leaderPanel = GameObject.Find("UMenuProManager/MenuCanvas/Leaderboard/InfoBar").transform;
        _Scores = GameObject.Find("scoreLines").GetComponentsInChildren<ScoreLine>();

        for (int i = 0; i < _Scores.Length; i++)
        {
            _Scores[i].SetScoreLine("", "", i);
        }

        // variations buttons should be disabled from the beginning
        for (int i = 0; i < 3; ++i)
        {
            Transform v = leaderPanel.parent.Find("InfoBar/menu/d" + (i+1));
            variations.Add(v);
            v.gameObject.SetActive(false);
        }

        // we dont have a list of activated scenes anymore i guess
        //GameObject.Find("UMenuProManager/MenuCanvas/Home/BannerArea/RegisterProtocols/News/Text")
        //    .GetComponent<Text>().text = GameObject.FindObjectOfType<PlayerPrefsManager>().ActivatedScenes;
    }

    /// <summary>
    /// Load xml file, set all door variables.
    /// </summary>
    void Start()
    {
        ppManager = GameObject.FindObjectOfType<PlayerPrefsManager>();

        UpdateSceneUI();
    }

    void ClearUI()
    {
        Transform parent1 = GameObject.Find("UMenuProManager/MenuCanvas/Play/ProtocolList/ProtocolsHolder/Protocols/content").transform;
        for (int i = 0; i < parent1.childCount; ++i)
        {
            Destroy(parent1.GetChild(i).gameObject);
        }

        Transform parent2 = GameObject.Find("UMenuProManager/MenuCanvas/Leaderboard/LeftBar/Scroll View/Viewport/Content").transform;
        for (int i = 0; i < parent1.childCount; ++i)
        {
            Destroy(parent2.GetChild(i).gameObject);
        }
    }

    public void OnSceneTabSwitch()
    {
        ClearUI();
        UpdateSceneUI();
    }

    void UpdateSceneUI()
    {
        // load xml
        TextAsset textAsset;

        PlayerPrefsManager pp = GameObject.FindObjectOfType<PlayerPrefsManager>();
        if (pp != null && pp.demoVersion)
        {
            textAsset = (TextAsset)Resources.Load("Xml/Scenes_Demo");
        }
        else
        {
            textAsset = (TextAsset)Resources.Load("Xml/Scenes");
        }

        XmlDocument xmlFile = new XmlDocument();
        xmlFile.LoadXml(textAsset.text);
        XmlNodeList xmlSceneList = xmlFile.FirstChild.NextSibling.FirstChild.ChildNodes;
        
        // leaderboard stuff
        bool firstScene = true;
        LeaderBoardSceneButton.buttons.Clear();

        foreach (XmlNode xmlSceneNode in xmlSceneList)
        {
           // bool activated = PlayerPrefs.GetInt(xmlSceneNode.Attributes["id"].Value + " activated") == 1;
            bool activated = true;
            bool hidden = xmlSceneNode.Attributes["hidden"] != null;
            if ((!activated && hidden) || hidden )
            {
                // not activated and hidden scene should not even create a panel, so just end up here
                continue;
            }

            // if we're here, then we have real scene, that is not hidden
            // instantiating panel
            GameObject sceneUnitObject = Instantiate(Resources.Load<GameObject>("Prefabs/UI/SceneSelectionUnit"),
                GameObject.Find("UMenuProManager/MenuCanvas/Play/ProtocolList/ProtocolsHolder/Protocols/content").transform);
            sceneUnitObject.name = "SceneSelectionUnit"; // i dont like that 'clone' word at the end, ugh
            LevelButton sceneUnit = sceneUnitObject.GetComponent<LevelButton>();
            
            if (!activated && !hidden)
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

            // now let's fill some actual info about the scene
            if (xmlSceneNode.Attributes["multiple"] != null)
            {
                sceneUnit.multiple = true;

                // so we're setting only scene title and picture 
                // saving everything else for dialogues

                // setting scene title
                sceneUnit.transform.Find("Title").GetComponent<Text>().text
                    = sceneUnit.displayName = xmlSceneNode.Attributes["name"].Value;

                ppManager.currentSceneVisualName = sceneUnit.displayName;
                ppManager.UpdateTestHighscore(0.0f);

                // saving bundle name for later
                string bundleName = sceneUnit.bundleName = xmlSceneNode.Attributes["bundleName"].Value;

                int i = 0;
                foreach (XmlNode variation in xmlSceneNode.ChildNodes)
                {
                    LevelButton.Info info = new LevelButton.Info();
                    // saving all the info for scene variation, use in dialogue
                    info.bundleName = bundleName;
                    info.sceneName = variation.Attributes["name"].Value;
                    info.displayName = variation.Attributes["displayname"].Value;
                    info.description = variation.Attributes["description"].Value;
                    info.image = Resources.Load<Sprite>("Sprites/ScenePreview/" + variation.Attributes["image"].Value);

                    sceneUnit.variations.Add(info);

                    if (i == 0)
                    {
                        // set the image as main if this is 1st variation
                        sceneUnit.image = sceneUnit.variations[i].image;
                        sceneUnit.transform.Find("LevelPreview").GetComponent<Image>().sprite = sceneUnit.image;

                        // also make 1st option 'selected'
                        sceneUnit.sceneName = sceneUnit.variations[i].sceneName;
                    }

                    // setting max 3 for now, dont see that UI supports more
                    if (++i > 2)
                        break;
                }
            }
            else
            {
                // saving info for loading
                sceneUnit.multiple = false;
                sceneUnit.sceneName = xmlSceneNode.Attributes["sceneName"].Value;
                sceneUnit.bundleName = xmlSceneNode.Attributes["bundleName"].Value;

                // setting scene title
                sceneUnit.transform.Find("Title").GetComponent<Text>().text
                    = sceneUnit.displayName = xmlSceneNode.Attributes["name"].Value;

                ppManager.currentSceneVisualName = sceneUnit.displayName;
                ppManager.UpdateTestHighscore(0.0f);

                // setting description
                if (xmlSceneNode.Attributes["description"].Value != "")
                {
                    // no description atm
                    //  = scene.Attributes["description"].Value;
                }

                // setting image
                if (xmlSceneNode.Attributes["image"] != null)
                {
                    sceneUnit.image = Resources.Load<Sprite>("Sprites/ScenePreview/" + xmlSceneNode.Attributes["image"].Value);
                    sceneUnit.transform.Find("LevelPreview").GetComponent<Image>().sprite = sceneUnit.image;
                }
            }

            // leaderboard stuff
            GameObject button = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI/LeaderBoardSceneButton"),
                GameObject.Find("UMenuProManager/MenuCanvas/Leaderboard/LeftBar/Scroll View/Viewport/Content").transform);
            LeaderBoardSceneButton buttonInfo = button.GetComponent<LeaderBoardSceneButton>();

            button.transform.GetChild(0).GetComponent<Text>().text = sceneUnit.displayName;
            buttonInfo.sceneName = sceneUnit.sceneName;
            buttonInfo.multiple = sceneUnit.multiple;
            if (buttonInfo.multiple)
            {
                foreach(LevelButton.Info v in sceneUnit.variations)
                {
                    buttonInfo.sceneNames.Add(v.sceneName);
                    buttonInfo.buttonNames.Add(v.displayName);
                }
            }

            if (firstScene)
            {
                firstScene = false;
                buttonInfo.OnMainButtonClick();
            }

            sceneUnit.testDisabled = (xmlSceneNode.Attributes["test"] != null
                && xmlSceneNode.Attributes["test"].Value == "disabled");
        }
    }
    
    public void UpdateLeaderBoard(string sceneName)
    {
        Debug.Log("UpdateLeaderBoard:::" + sceneName);
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

        if (entries == null) return;

        for (int i = 0; i < 10; ++i)
        {
            string name = sortedEntries[i].String("dname");
            string score = sortedEntries[i].String("score");
            
            if (i < _Scores.Length)
                _Scores[i].SetScoreLine(name, score, i);
        }
    }
}
