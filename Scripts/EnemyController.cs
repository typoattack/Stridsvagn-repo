using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class EnemyController : MonoBehaviour {

    public GameObject shot;
    public GameObject player;
    public Transform shotSpawn;
    public AudioClip shotSoundClose;
    public AudioClip shotSoundFar;
    public bool isTripleShot;

    private Rigidbody2D rb2D;
    private float fireRate;
    private float spread;
    private AudioSource audio;

    private float distanceToPlayer;
    public bool canFire;
    private float maxDetectionDistance;
    private RaycastHit2D bogey;

    private Animator muzzleAnimator;

    private void Start()
    {
        fireRate = Random.Range(2.5f, 5.0f);
        rb2D = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Tank");
        audio = GetComponent<AudioSource>();
        InvokeRepeating("Shoot", fireRate, fireRate);
        distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);
        canFire = false;
        if (!PlayerPrefs.HasKey("detectDistance"))
        {
            PlayerPrefs.SetFloat("detectDistance", 14.0f);
        }
        else
        {
            maxDetectionDistance = PlayerPrefs.GetFloat("detectDistance");
        }
        muzzleAnimator = this.transform.GetChild(2).GetChild(0).GetComponent<Animator>();
        GameController.enemyCount++;
    }

    private void Update()
    {
        //Mouse Position in the world. It's important to give it some distance from the camera. 
        //If the screen point is calculated right from the exact position of the camera, then it will
        //just return the exact same position as the camera, which is no good.
        //Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10f);

        //Angle between mouse and this object
        //float angle = AngleBetweenPoints(rb2D.position, player.transform.position);

        //spread = Random.Range(-TankController.enemyCount - 3.0f, TankController.enemyCount + 3.0f);
        //shotSpawn.localRotation = Quaternion.Euler(0, 0, spread);

        //Ta daa
        //rb2D.rotation = angle + 90f;

        distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);
    }

    private void FixedUpdate()
    {
        spread = Random.Range(-GameController.enemyCount - 3.0f, GameController.enemyCount + 3.0f);
        shotSpawn.localRotation = Quaternion.Euler(0, 0, spread);

        RaycastHit2D[] detection = Physics2D.CircleCastAll(transform.position, maxDetectionDistance, transform.forward, 0.0f, 1 << 6);
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

        RaycastHit2D sight = Physics2D.Raycast(shotSpawn.position, transform.up, maxDetectionDistance -1.0f, 9 << 6);
        if (sight.collider != null && sight.collider.tag == "Tank") canFire = true;
        else canFire = false;
    }

    private float AngleBetweenPoints(Vector2 a, Vector2 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    private void Shoot()
    {
        if (canFire)
        {
            if (distanceToPlayer <= maxDetectionDistance / 2.0f)
            {
                audio.PlayOneShot(shotSoundClose, 1.0f);
            }
            else
            {
                audio.PlayOneShot(shotSoundFar, 1.0f);
            }

            GameObject shell = Instantiate(shot, shotSpawn.transform, false) as GameObject;
            shell.transform.parent = null;

            if (isTripleShot)
            {
                GameObject shell2 = Instantiate(shot, shotSpawn.transform, false) as GameObject;
                GameObject shell3 = Instantiate(shot, shotSpawn.transform, false) as GameObject;
                shell2.transform.localRotation = Quaternion.Euler(0f, 0f, 10f);
                shell3.transform.localRotation = Quaternion.Euler(0f, 0f, -10f);
                shell2.transform.parent = null;
                shell3.transform.parent = null;
            }
            muzzleAnimator.SetTrigger("Fire");
        }
    }
}
