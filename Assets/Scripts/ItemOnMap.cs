using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemOnMap : MonoBehaviour
{
    //adjust this to change speed
    float speed = 2.5f;
    //adjust this to change how high it goes
    float height = 0.1f;
    float delayTime;
    bool lockUpdate = true;
    //public Texture2D cursorTexture;
    //public CursorMode cursorMode = CursorMode.Auto;
    //public Vector2 hotSpot = Vector2.zero;

    void Start()
    {
            StartCoroutine(Delay());
            
    }

    IEnumerator Delay()
    {	
    	delayTime = Random.Range(0.0f, 1.0f);
        yield return new WaitForSeconds(delayTime);
        lockUpdate = false;
    }


    void Update()
    {
    	if(!lockUpdate){
    		//get the objects current position and put it in a variable so we can access it later with less code
	        Vector3 pos = transform.position;
	        //calculate what the new Y position will be
	        float newY = Mathf.Sin((Time.time - delayTime) * speed);
	        //set the object's Y to the new calculated Y
	        transform.position = new Vector3(pos.x, newY * height, pos.z) ;
    	}
        
    }
    //void OnMouseOver()
    //{
    //    //If your mouse hovers over the GameObject with the script attached, output this message
    //    Debug.LogError("Mouse is over GameObject.");
    //}
    //void OnMouseEnter()
    //{
    //    Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    //}

    //void OnMouseExit()
    //{
    //    Cursor.SetCursor(null, Vector2.zero, cursorMode);
    //}
}
