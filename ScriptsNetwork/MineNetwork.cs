using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Netcode;

public class MineNetwork : NetworkBehaviour
{
    static string s_ObjectPoolTag = "ObjectPool";
    NetworkObjectPool m_ObjectPool;
    NetworkObjectPool m_PoolToReturn;

    //bool m_Bounce = false;
    int m_Damage = 10;
    TankControllerNetwork m_Owner;

    CircleCollider2D m_collider;
    public GameObject ricochet;

    public void Config(TankControllerNetwork owner, int damage, /*bool bounce,*/ NetworkObjectPool poolToReturn)
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
        //Assert.IsNotNull(Spawner.Singleton);
        //Spawner.Singleton.RegisterSpawnableObject(StridsvagnObjectType.Bullet, gameObject);
        //Spawner.Singleton.isGameOver.OnValueChanged += OnGameOver;
    }

    public override void OnNetworkDespawn()
    {
        //base.OnNetworkDespawn();
        //if (!Spawner.Singleton) return;

        //if (IsServer) Spawner.Singleton.UnregisterSpawnableObject(StridsvagnObjectType.Bullet, gameObject);
        //Spawner.Singleton.isGameOver.OnValueChanged -= OnGameOver;
    }

    private void DestroyMine()
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

    // Start is called before the first frame update
    void Start()
    {
        m_collider = GetComponent<CircleCollider2D>();
        m_collider.enabled = false;
        StartCoroutine(SetColliderActive());
    }

    // Update is called once per frame
    void Update()
    {
        
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

        if (otherObject.TryGetComponent<EnemyControllerNetwork>(out var enemyControllerNetwork))
        {
            enemyControllerNetwork.TakeDamage(m_Damage);
            BulletPop();
            DestroyMine();
        }

        if (otherObject.TryGetComponent<TankControllerNetwork>(out var tankControllerNetwork))
        {
            if (tankControllerNetwork != m_Owner)
            {
                tankControllerNetwork.TakeDamage(m_Damage);
                BulletPop();
                DestroyMine();
            }
        }
    }

    IEnumerator SetColliderActive()
    {
        yield return new WaitForSeconds(2);
        m_collider.enabled = true;
        this.transform.GetChild(1).gameObject.SetActive(true);
    }

    private void OnGameOver(bool oldValue, bool newValue)
    {
        // Is there anything we need to add in here?
        enabled = false;
    }
}
