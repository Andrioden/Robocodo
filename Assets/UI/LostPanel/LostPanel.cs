using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LostPanel : MonoBehaviour
{

    public GameObject panel;
    public Text textLabel;

    public static LostPanel instance;
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

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        //if (Input.GetKeyDown("escape") && panel.activeSelf)
        //    panel.SetActive(false);
    }

    public void Show(string text)
    {
        textLabel.text = text;
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
