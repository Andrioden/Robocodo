using UnityEngine;
using System.Collections;

public class LostPanel : MonoBehaviour
{

    public GameObject panel;

    public static LostPanel instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to created another instance of " + GetType() + ". Destroying.");
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
        if (Input.GetKeyDown("escape") && panel.activeSelf)
            panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
    }
}
