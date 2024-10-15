using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILocalEditButton : MonoBehaviour
{
    private GameObject hoverImage;
    public string key = "";
    private UILocalization uiLocalization;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetActive(bool _active)
    {
        gameObject.SetActive(_active);
        if (!_active && hoverImage != null)
            hoverImage.SetActive(false);
    }

    public void SetHoverImage(GameObject hoverImageObj)
    {
        hoverImage = hoverImageObj;
    }

    public void SetUILocalization(UILocalization uiLocalValue)
    {
        uiLocalization = uiLocalValue;
    }

    public void PointerEntered()
    {
        if (hoverImage != null)
            hoverImage.SetActive(true);
    }

    public void PointerExited() 
    { 
        if (hoverImage != null)
            hoverImage.SetActive(false);
    }

    public void OnButtonClicked()
    {
        if (uiLocalization != null)
            uiLocalization.InitiateLocalEdit(key);
    }
    
}
