using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundaboutBackgroundSpawner : MonoBehaviour
{
    public GameObject backgroundObject;
    private GameController gameController;

    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;
    public float interval;
    public bool gameControllerLimits;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();

        if (gameControllerLimits)
        {
            xMin = gameController.xMin;
            xMax = gameController.xMax;
            yMin = gameController.yMin;
            yMax = gameController.yMax;
        }

        for (float i = xMin; i <= xMax; i += interval)
        {
            for (float j = yMin; j <= yMax; j += interval)
            {
                Vector2 pos = new Vector2(i, j);
                Instantiate(backgroundObject, pos, transform.rotation, transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
