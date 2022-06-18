using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour
{
    //public GameObject thud;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("EnemyBullet"))
        {
            //GameObject ric = Instantiate(thud, other.transform, false) as GameObject;
            Destroy(collision.gameObject);
        }
    }
}
