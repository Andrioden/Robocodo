using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{

    private Camera mainCamera;
    private int userChosenQualityLevel;

    private void Start()
    {
        mainCamera = Camera.main;
        userChosenQualityLevel = QualitySettings.GetQualityLevel();
    }

    private void Update()
    {
        MoveCameraVertifalHorizontalDetector();
        MoveCameraUpDownDetector();
    }

    private void MoveCameraVertifalHorizontalDetector()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical") * Mathf.Sin(transform.rotation.eulerAngles.x * (Mathf.PI / 180));
        float z = Input.GetAxis("Vertical") * Mathf.Cos(transform.rotation.eulerAngles.x * (Mathf.PI / 180));

        transform.Translate(x * mainCamera.orthographicSize / 50, y * mainCamera.orthographicSize / 50, z * mainCamera.orthographicSize / 50, Space.Self);
    }

    private void MoveCameraUpDownDetector()
    {
        var d = Input.GetAxis("Mouse ScrollWheel");
        if (d > 0f)
        {
            mainCamera.orthographicSize *= 0.80f;
        }
        else if (d < 0f)
        {
            mainCamera.orthographicSize *= 1.20f;
        }

        if (mainCamera.orthographicSize > 20)
            QualitySettings.SetQualityLevel(0, false);
        else
            QualitySettings.SetQualityLevel(userChosenQualityLevel, false);
    }

}