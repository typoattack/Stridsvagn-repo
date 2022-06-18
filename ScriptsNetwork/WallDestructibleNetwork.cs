using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Netcode;
public class WallDestructibleNetwork : NetworkBehaviour
{
    static string s_ObjectPoolTag = "ObjectPool";

    NetworkObjectPool m_ObjectPool;

    public NetworkVariable<int> WallHealth = new NetworkVariable<int>(100);

    [SerializeField]
    Texture m_Box;

    [SerializeField]
    private GameObject m_ExplosionPrefab;

    private void Awake()
    {
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

        //Assert.IsNotNull(Spawner.Singleton);
        //Spawner.Singleton.RegisterSpawnableObject(StridsvagnObjectType.DestructibleWall, gameObject);
        //Spawner.Singleton.isGameOver.OnValueChanged += OnGameOver;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!Spawner.Singleton) return;

        //if (IsServer) Spawner.Singleton.UnregisterSpawnableObject(StridsvagnObjectType.DestructibleWall, gameObject);
        //Spawner.Singleton.isGameOver.OnValueChanged -= OnGameOver;
    }

    public void TakeDamage(int amount)
    {
        WallHealth.Value = WallHealth.Value - amount;

        if (WallHealth.Value <= 0)
        {
            WallHealth.Value = 0;

            bool isExpActive = true;
            GameObject explosion = m_ObjectPool.GetNetworkObject(m_ExplosionPrefab).gameObject;
            explosion.transform.position = transform.position;
            explosion.GetComponent<ExplosionScriptNetwork>().Config(isExpActive);
            explosion.GetComponent<NetworkObject>().Spawn(true);

            Destroy(GetComponent<BoxCollider2D>());
            this.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
            this.transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
            DestroyWallClientRpc();
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);

        // draw health bar background
        GUI.color = Color.grey;
        GUI.DrawTexture(new Rect(pos.x - 26, Screen.height - pos.y + 20, 52, 7), m_Box);

        // draw health bar amount
        GUI.color = Color.green;
        GUI.DrawTexture(new Rect(pos.x - 25, Screen.height - pos.y + 21, WallHealth.Value / 2, 5), m_Box);
    }

    // --- ClientRPCs ---

    [ClientRpc]
    public void DestroyWallClientRpc()
    {
        Destroy(GetComponent<BoxCollider2D>());
        this.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        this.transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
    }

    private void OnGameOver(bool oldValue, bool newValue)
    {
        // Is there anything we need to add in here?
        enabled = false;
    }
}
