using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class Eye : MonoBehaviour
{
	GameObject eyeObject;
	Eye eye;
	private int ownerID;
	private bool[] visibleToPlayers = new bool[]{false, false, false, false}; 
	private int belongTeam;//team1: white, team2: black
	private Vector3 eyeLocation;
	GameObject eyeLightObject;

	List<string> flagColors = new List<string>(new string[] { "White", "Black"});
	//constructor
	public Eye(int oID, int team, Vector3 loc)
	{
		ownerID = oID;
		belongTeam = team;
		eyeLocation = loc;
		if((int)PhotonNetwork.LocalPlayer.CustomProperties["team"] == belongTeam)
		{
			changePlayerVisibility(oID, true);
		}
		else
		{
			changePlayerVisibility(oID, false);
		}
		eyeObject = (GameObject)Instantiate(Resources.Load(flagColors[belongTeam-1]+"Flag"), eyeLocation, Quaternion.identity);
		eyeObject.layer = LayerMask.NameToLayer("Eye");
        // Make a game object
        eyeLightObject = (GameObject)Instantiate(Resources.Load("Eye Light"), new Vector3(eyeLocation.x, 2.41671f, eyeLocation.z), Quaternion.identity);
        // Add the light component
        Light lightComp = eyeLightObject.AddComponent<Light>();
        // Set color and position
        //lightComp.color = Color.blue;
        // Set the position (or any transform property)
        //lightGameObject.transform.position = new Vector3(0, 5, 0);
        Debug.Log("Eye(Ward) created at "+eyeLocation);
	}
    // Start is called before the first frame update
    void Start()
    {
        if(visibleToPlayers[(int)PhotonNetwork.LocalPlayer.CustomProperties["selectedCharacter"]-1])
        {
        	eyeObject.SetActive(true);
        	eyeLightObject.SetActive(true);
        }
        else
        {
        	eyeObject.SetActive(false);
        	eyeLightObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void changePlayerVisibility(int pID, bool vis)
    {
    	visibleToPlayers[pID-1] = vis;
    }
}
