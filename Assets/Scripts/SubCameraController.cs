﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubCameraController : MonoBehaviour
{
	[SerializeField]
	private Camera subCamera;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(7.5f, 16.0f, 
            7.5f);
        transform.rotation = Quaternion.Euler(90.0f, 0, 0);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        
    }
    public void UpdatePosition(Vector3 newPos){
    	subCamera.transform.position = newPos;
    }
}
