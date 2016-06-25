using UnityEngine;
using System.Collections;

public class RTSCamera : MonoBehaviour
{

    public float horizontalSpeed = 40;
    public float verticalSpeed = 40;
    //public float cameraRorateSpeed = 80;
    public float cameraDistance = 30;

    private float curDistance;


    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal") * horizontalSpeed * Time.deltaTime;
        float vertical = Input.GetAxis("Vertical") * verticalSpeed * Time.deltaTime;
        //float rotation = Input.GetAxis("Rotation");

        transform.Translate(Vector3.forward * vertical);
        transform.Translate(Vector3.right * horizontal);

        //if(Roration != 0)
        //{
        //    transform.Rotate(Vector3.up, Roration * cameraRorateSpeed * Time.deltaTime, Space.World);
        //}

        MoveCameraUpDownDetector();
    }

    public void PositionRelativeToPlayer(Transform player)
    {
        transform.position = new Vector3(player.transform.position.x, cameraDistance, player.transform.position.z + 5f);
    }

    private void MoveCameraUpDownDetector()
    {
        var scollInput = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(Vector3.down * scollInput * 15f);
    }
}
