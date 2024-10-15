using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CareUp.Localize;
using UnityEngine.UI;
using TMPro;
using UnityEngine.PlayerLoop;
using System.Security.Cryptography;
using System.Linq;

public class UILocalization : MonoBehaviour
{
    public bool isPasive = false;
    public string key = "";
    public string multikeyLine = "";
    private Text text;
    List<GameObject> editerItems = new List<GameObject>();
    private TextMeshProUGUI tPro;
    // private UILocalEditButton localEditButton;
    private List<UILocalEditButton> localEditButtons = new List<UILocalEditButton>();


    void Start()
    {
        Initialization();
    }

    public void Initialization()
    {
        text = GetComponent<Text>();
        tPro = GetComponent<TextMeshProUGUI>();
        if (!isPasive)
            UpdateText();

        // if (localEditButtons.Count == 0)
        InitEditButtons();
    }

    void InitEditButtons()
    {
        foreach(GameObject g in editerItems)
        {
            Destroy(g);
        }
        editerItems.Clear();
        localEditButtons.Clear();

        GameObject localHoverImageGO = Instantiate(
            Resources.Load<GameObject>("NecessaryPrefabs/UI/UILocalHoverImage"), transform) as GameObject;
        editerItems.Add(localHoverImageGO);
        GameObject localEditButtonPanel = Instantiate(
            Resources.Load<GameObject>("NecessaryPrefabs/UI/UILocalEditButtonPanel"), transform) as GameObject;
        editerItems.Add(localEditButtonPanel);
        List<string> keys = new List<string>(); 
        if (multikeyLine == "")
            keys.Add(key);
        else
        {
            keys = LocalizationManager.GetKeysFromMultiKey(multikeyLine, true);
        }

        for (int i = 0; i < keys.Count; i++)
        {
            GameObject localEditButtonGO = Instantiate(
                Resources.Load<GameObject>("NecessaryPrefabs/UI/UILocalEditButton"), 
                localEditButtonPanel.transform) as GameObject;
            editerItems.Add(localEditButtonGO);
            UILocalEditButton localEditButton = localEditButtonGO.GetComponent<UILocalEditButton>();
            localEditButton.SetHoverImage(localHoverImageGO);
            localHoverImageGO.SetActive(false);
            localEditButton.SetUILocalization(this);
            localEditButton.SetActive(false);
            localEditButton.key = keys[i];
            localEditButtons.Add(localEditButton);
        }
    }

    void OnEnable()
    {
        if (!isPasive)
            UpdateText();
    }

    public void UpdateText()
    {
        string marker = "";
        if (PlayerPrefsManager.toShowLocalMarkers)
            SetText("$$$$$$$$$$$$$$$$");
        if (key == "" && multikeyLine == "")
            return;

        if (multikeyLine == "")
        {
            string newText = LocalizationManager.GetLocalizedValue(key);
            if (newText != "")
            {
                if (PlayerPrefsManager.toShowLocalMarkers)
                    marker = "><><";
                SetText(marker + newText);
            }
        }
        else
        {
            if (PlayerPrefsManager.toShowLocalMarkers)
                marker = "%%^^";
            string newText = LocalizationManager.GetLocalizedWithMultiKey(multikeyLine);
            SetText(marker + newText);

        }
    }

    public void InitiateLocalEdit(string editKey = "")
    {

        InGameLocalEditTool localTool = GameObject.FindObjectOfType<InGameLocalEditTool>();
        if (localEditButtons.Count == 0)
            InitEditButtons();
        if (localTool != null && localEditButtons.Count != 0)
        {
            localTool.InitiateLocalEdit(editKey);
        }
    }

    public void SetText(string value)
    {
        if (text != null)
            text.text = value;
        if (tPro != null)
            tPro.text = value;
    }
   
    public string GetText()
    {
        text = GetComponent<Text>();
        tPro = GetComponent<TextMeshProUGUI>();
        string result = "";
        if (text != null)
            result = text.text;
        else if (tPro != null)
            result = tPro.text;
        return result;
    }

    void Update()
    {
        if (PlayerPrefsManager.GetDevMode())
        {
            foreach(UILocalEditButton b in localEditButtons)
                b.SetActive(Input.GetKey(KeyCode.LeftControl));
        }
    }
}
