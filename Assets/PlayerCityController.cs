using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class PlayerCityController : NetworkBehaviour, IClickable
{
    public MeshRenderer bodyMeshRenderer;

    // Use this for initialization
    void Start()
    {
        if (hasAuthority)
            bodyMeshRenderer.material.color = Color.blue;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Click()
    {
        if(isLocalPlayer)
        {            
            //CmdStartRobot(string code);
        }

        
    }

    [Command]
    void CmdStartRobot()
    {
        //PER DEF SERVER
    }

}
