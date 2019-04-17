﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightObject : MonoBehaviour {
    Transform target;
    HighlightControl hl_control;
    HighlightObject.type currentType = HighlightObject.type.NoChange;
    public GameObject content;
    public List<GameObject> BallElements;
    public List<GameObject> QubeElements;
    public List<GameObject> ArrowElements;
    bool gold = false;
    GameUI gameUI;
    public GameObject audioEffect;
    float lifetime = float.PositiveInfinity;
    float startDelay = 0;
    WalkToGroup currentWalkToGroup;
    PlayerScript player;

    protected void Start()
    {
        gameUI = GameObject.FindObjectOfType<GameUI>();
        currentWalkToGroup = ActionManager.NearestWalkToGroup(gameObject);
        player = GameObject.FindObjectOfType<PlayerScript>();
    }



    public enum type
    {
        NoChange,
        Ball,
        Qube,
        Arrow,
        none
    };

    public void setGold(bool value)
    {
        setMaterial("goldHint");
        foreach (GameObject b in BallElements)
            if (b.name == "b1")
                b.SetActive(false);
        foreach (GameObject q in QubeElements)
            if (q.name == "q1")
                q.SetActive(false);
        transform.localScale = 1.2f * transform.localScale;
    }

    public void setMaterial(string matName)
    {
        foreach (GameObject b in BallElements)
            b.GetComponent<MeshRenderer>().material = Resources.Load("Materials/" + matName, typeof(Material)) as Material;
        foreach (GameObject q in QubeElements)
            q.GetComponent<MeshRenderer>().material = Resources.Load("Materials/" + matName, typeof(Material)) as Material;
    }

    public void setType(HighlightObject.type _type)
    {
        currentType = _type;
        foreach (GameObject b in BallElements)
            b.SetActive(currentType == HighlightObject.type.Ball);
        foreach (GameObject q in QubeElements)
            q.SetActive(currentType == HighlightObject.type.Qube);
        foreach (GameObject a in ArrowElements)
            a.SetActive(currentType == HighlightObject.type.Arrow);
        if (_type == HighlightObject.type.Arrow)
        {
            transform.rotation = new Quaternion();
            transform.localScale = new Vector3(1,1,1);
        }
    }


    public void setStartDelay(float value)
    {
        if(value > 0)
        {
            content.SetActive(false);
            startDelay = value;
        }
    }



    public void setTarget(Transform t)
    {
        target = t;

        if (target.GetComponentInChildren<HighlightControl>() != null)
            hl_control = target.GetComponentInChildren<HighlightControl>();
        else
            hl_control = null;

        if (hl_control != null)
        {
            setType(hl_control.hl_type);
            transform.position = hl_control.transform.position;
            if (currentType != HighlightObject.type.Arrow)
            {   
                transform.rotation = hl_control.transform.rotation;
                transform.localScale = hl_control.transform.localScale;
            }
            
        }
        else if (target.GetComponent<Collider>() != null)
        {
            Collider c = target.gameObject.GetComponent<Collider>();
            transform.position = c.bounds.center;
        }
        else
        {
            transform.position = target.position;
        }
        if (currentType == HighlightObject.type.NoChange)
            setType(HighlightObject.type.Ball);
    }

    public void setTimer(float time)
    {
        lifetime = time;
    }

    public void Destroy()
    {
        GameObject.DestroyImmediate(gameObject);
    }

    // Update is called once per frame
    void Update () {
        if (startDelay > 0)
            startDelay -= Time.deltaTime;
        else if (!content.activeSelf)
        {
            if (!gameUI.currentAnimLock || player.currentWalkPosition != currentWalkToGroup)
            {
                startDelay = 2f;
            }
            else
            {
                content.SetActive(transform);
                audioEffect.SetActive(true);
            }
        }

        if (target == null)
            Destroy();
        if (lifetime < 0f)
            GameObject.DestroyImmediate(gameObject);

        if (lifetime < float.PositiveInfinity)
            lifetime -= Time.deltaTime;
        if (target != null){
            if (hl_control != null)
            {
                transform.position = hl_control.transform.position;
                if (currentType != HighlightObject.type.Arrow)
                    transform.rotation = hl_control.transform.rotation;

            }
            else if (target.GetComponent<Collider>() != null)
            {
                Collider c = target.gameObject.GetComponent<Collider>();
                transform.position = c.bounds.center;
            }
            else
            {
                transform.position = target.position;
                if (currentType != HighlightObject.type.Arrow)
                    transform.rotation = target.rotation;
            }
        }
	}
}
