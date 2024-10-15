using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SimpleJSON;
using System.IO;
using System.Text;

namespace CareUp.Localize
{
    public struct WordToChangeData
    {
        public WordToChangeData(int _dictID, string _key, string _newWord)
        {
            dictID = _dictID;
            key = _key;
            newWord = _newWord;
        }
        public int  dictID { get; set;}
        public string  key { get; set;}
        public string  newWord { get; set;}
    }

    class DictionaryEditor : EditorWindow
    {
        static bool toCheckInActionFiles = false;
        static string selectedName = "";
        static string dutchDict = "Dictionaries/Dutch/";
        static string englishDict = "Dictionaries/English/";
        static string localizationDebugInfo = "";
        static string uiOrganizeDictFileName = "";
        static int languageSelected = 0;
        static bool someKeysRepeat = false;
        static Vector2 scrollPos = new Vector2();
        static int selectedSet = -1;
        static string searchText = "";
        static List<WordToChangeData> wordsToChange = new List<WordToChangeData>();
        // List<WordToChangeData> keysToChange = new List<WordToChangeData>();
        static int removeFromSet = -1;
        static string keyToRemove = "";
        static List<Dictionary<string, string>> setOfDictionaries = new List<Dictionary<string, string>>();
        static string dictListFile = "dicts";
        static string editValue = "";
        static string editKey = "";
        static string editControlKey = "";
        static int showFilesForSet = -1;
        static string[] confirmActionOptions = new string[] {"Cancel", "Confirm"};
        static string showFilesForKey = "";
        static List<bool> dictUnfold = new List<bool>();
        static bool toForceUpdateKeys = false;
        static Dictionary<string, List<string>> keyUsageDict = new Dictionary<string, List<string>>();
        static List<string> filesWithValueInstance = new List<string>();

        static int editInSet = -1;
        //string editOriginalKey = "";
        static Vector2 TextEditorScroll = new Vector2();

        static List<string> dictNames = new List<string>();
        [MenuItem("Tools/CareUp Localization/Dictionary Editor")]
        static void Init()
        {
            EditorWindow.GetWindow<DictionaryEditor>();
        }

        static string GetCurrentDictPath()
        {
            if (languageSelected == 1)
                return englishDict;
            return dutchDict;
        }

        public static void SetSelectedDictName(string value)
        {
            selectedName = value;
            selectedSet = dictNames.IndexOf(selectedName);
        }

        public static void SetLocalizationIndex(int index)
        {
            languageSelected = index;
        }

        public static void ClearDicts()
        {
            setOfDictionaries = new List<Dictionary<string, string>>();
            dictNames = new List<string>();
            keyUsageDict = new Dictionary<string, List<string>>();
        }


        public static string FindInDictByKey(string _key, out int dictID, bool toRemoveBrackets = true)
        {
            dictID = -1;
            
            if (_key.Length < 3)
                return _key;
            if (_key[0] != '[' || _key[_key.Length - 1] != ']')
                return _key;
            string key = _key;
            if (toRemoveBrackets)
            {
                key = key.Substring(1, key.Length - 2);
            }
            if (dictNames.Count != 0)
            {
                for(int i = 0; i < dictNames.Count; i++)
                {
                    if (setOfDictionaries[i].ContainsKey(key))
                    {
                        dictID = i;
                        return(setOfDictionaries[i][key]);
                    }
                }
            }
            return _key;
        } 

        public static void UpdateDictionaryValue(int dictToEdit, string key, string newValue)
        {
            if (dictToEdit != -1)
            {
                if (setOfDictionaries[dictToEdit].ContainsKey(key))
                {
                    setOfDictionaries[dictToEdit][key] = newValue;
                }
            }
        }


        public static void SaveChanges(int _dictID = -1)
        {
            int startSaveFrom = 0;
            int endSaveOn = dictNames.Count;
            if (_dictID > -1)
            {
                startSaveFrom = _dictID;
                endSaveOn = _dictID + 1;
            }
            for (int i = startSaveFrom; i < endSaveOn; i++)
            {
                string dictName = dictNames[i];
                string actionFilePath = "Assets/Resources/" + GetCurrentDictPath() + dictName + ".json";

                var stringBuilder = new StringBuilder();
                stringBuilder.Append("{\n");
                int keysCount = setOfDictionaries[i].Keys.Count;
                int j = 0;
                foreach(string key in setOfDictionaries[i].Keys)
                {
                    string comma = ",";
                    if (j == (keysCount - 1))
                        comma = "";

                    stringBuilder.Append("  \"" + key + "\": \"" + 
                        setOfDictionaries[i][key].Replace("\n", "<br>").Replace("\r", "").Replace("\"", "“") + 
                        "\"" + comma + "\n");
                    j++;
                }
                stringBuilder.Append("}");

                using (StreamWriter swriter = new StreamWriter(actionFilePath))
                    swriter.Write(stringBuilder.ToString());
            }
        }
        public static void CopyToClipboard(string _value, bool toAddBrackets = false)
        {
            TextEditor te = new TextEditor();
            te.text = _value;
            if (toAddBrackets)
            te.text = "[" + _value + "]";
            te.SelectAll();
            te.Copy();
        }


/// <summary>
/// Closing a text editor with canceling or saving cahnges to the attribute
/// </summary>
/// <param name="toSave">To save or not to save</param>
        static void ExitTextEdit(bool toSave = false)
        {
            filesWithValueInstance.Clear();
            if (toSave && editKey != "")
            {
                if (editKey == editControlKey)
                {
                    setOfDictionaries[editInSet][editKey] = editValue;
                }
                else
                {
                    setOfDictionaries[editInSet].Remove(editControlKey);
                    if (setOfDictionaries[editInSet].ContainsKey(editKey))
                    {
                        setOfDictionaries[editInSet][editKey] = editValue;
                    }
                    else
                    {
                        setOfDictionaries[editInSet].Add(editKey, editValue);
                    }
                }
            }
            editInSet = -1;
            GUI.FocusControl(null);
        }

        static bool CheckForKeyCopies(string key, int _dictID)
        {
            for(int i = 0; i < setOfDictionaries.Count; i++)
            {
                if (i == _dictID)
                    continue;
                if (setOfDictionaries[i].ContainsKey(key))
                    return true;
            }
            return false;
        }

        static List<TextAsset> GetAllXMLFiles()
        {
            string projectResPath = "Assets/Resources/";

            List<TextAsset> textAssets = new List<TextAsset>();
            string[] xmlDataDirectories = new string[] 
            {
                "Xml/Actions",
                "Xml/RandomEvent",
                "Xml/AnimationSequences",
                "Xml/PersonDialogues",
                "Xml/Quiz"
                }; 
            foreach(string xmlDirPath in xmlDataDirectories)
            {
                if (!Directory.Exists(projectResPath + xmlDirPath))
                    continue;
                var info = new DirectoryInfo(projectResPath + xmlDirPath);
                FileInfo[] fileInfo = info.GetFiles();
                
                foreach (FileInfo file in fileInfo)
                {
                    if (file.Extension == ".xml")
                    {
                        // actionFiles.Add(file.Name.Split('.')[0]);
                        string resPath = xmlDirPath + "/" + file.Name.Split('.')[0];
                        TextAsset textAsset = Resources.Load(resPath, typeof(TextAsset)) as TextAsset;
                        textAssets.Add(textAsset);
                    }
                }
            }
            return textAssets;
        }

        static void CheckInActionFiles()
        {
            keyUsageDict.Clear();
            List<TextAsset> textAssets = GetAllXMLFiles();
            // TextAsset textAsset = Resources.Load("Xml/Actions/Actions_AED", typeof(TextAsset))  as TextAsset;
            for(int i = 0; i < setOfDictionaries.Count; i++)
            {
                foreach(string key in setOfDictionaries[i].Keys)
                {
                    if (!keyUsageDict.ContainsKey(key))
                    {
                        List<string> usedInFiles = new List<string>();

                        foreach(TextAsset asset in textAssets)
                        {
                            if (asset.text.Contains("[" + key + "]"))
                            {
                                usedInFiles.Add(asset.name);
                            }
                        }
                        if (usedInFiles.Count > 0)
                        {
                            keyUsageDict.Add(key, usedInFiles);
                        }
                    }
                }
            }
        }

        static void CheckValueInActionFiles(string _value, string key = "")
        {
            AssetDatabase.Refresh();
            filesWithValueInstance.Clear();
            List<TextAsset> textAssets = GetAllXMLFiles();
            foreach(TextAsset asset in textAssets)
            {
                bool contained = false;
                if (asset.text.Contains('\"' + _value + '\"'))
                {
                    filesWithValueInstance.Add(asset.name);
                    contained = true;
                }
                if (key != "" && contained)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append(asset.text);
                    stringBuilder.Replace('\"' + _value + '\"', "\"[" + key + "]\"");
                    using (StreamWriter swriter = new StreamWriter(AssetDatabase.GetAssetPath(asset)))
                        swriter.Write(stringBuilder.ToString());
                }
            }
        }

        static void OpenFileContinedKey(string fileName, string key)
        {
            string filePath = "Assets/Resources/Xml/Actions/" + fileName + ".xml";
            Debug.Log(filePath);
            Object fileObject = AssetDatabase.LoadAssetAtPath(filePath, (typeof(TextAsset)));
            int _line = 0;
            TextAsset textAsset = Resources.Load("Xml/Actions/" + fileName) as TextAsset;
            int i = 1;
            foreach(string textLine in textAsset.text.Split('\n'))
            {
                if(textLine.Contains("[" + key + "]"))
                {
                    _line = i;
                    Debug.Log(textLine);
                    break;
                }
                i++;
            }
            //Find line number
            AssetDatabase.OpenAsset(fileObject, _line);
        }

        public static string GetKeyIfTextInDict(Dictionary<string, string> _dict, string value)
        {
            foreach(string key in _dict.Keys)
            {
                if (value == _dict[key])
                    return key;
            }
            return "";
        }

        public static string GenerateUniqueKey(string prefix, List<string> keys, string value)
        {
            string newKey = "";
            value = value.Trim().ToLower();
            string[] valueWords = value.Split(' ');
            
            if (valueWords.Length > 3)
                newKey = prefix + valueWords[0] + " " + valueWords[1] + " " + valueWords[2];
            else
                newKey = prefix + value;

            string keyToCheck = newKey;
            while(keys.Contains(keyToCheck))
            {
                keyToCheck = newKey + Random.Range(0, 999).ToString("D3");
            }

            return keyToCheck;
        }

        void OnGUI()
        {
            var defaultColor = GUI.backgroundColor;
            someKeysRepeat = false;
            GUIStyle warningStyle  = new GUIStyle();
            warningStyle.normal.textColor = Color.red;

            GUIStyle greenStyle  = new GUIStyle();
            greenStyle.normal.textColor = new Color(0f, 0.5f, 0f);

            GUIStyle horizontalLine;
            horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            horizontalLine.fixedHeight = 1;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Check in XML: ", GUILayout.Width(90));
            toCheckInActionFiles = EditorGUILayout.Toggle(toCheckInActionFiles, GUILayout.Width(30));
            if (GUILayout.Button("Reload All Dictionary Data"))
                ReloadDictionaries();
            if (languageSelected == 0)
                GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Dutch Lang", GUILayout.Width(120)))
            {
                languageSelected = 0;
                ReloadDictionaries();
            }

            GUI.backgroundColor = defaultColor;

            if (languageSelected == 1)
                GUI.backgroundColor = Color.green;
            if (GUILayout.Button("English Lang", GUILayout.Width(120)))
            {
                languageSelected = 1;
                ReloadDictionaries();
            }
            GUI.backgroundColor = defaultColor;
        
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("[ " + dictNames.Count.ToString() + " ] dictionaries found");
            if (showFilesForKey != "" && showFilesForSet != -1)
            {
                if (!keyUsageDict.ContainsKey(showFilesForKey))
                {
                    showFilesForKey = "";
                    showFilesForSet = -1;
                }

                if (GUILayout.Button("<--- Go Back"))
                {
                    showFilesForKey = "";
                    showFilesForSet = -1;
                }
                else
                {
                    GUILayout.Box(GUIContent.none, horizontalLine);
                    EditorGUILayout.LabelField("The key [ " + showFilesForKey + " ] was found in:");
                    GUILayout.Box(GUIContent.none, horizontalLine);
                    foreach (string _file in keyUsageDict[showFilesForKey])
                    {
                        if (GUILayout.Button(_file))
                        {
                            Debug.Log(_file);
                            OpenFileContinedKey(_file, showFilesForKey);
                        }
                    }
                }
            }
            else if (editInSet != -1)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Cancel Edit"))
                    ExitTextEdit();
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Confirm Edit"))
                    ExitTextEdit(true);
                GUI.backgroundColor = defaultColor;
                EditorGUILayout.EndHorizontal();

                if (editInSet >= 0)
                    EditorGUILayout.LabelField("Editing key and value in dictionary [ " + dictNames[editInSet] + " ]", greenStyle);

                if (GUILayout.Button("Search by text in all Action Files"))
                {
                    CheckValueInActionFiles(editValue);
                }
                if (filesWithValueInstance.Count > 0)
                {
                    EditorGUILayout.LabelField("This text found in [ " + filesWithValueInstance.Count.ToString() + " ] files", greenStyle);
                    if (GUILayout.Button("Replace instances of text in all Action Files"))
                    {
                        CheckValueInActionFiles(editValue, editKey);
                    }
                }


                editKey = EditorGUILayout.TextField("Key: ", editKey);
                TextEditorScroll = EditorGUILayout.BeginScrollView(TextEditorScroll);
                editValue = EditorGUILayout.TextArea(editValue, GUILayout.Height(position.height - 30));
                EditorGUILayout.EndScrollView();
            }
            else
            {
                List<string> UIWords = new List<string>();
                if (setOfDictionaries.Count > 0 && dictNames.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    searchText = EditorGUILayout.TextField(searchText);

                    if (GUILayout.Button(new GUIContent("C", "Clear"), GUILayout.Width(22)))
                    {
                        GUI.FocusControl(null);
                        searchText = "";
                    }
                    EditorGUILayout.EndHorizontal();

                    List<string> dictsRollDownList = new List<string>();
                    dictsRollDownList.Add("__Show All__");
                    foreach (string d in dictNames)
                        dictsRollDownList.Add(d);
                    selectedSet = EditorGUILayout.Popup(selectedSet + 1, dictsRollDownList.ToArray()) - 1;

                    string saveButtonText = "Save all changes";
                    if (selectedSet != -1)
                    {
                        selectedName = dictsRollDownList[selectedSet + 1];
                        saveButtonText = "Save [ " + dictNames[selectedSet] + " ]";
                    }
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button(saveButtonText))
                        SaveChanges();
                    GUI.backgroundColor = defaultColor;
                    GUILayout.Box(GUIContent.none, horizontalLine);

                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                    int startFrom = 0;
                    int endOn = dictNames.Count;
                    if (selectedSet >= 0 && selectedSet < setOfDictionaries.Count)
                    {
                        startFrom = selectedSet;
                        endOn = selectedSet + 1;
                    }
                    for (int i = startFrom; i < endOn; i++)
                    {
                        EditorGUILayout.LabelField("[" + setOfDictionaries[i].Count.ToString() + "] " +
                            "Dictionary: [ " + dictNames[i] + " ]", greenStyle);
                        string arrowChar = "▶";
                        if (dictUnfold.Count < i)
                            dictUnfold.Add(true);
                        if (dictUnfold[i])
                            arrowChar = "▼";
                        if (GUILayout.Button(arrowChar, GUILayout.Width(22)))
                        {
                            dictUnfold[i] = !dictUnfold[i];
                        }
                        GUILayout.Box(GUIContent.none, horizontalLine);
                        if (dictUnfold[i])
                        {
                            if (GUILayout.Button("__Add Component__"))
                            {
                                editValue = "";
                                editKey = "";
                                editInSet = i;
                                editControlKey = editKey;
                                GUI.FocusControl(null);
                            }
                            foreach (string key in setOfDictionaries[i].Keys)
                            {
                                if (searchText != "")
                                {
                                    if (!key.ToLower().Contains(searchText.ToLower().Substring(1)) && 
                                        !setOfDictionaries[i][key].ToLower().Contains(searchText.ToLower()))
                                        continue;
                                }
                                EditorGUILayout.BeginHorizontal();
                                int usageNum = 0;
                                if (keyUsageDict.ContainsKey(key))
                                {
                                    usageNum = keyUsageDict[key].Count;
                                    string tootTip = "";
                                    foreach (string s in keyUsageDict[key])
                                        tootTip += s + "\n";
                                    GUIContent usageText = new GUIContent(usageNum.ToString(), tootTip);

                                    if (GUILayout.Button(usageText, GUILayout.Width(30)))
                                    {
                                        showFilesForSet = i;
                                        showFilesForKey = key;
                                        // EditorGUILayout.LabelField(usageText, greenStyle, GUILayout.Width(30));
                                    }
                                }
                                else
                                    EditorGUILayout.LabelField("  ", GUILayout.Width(30));

                                string _line = "[ " + key + " ] ";
                                if (CheckForKeyCopies(key, i))
                                {
                                    someKeysRepeat = true;
                                    EditorGUILayout.LabelField(_line, warningStyle);
                                }
                                else
                                    EditorGUILayout.LabelField(_line);
                                UIWords.Add("");

                                EditorGUI.BeginChangeCheck();
                                UIWords[UIWords.Count - 1] = EditorGUILayout.TextField(setOfDictionaries[i][key]);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    wordsToChange.Add(new WordToChangeData(i, key, UIWords[UIWords.Count - 1]));
                                }
                                if (GUILayout.Button(new GUIContent("C", "Copy key"), GUILayout.Width(22)))
                                {
                                    CopyToClipboard(key);
                                }
                                if (GUILayout.Button(new GUIContent("[C]", "Copy key in brackets"), GUILayout.Width(28)))
                                {
                                    CopyToClipboard(key, true);
                                }
                                if (GUILayout.Button(new GUIContent("E", "Edit"), GUILayout.Width(22)))
                                {
                                    filesWithValueInstance.Clear();
                                    editValue = setOfDictionaries[i][key];
                                    editKey = key;
                                    editInSet = i;
                                    editControlKey = editKey;
                                    GUI.FocusControl(null);
                                }
                                if (GUILayout.Button(new GUIContent("-", "Delete Key and Value"), GUILayout.Width(22)))
                                {
                                    removeFromSet = i;
                                    keyToRemove = key;
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }

                    }
                    EditorGUILayout.EndScrollView();
                    GUILayout.Box(GUIContent.none, horizontalLine);
                    if (someKeysRepeat)
                        EditorGUILayout.LabelField("Warning: Some Keys repeat in multiple dictionaries!", warningStyle);
                    if (wordsToChange.Count > 0)
                    {
                        foreach (WordToChangeData _data in wordsToChange)
                        {
                            setOfDictionaries[_data.dictID][_data.key] = _data.newWord;
                        }
                        wordsToChange.Clear();
                    }
                    if (removeFromSet != -1 && keyToRemove != "")
                    {
                        setOfDictionaries[removeFromSet].Remove(keyToRemove);
                        removeFromSet = -1;
                        keyToRemove = "";
                    }
                }
            }
        }
        public static void AddDictName(string dictName)
        {
            dictNames.Add(dictName);
        }

        public static string GetDictName(int index)
        {
            if (dictNames == null || index > (dictNames.Count - 1))
                return index.ToString();
            return dictNames[index];
        }


        public static void SetDictionary(Dictionary<string, string> dict, int index)
        {
            if (setOfDictionaries.Count > index)
            {
                setOfDictionaries[index] = dict;

            }
            else if (setOfDictionaries.Count == index)
            {
                setOfDictionaries.Add(dict);
            }

        }
        public static Dictionary<string, string> GetDict(int index)
        {
            return setOfDictionaries[index];
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

                string actionFilePath = "Assets/Resources/" + GetCurrentDictPath() + "dicts.txt";
                var stringBuilder = new StringBuilder();
                foreach (string fName in fileList)
                    stringBuilder.Append(fName + "\n");

                using (StreamWriter swriter = new StreamWriter(actionFilePath))
                    swriter.Write(stringBuilder.ToString());
                return fileList;
            }
            return null;
        }

        public static void ReloadDictionaries()
        {
            ClearDicts();
            dictUnfold.Clear();
            filesWithValueInstance.Clear();
            showFilesForKey = "";
            showFilesForSet = -1;
            selectedSet = -1;
            editInSet = -1;
            GUI.FocusControl(null);
            AssetDatabase.Refresh();

            List<string> dictListData = ListCurrentJsonFiles(GetCurrentDictPath());
            foreach(string dictName in dictListData)
            {   
                if (!string.IsNullOrEmpty(dictName))
                {
                    dictNames.Add(dictName);
                    dictUnfold.Add(true);
                    LoadDictionary(dictName);
                }
            }
            if (toCheckInActionFiles)
                CheckInActionFiles();
            if (dictNames.Contains(selectedName))
            {
                selectedSet = dictNames.IndexOf(selectedName);
            }
        }

        public static void LoadDictionary(string fileName)
        {
            
            Dictionary<string, string> currentDict = new Dictionary<string, string>();
            TextAsset _data = (TextAsset)Resources.Load(GetCurrentDictPath() + fileName);
            if (_data != null)
            {
                JSONNode data = JSON.Parse(_data.text);
                foreach (string key in data.Keys)
                {
                    if (!currentDict.ContainsKey(key))
                        currentDict.Add(key, data[key].ToString().Replace("<br>", "\n").Replace("\"","").Replace("\r", ""));
                }
            }
            setOfDictionaries.Add(currentDict);
        }
    }
}
