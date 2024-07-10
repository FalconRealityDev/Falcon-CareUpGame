﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class CharacterFirstSetup : MonoBehaviour
{
    public InputField BigNumberHolder;
    public InputField FullName;
    public InputField FullName2;
    private InputField FullName_saved;
    public List<GameObject> tabs;
    public Button NextButton;
    public PlayerAvatar Avatar;
    private int currentChar = 0;
    private int currentTab = 3;
    public GameObject NoBigPopUp;
    private bool DontHaveBIG = false;

    PlayerPrefsManager pref;
    
    void Start()
    {
        FullName_saved = FullName;
        pref = GameObject.FindObjectOfType<PlayerPrefsManager>();

        if (pref != null)
        {
            if (!PlayerPrefsManager.firstStart)
            {
                NextButton.transform.Find("Text").GetComponent<Text>().text = "Opslaan";
                FullName.text = pref.fullPlayerName;
                BigNumberHolder.text = pref.bigNumber;
            }
            else
            {
                Invoke("Initialize", 0.01f);
            }
        }
    }


#if UNITY_WEBGL

    [DllImport("__Internal")]
    private static extern void openWindow(string url);
#endif

    void Initialize()
    {
        SetCharacter(0);
    }

    public void OpenUrl_NewWindow(string url)
    {
#if UNITY_WEBGL && ! UNITY_EDITOR
        openWindow(url);
#else
        OpenUrl(url);
#endif
    }
    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

    public void BIGYesClicked()
    {
        FullName = FullName_saved;
        SetTab(0);
    }    

    public void BIGNoClicked()
    {
        FullName = FullName2;
        SetTab(2);
    }

    public void OpenBIGInfo()
    {
        string url = "https://www.bigregister.nl/";
        OpenUrl_NewWindow(url);
    }

    public void SetCharacter(int n)
    {
        if (pref != null)
        {
            Avatar.avatarData = PlayerPrefsManager.storeManager.CharacterItems[n].playerAvatar;
            Avatar.UpdateCharacter();
            currentChar = n;
        }
    }

    public void ShowNoBigNum(bool value)
    {
        if (!value && !DontHaveBIG)
            BigNumberHolder.transform.GetComponentInParent<Animator>().SetTrigger("red");
        NoBigPopUp.SetActive(value);
    }
  
    public void GoBackFromCharacterSelection()
    {
        if (FullName == FullName2)
            SetTab(2);
        else
            SetTab(0);
    }
    public void IDontHaveBIG()
    {
        DontHaveBIG = true;
        if (FullName.text != "")
            SetTab(1);
        NoBigPopUp.SetActive(false);
        PlayerPrefsManager.SetBIGNumber(BigNumberHolder.text);
    }

    bool CheckFirstTab(bool checkBIG = true)
    {
        bool check = true;
        if (checkBIG)
        {
            if (BigNumberHolder.text == "")
            {
                if (!DontHaveBIG)
                {
                    ShowNoBigNum(true);
                    return false;
                }
            }
        }

        if (FullName.text == "")
        {
            FullName.transform.GetComponentInParent<Animator>().SetTrigger("red");
            check = false;
        } 
        return check;
    }

    public void SetTab(int tab)
    {
        bool check = true;        

        if (currentTab == 0)
        {
            check = CheckFirstTab();
        }
        if (currentTab == 2)
        {
            check = CheckFirstTab(false);
        }

        if (check)
        {
            bool isPlayerInfoTab = currentTab == 0 || currentTab == 2;
            if (!PlayerPrefsManager.firstStart && isPlayerInfoTab)
            {
                tab = -1;
            }
            if (tab >= 0 && tab < tabs.Count)
            {
                foreach (GameObject t in tabs)
                {
                    t.SetActive(false);
                }
                tabs[tab].SetActive(true);
                currentTab = tab;
            }
            else
            {
                // save full name
                PlayerPrefsManager.SetFullName(FullName.text);
                // save big number
                PlayerPrefsManager.SetBIGNumber(BigNumberHolder.text);
                if (PlayerPrefsManager.firstStart)
                {
                    CharacterInfo.SetCharacterCharacteristicsWU(PlayerPrefsManager.storeManager.CharacterItems[currentChar]);
                }
                // set new character scene to be seen and saved info
                DatabaseManager.UpdateField("AccountStats", "CharSceneV2", "true");
                bool goToMainMenu = (DatabaseManager.FetchField("AccountStats", "TutorialCompleted") == "true");

                if (PlayerPrefsManager.GetDevMode() && PlayerPrefsManager.tutorialOnStart)
                    goToMainMenu = false;
                    
                if (goToMainMenu)
                {
                    bl_SceneLoaderUtils.GetLoader.LoadLevel("MainMenu");
                }
                else
                {
                    bl_SceneLoaderUtils.GetLoader.LoadLevel("Scenes_Tutorial", "scene/scenes_tutorial");
                }
            }
        }
    }
}
