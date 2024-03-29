﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public abstract class Unit : OwnedNetworkBehaviour
{

    [SyncVar]
    protected int x;
    public int X { get { return x; } }
    public int GetX() { return x; }

    [SyncVar]
    protected int z;
    public int Z { get { return z; } }
    public int GetZ() { return z; }

    [SyncVar]
    protected int health;
    public int Health { get { return health; } }

    // ********** SETTINGS **********
    public abstract int Settings_Damage();
    public abstract int Settings_StartHealth();

    protected void SetXzToTransformPosition()
    {
        if ((transform.position.x % 1) != 0 || (transform.position.z % 1) != 0)
            throw new Exception(string.Format("Transform position is not whole numbers, it is: {0},{1} ", transform.position.x, transform.position.z));

        x = (int)transform.position.x;
        z = (int)transform.position.z;
    }

    public bool Move(MoveDirection direction)
    {
        if (direction == MoveDirection.Up)
            return ChangePosition(x, z + 1);
        else if (direction == MoveDirection.Down)
            return ChangePosition(x, z - 1);
        else if (direction == MoveDirection.Right)
            return ChangePosition(x + 1, z);
        else if (direction == MoveDirection.Left)
            return ChangePosition(x - 1, z);
        else if (direction == MoveDirection.Random)
        {
            MoveDirection randomDirection = new List<MoveDirection>
            {
                MoveDirection.Up,
                MoveDirection.Down,
                MoveDirection.Right,
                MoveDirection.Left
            }.TakeRandom();
            return Move(randomDirection);
        }
        else
            throw new Exception("This acting entity can not move in direction: " + direction);
    }

    public void MoveTowards(int toPosX, int toPosZ)
    {
        float difX = Math.Abs(x - toPosX);
        float difZ = Math.Abs(z - toPosZ);

        int newPosX = x;
        int newPosZ = z;

        if (difX >= difZ)
            newPosX += GetIncremementOrDecrementToGetCloser(x, toPosX);
        else if (difX < difZ)
            newPosZ += GetIncremementOrDecrementToGetCloser(z, toPosZ);

        ChangePosition(newPosX, newPosZ);
    }

    public bool ChangePosition(int newPosX, int newPosZ)
    {
        if (newPosX >= WorldController.instance.Width || newPosX < 0 || newPosZ >= WorldController.instance.Height || newPosZ < 0)
            return false;
        else
        {
            x = newPosX;
            z = newPosZ;
            return true;
        }
    }

    private int GetIncremementOrDecrementToGetCloser(float posValue, float homeValue)
    {
        if (posValue > homeValue)
            return -1;
        else if (posValue < homeValue)
            return 1;
        else
            throw new Exception("Should not call this method without a value difference");
    }

    public Coordinate GetCoordinate()
    {
        return new Coordinate(x, z);
    }

    public T FindFirstOnCurrentTransformPosition<T>()
    {
        return FindNearbyCollidingGameObjects<T>()
            .Where(go => go.transform.position.x == x && go.transform.position.z == z)
            .Select(go => go.transform.root.GetComponent<T>())
            .FirstOrDefault();
    }

    public List<T> FindAllOnCurrentTransformPosition<T>()
    {
        return FindNearbyCollidingGameObjects<T>()
            .Where(go => go.transform.position.x == x && go.transform.position.z == z)
            .Select(go => go.transform.root.GetComponent<T>())
            .ToList();
    }

    public List<T> FindAllOnCurrentPosition<T>() where T : Unit
    {
        return FindNearbyCollidingGameObjects<T>()
            .Select(go => go.transform.root.GetComponent<T>())
            .Where(u => u.x == x && u.z == z)
            .ToList();
    }

    public List<IAttackable> FindNearbyAttackableTargets(float searchRadius = 7.0f)
    {
        return FindNearbyCollidingGameObjectsOfType<IAttackable>(searchRadius).Where(t => t.Targetable()).ToList();
    }

    /// <summary>
    /// Find any T type that is attached to an GameObject with an collider that is nearby and has an component of T type, within searchRadius. Ordered by ascending distance.
    /// </summary>
    public List<T> FindNearbyCollidingGameObjectsOfType<T>(float searchRadius = 7.0f)
    {
        return FindNearbyCollidingGameObjects<T>(searchRadius).Select(go => go.transform.root.GetComponent<T>()).ToList();
    }

    /// <summary>
    /// Find any GameObject with an collider that is nearby and has an component of T type, within searchRadius. Ordered by ascending distance.
    /// </summary>
    public List<GameObject> FindNearbyCollidingGameObjects<T>(float searchRadius = 7.0f)
    {
        return FindNearbyCollidingGameObjects(searchRadius).Where(go => go.transform.root.GetComponent<T>() != null).ToList();
    }

    /// <summary>
    /// Find any GameObject with an collider that is nearby, within searchRadius. Ordered by ascending distance.
    /// </summary>
    public List<GameObject> FindNearbyCollidingGameObjects(float searchRadius = 7.0f)
    {
        return Physics.OverlapSphere(transform.position, searchRadius)
             .Except(new[] { GetComponent<Collider>() })                // Should check if its not the same collider as current collider, not sure if it works
             .Where(c => c.transform.root.gameObject != gameObject)     // Check that it is not the same object
             .OrderBy(c => GameObjectUtils.Distance(gameObject, c.gameObject))
             .Select(c => c.gameObject)
             .ToList();
    }
}

public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right,
    Home, // Not working for all entities
    Random
}