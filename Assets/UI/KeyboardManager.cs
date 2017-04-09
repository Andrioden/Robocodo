using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class KeyboardManager : NetworkBehaviour
{
    public static bool KeyboardLock = false;

    private void Update()
    {
        if (isServer)
        {
            if (!KeyboardLock) //KeyboardLock is true when editing robot code. Don't want the game to pause/unpause everytime the host types 'p' in code.
            {
                if (Input.GetKeyDown(KeyCode.KeypadMinus) && Time.timeScale > 1)
                    WorldTimeController.instance.DecreaseTimeScale();

                else if (Input.GetKeyDown(KeyCode.KeypadPlus) && Time.timeScale < Settings.World_MaxTimeScale)
                    WorldTimeController.instance.IncreaseTimeScale();

                else if (Input.GetKeyDown(KeyCode.Pause) || Input.GetKeyDown(KeyCode.P))
                    WorldTimeController.instance.TogglePause();

                else if (Input.GetKeyDown(KeyCode.H))
                    RTSCamera.instance.PositionRelativeTo(WorldController.instance.FindClientsOwnPlayer().City.transform);
            }
        }

        if (Input.GetKeyDown("escape"))
            MouseManager.currentlySelected = null;
    }

    [Client]
    public static void KeyboardLockOn(string arg0 = "")
    {
        KeyboardLock = true;
    }

    [Client]
    public static void KeyboardLockOff(string arg0 = "")
    {
        KeyboardLock = false;
    }
}
