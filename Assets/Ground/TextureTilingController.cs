using UnityEngine;

public class TextureTilingController : MonoBehaviour
{
    public Texture texture;
    public float textureToMeshZ = 1f;

    private Vector3 prevScale = Vector3.one;
    private float prevTextureToMeshZ = -1f;

    void Start()
    {
        prevScale = gameObject.transform.lossyScale;
        prevTextureToMeshZ = textureToMeshZ;
    }

    public void RescaleTileTexture()
    {
        if (gameObject.transform.lossyScale != prevScale || !Mathf.Approximately(textureToMeshZ, prevTextureToMeshZ))
            UpdateTiling();

        prevScale = gameObject.transform.lossyScale;
        prevTextureToMeshZ = textureToMeshZ;
    }

    void UpdateTiling()
    {
        if (texture == null)
            Debug.LogError("Missing tile texture!");

        // A Unity plane is 10 units x 10 units
        float planeSizeX = 10f;
        float planeSizeZ = 10f;

        // Figure out texture-to-mesh width based on user set texture-to-mesh height
        float textureToMeshX = ((float)texture.width / texture.height) * textureToMeshZ;

        GetComponent<Renderer>().sharedMaterial.mainTextureScale = new Vector2(planeSizeX * gameObject.transform.lossyScale.x / textureToMeshX, planeSizeZ * gameObject.transform.lossyScale.z / textureToMeshZ);
    }
}