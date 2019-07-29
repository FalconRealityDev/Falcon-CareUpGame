﻿using UnityEngine;

public class EndButtonRemover : MonoBehaviour {

    public GameObject goToMenuButton;

    public GameObject ResultInfoHolder;
    [SerializeField]
    private GameObject ScorePanel = null;

    [SerializeField]
    private GameObject StepPanel = null;

    [SerializeField]
    private GameObject QuizPanel = null;

    [SerializeField]
    private GameObject SendScorePanel = null;

    [SerializeField]
    private GameObject CertificatePanel = null;

    public void ButtonClick () {
        goToMenuButton.SetActive (false);
    }

    public void OnToQuizClick () {
        StepPanel.GetComponent<Animator> ().SetBool ("pop", false);
        StepPanel.SetActive (false);

        QuizPanel.SetActive (true);
        QuizPanel.GetComponent<Animator> ().SetBool ("pop", true);
    }
    public void OnToScoreClick()
    {
        //GameObject.FindObjectOfType<EndscoreButtonManager>().
        QuizPanel.GetComponent<Animator>().SetBool("pop", false);
        QuizPanel.SetActive(false);

        ScorePanel.SetActive(true);
        ScorePanel.GetComponent<Animator>().SetBool("pop", true);
        float percent = GameObject.FindObjectOfType<EndScoreManager>().percent;
        int value = Mathf.FloorToInt(percent * 100f);
        GameObject.FindObjectOfType<EndScoreRadial>().StartAnimation(value);
    }
    //----------------

    public void ShowResultInfoHolder()
    {
        ResultInfoHolder.SetActive(true);
    }


    public void OnBackToQuizClick () {
        ScorePanel.GetComponent<Animator>().SetBool ("pop", false);
        ScorePanel.SetActive (false);

        QuizPanel.SetActive (true);
        QuizPanel.GetComponent<Animator>().SetBool ("pop", true);
    }

    public void OnBackToStepsClick()
    {
        QuizPanel.GetComponent<Animator>().SetBool("pop", false);
        QuizPanel.SetActive(false);

        StepPanel.SetActive(true);
        StepPanel.GetComponent<Animator>().SetBool("pop", true);
    }
    public void OnNextButton ()
    {
        ScorePanel.GetComponent<Animator>().SetBool("pop", false);
        ScorePanel.SetActive(false);

        CertificatePanel.SetActive(true);
        CertificatePanel.GetComponent<Animator>().SetBool("pop", true);
    }
    public void OnBackToScoreButton()
    {
        ScorePanel.GetComponent<Animator>().SetBool("pop", true);
        ScorePanel.SetActive(true);

        CertificatePanel.SetActive(false);
        CertificatePanel.GetComponent<Animator>().SetBool("pop", false);
    }
    public void OnSendScoreButton ()
    {
        CertificatePanel.GetComponent<Animator>().SetBool("pop", false);
        CertificatePanel.SetActive(false);

        SendScorePanel.SetActive(true);
        SendScorePanel.GetComponent<Animator>().SetBool("pop", true);
    }

    public void OnBackToCertificate()
    {
        CertificatePanel.SetActive(true);
        CertificatePanel.GetComponent<Animator>().SetBool("pop", true);

        SendScorePanel.SetActive(false);
        SendScorePanel.GetComponent<Animator>().SetBool("pop", false);
    }
}
