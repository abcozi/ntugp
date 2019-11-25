using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
	private int classID;
	private string className;
	
	public Item()
	{
		classID = -1;
		className = "";
	}
	public Item(int id, string name)
	{
		classID = id;
		className = name;
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
