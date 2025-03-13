using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TerrainCreator : MonoBehaviour
{
    [SerializeField] private List<Terrain> terrainList;
    public int terrainWidth = 1000;
    public int terrainHeight = 1000;
    public Material terrainMaterial;

    // Heightmap resolution
    public int pixWidth = 256;
    public int pixHeight = 256;

    private float[,] heightmap;
    [SerializeField] private List<Vector2> scales;

    void Start()
    {
        heightmap = new float[pixWidth, pixHeight];
        terrainList = GetComponentsInChildren<Terrain>().ToList();

        foreach (Terrain terrain in terrainList)
        {
            float xOffset = (terrain.transform.position.x / terrainWidth * pixWidth) - (terrain.transform.position.x / terrainWidth);
            float yOffset = (terrain.transform.position.z / terrainHeight * pixHeight) - (terrain.transform.position.z / terrainHeight);
            
            CalculatePerlinNoise(xOffset, yOffset);
            terrain.terrainData.SetHeights(0, 0, heightmap);
        }
    }

    private void CalculatePerlinNoise(float xOffset, float yOffset)
    {
        float y = 0;
        for (int j = 0; j < pixHeight; j++)
        {
            float x = 0;
            for (int i = 0; i < pixWidth; i++)
            {
                heightmap[j, i] = 0;

                foreach (Vector2 scale in scales)
                {
                    float sample = GetPerlinNoise(xOffset, yOffset, x, y, scale.x);
                    heightmap[j, i] += sample / scale.y;
                }

                x++;
            }
            y++;
        }
    }

    private float GetPerlinNoise(float xOffset, float yOffset, float x, float y, float scale)
    {
        float xCoord = (xOffset + x) / pixWidth * scale;
        float yCoord = (yOffset + y) / pixHeight * scale;
        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
