using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GauntletBackgroundSpawner : MonoBehaviour
{
    public GameObject backgroundObject;
    private GameController gameController;

    public float x;
    public float xOffset;
    public float yMin;
    public float yMax;
    public float interval;
    public bool gameControllerYMax;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        if (gameControllerYMax) yMax = gameController.yMax;

        float i = yMin;

        while (i * interval <= yMax)
        {
            float xRight = x + xOffset;
            float xLeft = -x + xOffset;
            float y = i * interval;
            Vector2 posLeft = new Vector2(xLeft, y);
            Vector2 posRight = new Vector2(xRight, y);
            Instantiate(backgroundObject, posLeft, transform.rotation, transform);
            Instantiate(backgroundObject, posRight, transform.rotation, transform);
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}