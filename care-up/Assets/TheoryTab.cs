using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CareUp.Localize;

public class TheoryTab : MonoBehaviour
{
    public Text Title;
    public Text Descr;
    Button ContinueButton;

    public void Show(bool value)
    {
        // force scroll to the top 
        Transform rectT = transform.Find("panel/quizElements/answerPanel/Panel/" +
            "w_descriptionPanel/ScrollViewDescription");
        ScrollRect rect = rectT != null ? rectT.GetComponent<ScrollRect>() : null;
        Transform scrollbarT = rectT != null ? rectT.Find("Scrollbar Vertical") : null;
        Scrollbar scrollbar = scrollbarT != null ? scrollbarT.GetComponent<Scrollbar>() : null;

        if (rect != null && scrollbar != null)
        {
            rect.normalizedPosition = new Vector2(0, 1);
            scrollbar.value = 1f;
        }
        else
        {
            Debug.LogError("TheoryTab scrollRect or scrollBar not found!");
        }

        gameObject.SetActive(value);
        PlayerAnimationManager.PauseAnimation(value);

        //autoplay action
        if (value && PlayerPrefsManager.simulatePlayerActions)
            Invoke("AutoContinue", 5f);
    }

    public void ShowTheory(string title_text, string descr_text, string buttonText = "")
    {
        Show(true);
        //@
        Title.text = LocalizationManager.GetValueIfKey(title_text);
        //@
        Descr.text = LocalizationManager.GetValueIfKey(descr_text);

        InGameLocalEditTool inGameLocalEditTool = PlayerPrefsManager.GetDevMode() ? GameObject.FindObjectOfType<InGameLocalEditTool>() : null;  
        if (inGameLocalEditTool != null)
        {
            //!
            inGameLocalEditTool.AddUILocalizationComponentToGO(Title.gameObject, title_text);
            //!
            inGameLocalEditTool.AddUILocalizationComponentToGO(Descr.gameObject, descr_text);

        }
    }

    void AutoContinue()
    {
        ContinueButton = transform.Find("panel/quizElements/Continue").GetComponent<Button>();
        if (ContinueButton != null)
            if (ContinueButton.gameObject.activeSelf)
                ContinueButton.onClick.Invoke();
    }
}
