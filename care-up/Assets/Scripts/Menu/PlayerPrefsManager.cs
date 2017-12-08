﻿using UnityEngine;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Handles quick access to saved data.
/// Volume | Tutorial completion | Scene completion + results
/// </summary>
public class PlayerPrefsManager : MonoBehaviour
{
    public bool VR = true;
    public bool practiceMode = true;

    private static PlayerPrefsManager instance;
    
    void Awake()
    {
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
        AudioListener.volume = Volume;
        Debug.Log("Volume is set to saved value: " + Volume);
        
        CheckSerial();
        
        if (PlayerPrefs.GetString("SerialKey") != "")
        {
            SetSerial(PlayerPrefs.GetString("SerialKey"));
        }
    }

    public float Volume
    {
        get { return PlayerPrefs.GetFloat("Volume"); }
        set { PlayerPrefs.SetFloat("Volume", value); }
    }

    public bool TutorialCompleted
    {
        get { return PlayerPrefs.GetInt("TutorialCompleted") == 1; }
        set { PlayerPrefs.SetInt("TutorialCompleted", value ? 1 : 0); }
    }
    
    private string SerialNumber
    {
        get { return PlayerPrefs.GetString("__SerialNumber"); }
        set
        {
            Debug.Log("Serial number set and saved: " + value);
            PlayerPrefs.SetString("__SerialNumber", value);
        }
    }

    public void SetSceneActivated(string sceneName, bool value)
    {
        if (value) Debug.Log(sceneName + " activated");
        PlayerPrefs.SetInt(sceneName + " activated", value ? 1 : 0);
    }

    public bool GetSceneActivated(string sceneName)
    {
        return PlayerPrefs.GetInt(sceneName + " activated") == 1;
    }

    public void SetSceneCompletionData(string sceneName, int stars, string time)
    {
        PlayerPrefs.SetInt(sceneName + "_completed", 1);
        PlayerPrefs.SetInt(sceneName + "_stars", stars);
        PlayerPrefs.SetString(sceneName + "_time", time);
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

    private void CheckSerial()
    {
        Debug.Log("Checking serial..");
        Debug.Log("Using serial: " + PlayerPrefs.GetString("__SerialNumber"));
        TextAsset[] serials = Resources.LoadAll<TextAsset>("Serials");
        foreach (TextAsset serialFile in serials)
        {
            string[] fLines = Regex.Split(serialFile.text, "\n|\r|\r\n");
            
            if (fLines.Length > 0)
            {
                string sceneGroup = fLines[0].Remove(0, 1);
                SetSceneActivated(sceneGroup, false);

                for (int i = 2; i < fLines.Length; ++i)
                {
                    if (string.Compare(SerialNumber, fLines[i], true) == 0)
                    {
                        SetSceneActivated(sceneGroup, true);
                    }
                }
            }
        }
    }

    public void SetSerial(string serial)
    {
        SerialNumber = serial;
        CheckSerial();
    }
}
