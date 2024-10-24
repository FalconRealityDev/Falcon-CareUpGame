﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For cases when Person consists of multiple objects.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PersonObjectPart : InteractableObject {

    private PersonObject person = null;

    public Transform Person
    {
        get
        {
            return person.transform;
        }
    }

    protected override void Start()
    {
        base.Start();

        Transform parent = transform.parent;
        while (person == null)
        {
            if (parent != null)
            {
                person = parent.GetComponent<PersonObject>();
                parent = parent.parent;
            } 
            else
            {
                break;
            }
        }
    }

    protected override void Update()
    {
        if (person != null)
        {
            person.CallUpdate(gameObject);
        }
        else
        {
            Debug.Log("No person");
        }
    }
}
