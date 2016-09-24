using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextPopup : MonoBehaviour
{
    public Animator animator;
    public Text textComponent;

    private string DEFAULT_VALUE = "TEXT MISSING";
    private Vector3 screenPosition;
    private Vector3 worldPosition;

    private void Awake()
    {
        if(!textComponent)
            Debug.LogError("Text component is missing.");
        if (!animator)
            Debug.LogError("Animator is missing.");

        textComponent.text = DEFAULT_VALUE;

        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        Destroy(gameObject, clipInfo[0].clip.length);
    }

    private void Update()
    {
        MaintainWorldPosition();
    }

    private void MaintainWorldPosition()
    {
        /* Doing this so that the text stays where it was spawned in world space and doesn't move when the screen moves. */
        screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        transform.position = screenPosition;
    }

    public void Configure(string text, Vector3 worldPosition, Color? color = null)
    {
        this.worldPosition = worldPosition;
        SetText(text, color);
        textComponent.enabled = true;
    }

    private void SetText(string text, Color? color = null)
    {
        if (!string.IsNullOrEmpty(text))
            textComponent.text = text;

        if (color != null && color.HasValue)
            textComponent.color = color.Value;
    }
}
