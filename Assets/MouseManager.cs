using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// For an click to be detected, this code now requires that the GameObject has an mesh with an collider
/// and the script that implements IClickable has to be attached to the parent of this colliding mesh.
/// </summary>
public class MouseManager : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        //Add this when we start creating GUI elements aka have an EventSystem
        //if (EventSystem.current.IsPointerOverGameObject())
        //    return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
        {
            GameObject ourHitObject = hitInfo.collider.transform.gameObject;

            if (ourHitObject.transform.parent == null)
                return;
            else
                ourHitObject = ourHitObject.transform.parent.gameObject;

            if (Input.GetMouseButtonDown(0))
            {
                IClickable clickableObject = ourHitObject.GetComponent<IClickable>();
                if (clickableObject != null)
                {
                    clickableObject.Click();
                    return;
                }
            }

        }

    }
}
