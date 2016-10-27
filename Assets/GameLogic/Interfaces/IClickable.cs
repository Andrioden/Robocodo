using UnityEngine;

public interface IClickable
{

    void Click();
    ClickablePriority ClickPriority();
    GameObject GetGameObject();

}

public enum ClickablePriority
{
    High = 1,          // Should only be 1 of those each square
    Medium = 2,    // Can be multiple each square
    Low = 3,
}