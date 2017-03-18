using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class KeyboardManager : NetworkBehaviour
{
    public static bool KeyboardLock = false;

    [SyncVar(hook = "OnTimeScaleUpdated")]
    private float timeScale;

    private float timeScaleBeforePause = 0;

    public override void OnStartClient()
    {
        Time.timeScale = timeScale;
    }

    private void Start()
    {
        timeScale = Time.timeScale;
    }

    private void Update()
    {
        if (isServer)
        {
            if (!KeyboardLock) //KeyboardLock is true when editing robot code. Don't want the game to pause/unpause everytime the host types 'p' in code.
            {
                if (Input.GetKeyDown(KeyCode.KeypadMinus) && Time.timeScale > 1)
                    timeScale = Time.timeScale - 1;
                else if (Input.GetKeyDown(KeyCode.KeypadPlus) && Time.timeScale < Settings.World_MaxTimeScale)
                    timeScale = Time.timeScale + 1;
                else if (Input.GetKeyDown(KeyCode.Pause) || Input.GetKeyDown(KeyCode.P))
                    PauseOrUnPause();
            }
        }

        if (Input.GetKeyDown("escape"))
            MouseManager.currentlySelected = null;
    }

    [Server]
    private void PauseOrUnPause()
    {
        if (Time.timeScale == 0) // Is paused
        {
            timeScale = timeScaleBeforePause;
            RpcHidePauseDialog();
        }
        else
        {
            timeScaleBeforePause = Time.timeScale;
            timeScale = 0;
            RpcShowPauseDialog();
        }
    }

    [Client]
    private void OnTimeScaleUpdated(float newTimeScale)
    {
        timeScale = newTimeScale;
        Time.timeScale = timeScale;
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

    [ClientRpc]
    private void RpcShowPauseDialog()
    {
        PauseDialog.instance.Show();
    }

    [ClientRpc]
    private void RpcHidePauseDialog()
    {
        PauseDialog.instance.Hide();
    }
}
