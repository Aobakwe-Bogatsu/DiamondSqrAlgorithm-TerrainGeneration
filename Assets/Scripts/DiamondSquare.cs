using System.Collections;
using System.Collections.Generic;
using UnityEngine; // MonoBehaviour, TerrainCollider, TerrainData

/// <summary>          *line 3    
/// When attached to a GameObject,
/// it can be used to randomize the heights
/// using the Diamond-Square algorithme
/// </summary>
[RequireComponent(typeof(TerrainCollider))]
public class DiamondSquare : MonoBehaviour
{
    // Data container of heights of a terrain
    public TerrainData data;
    // Size of the sides of a terrain
    public int size;                  
    /// </summary>
    // 2D array of heights
    private float[,] heights;
    // Control variable to determine smoothness of heights
    public float rangeReductionValue = 0.55f;

    public GameObject food; //**
    public int maxFood = 50; //
    public float spawnInterval = 2f; //

    public int foodCount = 0; // counts the number of food being spawned

   private float timer = 0f; //

    /// <summary>
    /// Used for initialization
    /// </summary>
    private void Awake()
    {
        data = transform.GetComponent<TerrainCollider>().terrainData;
        size = data.heightmapResolution;   //* what is height map resolution

        ExecuteDiamondSquare();   //***
        Reset();                  //***

        //Initialise the timer
        timer = 0f;  //

        return;
    }


    // Update is called once per frame
    void Update()
    {
        // food count tracks the number of food items spawning
        if (foodCount < maxFood) // checks if the food count is smaller than the maximum food items to be spawned (50)
        {
            //** Check the timer to control food spawning intervals
            timer += Time.deltaTime;
            if (timer >= spawnInterval) // if the timer has reached the time interval for spawning (2 sec) 
            {
                //Spawn 10 food items at a time (itterate 10 food items)
                for (int i = 0; i < 10; i++)
                {
                    if (foodCount < maxFood)
                    { // assign the spawn pos.              x- length               y- height    z- length       of the terrain
                        Vector3 spawnPosition = new Vector3(Random.Range(0f, 100f), 100, Random.Range(0f, 100f));
                        Instantiate(food, spawnPosition, Quaternion.identity);
                        foodCount++;
                    }
                    else
                    {
                        Debug.Log("MAX FOOD SPAWNED"); //**
                        break; // Stop spawning if the maximum limit is reached
                    }
                }
                // Reset the timer
                timer = 0f;
            }
        }
        else
        {
            Debug.Log("MAX FOOD SPAWNED");
        }



        if (Input.GetKeyDown(KeyCode.Space))   //* for new terrain to be generated every time you push spacebar key.
        {
            ExecuteDiamondSquare();   //***
        }

    }

    /// <summary>
    /// Resets the value of the terrain. If randomizeCornerValue is true then the
    /// corner heights will be randomized, else it will be flat.
    /// </summary>
    public void Reset()            // ** The diamond-step generates random heights to the terrain
    {
        heights = new float[size, size];

        //first step of DS algorithm                ****************************
        heights[0, 0] = Random.value; // top left        (1. Assign random values to the corners of the terrain)
        heights[size - 1, 0] = Random.value; // top right
        heights[0, size - 1] = Random.value; // bottom left
        heights[size - 1, size - 1] = Random.value;    //bottom right

        // Update the terrain data
        data.SetHeights(0, 0, heights);

        return;

    }

    /// <summary>
    /// Executes the Diamond Square algorithm on the terrain.             *******What exactly is DS algorithm & its function
    /// </summary>
    public void ExecuteDiamondSquare()
    {
        heights = new float[size, size];
        float average, range = 0.5f;
        int sideLength, halfSide, x, y;

        // While the side length is greater than 1
        // and reduced the side length by half each itteration
        for (sideLength = size - 1; sideLength > 1; sideLength /= 2)
        {
            halfSide = sideLength / 2;

            // Run the Diamond Step
            for (x = 0; x < size - 1; x += sideLength)   //adding the side length takes you to the opposite corner of the current sqr on the X-axis 
            {
                for (y = 0; y < size - 1; y += sideLength)  //adding the side length takes you to the opposite corner of the current sqr on the Y-axis
                {
                    // Get the averages of the corners
                    average = heights[x, y]; // top left ( 2. get the average of the corners)
                    average += heights[x + sideLength, y]; // top right 
                    average += heights[x, y + sideLength]; // bottom left
                    average += heights[x + sideLength, y + sideLength]; // bottom right 
                    average /= 4.0f; // there are 4 corners, so divide by 4 to get the average            ***************** NOTE the pattern :)

                    // 3. add a random value to the average
                    average += (Random.value * (range * 2.0f)) - range;  
                    //4. put this average in the middle of the terrain
                    heights[x + halfSide, y + halfSide] = average;
                }
            } //diamond step done

            // Run Square Step
            for (x = 0; x < size - 1; x += halfSide) //add half the length of a side to get the middle 
            {
                for (y = (x + halfSide) % sideLength; y < size - 1; y += sideLength)
                {
                    // Get the average of the corner
                    average = heights[(x - halfSide + size - 1) % (size - 1), y]; //top point of diamond
                    average += heights[(x + halfSide) % (size - 1), y];
                    average += heights[x, (y + halfSide) % (size - 1)];
                    average += heights[x, (y - halfSide + size - 1) % (size - 1)];
                    average /= 4.0f;         //* what does '/=' mean

                    // Offset by a random value
                    average += (Random.value * (range * 2.0f)) - range;

                    // Set the height value to be the calculated average
                    heights[x, y] = average;

                    // Set the height on the opposite edge if this is an edge piece
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
            //**** phone switche off by here
            // Lower the random value range
            range -= range * rangeReductionValue;
        }

        // Update the terrain heights
        data.SetHeights(0, 0, heights);
        return;
    }
}


