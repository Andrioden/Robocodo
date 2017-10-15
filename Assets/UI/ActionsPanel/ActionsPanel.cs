using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionsPanel : MonoBehaviour
{
    public Button techTreeButton;
    private Animator techTreeButtonAnimator;
    public Slider techProgressbar;

    private PlayerController localPlayer;

    public static ActionsPanel instance;
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

    void Start()
    {
        SetupTechTreeButton();
    }

    private void SetupTechTreeButton()
    {
        techTreeButton.onClick.RemoveAllListeners();
        techTreeButton.onClick.AddListener(delegate { TechTreeDialog.instance.Show(localPlayer); });
        techTreeButtonAnimator = techTreeButton.GetComponent<Animator>();
        InvokeRepeating("AnimateTechTreeButton", 0, 1f);
    }

    public void RegisterLocalPlayer(PlayerController player)
    {
        localPlayer = player;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void AnimateTechTreeButton()
    {
        if (localPlayer == null)
            return;

        bool techTreeIsIdle = false;
        if (localPlayer.TechTree.PlayerShouldSelectResearch() && !TechTreeDialog.instance.IsOpen())
            techTreeIsIdle = true;

        techTreeButtonAnimator.SetBool("DoGlow", techTreeIsIdle);

        if (!techTreeIsIdle && localPlayer.TechTree.activeResearch != null)
        {
            techProgressbar.gameObject.SetActive(true);
            techProgressbar.value = Mathf.Clamp01(localPlayer.TechTree.activeResearch.GetProgressPercent() / 100f);
        }
        else
        {
            techProgressbar.gameObject.SetActive(false);
        }
    }
}
