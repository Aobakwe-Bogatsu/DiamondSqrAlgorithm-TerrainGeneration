using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Randomly generates terrain heights using the Diamond-Square algorithm
/// and spawns food objects across the terrain over time.
/// </summary>
[RequireComponent(typeof(TerrainCollider))]
public class DiamondSquare : MonoBehaviour
{
    #region Terrain Settings

    // Terrain data used to modify terrain heights
    public TerrainData data;

    // Terrain heightmap resolution (number of grid points that define the terrain surface)
    public int size;

    // Stores terrain height values (2D array)
    private float[,] heights;

    // Controls smoothness of terrain heights
    [Range(0f, 1f)]
    public float rangeReductionValue = 0.55f;

    #endregion

    #region Food Spawning Settings

    // Food prefab to spawn
    public GameObject food;

    // Maximum number of food objects allowed
    public int maxFood = 50;

    // Time interval between food spawn cycles
    public float spawnInterval = 2f;

    // Number of spawned food objects
    public int foodCount = 0;

    // Tracks time between spawn cycles
    private float timer = 0f;

    #endregion

    /// <summary>
    /// Initialises terrain data and generates the first terrain.
    /// </summary>
    private void Awake()
    {
        data = GetComponent<TerrainCollider>().terrainData;
        size = data.heightmapResolution;

        ExecuteDiamondSquare();
        Reset();

        // Initialise the timer
        timer = 0f;
    }

    /// <summary>
    /// Handles food spawning and terrain regeneration input.
    /// Update() is called once per frame.
    /// </summary>
    private void Update()
    {
        // Check if the food count is smaller than the maximum allowed
        if (foodCount < maxFood)
        {
            // Track time between food spawn cycles
            timer += Time.deltaTime;

            // Spawn food when the spawn interval is reached
            if (timer >= spawnInterval)
            {
                // Spawn 10 food objects at a time
                for (int i = 0; i < 10; i++)
                {
                    if (foodCount < maxFood)
                    {
                        // Spawn food at a random position above the terrain
                        Vector3 spawnPosition = new Vector3(
                            Random.Range(0f, 100f), // x position
                            100f,                   // y position (fixed height above terrain)
                            Random.Range(0f, 100f) // z position
                        );

                        // Spawn (create a new copy of) the food prefab with no rotation applied
                        Instantiate(food, spawnPosition, Quaternion.identity);

                        foodCount++;
                    }
                    else
                    {
                        Debug.Log("MAX FOOD SPAWNED");
                        break;
                    }
                }

                // Reset the spawn timer
                timer = 0f;
            }
        }
        else
        {
            Debug.Log("MAX FOOD SPAWNED");
        }

        // Generate a new terrain when the spacebar is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteDiamondSquare();
        }
    }

    /// <summary>
    /// Resets terrain heights and initialises random corner values.
    /// </summary>
    public void Reset()
    {
        heights = new float[size, size];

        // 1. Initialise the four terrain corners with random height values
        heights[0, 0] = Random.value;                // top left corner
        heights[size - 1, 0] = Random.value;         // top right corner
        heights[0, size - 1] = Random.value;         // bottom left corner
        heights[size - 1, size - 1] = Random.value;  // bottom right corner

        // Apply the initial heights to the terrain
        data.SetHeights(0, 0, heights);
    }

    /// <summary>
    /// Executes the Diamond-Square algorithm on the terrain to generate random heights.
    /// </summary>
    public void ExecuteDiamondSquare()
    {
        heights = new float[size, size];

        float average;
        float range = 0.5f;

        int sideLength;
        int halfSide;
        int x;
        int y;

        // Reduce the working square size each iteration
        for (sideLength = size - 1; sideLength > 1; sideLength /= 2)
        {
            halfSide = sideLength / 2;

            // 2. Perform the Diamond Step
            for (x = 0; x < size - 1; x += sideLength)
            {
                for (y = 0; y < size - 1; y += sideLength)
                {
                    // Calculate the average height of the four corners
                    average = heights[x, y];
                    average += heights[x + sideLength, y];
                    average += heights[x, y + sideLength];
                    average += heights[x + sideLength, y + sideLength];

                    average /= 4.0f;

                    // Apply a random height offset
                    average += (Random.value * (range * 2.0f)) - range;

                    // Set the center point height
                    heights[x + halfSide, y + halfSide] = average;
                }
            }

            // 3. Perform the Square Step
            for (x = 0; x < size - 1; x += halfSide)
            {
                for (y = (x + halfSide) % sideLength; y < size - 1; y += sideLength)
                {
                    // Calculate the average height of surrounding points
                    average = heights[(x - halfSide + size - 1) % (size - 1), y]; // top point of diamond
                    average += heights[(x + halfSide) % (size - 1), y];            // right point of diamond
                    average += heights[x, (y + halfSide) % (size - 1)];            // bottom point of diamond
                    average += heights[x, (y - halfSide + size - 1) % (size - 1)];// left point of diamond

                    average /= 4.0f;

                    // Apply a random height offset
                    average += (Random.value * (range * 2.0f)) - range;

                    // Set the new height value
                    heights[x, y] = average;

                    // Mirror edge values to maintain seamless terrain
                    if (x == 0)
                    {
                        heights[size - 1, y] = average;
                    }

                    if (y == 0)
                    {
                        heights[x, size - 1] = average;
                    }
                }
            }

            // Gradually reduce the random value range
            range -= range * rangeReductionValue;
        }

        // Apply the generated terrain heights
        data.SetHeights(0, 0, heights);
    }
}