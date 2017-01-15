﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class PlayerController : NetworkBehaviour
{

    public GameObject cityRubblePrefab;

    [SyncVar]
    public string connectionId = "";

    private CityController __city;
    public CityController City
    {
        get
        {
            if (__city == null)
                __city = WorldController.instance.FindCityController(connectionId);
            return __city;
        }
    }

    [SyncVar]
    public string hexColor;

    [SyncVar]
    private string nick;
    public string Nick { get { return nick; } }

    [SyncVar]
    public double infectionContribution = 0;
    [SyncVar]
    public bool hasLost = false;

    private List<GameObject> ownedGameObjects = new List<GameObject>();


    // Use this for initialization
    private void Start()
    {
        if (isLocalPlayer)
        {
            ResourcePanel.instance.RegisterLocalPlayer(this);
            CmdRegisterPlayerNick(NetworkPanel.instance.nickInput.text);
            PositionCameraRelativeTo();
        }
    }

    [Server]
    public void SetColor(Color32 teamColor)
    {
        hexColor = Utils.ColorToHex(teamColor);
    }

    public void AddOwnedGameObject(GameObject go)
    {
        ownedGameObjects.Add(go);
    }

    public void ShowPopupForOwner(string text, Vector3 position, TextPopup.ColorType colorType)
    {
        if (isServer)
            TargetShowPopup(connectionToClient, text, position, colorType.Color());
        else
            TextPopupManager.instance.ShowPopupGeneric(text, position, colorType.Color());
    }

    [TargetRpc]
    private void TargetShowPopup(NetworkConnection target, string text, Vector3 position, Color color)
    {
        TextPopupManager.instance.ShowPopupGeneric(text, position, color);
    }


    [Command]
    private void CmdRegisterPlayerNick(string nick)
    {
        List<string> currentNicks = WorldController.instance.FindPlayerControllers().Where(p => !string.IsNullOrEmpty(p.Nick)).Select(p => p.Nick).ToList();

        if (!currentNicks.Contains(nick))
            this.nick = nick;
        else
            this.nick = nick + (currentNicks.Count(n => n.Contains(nick)) + 1);
    }

    [Server]
    public void LostAndDestroy()
    {
        hasLost = true;
        ownedGameObjects.ForEach(go => Destroy(go));
        TargetLost(connectionToClient);

        WorldController.instance.SpawnObject(cityRubblePrefab, (int)transform.position.x, (int)transform.position.z);
    }

    [TargetRpc]
    private void TargetLost(NetworkConnection target)
    {
        StackingRobotsOverhangManager.instance.DestroyAll();
    }

    private void PositionCameraRelativeTo()
    {
        RTSCamera.instance.PositionRelativeTo(transform);
    }

}