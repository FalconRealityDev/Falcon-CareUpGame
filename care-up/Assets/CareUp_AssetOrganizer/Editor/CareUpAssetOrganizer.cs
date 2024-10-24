﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets;
using UnityEditor.SceneManagement;
using System.Text;


public class CareUpAssetOrganizer : EditorWindow
{
    public static int itemsToProcess = 1;
    public static int itemsProcessed = 1;

    static string ListOfScenes = "BundleBuilderScenes";
    static string ListOfExtraScenes = "BundleBuilderExtraScenes";

    static Dictionary<string, List<string>> scenesData = new Dictionary<string, List<string>>();

    [MenuItem("Tools/Organize Assets in Bundles")]
    static void Init()
    {
        UnityEditor.EditorWindow window = GetWindow(typeof(CareUpAssetOrganizer));
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Set prefabs to Object Layer"))
        {
            string[] __prefabs = Directory.GetFiles("Assets/Resources_moved/Prefabs/");
            foreach (string p in __prefabs)
            {
                if (Path.GetExtension(p.ToLower()) == ".prefab")
                {
                    GameObject __prefab = (GameObject)AssetDatabase.LoadMainAssetAtPath(p);
                    if (__prefab.GetComponent<PickableObject>() != null)
                    {
                        PrefabUtility.InstantiatePrefab(__prefab);
                        Debug.Log(p);
                    }
                }
            }
        }

        if (GUILayout.Button("Select Dependency Prefabs"))
        {
            string scenePath = EditorSceneManager.GetActiveScene().path;
            string[] dep = AssetDatabase.GetDependencies(scenePath);
            GameObject prefabHolder = GameObject.Find("PrefabHolder");
            if (prefabHolder == null)
            {
                GameObject holder = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets/Resources/NecessaryPrefabs/PrefabHolder.prefab");
                prefabHolder = Instantiate(holder) as GameObject;
                prefabHolder.name = "PrefabHolder";
            }
            if (prefabHolder != null)
            {
                foreach (string d in dep)
                {
                    if (Path.GetExtension(d.ToLower()) == ".prefab" &&
                        d.Split('/')[2] == "Prefabs")
                    {
                        GameObject prefab = (GameObject)AssetDatabase.LoadMainAssetAtPath(d);
                        if (prefab.GetComponent<PickableObject>() != null)
                        {
                            GameObject prefabInst = PrefabUtility.InstantiatePrefab(prefab, prefabHolder.transform) as GameObject;
                            //GameObject prefabInst = Instantiate(prefab, prefabHolder.transform) as GameObject;
                            prefabInst.name = Path.GetFileNameWithoutExtension(d);
                        }
                    }
                }
            }


        }
        if (GUILayout.Button("++Start Creating Addressable Groups++"))
        {
            List<string> __paths = new List<string>();
            List<string> extraPaths = new List<string>();

            itemsToProcess = 1;
            itemsProcessed = 0;

            List<string> _resources = new List<string>();
            List<string> full_resources = new List<string>();

            scenesData.Clear();
            List<string> scenes = LoadScenesList(ListOfScenes);
            var scenesFolder = "Assets/Scenes/";
            List<string> matExt = new List<string> { ".mat", ".jpg", ".fbx", ".png", ".exr", ".ogg", ".prefab", ".ttf",
                ".wav", ".controller", ".anim", ".otf", ".mask", ".shader", ".psd", ".mp3", ".asset", ".tif", ".tga", ".tiff", ".jpeg",
                ".mesh", ".exr", ".renderTexture"};
            int i = 0;
            itemsToProcess = scenes.Count;
            foreach (string scene in scenes)
            {
                string scenePath = scenesFolder + scene.Split('@')[0] + ".unity";
                string sceneNameForAddr = scene.Split('@')[0];
                string scenePathForAddr = scenePath;
                if (scene.Contains('/'))
                {
                    scenePath = scene.Split('@')[0];
                    scenePathForAddr = scenePath;

                    string[] ssplit = scene.Split('@')[0].Split("/");
                    sceneNameForAddr = ssplit[ssplit.Length - 1];
                }

                if (scene.Contains('@'))
                {
                    scenePathForAddr = scene.Split('@')[1];
                }

                Object sceneObject = AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset));
                if (sceneObject == null)
                    continue;
                if (!scenesData.ContainsKey(scene.Split('@')[0]))
                    scenesData.Add(scene.Split('@')[0], new List<string>());


                AddAssetToGroup(scenePath, "scene-" + sceneNameForAddr.ToLower().Replace(' ', '_'), scenePathForAddr);
                __paths.Add(scenePath);
                extraPaths.Add(scenePath);
                string[] dep = AssetDatabase.GetDependencies(scenePath);
                foreach (string d in dep)
                {
                    if (matExt.Contains(Path.GetExtension(d.ToLower())))
                    {
                        if (!_resources.Contains(d))
                            _resources.Add(d);

                        scenesData[scene.Split('@')[0]].Add(d);
                    }
                    if (!full_resources.Contains(d))
                        full_resources.Add(d);
                }

                i++;
                itemsProcessed++;
                EditorUtility.DisplayProgressBar("Progress", "Processing scenes", (float)itemsProcessed / (float)itemsToProcess);
            }
            Dictionary<string, string> _resCont = new Dictionary<string, string>();
            List<string> bundleNames = new List<string>();
            itemsToProcess = _resources.Count;
            itemsProcessed = 0;
            foreach (string res in _resources)
            {
                string resContainerName = GetContainerName(res);
                if (!bundleNames.Contains(resContainerName))
                    bundleNames.Add(resContainerName);
                _resCont.Add(res, resContainerName);
                string groupName = "asset-" + (Mathf.Abs(resContainerName.GetHashCode())).ToString();
                AddAssetToGroup(res, groupName, res);
                if (!__paths.Contains(res))
                {
                    __paths.Add(res);
                    extraPaths.Add(res);
                }
                itemsProcessed++;
                string titleMessage = "Progressed " + itemsProcessed.ToString() + " of " + itemsToProcess.ToString();
                EditorUtility.DisplayProgressBar(titleMessage, "Processing assets | " + res, (float)itemsProcessed / (float)itemsToProcess);
            }

            // Build list of files that are not part of addressables but are important
            List<string> extraScenes = LoadScenesList(ListOfExtraScenes);
            foreach (string scene in extraScenes)
            {
                string scenePath = scene;

                Object sceneObject = AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset));
                if (sceneObject == null)
                    continue;
                extraPaths.Add(scenePath);
                string[] dep = AssetDatabase.GetDependencies(scenePath);
                foreach (string d in dep)
                {
                    extraPaths.Add(d);
                }
                // --------------------------------
            }
            LogFilePaths(extraPaths);
            Debug.Log("Finished");
        }

        EditorUtility.ClearProgressBar();

        if (GUILayout.Button("Remove Empty Addressable Groups"))
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var groups = settings.groups;
            List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroup> groupsToDelete =
                new List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroup>();
            foreach (var g in groups)
            {
                Debug.Log(g.name + " " + g.entries.Count.ToString());
                if (g.entries.Count == 0)
                    groupsToDelete.Add(g);
            }
            foreach (var g in groupsToDelete)
            {
                settings.RemoveGroup(g);
            }
        }
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    void LogFilePaths(List<string> __paths)
    {
        var stringBuilder = new StringBuilder();
        foreach (string p in __paths)
        {
            stringBuilder.Append(" 0 kb	 0.0% " + p + "\n");
        }
        using (StreamWriter swriter = new StreamWriter("organizer_files_log.txt"))
            swriter.Write(stringBuilder.ToString());
    }

    static void AddAssetToGroup(string path, string groupName, string pathForAddr)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.FindGroup(groupName);
        if (!group)
        {
            group = settings.CreateGroup(groupName, false, false, true, new List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroupSchema>
            { settings.DefaultGroup.Schemas[1] });
        }
        var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), group, false, true);

        if (entry == null)
        {
            Debug.Log($"Addressable : can't add {path} to group {groupName}");
        }
        else
        {
            if (groupName.Substring(0, 5) == "scene")
            {
                entry.address = pathForAddr;
            }
            else
            {
                entry.address = pathForAddr.ToLower().Replace("resources_moved", "resources");
            }
        }
    }


    static string IntToCode(int value)
    {
        string simbols = "abcdefghijklmnopqrstuvwxyz";
        int lNum = (int)(value / 10);
        int dNum = value % 10;
        string _code = simbols[lNum] + dNum.ToString();
        return _code;
    }

    static string GetContainerName(string resourcePath)
    {
        string containerName = "";
        int i = 0;
        foreach (string scene in scenesData.Keys)
        {
            if (scenesData[scene].Contains(resourcePath))
            {
                containerName += IntToCode(i);
            }
            i++;
        }

        return containerName;
    }

    static List<string> LoadScenesList(string listOfScenesFilePath)
    {
        List<string> scenes = new List<string>();
        TextAsset dictListData = (TextAsset)Resources.Load(listOfScenesFilePath);
        foreach (string dictName in dictListData.text.Split('\n'))
        {
            if (!string.IsNullOrEmpty(dictName))
            {
                if (dictName.Substring(0, 1) != "#")
                {
                    scenes.Add(dictName.Replace("\r", ""));
                }
            }
        }
        return scenes;
    }

}
