using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public void SceneLoader(int sceneIndex)
    {
    	Debug.Log("Scene Index: "+sceneIndex.ToString());
    	if(sceneIndex == 2)
    	{
    		if(PlayerPrefs.HasKey("NickName"))
    		{
    			PlayerPrefs.SetString("NickName", "");
    		}
    		PhotonNetwork.NickName = "";
    		Debug.Log("Initial username: "+PhotonNetwork.NickName);
    	}
    	SceneManager.LoadScene(sceneIndex);
    }
}
