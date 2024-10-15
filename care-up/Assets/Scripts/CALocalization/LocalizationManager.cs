using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UIElements;

namespace CareUp.Localize
{
    public static class LocalizationManager
    {
        static string dictListFile = "dicts";
        static string defaultDictFolder = "Dictionaries";
        static List<string> dictFileNames = new List<string>(){"Dutch", "English"};
        // static List<string> dicts = new List<string>();

        static InGameLocalEditTool inGameLocalEditTool;


        //public static LocalizationManager instance;
        private static bool loadedDicts = false;
        private static GameObject gameLogic;
        private static Dictionary<string, Dictionary<string, string>> localizedText;
        // private static bool isReady = false;
        //private string missingTextString = "Text not found";

        // Use this for initialization

        public static void LoadLocalizedText(string fileName, int langID = -1)
        {
            if (langID == -1)
                langID = PlayerPrefsManager.Lang;

            gameLogic = GameObject.Find("GameLogic");
            if (localizedText == null)
            { 
                localizedText = new Dictionary<string, Dictionary<string, string>>();
                localizedText[dictFileNames[0]] = new Dictionary<string, string>();
                localizedText[dictFileNames[1]] = new Dictionary<string, string>();
            }
            string dictName = dictFileNames[langID];

            TextAsset _data = (TextAsset)Resources.Load(GetDictPath(false, langID) + fileName);
            if (_data != null)
            {
                string jsonString = _data.text;
                JSONNode data = JSON.Parse(jsonString);
                foreach (string key in data.Keys)
                {
                    if (!localizedText[dictName].ContainsKey(key))
                        localizedText[dictName].Add(key, data[key].ToString().Replace("<br>", "\n").Replace("\"",""));
                }
            }
        }
        public static void ClearDicts()
        {
            localizedText.Clear();
        }


        public static string GetDictPath(bool onlyLangName = false, int langID = -1)
        {
            string dictFolder = dictFileNames[0];
            if (langID == -1)
                dictFolder = dictFileNames[PlayerPrefsManager.Lang];
            else
            {
                dictFolder = dictFileNames[langID];
            }
            if (!onlyLangName)
                dictFolder = defaultDictFolder + "/" + dictFolder + "/";
            return dictFolder;
        }

        public static List<string> GetLocalizationNames()
        {
            return dictFileNames;
        }


        public static void LoadAllDictionaries()
        {

            // if (dicts.Count == 0)
            // {
            //     TextAsset dictListData = (TextAsset)Resources.Load(GetDictPath() + dictListFile);

            // }
            for (int d = 0; d < dictFileNames.Count; d++)
            {
                List<string> dicts = new List<string>();
                TextAsset dictListData = (TextAsset)Resources.Load(GetDictPath(false, d) + dictListFile);

                foreach (string dn in dictListData.text.Split('\n'))
                {   
                    if (!string.IsNullOrEmpty(dn))
                    {
                        dicts.Add(dn);
                    }
                }
                for (int i = 0; i < dicts.Count; i++) 
                {
                    string fileName = dicts[i];
                    LoadLocalizedText(fileName.Replace("\r",""), d);
                }
            }
            loadedDicts = true;
        }

        public static string MergeKeys(string prefix, string key1, string key2)
        {
            string key1Clean = key1.Replace("[", "").Replace("]", "") + " ";
            if (key1 == "")
                key1Clean = "";
            string key2Clean = key2.Replace("[", "").Replace("]", "");
            string result = "[" + prefix + key1Clean + key2Clean + "]";

            return result;
        }

        public static int CountKeysInText(string text)
        {
            int keysCounted = 0;
            List<int> keyRanges = GetKeyRangesFromText(text);
            if (keyRanges != null)
                keysCounted = keyRanges.Count / 2;
            return keysCounted;
        }

        public static List<int> GetKeyRangesFromText(string text)
        {
            List<int> keyRanges = new List<int>();

            bool isInsideBrackets = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (!isInsideBrackets && text[i] == '[')
                {
                    isInsideBrackets = true;
                    keyRanges.Add(i);
                }
                else if (isInsideBrackets && text[i] == ']')
                { 
                    isInsideBrackets = false;
                    keyRanges.Add(i);
                }
            }
            if (keyRanges.Count > 0)
                return keyRanges;
            return null;
        }

        public static string StripBracketsFromKey(string key)
        {
            return key.Substring(1, key.Length - 2);
        }

        public static string GetValueIfKey(string key)
        {
            if (!loadedDicts)
                LoadAllDictionaries();
            if (key == null)
                return "";
            if (key.Length == 0)
                return "";
            string marker = "";
            if (PlayerPrefsManager.toShowLocalMarkers)
                marker = "**!!";
            string result = marker + key;
            bool debugMode = false;
            if (gameLogic != null)
            {
                if (gameLogic.GetComponent<ActionManager>() != null)
                    debugMode = gameLogic.GetComponent<ActionManager>().TextDebug;
            }

            if (CountKeysInText(key) > 0)
            {
                string value = GetLocalizedValue(StripBracketsFromKey(key));
                if (value != null)
                {
                    if (PlayerPrefsManager.toShowLocalMarkers)
                        marker = "^^^";
                    result = marker + value;
                    if (Application.isEditor && debugMode)
                    {
                        result = key + value;
                    }
                }
            }
            return result;
        }

        public static List<string> GetKeysFromMultiKey(string text, bool stripped = false)
        {

            List<int> keyRanges = GetKeyRangesFromText(text);
            List<string> keys = new List<string>(); 
            if (keyRanges.Count > 0)
            {
                for (int i = 0; i < keyRanges.Count / 2; i++)
                {
                    int currentRange = keyRanges[i * 2 + 1] - keyRanges[i * 2] + 1;
                    string currentKey = text.Substring(keyRanges[i*2], currentRange);
                    if (stripped)
                        currentKey = currentKey.Replace("[", "").Replace("]", "");
                    keys.Add(currentKey);
                }
            }
            return keys;
        }

        public static string GetLocalizedWithMultiKey(string text)
        {
            foreach(string key in GetKeysFromMultiKey(text))
            {
                text = text.Replace(key, GetLocalizedValue(StripBracketsFromKey(key)));
            }
            return text;
        }

        public static string GetLocalizedValue(string key, int langID = -1)
        {
            if (!loadedDicts)
                LoadAllDictionaries();
            if (langID == -1)
            {
                langID = PlayerPrefsManager.Lang;
            }
            if (inGameLocalEditTool == null)
                inGameLocalEditTool = GameObject.FindObjectOfType<InGameLocalEditTool>();
            if (inGameLocalEditTool != null && inGameLocalEditTool.dataLoaded)
            {
                string value = inGameLocalEditTool.GetLocalizedValue(key, langID);
                if (value != "")
                    return value;
            }
            if (localizedText != null && localizedText.Keys.Contains(dictFileNames[langID]))
            {
                if (localizedText[dictFileNames[langID]].Keys.Count > 0 && 
                    localizedText[dictFileNames[langID]].ContainsKey(key))
                {
                    return localizedText[dictFileNames[langID]][key];
                }
            }
            return "";
        }
    }
}