﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class StoreItem
{
    public int index;
    public int price;
    public string name;
    public string category;
    public bool purchased;

    public StoreItem() { index = -1; price = 0; }
    public StoreItem(int i, int p, string n, string c, bool s)
        { index = i; price = p; name = n; category = c; purchased = s; }
}

public class StoreCategory
{
    public List<StoreItem> items;
    public string name;
    public string icon;

    public StoreCategory() { items = new List<StoreItem>(); name = icon = ""; }
    public StoreCategory(List<StoreItem> list, string n, string i)
        { items = new List<StoreItem>(list); name = n; icon = i; }
}

public class StoreManager 
{
    private int currentCurrency = 0;
    private int currentPresents = 0;
    private List<StoreCategory> storeItems = new List<StoreCategory>();

    public List<StoreCategory> StoreItems { get { return storeItems; } }

    public int Currency { get { return currentCurrency; } }
    public int Presents { get { return currentPresents; } }

    public void Init(string storeXml = "Store")
    {
        // load up all items from xml into the list
        TextAsset textAsset = (TextAsset)Resources.Load("Xml/" + storeXml);

        XmlDocument xmlFile = new XmlDocument();
        xmlFile.LoadXml(textAsset.text);
        XmlNodeList xmlCatList = xmlFile.FirstChild.NextSibling.ChildNodes;

        foreach (XmlNode xmlCatNode in xmlCatList)
        {
            List<StoreItem> catItems = new List<StoreItem>();
            foreach (XmlNode xmlSceneNode in xmlCatNode.ChildNodes)
            {
                int index = -1, price = 1;
                int.TryParse(xmlSceneNode.Attributes["index"].Value, out index);
                int.TryParse(xmlSceneNode.Attributes["price"].Value, out price);
                bool purchased = DatabaseManager.FetchField("Store", index.ToString()) == "true";

                string name = xmlSceneNode.Attributes["name"].Value;
                string category = xmlSceneNode.Attributes["category"].Value;

                catItems.Add(new StoreItem(index, price, name, category, purchased));
            }

            string catName = (xmlCatNode.Attributes["name"] != null) ? xmlCatNode.Attributes["name"].Value : "";
            string catIcon = (xmlCatNode.Attributes["icon"] != null) ? xmlCatNode.Attributes["icon"].Value : "";
            storeItems.Add(new StoreCategory(catItems, catName, catIcon));
        }

        // get amount of currency/presents saved
        int.TryParse(DatabaseManager.FetchField("Store", "Currency"), out currentCurrency);
        int.TryParse(DatabaseManager.FetchField("Store", "Presents"), out currentPresents);
    }

    public void ModifyCurrencyBy(int amount)
    {
        currentCurrency += amount;
        DatabaseManager.UpdateField("Store", "Currency", currentCurrency.ToString());
    }

    public void ModifyPresentsBy(int amount)
    {
        currentPresents += amount;
        DatabaseManager.UpdateField("Store", "Presents", currentPresents.ToString());
    }

    public StoreItem FindItemByIndex(int index)
    {
        StoreItem result = new StoreItem();

        foreach (StoreCategory cat in storeItems)
        {
            result = cat.items.Find(x => x.index == index);
            if (result != null && result.index != -1) break;
        }

        return result;
    }

    public bool Purchase(int itemIndex)
    {
        StoreItem item = FindItemByIndex(itemIndex);
        if (item.index != -1 && currentCurrency >= item.price)
        {
            ModifyCurrencyBy(-item.price);
            item.purchased = true;
            DatabaseManager.UpdateField("Store", itemIndex.ToString(), "true");
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool GetPurchasedState(int itemIndex)
    {
        StoreItem item = FindItemByIndex(itemIndex);
        return (item.index != -1) ? item.purchased : false;
    }
    
    public List<StoreItem> GetStoreItemsByCategoryName(string categoryName)
    {
        StoreCategory category = storeItems.Find(x => x.name == categoryName);
        return (category.name != "") ? category.items : null;
    }

    // random present usage?
    public StoreItem GetRandomStoreItem(bool notPurchased = true, bool weighedByPrice = true)
    {
        List<StoreItem> items = new List<StoreItem>();
        foreach (StoreCategory cat in storeItems)
        {
            foreach (StoreItem item in cat.items)
            {
                items.Add(item);
            }
        }

        items.RemoveAll(x => x.price == 0);

        if (notPurchased)
        {
            items.RemoveAll(x => x.purchased == true);
        }

        if (weighedByPrice)
        {
            // get all different prices
            List<int> prices = new List<int>();
            foreach (StoreItem i in items)
            {
                if (!prices.Contains(i.price))
                    prices.Add(i.price);
            }

            // balance them out
            prices.Sort();
            float priceSum = 0;
            foreach (int i in prices)
                priceSum += 1.0f / i;
            float r = Random.Range(0.0f, priceSum);
            int result = 0;
            do {
                r -= 1.0f / prices[result++];
            } while (r > 0);

            items.RemoveAll(x => x.price != prices[result-1]);
        }
        
        return (items.Count > 0) ? items[Random.Range(0, items.Count - 1)] : null;
    }

    /// <summary>
    /// Attempt to unpack present and get reward
    /// </summary>
    /// <returns>Recieved item is returned</returns>
    public StoreItem UnpackPresent()
    {
        if (currentPresents == 0)
            return null;

        ModifyPresentsBy(-1);
        StoreItem item = GetRandomStoreItem();

        if (item != null)
        {
            item.purchased = true;
            DatabaseManager.UpdateField("Store", item.index.ToString(), "true");
        }

        return item;
    }
}
