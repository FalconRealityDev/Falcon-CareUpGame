﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CharacterCreationScene : MonoBehaviour
{
    public enum CharGender
    {
        Male,
        Female
    };

    public CharGender gender;
    public int headType;
    public int bodyType;
    public int glassesType;

    private GameObject maleChar;
    private GameObject femaleChar;
    private List<Transform> maleHeads = new List<Transform>();
    private List<Transform> femaleHeads = new List<Transform>();
    private List<Transform> femaleBodies = new List<Transform>();
    private List<Transform> maleBodies = new List<Transform>();
    private List<Transform> maleGlasses = new List<Transform>();
    private List<Transform> femaleGlasses = new List<Transform>();

    /*
    private Text genderLabel;
    private Text headLabel;
    private Text bodyLabel;
    private Text glassesLabel;
    */

    void Start()
    {
        // main objects for genders    
        femaleChar = GameObject.Find("Female_Citizens_customizable");
        maleChar = GameObject.Find("Male_Citizens_customizable");

        Initialize();

        // set up initial info
        SetCurrent(CharGender.Female, 0, 0, -1);
    }

    public void Initialize()
    {
        // bodies
        femaleChar.transform.GetComponentsInChildren<Transform>(true, femaleBodies);
        maleChar.transform.GetComponentsInChildren<Transform>(true, maleBodies);

        // filter bodies lists, leave only correct ones (thx unity3d)
        femaleBodies = femaleBodies.Where(b => b.name.Contains("f_body_")).ToList();
        maleBodies = maleBodies.Where(b => b.name.Contains("Body_")).ToList();

        // heads
        femaleChar.transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Neck/Bip001 Head/HEAD_CONTAINER")
            .GetComponentsInChildren<Transform>(true, femaleHeads);
        maleChar.transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Neck/Bip001 Head/HEAD_CONTAINER")
            .GetComponentsInChildren<Transform>(true, maleHeads);

        // trim first ones cuz it's "HEAD_CONTAINER"
        femaleHeads.RemoveAt(0);
        maleHeads.RemoveAt(0);

        //trim three last ones cuz those are glasses
        femaleGlasses = femaleHeads.GetRange(femaleHeads.Count - 3, 3);
        femaleHeads.RemoveRange(femaleHeads.Count - 3, 3);
        maleGlasses = maleHeads.GetRange(maleHeads.Count - 3, 3);
        maleHeads.RemoveRange(maleHeads.Count - 3, 3);

        /* set up labels/buttons
        genderLabel = GameObject.Find("Canvas/CharacterPanel/Panel/Gender/Label").GetComponent<Text>();
        headLabel = GameObject.Find("Canvas/CharacterPanel/Panel/Head/Label").GetComponent<Text>();
        bodyLabel = GameObject.Find("Canvas/CharacterPanel/Panel/Body/Label").GetComponent<Text>();
        glassesLabel = GameObject.Find("Canvas/CharacterPanel/Panel/Glasses/Label").GetComponent<Text>();
        */
    }
    void UpdateLabels()
    {
        /*
        genderLabel.text = (gender == CharGender.Male) ? "Male" : "Female";
        headLabel.text = "Head #" + (headType+1); // first type is 0
        bodyLabel.text = "Body #" + (bodyType+1); // which is weird for non-programmers :D
        glassesLabel.text = (glassesType >= 0) ? "Glasses #" + (glassesType + 1) : "None";
        */
    }

    void UpdateMaleHeads()
    {
        foreach (Transform h in maleHeads)
        {
            h.gameObject.SetActive(maleHeads.IndexOf(h) == headType && gender == CharGender.Male);
        }
    }

    void UpdateFemaleHeads()
    {
        foreach (Transform h in femaleHeads)
        {
            h.gameObject.SetActive(femaleHeads.IndexOf(h) == headType && gender == CharGender.Female);
        }
    }

    void UpdateMaleBodies()
    {
        foreach (Transform b in maleBodies)
        {
            b.gameObject.SetActive(maleBodies.IndexOf(b) == bodyType && gender == CharGender.Male);
        }
    }

    void UpdateFemaleBodies()
    {
        foreach (Transform b in femaleBodies)
        {
            b.gameObject.SetActive(femaleBodies.IndexOf(b) == bodyType && gender == CharGender.Female);
        }
    }

    void UpdateMaleGlasses()
    {
        foreach (Transform g in maleGlasses)
        {
            g.gameObject.SetActive(maleGlasses.IndexOf(g) == glassesType && gender == CharGender.Male);
        }
    }

    void UpdateFemaleGlasses()
    {
        foreach (Transform g in femaleGlasses)
        {
            g.gameObject.SetActive(femaleGlasses.IndexOf(g) == glassesType && gender == CharGender.Female);
        }
    }

    void UpdateActiveObjects()
    {
        maleChar.SetActive(gender == CharGender.Male);
        femaleChar.SetActive(gender == CharGender.Female);

        UpdateMaleHeads();
        UpdateFemaleHeads();

        UpdateMaleBodies();
        UpdateFemaleBodies();

        UpdateMaleGlasses();
        UpdateFemaleGlasses();
    }

    /// <summary>
    /// Currently no check for out of bounds, plz care
    /// </summary>
    /// <param name="glasses">-1 for None</param>
    void SetCurrent(CharGender g, int head, int body, int glasses)
    {
        gender = g;
        headType = head;
        bodyType = body;
        glassesType = glasses;

        UpdateLabels();
        UpdateActiveObjects();
    }

    public void PreviousGender()
    {
        // using "if" for now in case i'll add saving last body+head of different gender instead of just resetting
        if (gender == CharGender.Female)
        {
            gender = CharGender.Male;
        }
        else
        {
            gender = CharGender.Female;
        }

        // but for now we're just resetting
        bodyType = 0;
        headType = 0;

        UpdateActiveObjects();
        UpdateLabels();
    }

    public void NextGender()
    {
        // now that i think, same as previous function
        // at least while we have only 2 genders
        PreviousGender();
    }

    public void PreviousHead()
    {
        --headType; // -1
        if (headType < 0)
        {
            if (gender == CharGender.Female)
            {
                headType = femaleHeads.Count - 1;
            }
            else
            {
                headType = maleHeads.Count - 1;
            }
        }

        if (gender == CharGender.Female)
        {
            UpdateFemaleHeads();
        }
        else
        {
            UpdateMaleHeads();
        }

        UpdateLabels();
    }

    public void NextHead()
    {
        ++headType;
        if (gender == CharGender.Female)
        {
            if (headType >= femaleHeads.Count)
            {
                headType = 0;
            }
            UpdateFemaleHeads();
        }
        else
        {
            if (headType >= maleHeads.Count)
            {
                headType = 0;
            }
            UpdateMaleHeads();
        }
        UpdateLabels();
    }

    public void PreviousBody()
    {
        --bodyType;
        if (bodyType < 0)
        {
            if (gender == CharGender.Female)
            {
                bodyType = femaleBodies.Count - 1;
            }
            else
            {
                bodyType = maleBodies.Count - 1;
            }
        }

        if (gender == CharGender.Female)
        {
            UpdateFemaleBodies();
        }
        else
        {
            UpdateMaleBodies();
        }

        UpdateLabels();
    }

    public void NextBody()
    {
        ++bodyType;
        if (gender == CharGender.Female)
        {
            if (bodyType >= femaleBodies.Count)
            {
                bodyType = 0;
            }
            UpdateFemaleBodies();
        }
        else
        {
            if (bodyType >= maleBodies.Count)
            {
                bodyType = 0;
            }
            UpdateMaleBodies();
        }
        UpdateLabels();
    }

    public void PreviousGlasses()
    {
        --glassesType;
        if (glassesType < -1) // -1 means none, allow that one
        {
            // currently hardcoded 3 types of glasses
            glassesType = 2; // 2 is index of #3, 0 is index of #1
        }

        if (gender == CharGender.Female)
        {
            UpdateFemaleGlasses();
        }
        else
        {
            UpdateMaleGlasses();
        }

        UpdateLabels();
    }

    public void NextGlasses()
    {
        ++glassesType;

        if (glassesType > 2)
        {
            glassesType = -1;
        }

        if (gender == CharGender.Female)
        {
            UpdateFemaleGlasses();
        }
        else
        {
            UpdateMaleGlasses();
        }

        UpdateLabels();
    }

    public void MaleBtn()
    {
        SetCurrent(CharGender.Male, 0, 0, -1);
    }

    public void FemaleBtn()
    {
        SetCurrent(CharGender.Female, 0, 0, -1);
    }

    public void Save()
    {
        CharacterInfo.currentCharacter.SetCharacterCharacteristicsWU(
            ((gender == CharGender.Female) ? "Female" : "Male"),
            headType, bodyType, glassesType);

        // so while we're saving info we're back to our weird tutorial completion check
        if (PlayerPrefs.GetInt("FirstLogin") <= 1)
        {
            PlayerPrefs.SetInt("FirstLogin", 2);

            string sceneName = "Tutorial_UI";
            string bundleName = "tutorial_ui";
            bl_SceneLoaderUtils.GetLoader.LoadLevel(sceneName, bundleName);
        }
        else
        {
            // tutorial was already played, load main menu? 
            bl_SceneLoaderUtils.GetLoader.LoadLevel("MainMenu");
        }
    }
    
    public void ShowCharacter(GameObject male, GameObject female)
    {
        femaleChar = female;
        maleChar = male;

        Initialize();

        switch (CharacterInfo.currentCharacter.sex)
        {
            case "Female":
                {
                    foreach (Transform h in femaleHeads)
                    {
                        h.gameObject.SetActive(femaleHeads.IndexOf(h) == CharacterInfo.currentCharacter.headType);
                    }

                    foreach (Transform h in femaleBodies)
                    {
                        h.gameObject.SetActive(femaleBodies.IndexOf(h) == CharacterInfo.currentCharacter.bodyType);
                    }

                    foreach (Transform h in femaleGlasses)
                    {
                        h.gameObject.SetActive(femaleGlasses.IndexOf(h) == CharacterInfo.currentCharacter.glassesType);
                    }

                    break;
                }
            case "Male":
                {
                    foreach (Transform h in maleHeads)
                    {
                        h.gameObject.SetActive(maleHeads.IndexOf(h) == CharacterInfo.currentCharacter.headType);
                    }

                    foreach (Transform h in maleBodies)
                    {
                        h.gameObject.SetActive(maleBodies.IndexOf(h) == CharacterInfo.currentCharacter.bodyType);
                    }

                    foreach (Transform h in maleGlasses)
                    {
                        h.gameObject.SetActive(maleGlasses.IndexOf(h) == CharacterInfo.currentCharacter.glassesType);
                    }

                    break;
                }
        }
    }
}
