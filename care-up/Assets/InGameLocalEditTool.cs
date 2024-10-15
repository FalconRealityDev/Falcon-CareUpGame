using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using CareUp.Localize;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System;
using System.Linq;
using SimpleJSON;


public class InGameLocalEditTool : MonoBehaviour
{
    public bool dataLoaded = false;
    public GameObject dictEditPanel;
    public Text keyTextLine;
    string currentKey = "";
    public TMP_InputField valueText;
    static InGameLocalEditTool _instance;
    bool ctrlKeyDown = false;
    bool isDictEditPopupOpen = false;
    const float popupWaitTime = 0.2f;
    float popupTimeOut = 0f;
    public List<Button> langButtons;
    public Button applyButton;
    public Button deleteButton;
    public Button markerButton;
    
    int toolLangID = 0;

    Dictionary<string, Dictionary<string, string>> changesToLocalization = 
        new Dictionary<string, Dictionary<string,string>>();
    List<string> activeDicts = new List<string>();


    public void SetToolLangID(int tID)
    {
        toolLangID = tID;
        UpdateButtons(false);
        InitiateLocalEdit(currentKey, false);
    }

    void UpdateMarkerButton()
    {
        Color color = Color.white;
        string markerText = "Show Markers";
        if (PlayerPrefsManager.toShowLocalMarkers)
        {
            markerText = "Hide Markers";
            color = Color.green;
        }
        markerButton.GetComponentInChildren<Text>().text = markerText;
        markerButton.GetComponent<Image>().color = color;
    }

    public void SwitchMarkerMode()
    {
        PlayerPrefsManager.toShowLocalMarkers = !PlayerPrefsManager.toShowLocalMarkers;
        UpdateMarkerButton();
        RefrashTextElements();
    }

    void UpdateButtons(bool globalMode = true)
    {
        int currentLangID = 0;
        if (globalMode)
        {
            currentLangID = PlayerPrefsManager.Lang;
            toolLangID = currentLangID;
        }
        else
        {
            currentLangID = toolLangID;
        }
        
        for (int i = 0; i < langButtons.Count; i++)
        {
            langButtons[i].interactable = i != currentLangID;
        }
    }

    void Start()
    {
        LoadExistingChanges();
    }

    public void ApplyChange()
    {
        string value = valueText.text;
        AddOrChangeValue(currentKey, value, true);
        RefrashTextElements();
        UpdateDeleteButton();
        // dictEditPanel.SetActive(false);
    }

    public void DeleteSelectedEntry()
    {
        DeleteDictElement(currentKey);
        RefrashTextElements();
        InitiateLocalEdit(currentKey, false);
        // dictEditPanel.SetActive(false);
    }


    public void AddUILocalizationComponentToGO(GameObject go, string key)
    {
        if (LocalizationManager.CountKeysInText(key) == 0)
            return;
        bool singleKey = LocalizationManager.CountKeysInText(key) == 1;
        if (singleKey)
            key = LocalizationManager.StripBracketsFromKey(key);
        else
            Debug.Log(key);
        

        if (go.GetComponent<UILocalization>() == null)  
            go.AddComponent<UILocalization>();
        
        UILocalization goUILocalization = go.GetComponent<UILocalization>();    
        if (goUILocalization != null)
        {
            if (singleKey)
                goUILocalization.key = key;
            else
                goUILocalization.multikeyLine = key;
            goUILocalization.UpdateText();
            goUILocalization.Initialization();
        }
    }

    public string GetLocalizedValue(string key, int langID = -1)
    {
        if (langID == -1)
            langID = PlayerPrefsManager.Lang;

        if (!dataLoaded)
            return "";
        string localName = LocalizationManager.GetDictPath(true, langID);
        if (changesToLocalization.Keys.Contains(localName))
        {
            if (changesToLocalization[localName].Keys.Contains(key))
            {
                return changesToLocalization[localName][key];
            }
        }
        return "";
    }

    void SaveDictChanges(string dictName = "")
    {

        List<string> dictsToSave = new List<string>();
        if (dictName != "")
        {
            dictsToSave.Add(dictName);
        }
        else
        {
            foreach(string n in LocalizationManager.GetLocalizationNames())
                dictsToSave.Add(n);
        }
        foreach (string k in dictsToSave)
        {
            if (!changesToLocalization.Keys.Contains(k))
                continue;

            string filePath = GetFilePathFromDictName(k);
            JSONObject dataObj = new JSONObject();
            foreach (string dataKey in changesToLocalization[k].Keys)
            {
                dataObj.Add(dataKey, changesToLocalization[k][dataKey].Replace("\n", "<br>"));
            }
            File.WriteAllText(filePath, dataObj.ToString(4));
        }
    }
    
    string GetFilePathFromDictName(string dictName)
    {
        string dictFileName = dictName + "DictChanges.json";
        string filePath = Path.Combine(Application.persistentDataPath, dictFileName);
        return filePath;
    }

    void AddOrChangeValue(string key, string value, bool toSave = false, string dictKey = "")  
    {
        if (dictKey == "")
            dictKey = LocalizationManager.GetDictPath(true, toolLangID);
        if (!changesToLocalization.Keys.Contains(dictKey))
        {
            changesToLocalization.Add(dictKey, new Dictionary<string, string>());
        }
        changesToLocalization[dictKey][key] = value;
        if (toSave)
            SaveDictChanges(dictKey);
    }

    void UpdateDeleteButton()
    {
        string dictKey = LocalizationManager.GetDictPath(true, toolLangID);
        deleteButton.interactable = changesToLocalization[dictKey].ContainsKey(currentKey);
    }


    public void DeleteDictElement(string key)
    {
        string dictKey = LocalizationManager.GetDictPath(true, toolLangID);
        changesToLocalization[dictKey].Remove(key);
        SaveDictChanges(dictKey);
        UpdateDeleteButton();
    }


    void RefrashTextElements()
    {
        foreach (UILocalization u in GameObject.FindObjectsOfType<UILocalization>(true))
        {
            u.UpdateText();
        }
    }

    void Update()
    {
        if (!PlayerPrefsManager.GetDevMode())
            return;
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ctrlKeyDown = true;
            popupTimeOut = popupWaitTime;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            ctrlKeyDown = false;
            if (isDictEditPopupOpen)
            {
                transform.Find("Panel/DictEditorExtraPanel").GetComponent<Animation>().Play("dictEditClose");
                isDictEditPopupOpen = false;
            }
        }

        if (ctrlKeyDown && popupTimeOut > 0)
        {
            popupTimeOut -= Time.deltaTime;
            if (popupTimeOut < 0)
            {
                transform.Find("Panel/DictEditorExtraPanel").GetComponent<Animation>().Play("dictEditOpen");
                isDictEditPopupOpen = true;
            }
        }
    
    }

    void LoadExistingChanges()
    {
        if (dataLoaded)
            return;
        foreach(string localName in LocalizationManager.GetLocalizationNames())
        {
            string filePath = GetFilePathFromDictName(localName);
            if (!System.IO.File.Exists(filePath))
                continue;
            string jsonString = File.ReadAllText(filePath);
            JSONNode data = JSON.Parse(jsonString);
            foreach (string key in data.Keys)
            {
                AddOrChangeValue(key, 
                    data[key].ToString().Replace("<br>", "\n").Replace("\"",""), false, localName);
            }
        }
        dataLoaded = true;
        RefrashTextElements();
    }

    public void InitiateLocalEdit(string key, bool toUpdateButtons = true)
    {
        if (toUpdateButtons)
            UpdateButtons();
        string value = LocalizationManager.GetLocalizedValue(key, toolLangID);
        dictEditPanel.SetActive(true);
        string dictName = LocalizationManager.GetDictPath(true, toolLangID);
        keyTextLine.text = dictName + " : " + key;
        currentKey = key;
        valueText.text = value;
        UpdateDeleteButton();
    }

    void Awake()
    {
        if (_instance)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void HidePanel()
    {
        dictEditPanel.SetActive(false);
    }
    
    public void OpenRuntimeDictFolder()
    {
        Application.OpenURL("file://" + Application.persistentDataPath);
    }
}
