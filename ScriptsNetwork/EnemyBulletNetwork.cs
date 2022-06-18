using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Netcode;

public class EnemyBulletNetwork : NetworkBehaviour
{
    static string s_ObjectPoolTag = "ObjectPool";
    NetworkObjectPool m_ObjectPool;
    NetworkObjectPool m_PoolToReturn;
    Rigidbody2D m_Rigidbody2D;

    int m_Damage = 10;
    EnemyTurretNetwork m_Owner;
    public bool canReflect;

    public GameObject ricochet;

    public void Config(EnemyTurretNetwork owner, int damage, /*bool bounce,*/ NetworkObjectPool poolToReturn)
    {
        m_Owner = owner;
        m_Damage = damage;
        //m_Bounce = bounce;
        m_PoolToReturn = poolToReturn;
        /*
        if (IsServer)
        {
            // This is bad code don't use invoke.
            Invoke(nameof(DestroyBullet), lifetime);
        }
        */
    }

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_ObjectPool = GameObject.FindWithTag(s_ObjectPoolTag).GetComponent<NetworkObjectPool>();
        Assert.IsNotNull(m_ObjectPool, $"{nameof(NetworkObjectPool)} not found in scene. Did you apply the {s_ObjectPoolTag} to the GameObject?");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            enabled = false;
            return;
        }
    }

    public override void OnNetworkDespawn()
    {
        //base.OnNetworkDespawn();
        //if (!Spawner.Singleton) return;

        //if (IsServer) Spawner.Singleton.UnregisterSpawnableObject(StridsvagnObjectType.Bullet, gameObject);
        //Spawner.Singleton.isGameOver.OnValueChanged -= OnGameOver;
    }

    private void FixedUpdate()
    {
        RaycastHit2D reflector = Physics2D.CircleCast(transform.position, 0.2f, transform.forward, 0.0f, 1 << 10);
        if (reflector.collider != null && canReflect)
        {
            if ((reflector.collider.tag == "FriendlyShield")) calculateReflection(reflector);
        }
    }

    private void DestroyBullet()
    {
        if (!NetworkObject.IsSpawned)
        {
            return;
        }

        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Despawn();
        //NetworkObject.Despawn(true);
        //m_ObjectPool.ReturnNetworkObject(NetworkObject);
    }

    private void BulletPop()
    {
        Assert.IsTrue(NetworkManager.IsServer);

        bool isRicActive = true;
        GameObject ric = m_ObjectPool.GetNetworkObject(ricochet).gameObject;
        ric.transform.position = transform.position;
        ric.GetComponent<ExplosionScriptNetwork>().Config(isRicActive);
        ric.GetComponent<NetworkObject>().Spawn(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var otherObject = other.gameObject;

        if (!NetworkManager.Singleton.IsServer || !NetworkObject.IsSpawned)
        {
            return;
        }

        if (/*m_Bounce == false &&*/ (otherObject.CompareTag("Wall")))
        {

            if (otherObject.TryGetComponent<WallDestructibleNetwork>(out var wallDestructibleNetwork))
            {
                wallDestructibleNetwork.TakeDamage(m_Damage);
            }

            DestroyBullet();
        }

        if (otherObject.TryGetComponent<TankControllerNetwork>(out var tankControllerNetwork))
        {
            tankControllerNetwork.TakeDamage(m_Damage);
            BulletPop();
            DestroyBullet();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Boundary"))
        {
            DestroyBullet();
        }
    }

    private void calculateReflection(RaycastHit2D reflector)
    {
        m_Rigidbody2D.velocity = Vector2.Reflect(m_Rigidbody2D.velocity, reflector.normal);
    }

    private void OnGameOver(bool oldValue, bool newValue)
    {
        // Is there anything we need to add in here?
        enabled = false;
    }
}
