﻿using UnityEngine;
using UnityEngine.UI;

public class StoreViewModel : MonoBehaviour
{
    private Text currencyText;
    private Text presentNumberText;  

    void Start()
    {
        GameObject.FindObjectOfType<LoadCharacterScene>().LoadCharacter();

        currencyText = GameObject.Find("NumbersStackPanel/CurrencyPanel/Panel/Text").GetComponent<Text>();
        presentNumberText = GameObject.Find("NumbersStackPanel/PresentPanel/Panel/Text").GetComponent<Text>();

        currencyText.text = PlayerPrefsManager.storeManager.Currency.ToString();
        presentNumberText.text = PlayerPrefsManager.storeManager.Presents.ToString();
    }
}
