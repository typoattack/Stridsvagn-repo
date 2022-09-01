using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundaboutBackgroundSpawner : MonoBehaviour
{
    public GameObject roadSidewalkTile1;
    public GameObject roadSidewalkTile2;
    public GameObject roadSidewalkTile3;
    public GameObject roadSidewalkTile4;
    public GameObject roadIntersectionTile;
    public GameObject concreteTile1;
    public GameObject concreteTile2;
    public GameObject concreteTile3;
    private GameController gameController;

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
    public float interval;
    public bool gameControllerLimits;

    //Vector3 tileEulerAngles;
    //Quaternion tileRotation;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        /*
        if (gameControllerLimits)
        {
            xMin = gameController.xMin;
            xMax = gameController.xMax;
            yMin = gameController.yMin;
            yMax = gameController.yMax;
        }
        */

        xMin = gameController.xMin;
        xMax = gameController.xMax;
        yMin = gameController.yMin;
        yMax = gameController.yMax;
        xOuterMin = gameController.xMinWalls;
        xOuterMax = gameController.xMaxWalls;
        yOuterMin = gameController.yMinWalls;
        yOuterMax = gameController.yMaxWalls;

        for (int x = (int)xOuterMin; x < (int)xOuterMax; x++)
        {
            for (int y = (int)yOuterMin; y < (int)yOuterMax; y++)
            {
                Vector2 pos = new Vector2(x, y);
                if (x >= xInnerMin && x <= xInnerMax && y >= yInnerMin && y <= yInnerMax)
                {
                    spawnConcrete(pos);
                }
                else if (x % 5 == 0 || y % 5 == 0) spawnRoad(x, y);
                else spawnConcrete(pos);

            }
        }

        for (int x = -39; x <= 39; x++)
        {
            for (int y = (int)yMin - 4; y < (int)yOuterMin; y++)
            {
                Vector2 pos = new Vector2(x, y);
                if (x % 5 == 0 || y % 5 == 0) spawnRoad(x, y);
                else spawnConcrete(pos);
            }
            for (int y = (int)yOuterMax; y <= (int)yMax + 4; y++)
            {
                Vector2 pos = new Vector2(x, y);
                if (x % 5 == 0 || y % 5 == 0) spawnRoad(x, y);
                else spawnConcrete(pos);
            }
        }

        for (int y = -18; y <= 18; y++)
        {
            for (int x = (int)xMin - 4; x < (int)xOuterMin; x++)
            {
                Vector2 pos = new Vector2(x, y);
                if (x % 5 == 0 || y % 5 == 0) spawnRoad(x, y);
                else spawnConcrete(pos);
            }
            for (int x = (int)xOuterMax; x <= (int)xMax + 4; x++)
            {
                Vector2 pos = new Vector2(x, y);
                if (x % 5 == 0 || y % 5 == 0) spawnRoad(x, y);
                else spawnConcrete(pos);
            }
        }
        /*
        for (float i = xMin; i <= xMax; i += interval)
        {
            for (float j = yMin; j <= yMax; j += interval)
            {
                
                int randomAngle = Random.Range(0, 4);
                if (randomAngle == 1) tileEulerAngles = new Vector3(0, 0, 90);
                else if (randomAngle == 2) tileEulerAngles = new Vector3(0, 0, 180);
                else if (randomAngle == 3) tileEulerAngles = new Vector3(0, 0, 270);
                else tileEulerAngles = new Vector3(0, 0, 0);

                //moving the value of the Vector3 into Quanternion.eulerAngle format
                tileRotation.eulerAngles = tileEulerAngles;

                
                Vector2 pos = new Vector2(i, j);
                int randomNumber = Random.Range(1, 4);
                if (randomNumber == 1) Instantiate(roadSidewalkTile, pos, roadSidewalkTile.transform.rotation, transform);
                if (randomNumber == 2) Instantiate(concreteTile1, pos, concreteTile1.transform.rotation, transform);
                if (randomNumber == 3) Instantiate(concreteTile2, pos, concreteTile2.transform.rotation, transform);
                
                
            }
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void spawnRoad(int x, int y)
    {
        Vector2 pos = new Vector2(x, y);
        if (x % 5 == 0 && y % 5 == 0) Instantiate(roadIntersectionTile, pos, roadIntersectionTile.transform.rotation, transform);
        else if (x % 5 == 0)
        {
            if (y % 2 == 0) Instantiate(roadSidewalkTile2, pos, roadSidewalkTile2.transform.rotation, transform);
            else Instantiate(roadSidewalkTile1, pos, roadSidewalkTile1.transform.rotation, transform);
        }
        else
        {
            if (x % 2 == 0) Instantiate(roadSidewalkTile4, pos, roadSidewalkTile4.transform.rotation, transform);
            else Instantiate(roadSidewalkTile3, pos, roadSidewalkTile3.transform.rotation, transform);
        }
    }

    void spawnConcrete(Vector2 pos)
    {
        int randomNumber = Random.Range(1, 4);
        if (randomNumber == 1) Instantiate(concreteTile1, pos, concreteTile1.transform.rotation, transform);
        if (randomNumber == 2) Instantiate(concreteTile2, pos, concreteTile2.transform.rotation, transform);
        if (randomNumber == 3) Instantiate(concreteTile3, pos, concreteTile3.transform.rotation, transform);
    }
}
