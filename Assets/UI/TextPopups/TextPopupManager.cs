using UnityEngine;

public class TextPopupManager : MonoBehaviour {

    public TextPopup textPopupPrefab;

    public static TextPopupManager instance;
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

    private void Start () {
        if (!textPopupPrefab)   
            Debug.LogError("TextPopup prefab is missing.");
    }
	
    public void ShowPopupGeneric(string text, Vector3 worldPosition, Color? color)
    {
        TextPopup popup = Instantiate(textPopupPrefab, transform, false);        
        popup.Configure(text, worldPosition, color);
    }    
}
