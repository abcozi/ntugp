/*
	Global Variables
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{
    public static int mapSize = 15;
    public static int seed = 0;

    private void Awake()
    {
        seed = Random.RandomRange(0, 1000);
    }
}
