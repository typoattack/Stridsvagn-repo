using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultBackgroundSpawner : MonoBehaviour
{
    public GameObject backgroundObject;
    public GameObject backgroundObject2;
    public GameObject backgroundObject3;
    private GameController gameController;

    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;
    public float interval;
    public bool gameControllerLimits;

    //Vector3 tileEulerAngles;
    //Quaternion tileRotation;

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
                /*
                int randomAngle = Random.Range(0, 4);
                if (randomAngle == 1) tileEulerAngles = new Vector3(0, 0, 90);
                else if (randomAngle == 2) tileEulerAngles = new Vector3(0, 0, 180);
                else if (randomAngle == 3) tileEulerAngles = new Vector3(0, 0, 270);
                else tileEulerAngles = new Vector3(0, 0, 0);

                //moving the value of the Vector3 into Quanternion.eulerAngle format
                tileRotation.eulerAngles = tileEulerAngles;

                */
                Vector2 pos = new Vector2(i, j);
                int randomNumber = Random.Range(1, 4);
                if (randomNumber == 1) Instantiate(backgroundObject, pos, backgroundObject.transform.rotation, transform);
                if (randomNumber == 2) Instantiate(backgroundObject2, pos, backgroundObject2.transform.rotation, transform);
                if (randomNumber == 3) Instantiate(backgroundObject3, pos, backgroundObject3.transform.rotation, transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
