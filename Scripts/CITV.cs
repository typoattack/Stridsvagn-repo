using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CITV : MonoBehaviour
{
    private float maxDetectionDistance;
    private LineRenderer citvLaser;
    private RaycastHit2D bogey;
    private Rigidbody2D rb2D;
    private bool activateCITV;

    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        maxDetectionDistance = 100;
        citvLaser = GetComponent<LineRenderer>();
        activateCITV = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("CITV1") || Input.GetButtonDown("CITV2") /*&& activateCITV == false*/)
        {
            activateCITV = !activateCITV;
            //StartCoroutine(citvTimer());
        }
    }

    private void FixedUpdate()
    {
        RaycastHit2D[] detection = Physics2D.CircleCastAll(transform.position, maxDetectionDistance, transform.forward, 0.0f, 1 << 8);
        float closestDistanceSqr = Mathf.Infinity;

        foreach (RaycastHit2D blip in detection)
        {
            Vector2 directionToTarget = blip.transform.position - transform.position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bogey = blip;
            }
        }

        if (bogey)
        {
            float angle = AngleBetweenPoints(rb2D.position, bogey.transform.position);
            Vector2 directionToBogey = bogey.transform.position - transform.position;
            float dSqrToBogey = directionToBogey.sqrMagnitude;
            if (dSqrToBogey <= Mathf.Pow(maxDetectionDistance, 2)) rb2D.rotation = angle + 90f;
        }
        else transform.localRotation = Quaternion.Euler(0, 0, 0);

        RaycastHit2D sight = Physics2D.Raycast(transform.position, transform.up, maxDetectionDistance, 1 << 8);
        if (sight.collider != null && activateCITV == true)
        {
            //Debug.Log(sight.collider.name);
            citvLaser.enabled = true;
            Vector3[] positions = new Vector3[2] { transform.position, /*(Vector3)sight.point*/ transform.position + Vector3.Normalize((Vector3)sight.point - transform.position) * 5 };
            citvLaser.positionCount = 2;
            citvLaser.startWidth = 0.025f;
            citvLaser.endWidth = 0.5f;
            citvLaser.SetPositions(positions);
        }
        else
        {
            citvLaser.enabled = false;
        }
    }

    private float AngleBetweenPoints(Vector2 a, Vector2 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    IEnumerator citvTimer()
    {
        yield return new WaitForSeconds(2.0f);
        activateCITV = false;
    }
}
