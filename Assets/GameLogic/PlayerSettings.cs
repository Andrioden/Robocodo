using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
 * In the future the purpose of this class is to load and store presistent player settings. But atm it only last during the runtime of the application. = P
 * TODO: Serialize
 */
public static class PlayerSettings
{

    public static string Game_Nick = "Andriod";
    public static Scenario Game_ScenarioChoice = Scenario.Normal;
    public static int Game_Players = 2;
    public static int Game_AIs = 0;
    public static int Graphics_MaxFPS = 0;

    public static string MM_Server = "eu1-mm.unet.unity3d.com";


    public static void Load()
    {
        throw new NotImplementedException();
    }

    public static void Save()
    {
        throw new NotImplementedException();
    }

}
