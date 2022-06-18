using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveCircle : MonoBehaviour
{
    private Rigidbody2D rb2D;
    public Transform bumper;
    private GameController gameController;

    public float speed;
    public bool canMove;
    private float direction;

    private float xMin;
    private float xMax;
    private float yMin;
    private float yMax;
    public Vector2 rotationPoint;

    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        xMin = gameController.xMin;
        xMax = gameController.xMax;
        yMin = gameController.yMin;
        yMax = gameController.yMax;
        int decider = Random.Range(0, 2);
        if (decider == 0) direction = 0f;
        else direction = 180f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            float angle = AngleBetweenPoints(rb2D.position, rotationPoint);
            rb2D.rotation = angle + direction;

            transform.Translate(transform.up * speed * Time.deltaTime, Space.World);

            //RaycastHit2D guard = Physics2D.BoxCast(bumper.position, new Vector2(0.6f, 0.6f), 0.0f, transform.forward, 0.0f, 13 << 6);
            RaycastHit2D guard = Physics2D.CircleCast(bumper.position, 0.3f, transform.forward, 0.0f, 13 << 6);
            
            if (transform.position.x <= xMin ||
                transform.position.x >= xMax ||
                transform.position.y <= yMin ||
                transform.position.y >= yMax ||
                guard.collider != null)
            {
                transform.localRotation *= Quaternion.Euler(0, 0, 180);
                if (direction == 0f) direction = 180f;
                else direction = 0f;
            }
        }
    }

    private float AngleBetweenPoints(Vector2 a, Vector2 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }
}
