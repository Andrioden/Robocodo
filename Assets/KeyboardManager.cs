using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class KeyboardManager : NetworkBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            if (Input.GetKeyDown(KeyCode.KeypadMinus) && Time.timeScale > 1)
                RpcAdjustTimeScale(Time.timeScale - 1);
            else if (Input.GetKeyDown(KeyCode.KeypadPlus) && Time.timeScale < Settings.World_MaxTimeScale)
                RpcAdjustTimeScale(Time.timeScale + 1);
        }
    }

    [ClientRpc]
    private void RpcAdjustTimeScale(float newTimeScale)
    {
        Time.timeScale = newTimeScale;
    }
}
