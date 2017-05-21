using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using EpPathFinding.cs;

public abstract class AI : MonoBehaviour
{

    protected PlayerController player;

    protected static JumpPointParam _cachedJpParam;

    protected abstract void StartAI();

    // Use this for initialization
    private void Start()
    {
        player = GetComponent<PlayerController>();

        if (player == null)
            throw new Exception("Added a AI Script to a non-PlayerController game object. Shame on you.");
        else
            Log("AI READY TO TAKE OVER THE WORLD");

        StartAI();
    }

    protected List<T> GetOwnedRobot<T>(bool isStarted) where T : RobotController
    {
        return GetOwned<T>().Where(r => r.IsStarted == isStarted).ToList();
    }

    protected List<T> GetOwned<T>() where T : RobotController
    {
        return player.OwnedGameObjects
            .Where(go => go != null)
            .Select(go => go.GetComponent<T>())
            .Where(r => r != null) // If it has the component then we know its of the right T type
            .ToList();
    }

    protected List<Instruction> FindPathInstructions(GameObject from, GameObject to)
    {
        int fromX = (int)from.transform.position.x;
        int fromZ = (int)from.transform.position.z;
        int toX = (int)to.transform.position.x;
        int toZ = (int)to.transform.position.z;

        List<Instruction> instructionPath = new List<Instruction>();

        int previousX = fromX;
        int previousZ = fromZ;

        var path = FindPath(fromX, fromZ, toX, toZ);
        foreach (GridPos gridPos in path)
        {
            if (gridPos.x > previousX)
                for (int i = 0; i < gridPos.x - previousX; i++)
                    instructionPath.Add(new Instruction_Move(MoveDirection.Right));

            if (gridPos.x < previousX)
                for (int i = 0; i < previousX - gridPos.x; i++)
                    instructionPath.Add(new Instruction_Move(MoveDirection.Left));

            if (gridPos.y > previousZ)
                for (int i = 0; i < gridPos.y - previousZ; i++)
                    instructionPath.Add(new Instruction_Move(MoveDirection.Up));

            if (gridPos.y < previousZ)
                for (int i = 0; i < previousZ - gridPos.y; i++)
                    instructionPath.Add(new Instruction_Move(MoveDirection.Down));

            previousX = gridPos.x;
            previousZ = gridPos.y;
        }

        return instructionPath;
    }

    /// <summary>
    /// Keep in mind this method returns the start end and turn points of the path, and not all movement steps.
    /// </summary>
    private List<GridPos> FindPath(int fromX, int fromZ, int toX, int toZ)
    {
        JumpPointParam jpParam = GetJumpPointParam();

        GridPos from = new GridPos(fromX, fromZ);
        GridPos to = new GridPos(toX, toZ);
        jpParam.Reset(from, to);

        List<GridPos> routeFound = JumpPointFinder.FindPath(jpParam);

        return routeFound;
    }

    private JumpPointParam GetJumpPointParam()
    {
        if (_cachedJpParam == null)
        {
            BaseGrid searchGrid = new StaticGrid(WorldController.instance.Width, WorldController.instance.Height);

            for (int x = 0; x < WorldController.instance.Width; x++)
                for (int z = 0; z < WorldController.instance.Height; z++)
                    searchGrid.SetWalkableAt(x, z, true);

            _cachedJpParam = new JumpPointParam(searchGrid); // Cant configure it to not allow diagonally moving, has to be enforced by us
        }

        return _cachedJpParam;
    }

    protected void Log(string message)
    {
        if (Settings.Debug_EnableAiLogging)
            Debug.Log(string.Format("[AI] {0}: {1}", player.name, message));
    }

    protected void LogFormat(string message, params object[] args)
    {
        if (Settings.Debug_EnableAiLogging)
            Debug.LogFormat(string.Format("[AI] {0}: {1}", player.name, message), args);
    }

}