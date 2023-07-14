using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CareUp.Actions;

public class GameUIVR : MonoBehaviour
{
    private ActionManager actionManager;
    public List<string> activeHighlighted = new List<string>();

    public enum ItemControlButtonType
    {
        None,
        Combine,
        DecombineLeft,
        DecombineRight,
        NoTargetLeft,
        NoTargetRight,
        ZoomLeft,
        ZoomRight,
        DropLeft,
        DropRight,
        MoveLeft,
        MoveRight,
        Records,
        Prescription,
        Ipad,
        General,
        PaperAndPen,
        GeneralBack,
        RecordsBack,
        PrescriptionBack,
        MessageTabBack,
        Close,
        TalkBubble,
    }

    public enum MoveControlButtonType
    {
        None,
        MoveA,
        MoveB,
        MoveC,
        MoveD
    }

    public ItemControlButtonType buttonToBlink;
    public MoveControlButtonType moveButtonToBlink;
    public bool prescriptionButtonBlink;
    public bool recordsButtonBlink;
    public bool paperAndPenButtonblink;
    public bool dropLeftBlink = false;
    public bool dropRightBlink = false;
    public List<string> reqPlaces = new List<string>();
    private PlayerScript player;
    private HandsInventory handsInventory;
    public void UpdateButtonsBlink()
    {
        //+++++++++++++++++++++++++++++++++++++++++
    }


    public void UpdateHintPanel(List<ActionManager.StepData> subTasks, float UpdateHintDelay = 0f)
    {
        //+++++++++++++++++++++++++++++++++++++++++
    }

    // Start is called before the first frame update
    void Start()
    {
        handsInventory = GameObject.FindObjectOfType<HandsInventory>();
        actionManager = GameObject.FindObjectOfType<ActionManager>();
        player = GameObject.FindObjectOfType<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void RemoveHighlight(string prefix, string _name)
    {
        string hl_name = prefix + "_" + _name;

        GameObject hlObject = GameObject.Find(hl_name);
        if (hlObject != null)
        {
            if (hlObject.GetComponent<HighlightObject>())
                GameObject.Destroy(hlObject.gameObject);
            // hlObject.GetComponent<HighlightObject>().Destroy();
        }
    }


    public HighlightObject AddHighlight(Transform target, string prefix, HighlightObject.type hl_type = HighlightObject.type.NoChange, float startDelay = 0, float LifeTime = float.PositiveInfinity)
    {
        string hl_name = prefix + "_" + target.name;
        if (GameObject.Find(hl_name) != null)
            return null;

        // assets/resources/necessaryprefabs

        GameObject hl_obj = Instantiate(Resources.Load<GameObject>("NecessaryPrefabs/HighlightObject"), target.position, new Quaternion()) as GameObject;

        HighlightObject hl = hl_obj.GetComponent<HighlightObject>();
        hl.name = hl_name;
        hl.setTarget(target);
        if (hl_type != HighlightObject.type.NoChange)
            hl.setType(hl_type);
        hl.setTimer(LifeTime);
        if (startDelay > 0)
            hl.setStartDelay(startDelay);
        return hl_obj.GetComponent<HighlightObject>();
    }

    public void UpdateHelpHighlight()
    {

        //PointToObject = null;
        bool practiceMode = true;

        //if (prefs != null)
        //    practiceMode = prefs.practiceMode;
        if (!practiceMode)
            return;

        List<string> newHLObjects = new List<string>();

        string prefix = "helpHL";

        if (handsInventory == null)
            handsInventory = GameObject.FindObjectOfType<HandsInventory>();

        ObjectDataHolder leftHandObjectData = handsInventory.LeftHandObjectData();
        if (leftHandObjectData != null)
            RemoveHighlight(prefix, leftHandObjectData.name);
        ObjectDataHolder rightHandObjectData = handsInventory.RightHandObjectData();
        if (rightHandObjectData != null)
            RemoveHighlight(prefix, rightHandObjectData.name);

        foreach (Action a in actionManager.IncompletedActions)
        {
            string[] ObjectNames = new string[0];
            if (!player.IsInCopyAnimationState())
                a.ObjectNames(out ObjectNames);

            foreach (string objectToUse in ObjectNames)
            {
                GameObject currentObjectToUse = null;
                ObjectDataHolder currentObjectDataHolder = FindObjectDataByPrefabName(objectToUse);
                if (currentObjectDataHolder != null)
                    currentObjectToUse = currentObjectDataHolder.gameObject;

                if (currentObjectToUse != null)
                {
                    HighlightObject.type hl_type = HighlightObject.type.NoChange;
                    HighlightObject h = AddHighlight(currentObjectToUse.transform, prefix, hl_type, 2f + Random.Range(0f, 0.5f));
                    if (h != null)
                    {
                        h.setGold(true);
                    }
                    newHLObjects.Add(currentObjectToUse.name);
                    //    if (PointToObject == null)
                    //        PointToObject = GameObject.Find(objectToUse);
                    //    if (PointToObject != null)
                    //    {
                    //        if (PointToObject.GetComponent<PersonObject>() != null)
                    //            PointToObject = null;
                    //    }
                    //}
                    //else
                    //{
                    //    GameObject usableHL = null;

                    //    foreach (UsableObject u in GameObject.FindObjectsOfType<UsableObject>())
                    //    {
                    //        if (u.PrefabToAppear == objectToUse && u.PrefabToAppear != "")
                    //        {
                    //            usableHL = u.gameObject;
                    //            break;
                    //        }
                    //    }
                    //    if (usableHL == null)
                    //    {
                    //        foreach (PickableObject p in GameObject.FindObjectsOfType<PickableObject>())
                    //        {
                    //            if (p.prefabInHands == objectToUse && p.prefabInHands != "")
                    //            {
                    //                if (AlloweAutoAction && !autoObjectSelected)
                    //                {
                    //                    AutoActionObject = p.gameObject;
                    //                    autoObjectSelected = true;
                    //                    Invoke("AutoPlay", 4f);
                    //                }
                    //                HighlightObject h = AddHighlight(p.transform, prefix, HighlightObject.type.NoChange, 2f + Random.Range(0f, 0.5f));
                    //                if (h != null)
                    //                {
                    //                    h.setGold(true);
                    //                }
                    //                newHLObjects.Add(p.name);
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (AlloweAutoAction && !autoObjectSelected)
                    //        {
                    //            AutoActionObject = usableHL;
                    //            autoObjectSelected = true;
                    //            Invoke("AutoPlay", 4f);
                    //        }
                    //        HighlightObject h = AddHighlight(usableHL.transform, prefix, HighlightObject.type.NoChange, 2f + Random.Range(0f, 0.5f));
                    //        if (h != null)
                    //        {
                    //            h.setGold(true);
                    //        }
                    //        newHLObjects.Add(usableHL.name);
                    //    }
                }
            }
        }
     
        ////clear highlights
        for (int i = 0; i < activeHighlighted.Count; i++)
        {
            if (!newHLObjects.Contains(activeHighlighted[i]))
            {
                RemoveHighlight(prefix, activeHighlighted[i]);
            }
        }
        activeHighlighted.Clear();
        foreach (string s in newHLObjects)
        {
            activeHighlighted.Add(s);
        }
    }


    public static ObjectDataHolder FindObjectDataByPrefabName(string prefName)
    {
        foreach(ObjectDataHolder o in GameObject.FindObjectsOfType<ObjectDataHolder>())
        {
            if (o.objectPrefabNames.Contains(prefName))
                return o;
        }
        return null;
    }
}
