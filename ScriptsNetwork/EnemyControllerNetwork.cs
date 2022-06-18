using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Netcode;

public class EnemyControllerNetwork : NetworkBehaviour
{
    static string s_ObjectPoolTag = "ObjectPool";
    NetworkObjectPool m_ObjectPool;

    public Transform bumper;

    public float speed;
    private int rotation;
    public bool canMove;

    private float xMin;
    private float xMax;
    private float yMin;
    private float yMax;

    public static int numEnemies = 0;
    public NetworkVariable<int> Health = new NetworkVariable<int>(100);
    public int initialHealth;

    [SerializeField]
    Texture m_Box;

    Rigidbody2D m_Rigidbody2D;

    private float nextFire;
    public float fireRate;
    public float originalFireRate;

    [SerializeField]
    private GameObject m_Husk;
    [SerializeField]
    private GameObject m_ExplosionPrefab;

    private Animator muzzleAnimator;

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_ObjectPool = GameObject.FindWithTag(s_ObjectPoolTag).GetComponent<NetworkObjectPool>();
        Assert.IsNotNull(m_ObjectPool, $"{nameof(NetworkObjectPool)} not found in scene. Did you apply the {s_ObjectPoolTag} to the GameObject?");
    }

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);

        base.OnNetworkSpawn();
        if (!IsServer)
        {
            enabled = false;
            return;
        }

        rotation = Random.Range(0, 10);
        if (rotation < 5) transform.rotation = Quaternion.Euler(0, 0, 0);
        else transform.rotation = Quaternion.Euler(0, 0, -90);
        xMin = -14f;
        xMax = 14f;
        yMin = -19f;
        yMax = 19f;

        Assert.IsNotNull(Spawner.Singleton);
        //Spawner.Singleton.RegisterSpawnableObject(StridsvagnObjectType.Enemy, gameObject);
        Spawner.Singleton.isGameOver.OnValueChanged += OnGameOver;

        Health.Value = initialHealth;
        
        var turret = transform.GetChild(1).gameObject;
        if (turret.TryGetComponent<EnemyTurretNetwork>(out var childTurret))
        {
            turret.GetComponent<NetworkObject>().Spawn(true);
        }
        var shield = transform.GetChild(3).gameObject;
        if (shield.TryGetComponent<ShieldControllerNetwork>(out var childShield))
        {
            shield.GetComponent<NetworkObject>().Spawn(true);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!Spawner.Singleton) return;

        //if (IsServer) Spawner.Singleton.UnregisterSpawnableObject(StridsvagnObjectType.Enemy, gameObject);
        Spawner.Singleton.isGameOver.OnValueChanged -= OnGameOver;
    }

    private void DestroyEnemy()
    {
        if (!NetworkObject.IsSpawned)
        {
            return;
        }
        
        bool isExpActive = true;
        GameObject explosion = m_ObjectPool.GetNetworkObject(m_ExplosionPrefab).gameObject;
        explosion.transform.position = transform.position;
        explosion.GetComponent<ExplosionScriptNetwork>().Config(isExpActive);
        explosion.GetComponent<NetworkObject>().Spawn(true);

        GameObject destroyedTank = Instantiate(m_Husk);
        destroyedTank.transform.position = transform.position;
        destroyedTank.transform.rotation = transform.rotation;
        destroyedTank.GetComponent<NetworkObject>().Spawn(true);
        
        numEnemies--;
        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Despawn();
        //NetworkObject.Despawn(true);
        //m_ObjectPool.ReturnNetworkObject(NetworkObject);
    }

    public void TakeDamage(int amount)
    {
        Health.Value = Health.Value - amount;

        if (Health.Value <= 0)
        {
            Health.Value = 0;
            DestroyEnemy();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            transform.Translate(transform.up * speed * Time.deltaTime, Space.World);

            RaycastHit2D guard = Physics2D.BoxCast(bumper.position, new Vector2(0.6f, 0.6f), 0.0f, transform.forward, 0.0f, 13 << 6);

            if (transform.position.x <= xMin ||
                transform.position.x >= xMax ||
                transform.position.y <= yMin ||
                transform.position.y >= yMax ||
                guard.collider != null)
            {
                transform.localRotation *= Quaternion.Euler(0, 0, 180);
            }
        }
    }

    void OnGUI()
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);

        // draw health bar background
        GUI.color = Color.grey;
        GUI.DrawTexture(new Rect(pos.x - 26, Screen.height - pos.y + 20, 52, 7), m_Box);

        // draw health bar amount
        GUI.color = Color.green;
        GUI.DrawTexture(new Rect(pos.x - 25, Screen.height - pos.y + 21, (float)Health.Value / (float)initialHealth * (float)50f, 5), m_Box);
    }

    private void OnGameOver(bool oldValue, bool newValue)
    {
        // Is there anything we need to add in here?
        enabled = false;
    }
}
