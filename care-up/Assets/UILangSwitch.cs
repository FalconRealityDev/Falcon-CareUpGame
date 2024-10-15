using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CareUp.Localize;
using UnityEngine.UI;

public class UILangSwitch : MonoBehaviour
{
    public Button dutchLangButton;
    public Button englishLangButton;
    public void LanguageChange(int langID)
    {
        PlayerPrefsManager.Lang = langID;
        // LocalizationManager.ClearDicts();
        // LocalizationManager.LoadAllDictionaries();
        UpdateAllLangButtons();
        UpdateTextComponents();
    }

    public void UpdateTextComponents()
    {
        foreach(UILocalization uil in GameObject.FindObjectsOfType<UILocalization>(true))
        {
            uil.UpdateText();
        }
    }

    private void Start()
    {
        UpdateLangButtons();
    }


    public void UpdateAllLangButtons()
    {
        foreach (UILangSwitch uILangSwitch in GameObject.FindObjectsOfType<UILangSwitch>(true))
        {
            uILangSwitch.UpdateLangButtons();
        }
    }

    public void UpdateLangButtons()
    {
        englishLangButton.interactable = PlayerPrefsManager.Lang != 1;
        dutchLangButton.interactable = PlayerPrefsManager.Lang != 0;
    }
    
}
