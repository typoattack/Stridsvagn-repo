using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour
{
    public int HP;
    private float initialHP;
    public GameObject explosion;

    public static int numCrates = 0;

    // Start is called before the first frame update
    void Start()
    {
        initialHP = HP;
        numCrates += 1;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("EnemyBullet"))
        {
            Destroy(collision.gameObject);

            if (HP > 1) HP--;
            else
            {
                numCrates -= 1;
                HP = 0;
                GameObject exp = Instantiate(explosion, transform.position, transform.rotation) as GameObject;
                exp.transform.parent = null;

                Destroy(GetComponent<BoxCollider2D>());

                this.transform.GetChild(0).gameObject.SetActive(false);
                this.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }
}
