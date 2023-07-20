﻿using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using System.IO;

namespace CareUp.Localize
{
    public static class LocalizationManager
    {
        static string dictListFile = "dicts";
        static string defaultDictFolder = "Dictionaries";

        static List<string> dicts = new List<string>();


        //public static LocalizationManager instance;
        private static bool loadedDicts = false;
        private static GameObject gameLogic;
        private static Dictionary<string, string> localizedText;
        // private static bool isReady = false;
        //private string missingTextString = "Text not found";

        // Use this for initialization

        public static void LoadLocalizedText(string fileName)
        {

            gameLogic = GameObject.Find("GameLogic");
            if (localizedText == null)
                localizedText = new Dictionary<string, string>();
            TextAsset _data = (TextAsset)Resources.Load(GetCurrentDictPaht() + fileName);
            if (_data != null)
            {
                string jsonString = _data.text;
                JSONNode data = JSON.Parse(jsonString);
                foreach (string key in data.Keys)
                {
                    if (!localizedText.ContainsKey(key))
                        localizedText.Add(key, data[key].ToString().Replace("<br>", "\n").Replace("\"",""));
                }
            }
        }
        public static void ClearDicts()
        {
            localizedText.Clear();
        }



        private static string GetCurrentDictPaht()
        {
            string dictFolder = defaultDictFolder + "/Dutch/";
            PlayerPrefsManager playerPrefsManager = GameObject.FindObjectOfType<PlayerPrefsManager>();
            if (playerPrefsManager != null)
            {
                switch (PlayerPrefsManager.Lang)
                {
                    case 0:
                        {
                            dictFolder = defaultDictFolder + "/Dutch/";
                            break;
                        }
                    case 1:
                        {
                            dictFolder = defaultDictFolder + "/English/";
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            return dictFolder;
        }


        public static List<string> ListCurrentJsonFiles(string dictPath)
        {
            List<string> fileList = new List<string>();
            string fullDictDirPath = Application.dataPath + "/Resources/" + dictPath;
            if (Directory.Exists(fullDictDirPath))
            {
                DirectoryInfo d = new DirectoryInfo(fullDictDirPath);
                foreach (var file in d.GetFiles("*.json"))
                    fileList.Add(file.Name.Split(".")[0]);
                
                return fileList;
            }
            return null;
        }

        public static void LoadAllDictionaries()
        {
            if (dicts.Count == 0)
            {
                List<string> dictNames = ListCurrentJsonFiles(GetCurrentDictPaht());
                foreach (string dictName in dictNames)
                {   
                    if (!string.IsNullOrEmpty(dictName))
                    {
                        dicts.Add(dictName);
                    }
                }
            }
            //Resources.Load("Dictionaries/");
            foreach (string fileName in dicts)
            {
                LoadLocalizedText(fileName.Replace("\r",""));
            }
            loadedDicts = true;
        }

        public static string GetValueIfKey(string key)
        {
            if (!loadedDicts)
                LoadAllDictionaries();
            if (key == null)
                return "";
            if (key.Length == 0)
                return "";

            string result = "*****";
            bool debugMode = false;
            if (gameLogic != null)
            {
                if (gameLogic.GetComponent<ActionManager>() != null)
                    debugMode = gameLogic.GetComponent<ActionManager>().TextDebug;
            }

            if (key[0] == '[')
            {
                string value = GetLocalizedValue(key.Substring(1, key.Length - 2));
                if (value != null)
                {
                    result = value;
                    if (Application.isEditor && debugMode)
                    {
                        result = key + value;
                    }
                }
            }
            return result;
        }

        public static string GetLocalizedValue(string key)
        {
            if (!loadedDicts)
                LoadAllDictionaries();
            if (localizedText != null)
            {
                if (localizedText.Keys.Count > 0)
                {
                    if (localizedText.ContainsKey(key))
                    {
                        return localizedText[key];
                    }
                }
            }
            return "";
        }
    }
}