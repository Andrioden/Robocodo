using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class RobotMovementPreviewer : MonoBehaviour
{

    private int Settings_MaxPreviewInstructions = 200;

    public GameObject previewImagePrefab;
    public List<Texture> previewImageTextures;

    private List<GameObject> previewImages = new List<GameObject>();
    private float drawPreviewTime = -1f;

    private RobotController originalRobotController;
    private GameObject previeRobot;
    private RobotController previewRobotController;

    private List<CoordinatePreviewImage> processedCoordImgs;

    public void Update()
    {
        if (previeRobot != null && drawPreviewTime != -1f && drawPreviewTime < Time.time)
            DrawPreview();
    }

    public void Load(RobotController loadingRobotController)
    {
        Debug.Log("Loading");
        Unload();

        originalRobotController = loadingRobotController;
        originalRobotController.OnCurrentInstructionIndexChanged += Reload;

        previeRobot = originalRobotController.SpawnPreviewGameObjectClone();
        previeRobot.SetActive(false);
        previewRobotController = previeRobot.GetComponent<RobotController>();
        previewRobotController.isPreviewRobot = true;
        previewRobotController.SetOwner(originalRobotController.GetOwner());
        previewRobotController.InitDefaultValues();
        UpdateInstructions(originalRobotController.Instructions);
        previewRobotController.nextInstructionIndex = originalRobotController.nextInstructionIndex;

        DrawPreview();
    }

    public void UpdateInstructions(List<Instruction> instructions)
    {
        previewRobotController.SetInstructions(instructions);
        previewRobotController.PreviewReset();
    }

    private void Reload(int newValue)
    {
        Load(originalRobotController);
    }

    public void Unload()
    {
        if (originalRobotController != null)
            originalRobotController.OnCurrentInstructionIndexChanged -= Reload;

        DestroyPreview();

        Destroy(previeRobot);
        Destroy(previewRobotController);
        
        // Not really important, but do it to fully empty this class
        previeRobot = null;
        previewRobotController = null;
    }

    public void DrawPreviewAfterDelay(float secondsDelay)
    {
        drawPreviewTime = Time.time + secondsDelay;
    }

    private void DrawPreview()
    {
        DestroyPreview();

        processedCoordImgs = new List<CoordinatePreviewImage>();

        foreach (CoordinatePreviewImage coordImg in GetPreviewCoordinateImages())
        {
            GameObject previewImgGO = Instantiate(previewImagePrefab);
            previewImgGO.transform.position = new Vector3(coordImg.coordinate.x, previewImgGO.transform.position.y, coordImg.coordinate.z);
            Transform previewImgAdjustablePart = previewImgGO.transform.GetChild(0);

            AdjustImage(previewImgAdjustablePart, coordImg);
            AdjustRotation(previewImgAdjustablePart, coordImg);
            AdjustAlignment(previewImgAdjustablePart, coordImg);

            processedCoordImgs.Add(coordImg);
            previewImages.Add(previewImgGO);
        }

        StopUpdatingPreview();
    }

    private void AdjustImage(Transform previewImgAdjustablePart, CoordinatePreviewImage coordImg)
    {
        Texture imagePreviewTexture = previewImageTextures.FirstOrDefault(t => t.name == coordImg.previewImage.Name);
        if (imagePreviewTexture == null)
            throw new Exception(string.Format("Instruction with unknown image name '{0}' attempted to be previewd", coordImg.previewImage.Name));
        else
            previewImgAdjustablePart.GetComponent<MeshRenderer>().material.mainTexture = imagePreviewTexture;
    }

    private static void AdjustRotation(Transform previewImgAdjustablePart, CoordinatePreviewImage coordImg)
    {
        Vector3 rotation = previewImgAdjustablePart.transform.rotation.eulerAngles;
        if (coordImg.previewImage.Direction == Direction.Right)
            previewImgAdjustablePart.transform.rotation = Quaternion.Euler(rotation.x, 0, rotation.z);
        else if (coordImg.previewImage.Direction == Direction.Down)
            previewImgAdjustablePart.transform.rotation = Quaternion.Euler(rotation.x, 90, rotation.z);
        else if (coordImg.previewImage.Direction == Direction.Left)
            previewImgAdjustablePart.transform.rotation = Quaternion.Euler(rotation.x, 180, rotation.z);
        else if (coordImg.previewImage.Direction == Direction.Up)
            previewImgAdjustablePart.transform.rotation = Quaternion.Euler(rotation.x, 270, rotation.z);
    }

    private void AdjustAlignment(Transform previewImgAdjustablePart, CoordinatePreviewImage coordImg)
    {
        float localX = 0;
        float localZ = 0;

        // Vertical
        if (coordImg.previewImage.VerticalAlign == VerticalAlign.Dynamic)
        {
            if (processedCoordImgs.Any(x => Coordinate.IsEqual(x.coordinate, coordImg.coordinate) && x.previewImage.VerticalAlign == VerticalAlign.Top))
                coordImg.previewImage.VerticalAlign = VerticalAlign.Bottom;
            else
                coordImg.previewImage.VerticalAlign = VerticalAlign.Top;
        }

        if (coordImg.previewImage.VerticalAlign == VerticalAlign.Top)
            localZ = 0.25f;
        else if (coordImg.previewImage.VerticalAlign == VerticalAlign.Bottom)
            localZ = -0.25f;

        // Horizontal
        if (coordImg.previewImage.HorizontalAlign == HorizontalAlign.Dynamic)
        {
            if (processedCoordImgs.Any(i => Coordinate.IsEqual(i.coordinate, coordImg.coordinate) && i.previewImage.HorizontalAlign == HorizontalAlign.Right && i.previewImage.VerticalAlign == coordImg.previewImage.VerticalAlign))
                coordImg.previewImage.HorizontalAlign = HorizontalAlign.Left;
            else
                coordImg.previewImage.HorizontalAlign = HorizontalAlign.Right;
        }

        if (coordImg.previewImage.HorizontalAlign == HorizontalAlign.Right)
            localX = 0.25f;
        else if (coordImg.previewImage.HorizontalAlign == HorizontalAlign.Left)
            localX = -0.25f;

        previewImgAdjustablePart.localPosition = new Vector3(localX, 0, localZ);
    }

    private List<CoordinatePreviewImage> GetPreviewCoordinateImages()
    {
        if (previewRobotController.Instructions.Count == 0)
            return new List<CoordinatePreviewImage>();

        List<CoordinatePreviewImage> coordinateImages = new List<CoordinatePreviewImage>();

        // Add an initial coordinate so it can be used to detect movement direction for the first instruction
        coordinateImages.Add(new CoordinatePreviewImage(previewRobotController.GetCoordinate()));

        int instructionsRun = 0;
        while (true)
        {
            previewRobotController.ProcessNextInstruction();

            PreviewImage previewImage = previewRobotController.LastExecutedInstruction.Setting_PreviewImage();
            if (previewImage != null)
            {
                // Handling movement direction for Move instructions is a special rule
                if (previewRobotController.LastExecutedInstruction.GetType() == typeof(Instruction_Move))
                {
                    Coordinate prevCoordinate = coordinateImages[coordinateImages.Count - 1].coordinate;
                    Coordinate curCoordinate = previewRobotController.GetCoordinate();
                    if (curCoordinate.x != prevCoordinate.x || curCoordinate.z != prevCoordinate.z)
                    {
                        previewImage.Direction = FindCoordinateDirection(prevCoordinate, curCoordinate);
                        coordinateImages.Add(new CoordinatePreviewImage(curCoordinate, previewImage));
                    }
                }
                else // Normal preview image
                    coordinateImages.Add(new CoordinatePreviewImage(previewRobotController.GetCoordinate(), previewImage));
            }

            if (previewRobotController.MainLoopIterationCount > 0 || previewRobotController.Energy <= 0)
                break;
            else if (instructionsRun > Settings_MaxPreviewInstructions)
            {
                Debug.LogWarning("Robot preview algorithm exceeded settings for max preview instructions.");
                break;
            }

            instructionsRun++;
        }

        // Remove initial coordinate
        coordinateImages.RemoveAt(0);

        return coordinateImages;
    }

    private Direction FindCoordinateDirection(Coordinate from, Coordinate to)
    {
        if (from.z < to.z)
            return Direction.Up;
        else if (from.z > to.z)
            return Direction.Down;
        else if (from.x < to.x)
            return Direction.Right;
        else if (from.x > to.x)
            return Direction.Left;
        else
            throw new Exception("Tried to find coordinate between the same coordinate, should not happend!");
    }

    private void StopUpdatingPreview()
    {
        drawPreviewTime = -1f; // Stop updating
    }

    private void DestroyPreview()
    {
        foreach (GameObject image in previewImages)
            Destroy(image);

        previewImages = new List<GameObject>();
    }

}

public class CoordinatePreviewImage
{
    public Coordinate coordinate;
    public PreviewImage previewImage;

    public CoordinatePreviewImage(Coordinate coordinate)
    {
        this.coordinate = coordinate;
    }

    public CoordinatePreviewImage(Coordinate coordinate, PreviewImage previewImage)
    {
        this.coordinate = coordinate;
        this.previewImage = previewImage;
    }

}

public class PreviewImage
{
    public string Name;
    public Direction Direction;
    public VerticalAlign VerticalAlign;
    public HorizontalAlign HorizontalAlign;
}

public enum Direction
{
    None,
    Up,
    Down,
    Left,
    Right
}

public enum VerticalAlign
{
    Dynamic,
    Top,
    Bottom
}

public enum HorizontalAlign
{
    Dynamic,
    Left,
    Right
}