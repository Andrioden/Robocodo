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
    private GameObject currentlySelected = null;
    public GameObject CurrentlySelectedObject { get { return currentlySelected; } }

    private IClickable lastClicked = null;
    public IClickable LastClickedObject { get { return lastClicked; } }

    private int loopingClickableNumber = 0;
    private ClickablePriority previousClickablePriority;
    private PlayerController localPlayer;

    public static MouseManager instance;

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

    private void Update()
    {
        if (Input.GetButtonDown("Fire1") && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(mouseRay);

            List<IClickable> clickables = hits.Select(h => h.transform.root.GetComponent<IClickable>()).Where(c => c != null).ToList();
            clickables = (new HashSet<IClickable>(clickables)).ToList(); // Distinct

            if (clickables.Count == 0)
                ClearSelectedObject();
            else
                ClickObjectOfHighestPriorityInLoopingOrder(clickables);
        }
    }

    public void RegisterLocalPlayer(PlayerController player)
    {
        localPlayer = player;
    }

    public void ClickGameObject(GameObject gameObject)
    {
        IClickable clickableObject = gameObject.GetComponent<IClickable>();
        if (clickableObject != null)
        {
            clickableObject.Click();
            lastClicked = clickableObject;
            SelectIfSelectable(gameObject, clickableObject);
            return;
        }
        else
            ClearSelectedObject();
    }

    public void ClearSelectedObject()
    {
        lastClicked = null;
        currentlySelected = null;
    }

    private void SelectIfSelectable(GameObject gameObject, IClickable clickableObject)
    {
        if (clickableObject is ISelectable)
        {
            var owner = gameObject.GetComponent<OwnedNetworkBehaviour>().GetOwner();
            if (owner != null && owner == localPlayer)
                currentlySelected = gameObject;
        }
        else
            currentlySelected = null;
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

        ClickGameObject(highestClickables[loopingClickableNumber].GetGameObject());

        loopingClickableNumber++;
        previousClickablePriority = highestClickablePriority;
    }
}