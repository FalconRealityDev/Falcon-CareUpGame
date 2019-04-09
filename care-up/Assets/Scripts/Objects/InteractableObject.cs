﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// General abstract class for objects.
/// </summary>
public class InteractableObject : MonoBehaviour {
	public int ObjectID = 0;
    //HandsInventory handsInventory;

    public string description;
    public bool muplipleMesh = false;
    public Vector3 descriptionOffset;

    protected Renderer rend;
    protected Shader onMouseOverShader;

    static private Shader onMouseOverShaderSihlouette;
    static private Shader onMouseOverShadeOutline;
    static protected Shader onMouseExitShader;
    static protected Shader ghostShader;

    protected bool positionSaved = false;
    protected Vector3 savedPosition;
    protected Quaternion savedRotation;

    static protected CameraMode cameraMode;
    static protected Controls controls;
    static protected ActionManager actionManager;
    static protected GameObject itemDescription;
    static protected PlayerScript player;
    static protected HandsInventory inventory;

    public bool transparencyFix = false;

    public Vector3 SavedPosition
    {
        get { return savedPosition; }
    }

    public Quaternion SavedRotation
    {
        get { return savedRotation; }
    }

    protected virtual void Start()
    {
        //handsInventory = GameObject.FindObjectOfType<HandsInventory>();
        rend = GetComponent<Renderer>();

        if (onMouseOverShaderSihlouette == null)
        {
            onMouseOverShaderSihlouette = Shader.Find("Outlined/Diffuse");
        }

        if (onMouseOverShadeOutline == null)
        {
            onMouseOverShadeOutline = Shader.Find("Outlined/Diffuse");
        }

        onMouseOverShader = (muplipleMesh) ? onMouseOverShadeOutline : onMouseOverShaderSihlouette;

        if (onMouseExitShader == null)
        {
            onMouseExitShader = Shader.Find("Standard");
        }

        if (ghostShader == null)
        {
            ghostShader = Shader.Find("Unlit/GhostZ");
        }

        if (cameraMode == null)
        {
            cameraMode = GameObject.Find("GameLogic").GetComponent<CameraMode>();
            if (cameraMode == null) Debug.LogError("No Camera Mode found.");
        }

        if (controls == null)
        {
            controls = GameObject.Find("GameLogic").GetComponent<Controls>();
            if (controls == null) Debug.LogError("No Controls found");
        }

        if (actionManager == null)
        {
            actionManager = GameObject.Find("GameLogic").GetComponent<ActionManager>();
            if (actionManager == null) Debug.LogError("No action manager found");
        }

        if (player == null)
        {
            player = GameObject.Find("Player").GetComponent<PlayerScript>();
            if (player == null) Debug.LogError("No player");
        }

        if (inventory == null)
        {
            inventory = GameObject.Find("GameLogic").GetComponent<HandsInventory>();
            if (inventory == null) Debug.LogError("No inventory found");
        }

        if (itemDescription == null)
        {
            //itemDescription = Instantiate(Resources.Load<GameObject>("Prefabs/ItemDescription"),
            //    Vector3.zero, Quaternion.identity) as GameObject;
            itemDescription = GameObject.Find("ItemDescription");

            if (itemDescription == null)
            {
                Debug.LogError("No item description prefab found");
            }
            else
            {
                itemDescription.name = "ItemDescription";
                itemDescription.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Handle changing shader ( highlighting on aiming ) 
    /// and showing Description at bottom of the screen.
    /// </summary>
    protected virtual void Update()
    {
        if (cameraMode.CurrentMode == CameraMode.Mode.Free && !player.away && !player.robotUIopened)
        {
            bool selectedIsInteractable = (controls.SelectedObject != null && controls.CanInteract &&
                controls.SelectedObject.GetComponent<InteractableObject>() != null);

            PickableObject pickableObject = null;
            if (controls.SelectedObject != null)
            {
                pickableObject = controls.SelectedObject.GetComponent<PickableObject>();
            }

            bool notSihlouette = (pickableObject == null || (pickableObject != null && pickableObject.sihlouette == false));
            selectedIsInteractable &= notSihlouette;

            if ((controls.SelectedObject == gameObject && !cameraMode.animating) && notSihlouette)
            {
                if (controls.CanInteract)
                {
                    if (rend.material.shader == onMouseExitShader)
                    {
                        SetShaderTo(onMouseOverShader);
                        inventory.AddHighlight(transform, "hl");
                    }

                    if (!itemDescription.activeSelf)
                    {
                        itemDescription.SetActive(true);
                    }

                    itemDescription.GetComponentInChildren<Text>().text = (description == "") ? name : description;
                }
                else if (!controls.CanInteract && rend.material.shader == onMouseOverShader)
                {
                    SetShaderTo(onMouseExitShader);
                    inventory.RemoveHighlight("hl", transform.name);

                    itemDescription.SetActive(false);
                }
            }
            else
            {
                if (rend.material.shader == onMouseOverShader)
                {
                    inventory.RemoveHighlight("hl", transform.name);

                    SetShaderTo(onMouseExitShader);
                }

                if (!selectedIsInteractable)
                {
                    itemDescription.SetActive(false);
                }
            }
        }

        if (itemDescription.activeSelf && !player.itemControls.gameObject.activeSelf)
        {
            itemDescription.transform.GetChild(0).transform.position = Input.mousePosition + new Vector3(50.0f, 25.0f);
        }
    }

    public virtual void Reset()
    {
        if (rend)
        {
            SetShaderTo(onMouseExitShader);
            inventory.RemoveHighlight("hl", transform.name);
            itemDescription.SetActive(false);
        }
    }

    public static void ResetDescription()
    {
        itemDescription.SetActive(false);
    }

    protected bool ViewModeActive()
    {
        return cameraMode.CurrentMode == CameraMode.Mode.ObjectPreview;
    }

    public void SavePosition()
    {
        if (!positionSaved)
        {
            positionSaved = true;

            savedPosition = transform.position;
            savedRotation = transform.rotation;
        }
    }

    public void SavePosition(Vector3 pos, Quaternion rot, bool force = false)
    {
        if (!positionSaved || force)
        {
            positionSaved = true;

            savedPosition = pos;
            savedRotation = rot;
        }
    }

    public virtual void LoadPosition()
    {
        transform.position = savedPosition;
        transform.rotation = savedRotation;
    }

    public void BaseLoadPosition()
    {
        transform.position = savedPosition;
        transform.rotation = savedRotation;
    }

    public void GetSavesLocation(out Vector3 outPosition, out Quaternion outRotation)
    {
        outPosition = savedPosition;
        outRotation = savedRotation;
    }


    protected virtual void SetShaderTo(Shader shader)
    {
        foreach (Material m in rend.materials)
        {
            m.shader = shader;
        }
        
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (r.name != "ParticleHint" )
            {
                foreach (Material m in r.materials)
                {
                    m.shader = shader;
                }
            }
        }

        if (transparencyFix || true)
        {
            foreach (Material m in rend.materials)
            {
                m.renderQueue = 3000;
            }

            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                if (r.name != "ParticleHint")
                {
                    foreach (Material m in r.materials)
                    {
                        m.renderQueue = 3000;
                    }
                }
            }
        }
    }

    public void SetGhostShader()
    {
        rend = GetComponent<Renderer>();
        SetShaderTo(ghostShader);
    }

    public virtual void Highlight(bool value)
    {
        if (rend == null)
        {
            rend = GetComponent<Renderer>();
        }

        if (value)
        {
            if (rend.material.shader == onMouseExitShader)
                SetShaderTo(onMouseOverShader);
        }
        else
        {
            if (rend.material.shader == onMouseOverShader)
                SetShaderTo(onMouseExitShader);
                inventory.RemoveHighlight("hl", transform.name);
        }
    }
}
