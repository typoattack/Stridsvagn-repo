using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTurretReposition : MonoBehaviour
{
    public float positionX;
    public float positionY;
    public bool turretCanRotate;

    public TankTurretController turret;

    // Start is called before the first frame update
    void OnEnable()
    {
        turret = GameObject.Find("TankTurret").GetComponent<TankTurretController>();
        turret.positionX = positionX;
        turret.positionY = positionY;
        turret.canRotate = turretCanRotate;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
