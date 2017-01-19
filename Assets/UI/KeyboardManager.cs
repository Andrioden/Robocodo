using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class KeyboardManager : NetworkBehaviour
{
    public static bool KeyboardLock = false;

    private float timeScaleBeforePause = 0;

    private void Update()
    {
        if (isServer)
        {
            if (Input.GetKeyDown(KeyCode.KeypadMinus) && Time.timeScale > 1)
                RpcAdjustTimeScale(Time.timeScale - 1);
            else if (Input.GetKeyDown(KeyCode.KeypadPlus) && Time.timeScale < Settings.World_MaxTimeScale)
                RpcAdjustTimeScale(Time.timeScale + 1);
            else if (Input.GetKeyDown(KeyCode.Pause) || Input.GetKeyDown(KeyCode.P))
                PauseOrUnPause();
        }

        if (Input.GetKeyDown("escape"))
            MouseManager.currentlySelected = null;
    }

    [Server]
    private void PauseOrUnPause()
    {
        if (Time.timeScale == 0) // Is paused
            RpcAdjustTimeScale(timeScaleBeforePause);
        else
        {
            timeScaleBeforePause = Time.timeScale;
            RpcAdjustTimeScale(0);
        }
    }

    [ClientRpc]
    private void RpcAdjustTimeScale(float newTimeScale)
    {
        Time.timeScale = newTimeScale;
    }

    public static void KeyboardLockOn(string arg0 = "")
    {
        KeyboardLock = true;
    }

    public static void KeyboardLockOff(string arg0 = "")
    {
        KeyboardLock = false;
    }
}
