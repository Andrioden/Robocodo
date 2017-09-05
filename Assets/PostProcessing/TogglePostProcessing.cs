using UnityEngine;
using UnityEngine.PostProcessing;

public class TogglePostProcessing : MonoBehaviour {

    private PostProcessingBehaviour ppe;
    private bool toggle = true;

    private void Start()
    {
        ppe = Camera.main.GetComponent<PostProcessingBehaviour>();
    }

    private void Update () {
        if (Input.GetKeyUp(KeyCode.K))
        {
            toggle = !toggle;
            ppe.profile.ambientOcclusion.enabled = toggle;
            ppe.profile.vignette.enabled = toggle;
            ppe.profile.antialiasing.enabled = toggle;
            ppe.profile.bloom.enabled = toggle;
        }
    }
}
