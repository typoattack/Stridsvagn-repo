using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundaboutWallSpawner : MonoBehaviour
{
    private GameController gameController;

    public GameObject wall;
    public GameObject wallDestructible;

    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;
    public float xOuterMin;
    public float xOuterMax;
    public float yOuterMin;
    public float yOuterMax;
    public float xInnerMin;
    public float xInnerMax;
    public float yInnerMin;
    public float yInnerMax;

    public int backgroundMin;
    public int backgroundMax;

    private int decider;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();

        xMin = gameController.xMin;
        xMax = gameController.xMax;
        yMin = gameController.yMin;
        yMax = gameController.yMax;
        xOuterMin = gameController.xMinWalls;
        xOuterMax = gameController.xMaxWalls;
        yOuterMin = gameController.yMinWalls;
        yOuterMax = gameController.yMaxWalls;

        SpawnTargets();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnTargets()
    {
        for (int x = (int)xOuterMin; x <= (int)xOuterMax; x++)
        {
            for (int y = (int)yOuterMin; y <= (int)yOuterMax; y++)
            {
                if ((x >= xInnerMin && x <= xInnerMax && y >= yInnerMin && y <= yInnerMax)
                    || x % 5 == 0 || y % 5 == 0) continue;
                spawnWallSegment(x, y);
            }
        }
        
        for (int x = -17; x <= 17; x++)
        {
            for (int y = (int)yMin - 4; y <= (int)yOuterMin; y++)
            {
                if (x % 5 == 0 || y % 5 == 0) continue;
                else spawnWallSegment(x, y);
            }
            for (int y = (int)yOuterMax; y <= (int)yMax + 4; y++)
            {
                if (x % 5 == 0 || y % 5 == 0) continue;
                else spawnWallSegment(x, y);
            }
        }

        for (int y = -17; y<= 17; y++)
        {
            for (int x = (int)xMin - 4; x <= (int)xOuterMin; x++)
            {
                if (x % 5 == 0 || y % 5 == 0) continue;
                else spawnWallSegment(x, y);
            }
            for (int x = (int)xOuterMax; x <= (int)xMax + 4; x++)
            {
                if (x % 5 == 0 || y % 5 == 0) continue;
                else spawnWallSegment(x, y);
            }
        }
    }

    void spawnWallSegment(int x, int y)
    {
        decider = Random.Range(0, 10);
        Vector2 pos = new Vector2(x, y);
        if (decider >= 9)
        {
            decider = Random.Range(0, 4);
            if (decider == 0) Instantiate(wall, pos, transform.rotation);
            else Instantiate(wallDestructible, pos, transform.rotation);
        }
    }
}
