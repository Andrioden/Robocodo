using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using System.Linq;

public class ScavengerGroupController : ActingEntity, IAttackable
{

    // ********** SETTINGS **********
    public override int Settings_Damage() { return 1; }
    public override int Settings_StartHealth() { return 5; }

    // Use this for initialization
    private void Start()
    {
        x = transform.position.x;
        z = transform.position.z;
    }

    private void Update()
    {
        Move();
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
            WorldTickController.instance.HalfTickEvent -= AI_Ticker; // Avoids exception when quitting game
    }

    [Client]
    private void Move()
    {
        var newPosition = new Vector3(x, transform.position.y, z);
        transform.LookAt(newPosition);
        transform.position = Vector3.MoveTowards(transform.position, newPosition, (1.0f / Settings.World_IrlSecondsPerTick) * Time.deltaTime * 2 /* 2 = halftick */);
    }

    private void AI_Ticker(object sender)
    {
        int searchRadius = Settings.World_ScavengerAggressiveness + Utils.RandomInt(-2, 2);

        if (searchRadius <= 0)
            return;

        IAttackable target = FindNearbyAttackableTargets(searchRadius).FirstOrDefault(t => MathUtils.Distance(x, z, t.X(), t.Z()) <= searchRadius);
        if (target != null)
        {
            //Debug.Log("Distance: " + MathUtils.Distance(x, z, target.X(), target.Z()));
            if (MathUtils.Distance(x, z, target.X(), target.Z()) == 0)
                target.TakeDamage(Settings_Damage());
            else
                MoveTowards(target.X(), target.Z());
        }
        else
            Move(MoveDirection.Random);
    }

    [Server]
    public PlayerCityController GetOwnerCity()
    {
        return null;
    }

    [Server]
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.LogFormat("Scavenger {0} took {1} damage and now has {2} health", name, damage, health);

        if (health <= 0)
        {
            WorldTickController.instance.HalfTickEvent -= AI_Ticker;
            RpcSyncState(false, x, z);
        }
    }

    [Server]
    public bool Targetable()
    {
        return true;
    }

    [Server]
    public void Initialize(float posX, float posZ)
    {
        gameObject.SetActive(true);

        health = Settings_StartHealth();

        WorldTickController.instance.HalfTickEvent += AI_Ticker;

        SetPosition(posX, posZ);
        RpcSyncState(gameObject.activeSelf, x, z);
    }

    [ClientRpc]
    public void RpcSyncState(bool active, float posX, float posZ)
    {
        gameObject.SetActive(active);
        SetPosition(posX, posZ);
    }

    private void SetPosition(float posX, float posZ)
    {
        x = posX;
        z = posZ;
        transform.position = new Vector3(posX, transform.position.y, posZ);
    }
}