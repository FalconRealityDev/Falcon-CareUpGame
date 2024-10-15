using UnityEngine;
using UnityEngine.UI;
using CareUp.Localize;

/// <summary>
/// General class for objects.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    public string PrefabName = "";
    public int ObjectID = 0;
    public string description;
    public string nameArticle;
    public bool muplipleMesh = false;
    public Vector3 descriptionOffset;
    public AssetSource assetSource;
    public bool transparencyFix = false;

    protected bool positionSaved = false;
    protected Vector3 savedPosition;
    protected Quaternion savedRotation;
    protected Vector3 savedScale;
    protected Renderer rend;

    static protected CameraMode cameraMode;
    static protected Controls controls;
    static protected ActionManager actionManager;
    static protected GameObject itemDescription;
    static protected PlayerScript player;
    static protected HandsInventory inventory;
    static protected Shader ghostShader;

    private static Transform descriptionPanelPosition;
    private static bool selectedIsInteractable;
    private static Text descriptionText;



    //private bool hasHighlight = false;
    private Vector3 descriptionPanelOffset = new Vector3(50.0f, 25.0f);
    private GameUI gameUI;

    public enum AssetSource
    {
        None,
        Included,
        Resources,
        Bundle
    };

    public Vector3 SavedPosition
    {
        get { return savedPosition; }
    }

    public Quaternion SavedRotation
    {
        get { return savedRotation; }
    }

    public Vector3 SavedScale
    {
        get { return savedScale; }
    }

    protected virtual void Start()
    {
        savedScale = transform.lossyScale;
        gameUI = GameObject.FindObjectOfType<GameUI>();
        rend = GetComponent<Renderer>();

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
            itemDescription = GameObject.Find("ItemDescription");

            if (itemDescription == null)
            {
                Debug.LogError("No item description prefab found");
            }
            else
            {
                descriptionPanelPosition = itemDescription.transform.GetChild(0).transform;
                descriptionText = itemDescription.GetComponentInChildren<Text>();
                itemDescription.name = "ItemDescription";
                if (PlayerPrefsManager.GetDevMode() && !Input.GetKey(KeyCode.LeftControl))
                    itemDescription.SetActive(false);
            }
        }
    }

    public void SetDescription()
    {
        if (PlayerPrefsManager.GetDevMode() && Input.GetKey(KeyCode.LeftControl))
            return;
        if (cameraMode.CurrentMode == CameraMode.Mode.Free)
        {
            if (!player.away && !player.robotUIopened && !cameraMode.animating)
            {

                if (LocalizationManager.GetValueIfKey(description) == "Werkveld")
                {
                    if (!actionManager.CompareUseObject("WorkField"))
                    {
                        return;
                    }
                }

                PickableObject pickableObject = null;

                if (controls.SelectedObject != null)
                {
                    pickableObject = controls.SelectedObject.GetComponent<PickableObject>();
                }

                bool notSihlouette = pickableObject == null || (pickableObject != null && pickableObject.sihlouette == false);

                if (!string.IsNullOrEmpty(description) && notSihlouette)
                {
                    itemDescription.SetActive(true);
                    //@
                    descriptionText.text = (description == "") ? name : LocalizationManager.GetValueIfKey(description);
                
                    InGameLocalEditTool inGameLocalEditTool = PlayerPrefsManager.GetDevMode() ? GameObject.FindObjectOfType<InGameLocalEditTool>() : null;  

                    if (inGameLocalEditTool != null)
                    {
                        //!
                        inGameLocalEditTool.AddUILocalizationComponentToGO(
                            descriptionText.gameObject, description);

                    }
                }
            }
        }
    }

    protected virtual void Update()
    {

    }

    public virtual void Reset()
    {
        if (rend)
        {
            gameUI.RemoveHighlight("hl", transform.name);
            //hasHighlight = false;
            if (PlayerPrefsManager.GetDevMode() && !Input.GetKey(KeyCode.LeftControl))
                itemDescription.SetActive(false);
        }
    }

    public static void ResetDescription()
    {
        if (PlayerPrefsManager.GetDevMode() && !Input.GetKey(KeyCode.LeftControl))
            itemDescription.SetActive(false);
    }

    protected bool ViewModeActive()
    {
        return cameraMode.CurrentMode == CameraMode.Mode.ObjectPreview;
    }

    public void SavePosition()
    {
        savedPosition = transform.position;
        savedRotation = transform.rotation;
        savedScale = transform.lossyScale;
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

    public virtual void LoadPosition(Transform overridePos = null, bool forceParent = false)
    {
        transform.position = overridePos == null ? savedPosition : overridePos.position;
        transform.rotation = overridePos == null ? savedRotation : overridePos.rotation;
        Transform p = transform.parent;
        transform.parent = null;
        transform.localScale = savedScale;
        //for some reason, if you forcePick it doesnt remember it's proper parent
        //to avoid item staying in hand after force drop - parent is set to null
        //(only supposed to affect forcedDrops with override position)
        transform.parent = forceParent ? overridePos : (overridePos == null ? p : null);
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

    protected virtual void SetShaderTo(Shader shader, bool toHide = false)
    {
        foreach (Material m in rend.materials)
        {
            m.shader = shader;
        }

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (r.name != "ParticleHint")
            {
                foreach (Material m in r.materials)
                {
                    m.shader = shader;
                }
            }
            if (toHide)
            {
                r.enabled = false;
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

    public void SetGhostShader(bool toHide = false)
    {
        rend = GetComponent<Renderer>();
        SetShaderTo(ghostShader, toHide);
    }

    public virtual void Highlight(bool value)
    {
        if (rend == null)
        {
            rend = GetComponent<Renderer>();
        }

        if (value == false)
        {
            gameUI.RemoveHighlight("hl", transform.name);
            //hasHighlight = false;
        }
    }

    void OnMouseExit() => ResetDescription();

    void OnMouseEnter() => SetDescription();
}
