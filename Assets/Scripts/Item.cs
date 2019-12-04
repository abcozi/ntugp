using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    private static int purplePossibility = 5, bluePossibility = 10, greenPossibility = 35, whitePossibility = 50;
    private static int purpleStorePossibility = 50, blueStorePossibility = 35, greenStorePossibility = 10, whiteStorePossibility = 5;
    private int price, times;
	private string id, itemName, resourceName, description, level;
	
	public Item()
	{
        id = "";
        itemName = "";
        resourceName = "";
        description = "";
        level = "";

    }
	public Item(string id, string name, string resourceName, string description, string level, int price, int times)
	{
        this.id = id;
        this.itemName = name;
        this.resourceName = resourceName;
        this.description = description;
        this.level = level;
        this.price = price;
        this.times = times;
    }
    public string GetID()
    {
        return id;
    }
    public void SetId(string id)
    {
        this.id = id;
    }
    public string GetName()
    {
        return itemName;
    }
    public void SetName(string name)
    {
        this.itemName = name;
    }
    public string GetResourceName()
    {
        return resourceName;
    }
    public void SetResourceName(string resourceName)
    {
        this.resourceName = resourceName;
    }
    public string GetDescription()
    {
        return description;
    }
    public void SetDescription(string description)
    {
        this.description = description;
    }
    public string GetLevel()
    {
        return level;
    }
    public void SetLevel(string level)
    {
        this.level = level;
    }
    public int GetPrice()
    {
        return price;
    }
    public void SetPrice(int price)
    {
        this.price = price;
    }
    public int GetTimes()
    {
        return times;
    }
    public void SetTimes(int times)
    {
        this.times = times;
    }
    public static int GetPurplePossibility()
    {
        return purplePossibility;
    }
    public static void SetPurplePossibility(int purplePossibility)
    {
        Item.purplePossibility = purplePossibility;
    }
    public static int GetBluePossibility()
    {
        return bluePossibility;
    }
    public static void SetBluePossibility(int bluePossibility)
    {
        Item.bluePossibility = bluePossibility;
    }
    public static int GetGreenPossibility()
    {
        return greenPossibility;
    }
    public static void SetGreenPossibility(int greenPossibility)
    {
        Item.greenPossibility = greenPossibility;
    }
    public static int GetwhitePossibility()
    {
        return whitePossibility;
    }
    public static void SetWhitePossibility(int whitePossibility)
    {
        Item.whitePossibility = whitePossibility;
    }
    public static int GetPurpleStorePossibility()
    {
        return purpleStorePossibility;
    }
    public static void SetPurpleStorePossibility(int purpleStorePossibility)
    {
        Item.purpleStorePossibility = purpleStorePossibility;
    }
    public static int GetBlueStorePossibility()
    {
        return blueStorePossibility;
    }
    public static void SetBluStorePossibility(int blueStorePossibility)
    {
        Item.blueStorePossibility = blueStorePossibility;
    }
    public static int GetGreenStorePossibility()
    {
        return greenStorePossibility;
    }
    public static void SetGreenStorePossibility(int greenStorePossibility)
    {
        Item.greenStorePossibility = greenStorePossibility;
    }
    public static int GetWhiteStorePossibility()
    {
        return whiteStorePossibility;
    }
    public static void SetWhiteStorePossibility(int whiteStorePossibility)
    {
        Item.whiteStorePossibility = whiteStorePossibility;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
