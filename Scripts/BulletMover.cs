using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMover : MonoBehaviour
{
    public float speed;
    private Rigidbody2D rb2D;
    public bool isEnemy;
    public bool canReflect;
    public GameObject ricochet;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        rb2D.velocity = transform.up * speed;
    }

    private void FixedUpdate()
    {
        RaycastHit2D reflector = Physics2D.CircleCast(transform.position, 0.2f, transform.forward, 0.0f, 1 << 10);
        if (reflector.collider != null && canReflect)
        {
            if ((!isEnemy && reflector.collider.tag == "EnemyShield") ||
                (isEnemy && reflector.collider.tag == "FriendlyShield")) calculateReflection(reflector);
        }
    }

    private float AngleBetweenPoints(Vector2 a, Vector2 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Boundary"))
        {
            Destroy(gameObject);
        }
    }

    private void calculateReflection(RaycastHit2D reflector)
    {
        rb2D.velocity = Vector2.Reflect(rb2D.velocity, reflector.normal);
        gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }
}
