using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
	private GameObject miniMap;
	private Button button;
	[SerializeField]
	private Camera subCamera;
	[SerializeField]
	private SubCameraController controller;
	
	Vector3 mousePosition = new Vector3((float)Screen.width*725.0f/800.0f, 0, (float)Screen.height*75.0f/150.0f);
	Rect tempRect;
    Vector3 miniPos;
	Vector2 lerp;	
	float newX;
	float newZ;
    Vector4 camMinMax; //x = min x, y = z min, z = x max, w = z max
		
	void Start()
	{
		miniMap = GameObject.Find("MiniMap");
		tempRect = getRect(miniMap);
		button = miniMap.GetComponent<Button>();
		getMousePosition();
	}
	Rect getRect(GameObject go)
	{
        RectTransform rt = go.GetComponent<RectTransform> ();
        return new Rect (Mathf.RoundToInt (rt.anchorMin.x * Screen.width), Mathf.RoundToInt ((1f-rt.anchorMax.y) * Screen.height), Mathf.RoundToInt ((rt.anchorMax.x - rt.anchorMin.x) * Screen.width), Mathf.RoundToInt ((rt.anchorMax.y - rt.anchorMin.y) * Screen.height));
    }
    void Update()
    {
    	//Z: zoom in, X: zoom out, direction keys: move camera;
    	float diff = 0.5f*(16.0f - subCamera.transform.position.y);
    	if(Input.GetKeyDown(KeyCode.Z))
	    {//zoom in
	    	Debug.Log("ZoomIn");
	    	ZoomIn(mousePosition);
	    }
	    else if(Input.GetKeyDown(KeyCode.X))
	    {
	    	ZoomOut(mousePosition);
	    }
	    else if(Input.GetKeyDown(KeyCode.UpArrow))
	    {
	    	ViewUp(mousePosition, diff);
	    }
	    else if(Input.GetKeyDown(KeyCode.DownArrow))
	    {
	    	ViewDown(mousePosition, diff);
	    }
	    else if(Input.GetKeyDown(KeyCode.LeftArrow))
	    {
	    	ViewLeft(mousePosition, diff);
	    }
	    else if(Input.GetKeyDown(KeyCode.RightArrow))
	    {
	    	ViewRight(mousePosition, diff);
	    }
    }
    public void getMousePosition()
    {
 		try
 		{
    		mousePosition = Input.mousePosition;
 		}
    	catch
    	{
    		mousePosition = new Vector3((float)Screen.width*725.0f/800.0f, 0, (float)Screen.height*75.0f/150.0f);
 		}
    	miniPos = mousePosition;
		lerp = new Vector2((miniPos.x - tempRect.x) / tempRect.width, (miniPos.y - tempRect.y) / tempRect.height);
		
		newX = (float) 15.0f*((float) miniPos.x - (float) Screen.width * 650.0f/800.0f)/(-(float)miniPos.x + (float)Screen.width * 1.0f);
		newZ = (float) 15.0f*((float) miniPos.y - 0.0f) / (-(float)miniPos.y + (float)Screen.height * 150.0f / 500.0f);
		if(newX >= 14.0f)
		{
			newX = 14.0f;
		}
		else if(newX <= 0.0f)
		{
			newX = 0.0f;
		}
		if(newZ >= 14.0f)
		{
			newZ = 14.0f;
		}
		else if(newZ <= 0.0f)
		{
			newZ = 0.0f;
		}
		camMinMax = new Vector4(subCamera.transform.position.x, subCamera.transform.position.z, newX, newZ); //x = min x, y = z min, z = x max, w = z max
		Debug.Log("newX: "+newX.ToString()+", newZ: "+newZ.ToString());
    }
	public void ZoomIn(Vector3 mousePosition)
	{
		if(subCamera.transform.position.y >= (float)5.0)
		{
			Vector3 camPos = new Vector3(Mathf.Lerp(camMinMax.x, camMinMax.z, lerp.x), subCamera.transform.position.y - (float)1.0, Mathf.Lerp(camMinMax.y, camMinMax.w, lerp.y));
			Debug.Log("ZoomIn: ("+newX.ToString()+", "+(subCamera.transform.position.y - 1.0f).ToString()+", "+newZ.ToString()+")");
			controller.UpdatePosition(camPos);
		}
		else
		{
			Vector3 camPos = new Vector3(Mathf.Lerp(camMinMax.x, camMinMax.z, lerp.x), subCamera.transform.position.y, Mathf.Lerp(camMinMax.y, camMinMax.w, lerp.y));
			Debug.Log("ZoomIn: ("+newX.ToString()+", "+(5.0f).ToString()+", "+newZ.ToString()+")");
			controller.UpdatePosition(camPos);
		}
	}
	public void ZoomOut(Vector3 mousePosition)
	{
		if(subCamera.transform.position.y <= (float)15.0)
		{
			Vector3 camPos = new Vector3(Mathf.Lerp(camMinMax.x, camMinMax.z, lerp.x), subCamera.transform.position.y + (float)1.0, Mathf.Lerp(camMinMax.y, camMinMax.w, lerp.y));
			Debug.Log("ZoomIn: ("+newX.ToString()+", "+(subCamera.transform.position.y + 1.0f).ToString()+", "+newZ.ToString()+")");
			controller.UpdatePosition(camPos);
		}
		else
		{
			Vector3 camPos = new Vector3(Mathf.Lerp(camMinMax.x, camMinMax.z, lerp.x), subCamera.transform.position.y, Mathf.Lerp(camMinMax.y, camMinMax.w, lerp.y));
			Debug.Log("ZoomIn: ("+newX.ToString()+", "+(15.0f).ToString()+", "+newZ.ToString()+")");
			controller.UpdatePosition(camPos);
		}
	}
	public void ViewUp(Vector3 mousePosition, float diff)
	{
		float cameraNewZ = subCamera.transform.position.z + 1.0f;
		if(cameraNewZ >= 7.5f + diff)
		{
			cameraNewZ = 7.5f + diff;
		}
		else if(cameraNewZ <= 7.5f - diff)
		{
			cameraNewZ = 7.5f - diff;
		}
		Vector3 camPos = new Vector3(subCamera.transform.position.x, subCamera.transform.position.y, cameraNewZ);
		controller.UpdatePosition(camPos);
	}
	public void ViewDown(Vector3 mousePosition, float diff)
	{
		float cameraNewZ = subCamera.transform.position.z - 1.0f;
		if(cameraNewZ >= 7.5f + diff)
		{
			cameraNewZ = 7.5f + diff;
		}
		else if(cameraNewZ <= 7.5f - diff)
		{
			cameraNewZ = 7.5f - diff;
		}
		Vector3 camPos = new Vector3(subCamera.transform.position.x, subCamera.transform.position.y, cameraNewZ);
		controller.UpdatePosition(camPos);
	}
	public void ViewLeft(Vector3 mousePosition, float diff)
	{
		float cameraNewX = subCamera.transform.position.x - 1.0f;
		if(cameraNewX <= 7.5f - diff)
		{
			cameraNewX = 7.5f - diff;
		}
		else if(cameraNewX >= 7.5f + diff)
		{
			cameraNewX = 7.5f + diff;
		}
		Vector3 camPos = new Vector3(cameraNewX, subCamera.transform.position.y, subCamera.transform.position.z);
		controller.UpdatePosition(camPos);
	}
	public void ViewRight(Vector3 mousePosition, float diff)
	{
		float cameraNewX = subCamera.transform.position.x + 1.0f;
		if(cameraNewX <= 7.5f - diff)
		{
			cameraNewX = 7.5f - diff;
		}
		else if(cameraNewX >= 7.5f + diff)
		{
			cameraNewX = 7.5f + diff;
		}
		Vector3 camPos = new Vector3(cameraNewX, subCamera.transform.position.y, subCamera.transform.position.z);
		controller.UpdatePosition(camPos);
	}
}
