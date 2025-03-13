using System;
using System.Collections.Generic;
using UnityEngine;

public class OceanCreator : MonoBehaviour
{
    public int dimension = 100;
    public Vector2 childrenCounts;
    public List<OceanChild> children = new List<OceanChild>();

    public float uvScale = 1.0f; 
    public Octave[] octaves;
    private Vector3[] verts;
    private Mesh sharedMesh;
    private Material sharedMaterial;

    [Serializable]
    public struct Octave
    {
        public Vector2 speed;
        public Vector2 scale;
        public float height;
        public bool alternate;
    }

    void Start()
    {
        SetupMesh();
        CreateChildOceans();
    }

    private void FixedUpdate()
    {
        for (int x = 0; x <= dimension; x++)
        {
            for (int z = 0; z <= dimension; z++)
            {
                float y = CalculateHeight(x, z);

                // For smooth transition between the meshes
                if (x > dimension - 3)
                {
                    float ratio = (dimension - x + 2) * 0.2f;
                    y = (verts[GetIndex(dimension - x, z)].y * (1 - ratio) + y * ratio);
                    verts[GetIndex(dimension - x, z)].y = (verts[GetIndex(dimension - x, z)].y * ratio + y * (1 - ratio));
                }
                if (z > dimension - 3)
                {
                    float ratio = (dimension - z + 2) * 0.2f;
                    y = (verts[GetIndex(x, dimension - z)].y * (1 - ratio) + y * ratio);
                    verts[GetIndex(x, dimension - z)].y = (verts[GetIndex(x, dimension - z)].y * ratio + y * (1 - ratio));
                }

                if (x == dimension)
                {
                    verts[GetIndex(0, z)].y = y;
                }
                if (z == dimension)
                {
                    verts[GetIndex(x, 0)].y = y;
                }

                verts[GetIndex(x, z)] = new Vector3(x, y, z);
            }
        }

        sharedMesh.vertices = verts;
        sharedMesh.RecalculateNormals();
    }

    private void CreateChildOceans()
    {
        float offset = (float)dimension / 2;

        for (int x = 0; x < childrenCounts.x; x++)
        {
            for (int z = 0; z < childrenCounts.y; z++)
            {
                // Create the parent GameObject
                GameObject parent = new GameObject("OceanParent");
                parent.transform.parent = transform;
                parent.transform.position = new Vector3(
                    transform.position.x + x * dimension,
                    transform.position.y,
                    transform.position.z + z * dimension
                ) + new Vector3(dimension / 2, 0, dimension / 2);

                // Create the child GameObject
                GameObject child = new GameObject("OceanChild");
                child.transform.parent = parent.transform;
                child.transform.localPosition = new Vector3(-offset, 0, -offset);
                child.tag = tag;

                // Add and set up the OceanChild component
                OceanChild oceanChild = child.AddComponent<OceanChild>();
                oceanChild.Initialize(ref sharedMesh, sharedMaterial);
                children.Add(oceanChild);
            }
        }
    }

    private void SetupMesh()
    {
        if (!GetComponent<MeshRenderer>())
            gameObject.AddComponent<MeshRenderer>();

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer.sharedMaterial == null)
        {
            renderer.sharedMaterial = new Material(Shader.Find("Standard"));
        }
        sharedMaterial = renderer.sharedMaterial;

        sharedMesh = new Mesh();
        sharedMesh.vertices = GenerateVerts();
        sharedMesh.triangles = GenerateTries();
        sharedMesh.uv = GenerateUVs();
        sharedMesh.RecalculateBounds();
        sharedMesh.RecalculateNormals();

       verts = sharedMesh.vertices;

        gameObject.AddComponent<MeshFilter>().sharedMesh = sharedMesh;
    }

    private Vector3[] GenerateVerts()
    {
        Vector3[] verts = new Vector3[(dimension + 1) * (dimension + 1)];
        for (int x = 0; x < dimension + 1; x++)
        {
            for (int z = 0; z < dimension + 1; z++)
            {
                verts[x * (dimension + 1) + z] = new Vector3(x, 0, z);
            }
        }
        return verts;
    }

    private int GetIndex(int x, int z)
    {
        return x * (dimension + 1) + z;
    }

    private int[] GenerateTries()
    {
        int[] tries = new int[dimension * dimension * 6];
        for (int x = 0; x < dimension; x++)
        {
            for (int z = 0; z < dimension; z++)
            {
                int index = (x * dimension + z) * 6;
                tries[index + 0] = GetIndex(x, z);
                tries[index + 1] = GetIndex(x + 1, z + 1);
                tries[index + 2] = GetIndex(x + 1, z);
                tries[index + 3] = GetIndex(x, z);
                tries[index + 4] = GetIndex(x, z + 1);
                tries[index + 5] = GetIndex(x + 1, z + 1);
            }
        }
        return tries;
    }

    private Vector2[] GenerateUVs()
    {
        Vector2[] uvs = new Vector2[(dimension + 1) * (dimension + 1)];
        for (int x = 0; x <= dimension; x++)
        {
            for (int z = 0; z <= dimension; z++)
            {
                Vector2 vector = new Vector2((x / uvScale) % 2, (z / uvScale) % 2);
                uvs[GetIndex(x, z)] = new Vector2(
                    vector.x <= 1 ? vector.x : 2 - vector.x,
                    vector.y <= 1 ? vector.y : 2 - vector.y
                );
            }
        }
        return uvs;
    }

    private float CalculateHeight(int x, int z)
    {
        float height = 0f;
        for (int o = 0; o < octaves.Length; o++)
        {
            float perlin = octaves[o].alternate
                ? Mathf.Cos(
                    Mathf.PerlinNoise((x * octaves[o].scale.x) / dimension, (z * octaves[o].scale.y) / dimension) 
                    * Mathf.PI * 2f + octaves[o].speed.magnitude * Time.time
                  ) * octaves[o].height
                : (Mathf.PerlinNoise(
                    (x * octaves[o].scale.x + Time.time * octaves[o].speed.x) / dimension,
                    (z * octaves[o].scale.y + Time.time * octaves[o].speed.y) / dimension
                  ) - 0.5f) * octaves[o].height;
            height += perlin;
        }
        return height;
    }
}
