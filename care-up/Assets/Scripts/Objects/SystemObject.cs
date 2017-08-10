﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Object that loads certain scene or performs an action, not related to gameplay.
/// </summary>
public class SystemObject : InteractableObject {

    public string sceneName;

    private LoadingScreen loadingScreen;
    private PlayerPrefsManager prefs;

    protected override void Start()
    {
        base.Start();

        if (GameObject.Find("Preferences") != null)
        {
            loadingScreen = GameObject.Find("Preferences").GetComponent<LoadingScreen>();
            if (loadingScreen == null) Debug.LogError("No loading screen found");

            prefs = GameObject.Find("Preferences").GetComponent<PlayerPrefsManager>();
            if (prefs == null) Debug.LogError("No PlayerPrefsManager");
        }
        else
        {
            Debug.LogWarning("No 'preferences' found. Game needs to be started from first scene");
        }
    }

    protected override void Update()
    {
        if (prefs != null && prefs.VR)
        {
            base.Update();
        }
    }

    // called when player interacts with object
    public virtual void Use(bool confirmed = false)
    {
        if (!prefs.VR)
            return;

        if (sceneName == "_Start")
        {
            // temporary until scene selection is designed
            loadingScreen.LoadLevel("Tutorial");
            /*
            if (!GameObject.Find("Preferences").GetComponent<PlayerPrefsManager>().TutorialCompleted)
            {
                if (GameObject.Find("_SkipTutorial"))
                {
                    Debug.LogWarning("TutorialSkip found! => Skipping tutorial");
                    loadingScreen.LoadLevel("SceneSelection");
                }
                else
                {
                    Debug.Log("Tutorial is not completed.");
                    loadingScreen.LoadLevel("Tutorial");
                }
            }
            else
            {
                Debug.Log("Tutorial is completed.");
                loadingScreen.LoadLevel("SceneSelection");
            }*/
        }
        else {
            if (sceneName == "_Continue")
            {
                GameObject.Find("Preferences").GetComponent<SaveLoadManager>().LoadLevel();
            }
            else if (sceneName == "_Exit")
            {
                Application.Quit();
            }
            else {
                if (confirmed || sceneName == "Tutorial" || sceneName == "Options")
                {
                    loadingScreen.LoadLevel(sceneName);
                    //SceneManager.LoadScene(sceneName);
                }
                else {
                    cameraMode.ToggleCameraMode(CameraMode.Mode.ConfirmUI);
                }
            }
        }
    }

    protected override void SetShaderTo(Shader shader)
    {
        foreach (Material m in rend.materials)
        {
            m.shader = shader;
        }
    }
}
