using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class World : MonoBehaviour
{
    public int worldSizeX;
    public int worldSizeY;
    public int worldSizeZ;
    
    private int chunkSize = 16; // Assuming chunk size is 16x16x16

    private Dictionary<Vector3, Chunk> chunks;

    public static World Instance { get; private set; }

    public Material VoxelMaterial;
    public float[,,] voxelData;
    private float[,,] chunkDensities;
    public float renderThreshold = 0.5f;

    public TMP_Text loading;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: if you want this to persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator LoadVoxelData() {
        voxelData = VolumeDataReader.ReadSkullData();
        Debug.Log("Read Data");
        chunks = new Dictionary<Vector3, Chunk>();
        worldSizeX = (int) Mathf.Ceil(256 / chunkSize);
        worldSizeY = (int) Mathf.Ceil(113 / chunkSize);
        worldSizeZ = (int) Mathf.Ceil(256 / chunkSize);
        GenerateWorld();
        transform.localScale = new Vector3(1, -2, 1);
        Debug.Log("Generated World");
        yield return null;

    }



    void Start()
    {
        StartCoroutine(LoadVoxelData());
    }

    public Chunk GetChunkAt(Vector3 globalPosition)
    {
        // Calculate the chunk's starting position based on the global position
        Vector3Int chunkCoordinates = new Vector3Int(
            Mathf.FloorToInt(globalPosition.x / chunkSize) * chunkSize,
            Mathf.FloorToInt(globalPosition.y / chunkSize) * chunkSize,
            Mathf.FloorToInt(globalPosition.z / chunkSize) * chunkSize
        );

        // Retrieve and return the chunk at the calculated position
        if (chunks.TryGetValue(chunkCoordinates, out Chunk chunk))
        {
            return chunk;
        }

        // Return null if no chunk exists at the position
        return null;
    }

    private void GenerateWorld()
    {
        chunkDensities = new float[worldSizeX, worldSizeY, worldSizeZ];
        float totalDensity = 0;
        Vector3 offset = Vector3.zero;
        for (ushort x = 0; x < worldSizeX; x++)
        {
            for (ushort y = 0; y < worldSizeY; y++)
            {
                for (ushort z = 0; z < worldSizeZ; z++)
                {
                    Vector3 chunkPosition = new Vector3((x - worldSizeX / 2) * chunkSize, 
                        (y - worldSizeY / 2) * chunkSize, (z - worldSizeZ / 2) * chunkSize);
                    GameObject newChunkObject = new GameObject($"Chunk_{x}_{y}_{z}");
                    newChunkObject.transform.position = chunkPosition;
                    newChunkObject.transform.parent = this.transform;

                    Chunk newChunk = newChunkObject.AddComponent<Chunk>();
                    ushort[] xBounds = new ushort[]{(ushort) (x * 16), (ushort) ((x + 1) * 16)};
                    ushort[] yBounds = new ushort[]{(ushort) (y * 16), (ushort) ((y + 1) * 16)};
                    ushort[] zBounds = new ushort[]{(ushort) (z * 16), (ushort) Mathf.Min(112, (z + 1) * 16)};
                    
                    float density = newChunk.Initialize(chunkSize, xBounds, yBounds, zBounds);
                    chunks.Add(chunkPosition, newChunk);
                    chunkDensities[x, y, z] = density;
                    totalDensity += chunkDensities[x, y, z];
                }
            }
        }

        // Normalize chunk densities and calculate offset
        for (ushort x = 0; x < worldSizeX; x++)
        {
            for (ushort y = 0; y < worldSizeY; y++)
            {
                for (ushort z = 0; z < worldSizeZ; z++)
                {
                    Vector3 chunkPosition = new Vector3((x - worldSizeX / 2) * chunkSize, 
                        (y - worldSizeY / 2) * chunkSize, (z - worldSizeZ / 2) * chunkSize);
                    chunkDensities[x, y, z] /= totalDensity;
                    offset -= chunkPosition * chunkDensities[x, y, z];
                }
            }
        }

        // Apply offset
        for (ushort x = 0; x < worldSizeX; x++)
        {
            for (ushort y = 0; y < worldSizeY; y++)
            {
                for (ushort z = 0; z < worldSizeZ; z++)
                {
                    Vector3 chunkPosition = new Vector3((x - worldSizeX / 2) * chunkSize, 
                        (y - worldSizeY / 2) * chunkSize, (z - worldSizeZ / 2) * chunkSize);
                    chunks[chunkPosition].transform.position += offset;
                }
            }
        }
    }

}