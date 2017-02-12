using UnityEngine;
using System.Collections;

public class ScrollTankTracks : MonoBehaviour
{
    private Renderer tankTracksRenderer;
    private float scrollSpeed = 400f;
    private float offset;
    private Vector2 oldPosition;
    private Vector2 newPosition;
    private float distance;

    void Start()
    {
        tankTracksRenderer = GetComponent<Renderer>();
        oldPosition = transform.position;
    }

    void Update()
    {
        newPosition = new Vector2(transform.position.x, transform.position.z);
        distance = Vector2.Distance(oldPosition, newPosition);
        oldPosition = newPosition;

        offset = (offset + distance * scrollSpeed * Time.deltaTime) % 10;
        tankTracksRenderer.material.mainTextureOffset = new Vector2(0, offset);
    }
}
