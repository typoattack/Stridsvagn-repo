using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTurretController : MonoBehaviour
{
    public Transform shotSpawn;
    public TankTurretReposition turret;

    public bool canRotate;
    public float turnSpeed = 25.0f;
    private float lts;
    private float rts;

    private LineRenderer laserSight;

    public float positionX;
    public float positionY;

    // Start is called before the first frame update
    void Start()
    {
        laserSight = GetComponent<LineRenderer>();
        
        //turret = GameObject.FindGameObjectWithTag("ActiveTurret").GetComponent<TankTurretReposition>();
        //positionX = turret.positionX;
        //positionY = turret.positionY;
        //transform.localPosition = new Vector3(positionX, positionY, 0);
        //canRotate = turret.turretCanRotate;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3(positionX, positionY, 0);
    }

    private void FixedUpdate()
    {
        lts = Input.GetAxis("RotateTurretL");
        rts = Input.GetAxis("RotateTurretR");

        float turnLeft = turnSpeed * lts;
        float turnRight = turnSpeed * rts;

        if (!canRotate) transform.localRotation = Quaternion.Euler(Vector3.forward);
        if (lts > 0f && canRotate) { transform.Rotate(Vector3.forward, turnLeft * Time.deltaTime); } //Rotate left
        if (rts > 0f && canRotate) { transform.Rotate(Vector3.forward, -turnRight * Time.deltaTime); } //Rotate right

        RaycastHit2D sight = Physics2D.Raycast(shotSpawn.position, transform.up, 7.0f, 3 << 8);
        if (sight.collider != null)
        {
            //Debug.Log(sight.collider.name);
            laserSight.enabled = true;
            Vector3[] positions = new Vector3[2] { shotSpawn.position, (Vector3)sight.point };
            laserSight.positionCount = 2;
            laserSight.startWidth = 0.025f;
            laserSight.endWidth = 0.025f;
            laserSight.SetPositions(positions);
        }
        else
        {
            laserSight.enabled = false;
        }
    }
}
