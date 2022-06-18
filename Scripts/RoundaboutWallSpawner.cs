using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundaboutWallSpawner : MonoBehaviour
{
    private GameController gameController;

    public GameObject wall;
    public GameObject wallDestructible;
    public GameObject backgroundTile;
    public int backgroundInterval;

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
        for (int x = backgroundMin; x <= backgroundMax; x += backgroundInterval)
        {
            for (int y = backgroundMin; y <= backgroundMax; y += backgroundInterval)
            {
                Vector2 pos = new Vector2(x, y);
                Instantiate(backgroundTile, pos, transform.rotation);
            }
        }

        for (int x = (int)xOuterMin; x <= (int)xOuterMax; x += 2)
        {
            for (int y = (int)yOuterMin; y <= (int)yOuterMax; y++)
            {
                if (x >= xInnerMin && x <= xInnerMax && y >= yInnerMin && y <= yInnerMax) continue;
                spawnWallSegment(x, y);
            }
        }

        for (int x = -17; x <= 17; x+= 2)
        {
            for (int y = (int)yMin; y <= (int)yOuterMin; y++) spawnWallSegment(x, y);
            for (int y = (int)yOuterMax; y <= (int)yMax; y++) spawnWallSegment(x, y);
        }

        for (int y = -17; y<= 17; y++)
        {
            for (int x = (int)xMin; x <= (int)xOuterMin; x += 2) spawnWallSegment(x, y);
            for (int x = (int)xOuterMax; x <= (int)xMax; x += 2) spawnWallSegment(x, y);
        }
    }

    void spawnWallSegment(int x, int y)
    {
        decider = Random.Range(0, 10);
        Vector2 pos = new Vector2(x, y);
        if (decider >= 9)
        {
            decider = Random.Range(0, 2);
            if (decider == 0) Instantiate(wall, pos, transform.rotation);
            else Instantiate(wallDestructible, pos, transform.rotation);
        }
    }
}
