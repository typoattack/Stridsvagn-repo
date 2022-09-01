using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDestroy : MonoBehaviour
{
    private int gameMode;
    public int HP;
    private float initialHP;
    public GameObject explosion;
    public GameObject ricochet;
    public GameObject husk;
    private int killsCampaign;

    [SerializeField]
    Texture Box;

    private void Start()
    {
        initialHP = (float)HP;

        if (!PlayerPrefs.HasKey("gameMode"))
        {
            PlayerPrefs.SetInt("gameMode", 3);
        }
        else
        {
            gameMode = PlayerPrefs.GetInt("gameMode");
        }

        if (!PlayerPrefs.HasKey("killsCampaign"))
        {
            PlayerPrefs.SetInt("killsCampaign", 0);
        }
        else
        {
            killsCampaign = PlayerPrefs.GetInt("killsCampaign");
        }
    }

    private void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Mine"))
        {
            Destroy(collision.gameObject);

            if (HP > 1)
            {
                HP--;
                GameObject ric = Instantiate(ricochet, collision.transform, false) as GameObject;
                ric.transform.parent = null;
            }
            else
            {
                if (gameMode == 4)
                {
                    killsCampaign += 1;
                    PlayerPrefs.SetInt("killsCampaign", killsCampaign);
                }
                GameObject exp = Instantiate(explosion, transform.position, transform.rotation) as GameObject;
                exp.transform.parent = null;
                GameController.enemyCount--;
                GameObject remains = Instantiate(husk, transform.position, transform.rotation) as GameObject;
                remains.transform.parent = null;
                Destroy(this.transform.parent.gameObject);
            }
        }
    }

    void OnGUI()
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);

        // draw health bar background
        GUI.color = Color.grey;
        GUI.DrawTexture(new Rect(pos.x - 26, Screen.height - pos.y + 20, 52, 7), Box);

        // draw health bar amount
        if (HP > (initialHP / 2)) GUI.color = Color.green;
        if (HP <= (initialHP / 2)) GUI.color = Color.yellow;
        if (HP <= (initialHP / 4)) GUI.color = Color.red;
        GUI.DrawTexture(new Rect(pos.x - 25, Screen.height - pos.y + 21, (float)HP / initialHP * (float)50f, 5), Box);
    }
}