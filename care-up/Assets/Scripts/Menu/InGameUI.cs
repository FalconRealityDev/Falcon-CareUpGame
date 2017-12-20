﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour {

    private Transform ui;
    private Transform main;
    private Transform options;
    private Transform controlsUI;

    private GameObject game;
    private Controls controls;
    private GameTimer timer;

    private PlayerScript player;
    private Crosshair crosshair;
    private Animator animator;

    private bool playerState = false;
    private float animatorSpeed = 1f;

    PlayerPrefsManager prefsManager;

    Slider volumeSlider;
    Dropdown qualityDropdown;
    Dropdown resolutionDropdown;
    Toggle fullscrToggle;
    GameObject escapeButton;

    List<Resolution> resolutions;

    private Selectable gamepadDefault;

	void Start () {

        ui = transform.GetChild(0);
        main = ui.GetChild(0);
        options = ui.GetChild(1);
        controlsUI = ui.GetChild(2);

        game = GameObject.Find("GameLogic");
        if (game != null)
        {
            controls = game.GetComponent<Controls>();
            timer = game.GetComponent<GameTimer>();
        }

        player = GameObject.Find("Player").GetComponent<PlayerScript>();
        crosshair = GameObject.Find("Player").GetComponent<Crosshair>();
        animator = player.transform.GetChild(0).GetChild(0).GetComponent<Animator>();

        if (GameObject.Find("Preferences"))
        {
            prefsManager = GameObject.Find("Preferences").GetComponent<PlayerPrefsManager>();
        }

        Transform group = transform.GetChild(0).GetChild(1).GetChild(1);

        volumeSlider = group.GetChild(0).GetChild(1).GetComponent<Slider>();
        qualityDropdown = group.GetChild(1).GetChild(1).GetComponent<Dropdown>();
        resolutionDropdown = group.GetChild(2).GetChild(1).GetComponent<Dropdown>();
        fullscrToggle = group.GetChild(3).GetChild(1).GetComponent<Toggle>();

        if (prefsManager != null)
        {
            volumeSlider.value = prefsManager.Volume;
        }

        List<string> qNames = new List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(qNames);
        qualityDropdown.value = QualitySettings.GetQualityLevel();

        resolutions = new List<Resolution>(Screen.resolutions);

        List<string> rNames = new List<string>();
        foreach (Resolution r in resolutions)
        {
            rNames.Add(r.width + "x" + r.height);
        }
        resolutionDropdown.AddOptions(rNames);
        resolutionDropdown.value = resolutions.IndexOf(Screen.currentResolution);

        fullscrToggle.isOn = Screen.fullScreen;

        gamepadDefault = main.GetChild(0).GetComponent<Button>();

        escapeButton = GameObject.Find("TouchEscapeButton");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }

        if (ui.gameObject.activeSelf)
        {
            GamepadSwitch.HandleUpdate(gamepadDefault);
        }
    }

    public void Toggle()
    {
        if (ui.gameObject.activeSelf)
        {
            ui.gameObject.SetActive(false);
            if (game != null)
            {
                controls.keyPreferences.ToggleLock();
                timer.enabled = true;
            }

            player.enabled = playerState;
            crosshair.enabled = prefsManager == null ? false : prefsManager.VR;

            animator.speed = animatorSpeed;
            Time.timeScale = 1f;

            ToggleAllSounds(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            escapeButton.SetActive(false);
        }
        else
        {
            ui.gameObject.SetActive(true);
            if (game != null)
            {
                controls.keyPreferences.ToggleLock();
                timer.enabled = false;
            }

            playerState = player.enabled;
            player.enabled = false;
            crosshair.enabled = false;

            animatorSpeed = animator.speed;
            animator.speed = 0.0f;
            Time.timeScale = 0f;

            ToggleAllSounds(false);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            escapeButton.SetActive(true);
        }
    }

    public void OnContinueButtonClick()
    {
        Toggle();
    }

    public void OnSaveButtonClick()
    {
        GameObject.Find("Preferences").GetComponent<SaveLoadManager>().Save();
    }

    public void OnOptionsButtonClick()
    {
        main.gameObject.SetActive(false);
        options.gameObject.SetActive(true);

        gamepadDefault = options.Find("BackButton").GetComponent<Button>();
    }

    public void OnOptionsBackButtonClick()
    {
        main.gameObject.SetActive(true);
        options.gameObject.SetActive(false);

        gamepadDefault = main.GetChild(0).GetComponent<Button>();

        // save some heavy settings
        QualitySettings.SetQualityLevel(qualityDropdown.value, true);
        Screen.SetResolution(resolutions[resolutionDropdown.value].width,
            resolutions[resolutionDropdown.value].height, fullscrToggle.isOn);
    }

    public void OnControlsButtonClick()
    {
        main.gameObject.SetActive(false);
        controlsUI.gameObject.SetActive(true);

        gamepadDefault = controlsUI.Find("BackButton").GetComponent<Button>();

    }

    public void OnControlsBackButtonClick()
    {
        main.gameObject.SetActive(true);
        controlsUI.gameObject.SetActive(false);

        gamepadDefault = main.GetChild(0).GetComponent<Button>();
    }

    public void OnExitButtonClick()
    {
        GameObject.Find("Preferences").GetComponent<LoadingScreen>().LoadLevel("Menu");
    }

    public void ToggleAllSounds(bool value)
    {
        AudioSource [] audio = GameObject.FindObjectsOfType<AudioSource>();
        foreach ( AudioSource a in audio )
        {
            if ( value )
            {
                a.UnPause();
            }
            else
            {
                a.Pause();
            }
        }
    }

    public void OnVolumeSliderChange()
    {
        AudioListener.volume = volumeSlider.value;
        if (prefsManager != null)
        {
            prefsManager.Volume = volumeSlider.value;
        }
    }
}
