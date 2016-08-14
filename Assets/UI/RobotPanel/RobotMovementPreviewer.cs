using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RobotMovementPreviewer
{

    private int Settings_MaxPreviewInstructions = 200;

    private GameObject robotClone;
    private RobotController robotCloneController;

    public RobotMovementPreviewer(RobotController selectedRobot, List<string> instructions)
    {
        robotClone = selectedRobot.SpawnPreviewGameObjectClone();
        robotClone.SetActive(false);
        robotCloneController = robotClone.GetComponent<RobotController>();
        robotCloneController.isPreviewRobot = true;
        robotCloneController.InitDefaultValues();
        UpdateInstructions(instructions);
    }

    public void Destroy()
    {
        GameObject.Destroy(robotCloneController);
        GameObject.Destroy(robotClone);
    }

    public void UpdateInstructions(List<string> instructions)
    {
        robotCloneController.SetInstructions(instructions);
        robotCloneController.PreviewResetRobot();
    }

    public List<CoordinateDirection> GetPreviewCoordinateDirections()
    {
        List<Coordinate> coords = new List<Coordinate>();
        coords.Add(robotCloneController.GetCoordinate());

        int instructionsRun = 0;
        while (true)
        {
            robotCloneController.RunNextInstruction(null);
            Coordinate prevCoordinateDir = coords[coords.Count - 1];
            Coordinate nextCoordinate = robotCloneController.GetCoordinate();
            if (nextCoordinate.x != prevCoordinateDir.x || nextCoordinate.z != prevCoordinateDir.z)
                coords.Add(nextCoordinate);

            if (robotCloneController.InstructionsMainLoopCount > 0 || robotCloneController.Energy <= 0)
                break;
            else if (instructionsRun > Settings_MaxPreviewInstructions)
            {
                Debug.LogWarning("Robot preview algorithm exceeded settings for max preview instructions.");
                break;
            }

            instructionsRun++;
        }

        List<CoordinateDirection> coordinateDirections = new List<CoordinateDirection>();
        for (int i = 0; i < coords.Count - 1; i++)
            coordinateDirections.Add(FindCoordinateDirection(coords[i], coords[i + 1]));

        if (coordinateDirections.Count > 0)
        {
            Direction secondLastDirection = coordinateDirections[coordinateDirections.Count - 1].direction;
            coordinateDirections.Add(new CoordinateDirection(coords[coords.Count - 1], secondLastDirection));
        }

        return coordinateDirections;
    }

    private CoordinateDirection FindCoordinateDirection(Coordinate from, Coordinate to)
    {
        if (from.z < to.z)
            return new CoordinateDirection(from, Direction.Up);
        else if (from.z > to.z)
            return new CoordinateDirection(from, Direction.Down);
        else if (from.x < to.x)
            return new CoordinateDirection(from, Direction.Right);
        else if (from.x > to.x)
            return new CoordinateDirection(from, Direction.Left);
        else
            throw new Exception("Tried to find coordinate between the same coordinate, should not happend!");
    }

}

public class CoordinateDirection
{
    public int x;
    public int z;
    public Direction direction;

    public CoordinateDirection(Coordinate coordinate, Direction direction)
    {
        x = coordinate.x;
        z = coordinate.z;
        this.direction = direction;
    }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}