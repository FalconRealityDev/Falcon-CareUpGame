﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class UMP_LevelInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public Text Title;
    public Text Description;
    public Text PlayText;
    public Image Preview;
    //Name of scene of build setting
    private string LevelName;
    static Button_Functions sounds;

    private void Start()
    {
        sounds = GameObject.FindObjectOfType<Button_Functions>();
    }

    /// <summary>
    /// Level Info
    /// </summary>
    /// <param name="title"></param>
    /// <param name="desc"></param>
    /// <param name="preview"></param>
    /// <param name="scene"></param>
    public void GetInfo(string title,string desc,Sprite preview,string scene,string pn)
    {
        Title.text = title;
        //Description.text = desc;
        Preview.sprite = preview;
        PlayText.text = pn;
        LevelName = scene;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OpenLevel() { LoadLevel(LevelName); }

    public static void LoadLevel(string scene)
    {
        #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene(scene);
        #else
            Application.LoadLevel(scene);
        #endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Forward"></param>
    public void EventMouse(bool Forward = true)
    {
        Animator a = this.GetComponent<Animator>();
        if (Forward)
        {
            a.SetBool("show", true);
        }
        else
        {
            a.SetBool("show", false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventMouse(true);
        sounds.OnButtonHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventMouse(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        sounds.OnButtonClick();
    }
}