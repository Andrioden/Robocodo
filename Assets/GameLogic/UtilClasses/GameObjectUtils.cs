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

    public static double Distance(GameObject go1, GameObject go2)
    {
        return MathUtils.Distance(go1.transform.position.x, go1.transform.position.z, go2.transform.position.x, go2.transform.position.x);
    }

}

