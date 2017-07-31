﻿using System;
using UnityEngine;
using UnityEngine.Networking;

public class WorldTimeController : NetworkBehaviour
{
    [SyncVar(hook = "OnTimeScaleUpdated")]
    private float timeScale;
    private float timeScaleBeforePause = 0;

    private float timeDebug;

    public static WorldTimeController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to create another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        timeScale = Time.timeScale;

        if (Settings.GUI_EnableGameLobby)
        {
            GameLobbyPanel.instance.Show(isServer);
        }
        else
            WorldTickController.instance.StartTick();
    }

    private void Update()
    {
        timeDebug = Time.time;
    }

    public override void OnStartClient()
    {
        Time.timeScale = timeScale;
    }

    [Server]
    public void StartGame()
    {
        RpcHideGameLobbyPanel();
        WorldTickController.instance.StartTick();
    }

    [Server]
    public void TogglePause()
    {
        if (Time.timeScale == 0)
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

    [Server]
    public void SetTimeScale(int newTimeScale)
    {
        timeScale = newTimeScale;
    }

    [Server]
    public void DecreaseTimeScale()
    {
        timeScale = Time.timeScale - 1;
    }

    [Server]
    public void DecreaseTimeScaleSlightly()
    {
        timeScale = Time.timeScale - 0.05f;
    }

    [Server]
    public void IncreaseTimeScale()
    {
        timeScale = Time.timeScale + 1;
    }

    [Server]
    public void IncreaseTimeScaleSlightly()
    {
        timeScale = Time.timeScale + 0.05f;
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

    [ClientRpc]
    private void RpcHideGameLobbyPanel()
    {
        GameLobbyPanel.instance.Hide();
    }
}
