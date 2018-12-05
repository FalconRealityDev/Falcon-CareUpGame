﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_Movement : TutorialManager
{

    public AudioClip Popup;
    AudioSource audioSource;

    public enum TutorialStep
    {
        First,
        Welcome,
        PointsExpl,
        MoveToTable,
        MoveBack,
        MoveToDoctor,
        FreeLookExpl,
        MoveWithFreeLook,
        Done,
        None
    }

    private TutorialStep currentStep = TutorialStep.First;

    private GameObject wfPos;
    private GameObject docPos;


    protected override void Start()
    {
        base.Start();

        wfPos = GameObject.Find("WorkFieldPos");
        docPos = GameObject.Find("DoctorPos");

        wfPos.SetActive(false);
        docPos.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();

		if (!Paused())
        {

            audioSource = GetComponent<AudioSource> ();

            switch (currentStep)
            {
                case TutorialStep.First:
                    audioSource.PlayOneShot (Popup, 0.1F);
                    currentStep = TutorialStep.Welcome;
					hintsN.SetSize(788f, 524.9f);
					hintsN.LockTo("UI(Clone)", new Vector3(-393.80f, 214.70f, 0.00f));
                    //hintsBox.anchoredPosition = new Vector2(-0.00011874f, 0.00024414f);
                    //hintsBox.sizeDelta = new Vector2(788f, 524.9f);
                    UItext.text = "Welkom, in deze leermodule leer je hoe je door de omgeving van Care Up kunt bewegen. ";
                    SetUpTutorialNextButton();
                    break;
                case TutorialStep.Welcome:
                    if (nextButtonClicked)
                    {
                        audioSource.PlayOneShot (Popup, 0.1F);
                        //hintsBox.anchoredPosition = new Vector2(165f, -265.64f);
                        //hintsBox.sizeDelta = new Vector2(788f, 524.9f);
                        currentStep = TutorialStep.PointsExpl;

                        UItext.text = "Binnen Care Up kun je bewegen door te klikken op interessante objecten. Je ontdekt de objecten door er met de muis overheen te bewegen. Speel je op de tablet of telefoon? Dan zijn de namen van de objecten ook weergegeven. ";
                        SetUpTutorialNextButton();
                    }
                    break;
                case TutorialStep.PointsExpl:
                    if (nextButtonClicked)
                    {
                        audioSource.PlayOneShot (Popup, 0.1F);
                        //hintsBox.anchoredPosition = new Vector2(165f, -265.64f);
                        //hintsBox.sizeDelta = new Vector2(472.5f, 298.9f);
                        hintsN.LockTo("WorkField", new Vector3(0.00f, 0.41f, -0.66f));
						hintsN.ResetSize();
                        currentStep = TutorialStep.MoveToTable;
                        UItext.text = "Probeer nu naar het werkveld te bewegen. Dit kun je doen door op het werkveld te klikken.";

                        player.tutorial_movedTo = false;
                        wfPos.SetActive(true);
                    }
                    break;
                case TutorialStep.MoveToTable:
                    if (player.tutorial_movedTo)
                    {
                        audioSource.PlayOneShot (Popup, 0.1F);
                        wfPos.SetActive(false);
                        player.tutorial_movedTo = false;

                        //hintsBox.anchoredPosition = new Vector2(681f, 175f);
                        //hintsBox.sizeDelta = new Vector2(472.5f, 298.9f);
                        hintsN.LockTo("MoveBackButton", new Vector3(-59.90f, -87.00f, 0.00f));
                        hintsN.SetIconPosition(1);
                        currentStep = TutorialStep.MoveBack;
                        UItext.text = "We zijn naar het werkveld verplaatst. Op ieder moment kun je terugkeren naar de beginpositie. Dit wordt het overzicht genoemd. Wil je terug? Klik dan rechtsboven in op de knop ‘Terug naar overzicht’. ";

                        player.tutorial_movedBack = false;
                    }
                    break;
                case TutorialStep.MoveBack:
                    if (player.tutorial_movedBack)
                    {
                        audioSource.PlayOneShot (Popup, 0.1F);
                        player.tutorial_movedBack = false;
                        //hintsBox.anchoredPosition = new Vector2(-471f, 35f);
                        //hintsBox.sizeDelta = new Vector2(472.5f, 298.9f);
						hintsN.SetIconPosition(0);
                        hintsN.LockTo("doc", new Vector3(-59.36f, 8.00f, -20.36f));
                        currentStep = TutorialStep.MoveToDoctor;
                        UItext.text = "Heel goed. Probeer nu richting je collega te bewegen door op haar te klikken.";

                        player.tutorial_movedTo = false;
                        docPos.SetActive(true);
                    }
                    break;
                case TutorialStep.MoveToDoctor:
                    if (player.tutorial_movedTo)
                    {
                        audioSource.PlayOneShot (Popup, 0.1F);
                        docPos.SetActive(false);
                        player.tutorial_movedTo = false;
                        //hintsBox.anchoredPosition = new Vector2(-0.00011874f, 0.00024414f);
                        //hintsBox.sizeDelta = new Vector2(788f, 524.9f);
                        hintsN.LockTo("/doc", new Vector3(345.90f, 1.40f, 707.00f));
                        hintsN.SetSize(675f, 400f);
                        currentStep = TutorialStep.FreeLookExpl;
                        UItext.text = "Wil je om je heen kijken? Dat kan! Speel je op de computer dan doe je dit door de linkermuisknop in te drukken en ingedrukt te houden. Beweeg vervolgens de muis om rond te kijken. Laat de linkermuisknop los om te stoppen. Speel je op de mobiel of tablet? Kijk dan rond door met je vinger over het beeld te swipen. Klik hierbij niet op de interessante objecten. ";
                        SetUpTutorialNextButton();
                    }
                    break;
                case TutorialStep.FreeLookExpl:
                    if (nextButtonClicked)
                    {
                        audioSource.PlayOneShot (Popup, 0.1F);
                        //hintsBox.anchoredPosition = new Vector2(165f, -265.64f);
                        //hintsBox.sizeDelta = new Vector2(472.5f, 298.9f);
                        currentStep = TutorialStep.MoveWithFreeLook;
                        UItext.text = "Beweeg vanaf je collega direct naar het werkveld zonder terug te keren naar het overzicht.";
						hintsN.ResetSize();
						hintsN.SetIconPosition(1);
                        hintsN.LockTo("doc", new Vector3(0.48f, 1.32f, 1.22f));
                        player.MoveBackButtonObject.SetActive(false);
                        player.tutorial_movedTo = false;
                        wfPos.SetActive(true);
                    }
                    break;
                case TutorialStep.MoveWithFreeLook:
                    if (player.tutorial_movedTo)
                    {
                        audioSource.PlayOneShot (Popup, 0.1F);
                        wfPos.SetActive(false);
                        //hintsBox.anchoredPosition = new Vector2(502f, -346f);
                        //hintsBox.sizeDelta = new Vector2(472.5f, 298.9f);
						hintsN.LockTo("UI(Clone)", new Vector3(376.67f, -44.90f, 0.00f));
						hintsN.SetIconPosition(0);
                        currentStep = TutorialStep.Done;
                        UItext.text = "Gefeliciteerd! Je weet nu hoe je kunt rond bewegen binnen Care Up.";
                    }
                    break;
                case TutorialStep.Done:
                    currentStep = TutorialStep.None;
                    TutorialEnd();
                    break;
            }
        }
	}
}
