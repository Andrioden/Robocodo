using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// For an click to be detected, this code now requires that the GameObject has an mesh with an collider
/// and a script that implements IClickable or ISelectable has to be attached to the parent of this colliding mesh.
/// </summary>
public class MouseManager : MonoBehaviour
{
    public static GameObject currentlySelected = null;

    private int loopingClickableNumber = 0;
    private ClickablePriority previousClickablePriority;

    private void Update()
    {
        if (Input.GetButtonDown("Fire1") && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(mouseRay);

            List<IClickable> clickables = hits.Select(h => h.transform.root.GetComponent<IClickable>()).Where(c => c != null).ToList();
            clickables = (new HashSet<IClickable>(clickables)).ToList(); // Distinct

            if (clickables.Count == 0)
                currentlySelected = null;
            else
                ClickObjectOfHighestPriorityInLoopingOrder(clickables);
        }
    }

    private void ClickObjectOfHighestPriorityInLoopingOrder(List<IClickable> clickables)
    {
        ClickablePriority highestClickablePriority = clickables.OrderBy(c => c.ClickPriority()).First().ClickPriority();
        List<IClickable> highestClickables = clickables.Where(c => c.ClickPriority() == highestClickablePriority).ToList();
        //Debug.Log("Order of highestClickables: " + string.Join(",", highestClickables.Select(c => c.GetGameObject().name).ToArray()));

        if (previousClickablePriority != highestClickablePriority)
            loopingClickableNumber = 0;

        if (loopingClickableNumber >= highestClickables.Count)
            loopingClickableNumber = 0;

        ClickAndSelectGameObject(highestClickables[loopingClickableNumber].GetGameObject());

        loopingClickableNumber++;
        previousClickablePriority = highestClickablePriority;
    }

    public static void ClickAndSelectGameObject(GameObject gameObject)
    {
        IClickable clickableObject = gameObject.GetComponent<IClickable>();
        if (clickableObject != null)
        {
            clickableObject.Click();
            if (clickableObject is ISelectable)
                currentlySelected = gameObject;
            else
                currentlySelected = null;

            return;
        }
        else
            currentlySelected = null;
    }
}