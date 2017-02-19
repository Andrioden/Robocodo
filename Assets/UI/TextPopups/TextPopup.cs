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
        if (!textComponent)
            Debug.LogError("Text component is missing.");
        if (!animator)
            Debug.LogError("Animator is missing.");

        textComponent.text = DEFAULT_VALUE;

        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        Destroy(gameObject, clipInfo[0].clip.length);
    }

    private void Update()
    {
        AdjustPositionToCamera();
    }

    public void Initialize(string text, Vector3 worldPosition, Color? color = null)
    {
        this.worldPosition = worldPosition;
        AdjustPositionToCamera();
        SetText(text, color);
        textComponent.enabled = true;
    }

    private void AdjustPositionToCamera()
    {
        /* Doing this so that the text stays where it was spawned in world space and doesn't move when the screen moves. */
        transform.position = Camera.main.WorldToScreenPoint(worldPosition);
    }

    private void SetText(string text, Color? color = null)
    {
        if (!string.IsNullOrEmpty(text))
            textComponent.text = text;

        if (color != null && color.HasValue)
            textComponent.color = color.Value;
    }

    public class ColorType
    {
        public static ColorType DEFAULT { get { return new ColorType("12FFFFFF"); } }
        public static ColorType POSITIVE { get { return new ColorType("F9862DFF"); } }
        public static ColorType NEGATIVE { get { return new ColorType("FF1414FF"); } }

        private string hexColor;

        public ColorType(string hexColor)
        {
            this.hexColor = hexColor;
        }

        public Color Color()
        {
            return Utils.HexToColor(hexColor);
        }
    }
}
