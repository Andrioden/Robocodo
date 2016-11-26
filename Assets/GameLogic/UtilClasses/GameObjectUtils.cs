using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameObjectUtils : ScriptableObject
{

    public static PlayerCityController FindClientsOwnPlayerCity()
    {
        return FindObjectsOfType<PlayerCityController>().FirstOrDefault(p => p.hasAuthority);
    }

}

