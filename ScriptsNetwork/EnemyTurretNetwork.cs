using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class EnemyTurretNetwork : NetworkBehaviour
{
    static string s_ObjectPoolTag = "ObjectPool";

    private const float k_ShootingRandomThreshold = 0.500f;
    private const float k_ShootTimer = 5.0f;

    NetworkObjectPool m_ObjectPool;
    public GameObject BulletPrefab;
    public Transform shotSpawn;
    public AudioSource audio;
    public AudioClip fireSoundClose;
    public AudioClip fireSoundFar;
    public bool isTripleShot;
    public int damage;
    public float GraceShootingPeriod = 2.5f; // A period of time in which the enemy will not shoot at the start

    private Rigidbody2D m_rb2D;
    private float fireRate;
    private float spread;
    private float distanceToPlayer;
    private float maxDetectionDistance;
    private RaycastHit2D bogey;
    private bool isAimed;

    public bool canShoot { get; set; }

    private float m_ShootTimer = 0.0f;
    private float m_FirstShootTimeAfterSpawn = 0.0f;

    private Animator muzzleAnimator;

    private void Awake()
    {
        m_rb2D = GetComponent<Rigidbody2D>();
        m_ObjectPool = GameObject.FindWithTag(s_ObjectPoolTag).GetComponent<NetworkObjectPool>();
        Assert.IsNotNull(m_ObjectPool, $"{nameof(NetworkObjectPool)} not found in scene. Did you apply the {s_ObjectPoolTag} to the GameObject?");
        canShoot = false;
        m_FirstShootTimeAfterSpawn = Single.PositiveInfinity;
        maxDetectionDistance = 14.0f;
        muzzleAnimator = this.transform.GetChild(2).GetChild(0).GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
        isAimed = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            canShoot = false;

            m_FirstShootTimeAfterSpawn =
                Time.time + Random.Range(GraceShootingPeriod - 0.1f, GraceShootingPeriod + 0.75f);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!Spawner.Singleton) return;
    }

    void Fire(Vector3 direction)
    {
        FireClientRpc();
        
        GameObject bullet = m_ObjectPool.GetNetworkObject(BulletPrefab).gameObject;
        bullet.transform.position = transform.position + direction;

        var bulletRb = bullet.GetComponent<Rigidbody2D>();
        Vector2 velocity;

        velocity = m_rb2D.velocity;
        velocity += (Vector2)(direction) * 3;
        bulletRb.velocity = velocity;
        bullet.GetComponent<EnemyBulletNetwork>().Config(this, damage, m_ObjectPool);

        bullet.GetComponent<NetworkObject>().Spawn(true);
        
    }

    private void FixedUpdate()
    {
        spread = Random.Range(-EnemyControllerNetwork.numEnemies - 3.0f, EnemyControllerNetwork.numEnemies + 3.0f);
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
            float angle = AngleBetweenPoints(m_rb2D.position, bogey.transform.position);
            Vector2 directionToBogey = bogey.transform.position - transform.position;
            float dSqrToBogey = directionToBogey.sqrMagnitude;
            if (dSqrToBogey <= Mathf.Pow(maxDetectionDistance, 2))
            {
                m_rb2D.rotation = angle + 90f;
                RotateTurretClientRpc(angle);
            }
        }
        else transform.localRotation = Quaternion.Euler(0, 0, 0);
        
        RaycastHit2D sight = Physics2D.Raycast(shotSpawn.position, transform.up, maxDetectionDistance - 1.0f, 9 << 6);
        if (sight.collider != null && sight.collider.tag == "Tank") isAimed = true;
        else isAimed = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time <= m_FirstShootTimeAfterSpawn)
        {
            // Wait for the grace shooting period to pass
            return;
        }

        bool bCanShootThisFrame = false;
        if (IsServer && canShoot)
            if (Random.Range(0, 1.0f) > k_ShootingRandomThreshold)
                bCanShootThisFrame = true;

        if (m_ShootTimer > 0)
            m_ShootTimer -= Time.deltaTime;
        else
        {
            if (!bCanShootThisFrame || !isAimed) return;
            m_ShootTimer = Random.Range(k_ShootTimer - 0.05f, k_ShootTimer + 0.25f);
            Shoot();
            return;
        }
    }

    private float AngleBetweenPoints(Vector2 a, Vector2 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    private void Shoot()
    {
        FireServerRpc();
    }

    // --- ServerRPCs ---
    [ServerRpc(RequireOwnership = false)]
    public void FireServerRpc()
    {
        var up = shotSpawn.transform.up;

        if (isTripleShot)
        {
            Fire(Quaternion.Euler(0, 0, 10) * up);
            Fire(Quaternion.Euler(0, 0, -10) * up);
            Fire(up);
        }
        else
        {
            Fire(up);
        }
    }

    // --- ClientRPCs ---
    [ClientRpc]
    public void RotateTurretClientRpc(float angle)
    {
        if (IsLocalPlayer) return;

        m_rb2D.rotation = angle + 90f;
    }

    [ClientRpc]
    public void FireClientRpc()
    {
        Vector2 directionToBogey = bogey.transform.position - transform.position;
        float dSqrToBogey = directionToBogey.sqrMagnitude;
        if (dSqrToBogey <= Mathf.Pow(maxDetectionDistance, 2) / 4.0f)
        {
            audio.PlayOneShot(fireSoundClose);
        }
        else
        {
            audio.PlayOneShot(fireSoundFar);
        }
        muzzleAnimator.SetTrigger("Fire");
    }
}
