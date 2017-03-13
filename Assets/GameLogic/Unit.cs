using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public abstract class Unit : OwnedNetworkBehaviour
{

    [SyncVar]
    public float x;
    public int X() { return (int)x; }

    [SyncVar]
    public float z;
    public int Z() { return (int)z; }

    [SyncVar]
    protected int health;
    public int Health { get { return health; } }

    // ********** SETTINGS **********
    public abstract int Settings_Damage();
    public abstract int Settings_StartHealth();

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

    public bool ChangePosition(float newPosX, float newPosZ)
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

    public void MoveTowards(int toPosX, int toPosZ)
    {
        SanityCheckIsWholeNumber("position X", x);
        SanityCheckIsWholeNumber("position Z", z);
        SanityCheckIsWholeNumber("to position X", toPosX);
        SanityCheckIsWholeNumber("to position Z", toPosZ);

        float difX = Math.Abs(x - toPosX);
        float difZ = Math.Abs(z - toPosZ);

        if (difX >= difZ)
            x += GetIncremementOrDecrementToGetCloser(x, toPosX);
        else if (difX < difZ)
            z += GetIncremementOrDecrementToGetCloser(z, toPosZ);
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

    private void SanityCheckIsWholeNumber(string friendlyName, float number)
    {
        if ((number % 1) != 0)
            throw new Exception(string.Format("Robot {0} is not a whole number, it is: '{1}'", friendlyName, number));
    }

    public Coordinate GetCoordinate()
    {
        return new Coordinate((int)x, (int)z);
    }

    public T FindFirstOnCurrentPosition<T>()
    {
        return FindNearbyCollidingGameObjects<T>()
            .Where(go => go.transform.position.x == x && go.transform.position.z == z)
            .Select(go => go.transform.root.GetComponent<T>())
            .FirstOrDefault();
    }

    public List<T> FindAllOnCurrentPosition<T>()
    {
        return FindNearbyCollidingGameObjects<T>()
            .Where(go => go.transform.position.x == x && go.transform.position.z == z)
            .Select(go => go.transform.root.GetComponent<T>())
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