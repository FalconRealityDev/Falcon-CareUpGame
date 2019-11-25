﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    //[SerializeField]
    //private Sprite tabIdle = default,
    //               tabActive = default,
    //               topTabIdle = default,
    //               topTabActive = default,
    //               buyBtnSprite = default,
    //               putOnBtnSprite = default;
    public List<Button> TabButtons;
    public List<GameObject> TabContainers;
    [SerializeField]
    private GameObject buyBtn = default,
                       confirmPanel = default,
                       purchasedBtn = default,
                       renewBtn = default,
                       onSaleBtn = default;

    private GameObject buyBtnCoin = default,
                       productItem = default;
    bool initialized = false;
    int pages = 3;
    //private Transform tabParent = default,
    //                  itemParent = default;

    [SerializeField]
    private UIParticleSystem currencyParticles = default(UIParticleSystem);

    private Text buyBtnText = default;

    private TabButton selectedTab;
    int selectedTabIndex = 0;

    private List<ProductButton> DressedButtons = new List<ProductButton>();
    private List<ProductButton> SelectedButtons = new List<ProductButton>();

    private ProductButton selectedItemBtn = null;
    private PlayerAvatar mainAvatar;
    CharacterСarousel carousel;
    PlayerPrefsManager pref;
    
    public void SwitchTab(int value)
    {
        foreach (Button b in TabButtons)
        {
            b.interactable = true;
        }
        TabButtons[value].interactable = false;
        foreach (GameObject g in TabContainers)
        {
            g.SetActive(false);
        }
        TabContainers[value].SetActive(true);
    }


    public void ShowConfirmPanel(bool toShow)
    {
        confirmPanel.SetActive(toShow);
    }

    //public void Subscribe(TabButton button)
    //{
    //    if (tabs == null)
    //        tabs = new List<TabButton>();

    //    tabs.Add(button);
    //}

    //public void OnTabEnter(TabButton button)
    //{
    //    //ResetTabs();

    //    if (button != selectedTab)
    //    {
    //        button.GetComponent<CanvasGroup>().alpha = 0.8f;
    //        button.background.rectTransform.localScale = new Vector3(1.01f, 1.01f, 1);
    //    }
    //}

    //public void OnTabExit(TabButton button) { }

    //public void OnTabSelected(TabButton button)
    //{
    //    selectedTab = button;

    //    ResetTabs();

    //    if (button == tabs[0])
    //    {
    //        ModifyTab(button, topTabActive, new Vector3(1.15f, 1.15f, 1f));
    //    }
    //    else
    //    {
    //        ModifyTab(button, tabActive, new Vector3(1.15f, 1.15f, 1f));
    //    }

    //    int index = button.transform.GetSiblingIndex();
    //    selectedTabIndex = index;
    //    for (int i = 0; i < pages.Count; i++)
    //    {
    //        if (i == index)
    //            pages[i].SetActive(true);
    //        else
    //            pages[i].SetActive(false);
    //    }
    //}

    //public void ResetTabs()
    //{
    //    foreach (TabButton button in tabs)
    //    {
    //        if (button != null)
    //        {
    //            button.GetComponent<CanvasGroup>().alpha = 1f;

    //            if (button == selectedTab)
    //                continue;

    //            if (button == tabs[0])
    //            {
    //                ModifyTab(button, topTabIdle, new Vector3(1, 1, 1));
    //            }
    //            else
    //            {
    //                ModifyTab(button, tabIdle, new Vector3(1, 1, 1));
    //            }
    //        }
    //    }
    //}

    void Dress()
    {
        if (selectedItemBtn != null)
        {
            if (selectedItemBtn.item.purchased)
            {
                if (DressedButtons[selectedTabIndex] != null)
                    DressedButtons[selectedTabIndex].SetDressOn(false);
                DressedButtons[selectedTabIndex] = selectedItemBtn;
                DressedButtons[selectedTabIndex].SetDressOn(true);
            }
        }
    }

    public void SelectItem(ProductButton btn)
    {
        if (selectedItemBtn != null)
        {
            selectedItemBtn.Select(false);
        }
        if (btn != null)
        {
            selectedItemBtn = btn;
            Dress();
            selectedItemBtn.Select(true);
            buyBtnCoin.SetActive(true);

            if (btn.item.category == "Hat")
            {
                mainAvatar.LoadNewHat(btn.item.name);
            }
            else if (btn.item.category == "Glasses")
            {
                mainAvatar.LoadNewGlasses(btn.item.index);
            }
            else if (btn.item.category == "Body")
            {
                if (btn.item.purchased)
                    CharacterInfo.bodyType = btn.item.index;
                mainAvatar.avatarData.bodyType = btn.item.index;
                mainAvatar.UpdateCharacter();
            }
            if (btn.item.purchased)
            {
                CharacterInfo.UpdateCharacter(btn.item);
                carousel.UpdateSelected(mainAvatar.avatarData);
            }
        }

        UpdatePurchesBtn();
    }

    public void UpdatePurchesBtn()
    {
        if (selectedItemBtn != null)
        {
            if (!selectedItemBtn.item.purchased)
            {
                buyBtnText.text = selectedItemBtn.item.price.ToString();
                buyBtnCoin.SetActive(true);
                buyBtn.SetActive(true);
                // buyBtn.GetComponent<Image>().sprite = buyBtnSprite;
            }
            else
            {
                buyBtn.SetActive(false);
            }
        }
        else
        {
            buyBtn.SetActive(false);
        }
    }

    public void ResetBuyBtn()
    {
        buyBtn.SetActive(false);
    }

    public void PurchesBtnClicked()
    {
        if (selectedItemBtn != null)
        {
            if (!selectedItemBtn.item.purchased)
                ShowConfirmPanel(true);
        }
        UpdatePurchesBtn();
    }

    public void PurchaseSelectedItem()
    {
        StoreItem item = selectedItemBtn.item;
        CharacterInfo.UpdateCharacter(item);
        GameObject.FindObjectOfType<LoadCharacterScene>().LoadCharacter();

        if (!item.purchased)
        {
            if (PlayerPrefsManager.storeManager.Purchase(item.index))
            {
                mainAvatar.SetAnimationAction(CareUpAvatar.Actions.Dance, true);

                selectedItemBtn.SetPurchased(true);
                GetComponent<StoreViewModel>().UpdateCurrancyPanel();
                //GameObject.Find("TitlePanel/TitlePanel/Panel/CurrencyPanel/ValuePanel/Text").GetComponent<Text>().text
                //    = PlayerPrefsManager.storeManager.Currency.ToString();
                if (item.price > 0)
                {
                    GameObject.Find("cashRegisterEffect").GetComponent<AudioSource>().Play();
                    currencyParticles.Play();
                }
                else
                {
                    GameObject.Find("swoopEffect").GetComponent<AudioSource>().Play();
                }
                if (selectedTabIndex == 2)
                {
                    CharacterInfo.bodyType = selectedItemBtn.item.index;
                    mainAvatar.avatarData.bodyType = selectedItemBtn.item.index;
                    mainAvatar.UpdateCharacter();
                }
                carousel.UpdateSelected(mainAvatar.avatarData);
            }
            else
            {
                GameObject.FindObjectOfType<UMP_Manager>().ShowDialog(8);
            }
        }
        ShowConfirmPanel(false);
        UpdatePurchesBtn();
        Dress();
    }

    public void InitializeTabPanel()
    {
        if (initialized)
            return;

        SwitchTab(0);
        carousel = GameObject.FindObjectOfType<CharacterСarousel>();
        pref = GameObject.FindObjectOfType<PlayerPrefsManager>();
        InitializeElements();
        buyBtnCoin.SetActive(false);

        foreach (StoreCategory category in PlayerPrefsManager.storeManager.StoreItems)
        {
            InitializePrefabs(category);
            DressedButtons.Add(null);
        }

        //for (int i = 1; i < pagesContainer.transform.childCount; i++)
        //{
        //    pages.Add(pagesContainer.transform.GetChild(i).gameObject);
        //}

        //OnTabSelected(tabs[0]);
        // DisplayItemsInStore();
        UpdatePurchesBtn();
        initialized = true;
    }

    public void DisplayItemsInStore()
    {
        InitializeTabPanel();
        CharacterItem currentCharacter = null;
        if (pref != null)
        {
            if (PlayerPrefsManager.storeManager.CharacterItems.Count >= pref.CarouselPosition)
                currentCharacter = PlayerPrefsManager.storeManager.CharacterItems[pref.CarouselPosition];
        }
        for (int i = 0; i < pages; i++)
        {
            //"Protocols/content"
            Transform itemParent = TabContainers[i].transform.Find("Protocols/content");

            foreach (Transform child in itemParent)
            {
                GameObject.Destroy(child.gameObject);
            }

            StoreItem baseItem = null;
            if (currentCharacter != null)
            {
                //Hats------------------------
                if (i == 0)
                {
                    StoreItem xItem = new StoreItem(0, 0, "x", "Hat", true);
                    ProductButton xBtn = InstantiateProduct(xItem, i);
                    if (currentCharacter.playerAvatar.hat == "")
                    {
                        DressedButtons[0] = xBtn;
                        xBtn.SetDressOn(true);
                    }
                    if (currentCharacter.defaultAvatarData.hat != "")
                    {
                        baseItem = new StoreItem(0, 0, currentCharacter.defaultAvatarData.hat, "Hat", true);
                        ProductButton baseHatBtn = InstantiateProduct(baseItem, i);
                        if (currentCharacter.playerAvatar.hat == currentCharacter.defaultAvatarData.hat)
                        {
                            DressedButtons[0] = baseHatBtn;
                            baseHatBtn.SetDressOn(true);
                        }
                    }
                }
                //Glasses------------------------
                else if (i == 1)
                {
                    StoreItem xxItem = new StoreItem(-500, 0, "x", "Glasses", true);
                    ProductButton xxBtn = InstantiateProduct(xxItem, i);
                    if (mainAvatar.avatarData.glassesType == -1)
                    {
                        DressedButtons[1] = xxBtn;
                        xxBtn.SetDressOn(true);
                    }
                    if (currentCharacter.defaultAvatarData.glassesType != -1)
                    {
                        int gl = currentCharacter.defaultAvatarData.glassesType;
                        baseItem = new StoreItem(gl, 0, "gl_" + gl.ToString(), "Glasses", true);
                        ProductButton baseGlassesBtn = InstantiateProduct(baseItem, i);
                        if (currentCharacter.playerAvatar.glassesType == currentCharacter.defaultAvatarData.glassesType)
                        {
                            DressedButtons[1] = baseGlassesBtn;
                            baseGlassesBtn.SetDressOn(true);
                        }
                    }
                }
                //Bodies------------------------
                else if (i == 2)
                {
                    int _body = currentCharacter.defaultAvatarData.bodyType;
                    baseItem = new StoreItem(_body, 0, "body_" + _body.ToString(), "Body", true);

                    ProductButton baseBodyBtn = InstantiateProduct(baseItem, i);

                    if (currentCharacter.playerAvatar.bodyType == currentCharacter.defaultAvatarData.bodyType)
                    {
                        DressedButtons[2] = baseBodyBtn;
                        baseBodyBtn.SetDressOn(true);
                    }
                }
            }

            if (mainAvatar == null)
                mainAvatar = GameObject.Find("MainPlayerAvatar").GetComponent<PlayerAvatar>();
            if (pref == null)
                pref = GameObject.FindObjectOfType<PlayerPrefsManager>();
            
            int avIndex = mainAvatar.avatarData.GetHatOffsetIndex();

            foreach (StoreItem item in PlayerPrefsManager.storeManager.StoreItems[i].items)
            {
                if (i == 0)
                {
                    HatsPositioningDB.HatInfo info = pref.hatsPositioning.GetHatInfo(avIndex, item.name);
                    if (info != null)
                        if (info.excluded)
                            continue;
                }
                if (baseItem != null)
                {
                    if (baseItem.name == item.name)
                        continue;
                }

                ProductButton btn = null;

                if (item.category == "Body")
                {
                    if (currentCharacter.defaultAvatarData.gender == CareUpAvatar.Gender.Female)
                    {
                        if (item.index >= 100000)
                        {
                            btn = InstantiateProduct(item, i);
                        }
                    }
                    else if (item.index < 100000)
                    {
                        btn = InstantiateProduct(item, i);
                    }
                }
                else
                {
                    btn = InstantiateProduct(item, i);
                }

                if (i == 0 && currentCharacter.playerAvatar.hat == item.name)
                {
                    DressedButtons[0] = btn;
                    btn.SetDressOn(true);
                }
                else if (i == 1 && currentCharacter.playerAvatar.glassesType == item.index)
                {
                    DressedButtons[1] = btn;
                    btn.SetDressOn(true);
                }
                else if (i == 2 && currentCharacter.playerAvatar.bodyType == item.index)
                {
                    DressedButtons[2] = btn;
                    btn.SetDressOn(true);
                }
            }

        }
    }

    private void Start()
    {
   

        InitializeTabPanel();
    }

    private void InitializePrefabs(StoreCategory storeCategory)
    {
        // setting tab button
        //tab = Instantiate(tabBtnPrefab, tabParent);
        //tab.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/{storeCategory.icon}");

        //page = Instantiate(tabPagePrefab, pagesContainer.transform);
        //itemParent = page.transform.Find("StoreTabPage/content");
    }

    private ProductButton InstantiateProduct(StoreItem item, int TabNum)
    {
        Transform tab = TabContainers[TabNum].transform.Find("Protocols/content");
        GameObject i = Instantiate(productItem, tab);
        ProductButton btn = i.GetComponent<ProductButton>();
        btn.Initialize(item, this);
        return btn;
    }

    private void ModifyTab(TabButton button, Sprite sprite, Vector3 vector3)
    {
        button.background.rectTransform.localScale = vector3;
        button.background.sprite = sprite;
    }

    private void InitializeElements()
    {
        mainAvatar = GameObject.Find("MainPlayerAvatar").GetComponent<PlayerAvatar>();
        buyBtnText = buyBtn.transform.Find("Text").GetComponent<Text>();
        buyBtnCoin = buyBtn.transform.Find("Coin").gameObject;
        //pagesContainer = GameObject.Find("PageContainer");

        //tabBtnPrefab = Resources.Load<GameObject>("NecessaryPrefabs/StoreTab");
        //tabPagePrefab = Resources.Load<GameObject>("NecessaryPrefabs/PageHolder");
        productItem = Resources.Load<GameObject>("NecessaryPrefabs/UI/ProductPanelBase");
        //tabParent = GameObject.Find("StoreTabContainer").transform;
    }
}