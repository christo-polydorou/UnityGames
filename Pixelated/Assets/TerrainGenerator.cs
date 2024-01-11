using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Tile Sprites")]

    public Sprite dirt;
    public Sprite grass;
    public Sprite stone;
    public Sprite log;
    public Sprite logBase;
    public Sprite leaf;

    [Header("Trees")]
    public int treeChance = 10;
    public int minTreeHeight = 3;
    public int maxTreeHeight = 6;

    [Header("Generation settings")]
    public int chunkSize = 16;
    public int dirtLayerHeight = 5;
    public int grassLayerHeight = 1;
    public bool generateCaves = false;
    public float surfaceValue = 0.5f;
    public int worldWidth = 100;
    public int worldHeight = 70;
    public float heightMultiplier = 4f;
    public int heightAddition = 25;

    [Header("Noise settings")]
    public float caveFreq = 0.05f;
    public float terrainFreq = 0.05f;
    public float seed;
    public Texture2D noiseTexture;

    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();


    private void Start()
    {
        seed = Random.Range(-10000, 10000);
        GenerateNoiseTexture();
        CreateChunks();
        GenerateTerrain();
    }

    public void CreateChunks()
    {
        int worldSize = worldWidth;
        int numChunks = worldSize / chunkSize;
        worldChunks = new GameObject[numChunks];

        for (int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = i.ToString();
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }

    public void GenerateTerrain()
    {

        for (int x = 0; x < worldWidth; x++)
        {
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;

            for (int y = 0; y < height; y++)
            {

                Sprite tileSprite;
                if (y < height - dirtLayerHeight)
                {
                    tileSprite = stone;
                }

                else if (y < height - 1)
                {
                    tileSprite = dirt;
                }
                else
                {
                    tileSprite = grass;

                }

                if (generateCaves)
                {
                    if (noiseTexture.GetPixel(x, y).r > surfaceValue)
                    {
                        PlaceTile(tileSprite, x, y);
                    }
                }
                else
                {
                    PlaceTile(tileSprite, x, y);
                }

                if (y >= height - 1)
                {
                    int t = Random.Range(0, treeChance);
                    if (t == 1)
                    {
                        if (worldTiles.Contains(new Vector2(x, y)))
                        {
                            GenerateTree(x, y + 1);
                        }
                    }
                }
            }

            //for (int y = worldHeight; y >= height; y--)
            //{
            //    Sprite tileSprite = sky;
            //    PlaceTile(tileSprite, x, y);
            //}
        }
    }

    public void GenerateTree(int x, int y)
    {
        int treeHeight = Random.Range(minTreeHeight, maxTreeHeight);

        // Generate Logs
        PlaceTile(log, x, y);
        for (int i = 1; i <= treeHeight; i++)
        {
            PlaceTile(log, x, y + i);
        }

        // Generate leaves
        PlaceTile(leaf, x, y + treeHeight);
        PlaceTile(leaf, x, y + treeHeight + 1);
        PlaceTile(leaf, x , y + treeHeight + 2);
        PlaceTile(leaf, x - 1, y + treeHeight);
        PlaceTile(leaf, x + 1, y + treeHeight);
        PlaceTile(leaf, x + 1, y + treeHeight + 1);
        PlaceTile(leaf, x -1, y + treeHeight + 1);

    }

    public void GenerateNoiseTexture()
    {
        noiseTexture = new Texture2D(worldWidth, worldHeight);

        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                noiseTexture.SetPixel(x, y, new Color(v, v, v));
            }
        }

        noiseTexture.Apply();
    }

    public void PlaceTile(Sprite tileSprite, int x, int y)
    {
        GameObject newTile = new GameObject();

        float chunkCoord = Mathf.Round(x / chunkSize) * chunkSize;
        chunkCoord /= chunkSize;
        newTile.transform.parent = worldChunks[(int)chunkCoord].transform;

        newTile.AddComponent<SpriteRenderer>();
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprite;
        newTile.name = tileSprite.name;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

        worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f));
    }

}
