using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CareUp.Actions;
using UnityEngine.UI;
using TMPro;
using CareUp.Localize;

public class ActionSubjectWindow : MonoBehaviour
{
    public Transform scrollContent;
    ActionManager actionManager;
    Color subjectBaseColor = new Color();
    Color actionBaseColor = new Color();
    private PlayerPrefsManager manager;


    Color defaultColor;
    Color currentStepColor;
    Color complitedStepColor;

    Dictionary<int, List<GameObject>> ActionStepElements = new Dictionary<int, List<GameObject>>();
    void Start()
    {
        actionManager = GameObject.FindObjectOfType<ActionManager>();
        manager = GameObject.FindObjectOfType<PlayerPrefsManager>();
        BuildActionsList();
    }

    public void RefrashWindow()
    {
        if (actionManager != null && manager != null)
        {
            int currentActionIndex = actionManager.CurrentActionIndex;
            foreach (Action a in actionManager.actionList)
            {
                if (ActionStepElements.ContainsKey(a.info.sequentialNumber))
                {
                    int index = a.SubIndex;
                    foreach(GameObject g in ActionStepElements[a.info.sequentialNumber])
                    {
                        Image img = g.transform.GetChild(0).GetComponent<Image>();

                        //Is current step
                        if (currentActionIndex == index)
                        {
                            if (!manager.practiceMode)
                                g.SetActive(false);
                            else
                                img.color = new Color(.4f, .8f, .97f, 0.4f);
                        }

                        //Passed step
                        else if (index < currentActionIndex)
                        {
                            if (!manager.practiceMode)
                                g.SetActive(true);
                            else
                                img.color = new Color(0f, 0.7f, 0f, 0.2f);
                        }
                        //A step that has not yet been reached
                        else
                        {
                            if (!manager.practiceMode)
                                g.SetActive(false);
                            else
                                img.color = new Color(.6f, .6f, .6f, 0.4f);
                        }
                        //Complited correctly step
                        if (a.info.matched)
                        {
                            if (!manager.practiceMode)
                                g.SetActive(true);
                            //Step completed wrongly
                            if (actionManager.IsActionDoneWrong(a))
                            {
                                img.color = new Color(1f, .0f, .0f, 0.6f);
                            }
                            else
                            {
                                Transform checkMark = g.transform.Find("Panel/CheckMark");
                                if (checkMark != null)
                                {
                                    checkMark.GetComponent<Image>().enabled = true;
                                }
                                img.color = new Color(0f, 0.9f, 0.1f, 0.3f);
                            }
                        }
                    }
                }
            }
        }
    }

    public void BuildActionsList()
    {
        if (actionManager != null)
        {
            InGameLocalEditTool inGameLocalEditTool = PlayerPrefsManager.GetDevMode() ? GameObject.FindObjectOfType<InGameLocalEditTool>() : null;  
            
            foreach (Action a in actionManager.actionList)
            {   
                List<GameObject> currentActionElements = new List<GameObject>();
                if (a.info.subjectTitle != "")
                {
                    GameObject subjectTitleObj = GameObject.Instantiate(Resources.Load<GameObject>(
                        "NecessaryPrefabs/UI/SubjectTitle"), scrollContent);
                    subjectTitleObj.transform.Find("Panel/Text").GetComponent<TextMeshProUGUI>().text =
                        a.info.subjectTitle;
                    currentActionElements.Add(subjectTitleObj);
                }
                GameObject actionStep = GameObject.Instantiate(Resources.Load<GameObject>(
                    "NecessaryPrefabs/UI/SubjectStep"), scrollContent);
                //@
                actionStep.transform.Find("Panel/Text").GetComponent<TextMeshProUGUI>().text =
                    LocalizationManager.GetValueIfKey(a.info.shortDescr);

                if (inGameLocalEditTool != null)
                {
                    //!
                    inGameLocalEditTool.AddUILocalizationComponentToGO(
                        actionStep.transform.Find("Panel/Text").gameObject, a.info.shortDescr);
                }

                currentActionElements.Add(actionStep);
                ActionStepElements.Add(a.info.sequentialNumber, currentActionElements);
            }
            GameObject.Instantiate(Resources.Load<GameObject>(
                "NecessaryPrefabs/UI/SubjectEmpty"), scrollContent);
            
        }
        RefrashWindow();
    }
}
