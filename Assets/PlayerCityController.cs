using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class PlayerCityController : NetworkBehaviour, IClickable
{
    // Use this for initialization
    void Start()
    {

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
