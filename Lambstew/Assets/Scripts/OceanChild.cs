using UnityEngine;

public class OceanChild : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    public void Initialize(ref Mesh sharedMesh, Material sharedMaterial)
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = sharedMesh;

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = sharedMaterial;
    }
}

