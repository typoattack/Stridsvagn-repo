using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Netcode;

public class Buff
{
    public enum BuffType
    {
        Rocket,
        Triple,
        Health,
        Mine,
        Shield,
        Last
    };

    public static Color[] bufColors = { Color.red, Color.blue, Color.cyan, Color.yellow, Color.green, Color.magenta, new Color(1, 0.5f, 0), new Color(0, 1, 0.5f) };

    public static Color GetColor(BuffType bt)
    {
        return bufColors[(int)bt];
    }
};

public class TankControllerNetwork : NetworkBehaviour
{
    static string s_ObjectPoolTag = "ObjectPool";

    NetworkObjectPool m_ObjectPool;

    [Header("Weapon Settings")]
    public GameObject BulletPrefab;
    public GameObject RocketPrefab;
    public GameObject MinePrefab;
    public GameObject HuskPrefab;
    public GameObject ExplosionPrefab;
    public AudioSource audio;
    public AudioClip fireSound;
    public AudioClip rocketSound;
    private bool canShoot;

    public NetworkVariable<int> RocketCounter = new NetworkVariable<int>(0);
    public NetworkVariable<int> TripleShotCounter = new NetworkVariable<int>(0);
    public NetworkVariable<int> MineCounter = new NetworkVariable<int>(0);
    public NetworkVariable<int> ShieldCounter = new NetworkVariable<int>(0);

    [Header("Movement Settings")]
    public float moveSpeed = 1.0f;
    public float turnSpeed = 25.0f;
    private bool canMove;

    [Header("Player Settings")]
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>(new FixedString32Bytes(""));
    public NetworkVariable<int> Health = new NetworkVariable<int>(100);
    public int initialHealth;
    private NetworkVariable<int> m_Lives = new NetworkVariable<int>(3);

    private SceneTransitionHandler.SceneStates m_CurrentSceneState;
    private bool m_HasGameStarted;

    private bool m_IsAlive = true;
    public bool IsAlive => m_Lives.Value > 0;

    private ClientRpcParams m_OwnerRPCParams;

    [SerializeField]
    Texture m_Box;

    // for client movement command throttling
    private float m_OldLts;
    private float m_OldRts;
    
    // server movement
    private NetworkVariable<float> m_LeftTrack = new NetworkVariable<float>();
    private NetworkVariable<float> m_RightTrack = new NetworkVariable<float>();

    Rigidbody2D m_Rigidbody2D;

    public Transform shotSpawn;

    private float nextFire;
    public float fireRate;
    public float originalFireRate;

    private Animator muzzleAnimator;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_HasGameStarted = false;
    }

    void Update()
    {
        switch (m_CurrentSceneState)
        {
            case SceneTransitionHandler.SceneStates.Ingame:
                {
                    InGameUpdate();
                    break;
                }
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsClient)
        {
            m_Lives.OnValueChanged -= OnLivesChanged;
        }

        if (Spawner.Singleton)
        {
            Spawner.Singleton.isGameOver.OnValueChanged -= OnGameStartedChanged;
            Spawner.Singleton.hasGameStarted.OnValueChanged -= OnGameStartedChanged;
        }
    }

    private void SceneTransitionHandler_clientLoadedScene(ulong clientId)
    {
        SceneStateChangedClientRpc(m_CurrentSceneState);
    }

    [ClientRpc]
    private void SceneStateChangedClientRpc(SceneTransitionHandler.SceneStates state)
    {
        if (!IsServer) SceneTransitionHandler.sceneTransitionHandler.SetSceneState(state);
    }

    private void SceneTransitionHandler_sceneStateChanged(SceneTransitionHandler.SceneStates newState)
    {
        m_CurrentSceneState = newState;
    }

    public override void OnNetworkSpawn()
    {
        GetComponent<AudioListener>().enabled = IsOwner;
        base.OnNetworkSpawn();

        // Bind to OnValueChanged to display in log the remaining lives of this player
        // And to update InvadersGame singleton client-side
        m_Lives.OnValueChanged += OnLivesChanged;

        if (IsServer) m_OwnerRPCParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };

        if (!Spawner.Singleton)
            Spawner.OnSingletonReady += SubscribeToDelegatesAndUpdateValues;
        else
            SubscribeToDelegatesAndUpdateValues();

        if (IsServer) SceneTransitionHandler.sceneTransitionHandler.OnClientLoadedScene += SceneTransitionHandler_clientLoadedScene;

        SceneTransitionHandler.sceneTransitionHandler.OnSceneStateChanged += SceneTransitionHandler_sceneStateChanged;
    }

    private void SubscribeToDelegatesAndUpdateValues()
    {
        Spawner.Singleton.hasGameStarted.OnValueChanged += OnGameStartedChanged;
        Spawner.Singleton.isGameOver.OnValueChanged += OnGameStartedChanged;

        if (IsClient && IsOwner)
        {
            Spawner.Singleton.SetLives(m_Lives.Value);
        }

        DontDestroyOnLoad(gameObject);

        m_ObjectPool = GameObject.FindWithTag(s_ObjectPoolTag).GetComponent<NetworkObjectPool>();
        Assert.IsNotNull(m_ObjectPool, $"{nameof(NetworkObjectPool)} not found in scene. Did you apply the {s_ObjectPoolTag} to the GameObject?");

        muzzleAnimator = this.transform.GetChild(2).GetChild(0).GetComponent<Animator>();
        initialHealth = Health.Value;
        lineRenderer = GetComponent<LineRenderer>();
        audio = GetComponent<AudioSource>();
        canMove = true;
        canShoot = true;
    }

    private void OnGameStartedChanged(bool previousValue, bool newValue)
    {
        m_HasGameStarted = newValue;
    }

    private void OnLivesChanged(int previousAmount, int currentAmount)
    {
        // Hide graphics client side upon death
        if (currentAmount <= 0 && IsClient && TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            spriteRenderer.enabled = false;

        if (!IsOwner) return;
        Debug.LogFormat("Lives {0} ", currentAmount);
        if (Spawner.Singleton != null) Spawner.Singleton.SetLives(m_Lives.Value);

        if (m_Lives.Value <= 0)
        {
            m_IsAlive = false;
        }
    }

    private void InGameUpdate()
    {
        if (/*!IsLocalPlayer || !IsOwner ||*/ !m_HasGameStarted) return;
        if (!m_IsAlive) return;

        if (IsServer)
        {
            UpdateServer();
        }

        if (IsClient)
        {
            UpdateClient();
        }
    }

    public void TakeDamage(int amount)
    {
        Health.Value = Health.Value - amount;

        if (Health.Value <= 0)
        {
            Health.Value = 0;
            canMove = false;
            canShoot = false;
            gameObject.transform.localScale = new Vector3(0, 0, 0);
            DestroyDespawnClientRpc();

            RocketCounter.Value = 0;
            MineCounter.Value = 0;
            TripleShotCounter.Value = 0;
            ShieldCounter.Value = 0;
            m_Lives.Value--;
            
            GameObject husk = Instantiate(HuskPrefab);
            husk.transform.position = transform.position;
            husk.transform.rotation = transform.rotation;
            husk.GetComponent<NetworkObject>().Spawn(true);
            
            bool isExpActive = true;
            GameObject explosion = m_ObjectPool.GetNetworkObject(ExplosionPrefab).gameObject;
            explosion.transform.position = transform.position;
            explosion.GetComponent<ExplosionScriptNetwork>().Config(isExpActive);
            explosion.GetComponent<NetworkObject>().Spawn(true);
            
            if (m_Lives.Value > 0) StartCoroutine(Respawn());
            else
            {
                Spawner.Singleton.SetGameEnd(GameOverReason.Death);
                NotifyGameOverClientRpc(GameOverReason.Death, m_OwnerRPCParams);

                // Hide graphics of this player object server-side. Note we don't want to destroy the object as it
                // may stop the RPC's from reaching on the other side, as there is only one player controlled object
                if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                    spriteRenderer.enabled = false;
            }
        }
    }

    void Fire(Vector3 direction)
    {
        FireClientRpc();

        int damage = 10;
        
        GameObject bullet = m_ObjectPool.GetNetworkObject(BulletPrefab).gameObject;
        bullet.transform.position = transform.position + direction;

        var bulletRb = bullet.GetComponent<Rigidbody2D>();
        Vector2 velocity;

        velocity = m_Rigidbody2D.velocity;
        velocity += (Vector2)(direction) * 3;
        bulletRb.velocity = velocity;
        bullet.GetComponent<Bullet>().Config(this, damage, m_ObjectPool);

        bullet.GetComponent<NetworkObject>().Spawn(true);
        

    }

    void FireRocket(Vector3 direction)
    {
        FireClientRpc();

        int damage = 10;
        
        GameObject rocket = m_ObjectPool.GetNetworkObject(RocketPrefab).gameObject;
        rocket.transform.position = transform.position + direction;
        rocket.transform.rotation = transform.rotation;

        var rocketRb = rocket.GetComponent<Rigidbody2D>();
        Vector2 velocity;

        velocity = m_Rigidbody2D.velocity;
        velocity += (Vector2)(direction) * 10;
        rocketRb.velocity = velocity;
        rocket.GetComponent<Bullet>().Config(this, damage, m_ObjectPool);

        rocket.GetComponent<NetworkObject>().Spawn(true);
        
    }

    void DropMine()
    {
        FireClientRpc();

        int damage = 10;
        
        GameObject mine = m_ObjectPool.GetNetworkObject(MinePrefab).gameObject;
        mine.transform.position = transform.position;

        mine.GetComponent<MineNetwork>().Config(this, damage, m_ObjectPool);

        mine.GetComponent<NetworkObject>().Spawn(true);
        
    }

    void ActivateShield()
    {
        transform.GetChild(3).gameObject.SetActive(true);
        ActivateShieldClientRpc();
        StartCoroutine(DeactivateShieldTimer());
    }

    void LateUpdate()
    {
        if (IsLocalPlayer)
        {
            // center camera.. only if this is MY player!
            Vector3 pos = transform.position;
            pos.z = -50;
            Camera.main.transform.position = pos;
        }
    }

    void UpdateServer()
    {
        if (m_LeftTrack.Value != 0 || m_RightTrack.Value != 0)
        {
            float turnLeft = turnSpeed * m_LeftTrack.Value;
            float turnRight = turnSpeed * m_RightTrack.Value;

            if (m_RightTrack.Value > 0f && m_LeftTrack.Value > 0f) { transform.Translate(Vector3.up * moveSpeed * Time.deltaTime); } //Forward movement if both triggers depressed
            if (m_RightTrack.Value < 0f && m_LeftTrack.Value < 0f) { transform.Translate(Vector3.up * -moveSpeed * Time.deltaTime); } //Backward movement if both triggers depressed
            if (m_LeftTrack.Value > 0f) { transform.Rotate(Vector3.forward, -turnLeft * Time.deltaTime); } //Track left
            if (m_RightTrack.Value < 0f) { transform.Rotate(Vector3.forward, turnRight * Time.deltaTime); } //Track left
            if (m_LeftTrack.Value > 0f && m_RightTrack.Value < 0f) { transform.Rotate(Vector3.forward, -turnLeft * Time.deltaTime); } //Track left
            if (m_RightTrack.Value > 0f) { transform.Rotate(Vector3.forward, turnRight * Time.deltaTime); } //Track right
            if (m_LeftTrack.Value < 0f) { transform.Rotate(Vector3.forward, -turnLeft * Time.deltaTime); } //Track right
            if (m_RightTrack.Value > 0f && m_LeftTrack.Value < 0f) { transform.Rotate(Vector3.forward, turnRight * Time.deltaTime); } //Track right
        }
    }

    void UpdateClient()
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        // movement
        float lts = 0;
        lts = Input.GetAxis("Vertical");
        float rts = 0;
        rts = Input.GetAxis("Vertical2");

        if (m_OldLts != lts || m_OldRts != rts)
        {
            if (canMove) MovementServerRpc(lts, rts);
            else MovementServerRpc(0f, 0f);
            m_OldLts = lts;
            m_OldRts = rts;
        }

        // fire
        if (Input.GetButtonDown("Fire1") && Time.time > nextFire && canShoot)
        {
            nextFire = Time.time + fireRate;
            FireServerRpc();
            if (ShieldCounter.Value > 0)
            {
                //audio.PlayOneShot(ShieldSound);
                transform.GetChild(3).gameObject.SetActive(true);
            }
            else
            {
                if (RocketCounter.Value > 0) audio.PlayOneShot(rocketSound);
                else audio.PlayOneShot(fireSound);
                muzzleAnimator.SetTrigger("Fire");
            }
        }
        
        RaycastHit2D sight = Physics2D.Raycast(shotSpawn.position, transform.up, 7.0f, 3 << 8);
        if (sight.collider != null)
        {
            lineRenderer.enabled = true;
            Vector3[] positions = new Vector3[2] { shotSpawn.position, (Vector3)sight.point };
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.025f;
            lineRenderer.endWidth = 0.025f;
            lineRenderer.SetPositions(positions);
            //DrawLineClientRpc(shotSpawn.position, (Vector3)sight.point);
        }
        else
        {
            lineRenderer.enabled = false;
            StopDrawLineClientRpc();
        }
        
        if (Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }
    }

    public void AddBuff(Buff.BuffType buff)
    {
        if (buff == Buff.BuffType.Rocket)
        {
            RocketCounter.Value = 5;
            MineCounter.Value = 0;
            TripleShotCounter.Value = 0;
            ShieldCounter.Value = 0;
        }

        if (buff == Buff.BuffType.Mine)
        {
            MineCounter.Value = 5;
            RocketCounter.Value = 0;
            TripleShotCounter.Value = 0;
            ShieldCounter.Value = 0;
        }

        if (buff == Buff.BuffType.Triple)
        {
            TripleShotCounter.Value = 5;
            MineCounter.Value = 0;
            RocketCounter.Value = 0;
            ShieldCounter.Value = 0;
        }

        if (buff == Buff.BuffType.Shield)
        {
            ShieldCounter.Value = 1;
            RocketCounter.Value = 0;
            MineCounter.Value = 0;
            TripleShotCounter.Value = 0;
        }

        if (buff == Buff.BuffType.Health)
        {
            Health.Value += 50;
            if (Health.Value >= initialHealth)
            {
                Health.Value = initialHealth;
            }
        }

    }

    // --- ServerRPCs ---
    [ServerRpc]
    public void MovementServerRpc(float leftInput, float rightInput)
    {
        m_LeftTrack.Value = leftInput;
        m_RightTrack.Value = rightInput;
    }

    [ServerRpc]
    public void FireServerRpc()
    {
        //if (Time.time > nextFire)
        {
            //nextFire = Time.time + fireRate;
            var up = transform.up;
            
            if (TripleShotCounter.Value > 0)
            {
                Fire(Quaternion.Euler(0, 0, 10) * up);
                Fire(Quaternion.Euler(0, 0, -10) * up);
                Fire(up);
                TripleShotCounter.Value--;
            }
            else if (RocketCounter.Value > 0)
            {
                FireRocket(up);
                RocketCounter.Value--;
            }
            else if (MineCounter.Value > 0)
            {
                DropMine();
                MineCounter.Value--;
            }
            else if (ShieldCounter.Value > 0)
            {
                ActivateShield();
                ShieldCounter.Value--;
            }
            else
            {
                Fire(up);
            }
        }
    }

    // --- ClientRPCs ---
    [ClientRpc]
    public void FireClientRpc()
    {
        if (IsLocalPlayer) return;

        if (RocketCounter.Value > 0) audio.PlayOneShot(rocketSound);
        else audio.PlayOneShot(fireSound);
        muzzleAnimator.SetTrigger("Fire");
    }

    [ClientRpc]
    public void DrawLineClientRpc(Vector3 start, Vector3 end)
    {
        if (IsLocalPlayer) return;

        lineRenderer.enabled = true;
        Vector3[] positions = new Vector3[2] { start, end };
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.025f;
        lineRenderer.endWidth = 0.025f;
        lineRenderer.SetPositions(positions);
    }

    [ClientRpc]
    public void StopDrawLineClientRpc()
    {
        if (IsLocalPlayer) return;

        lineRenderer.enabled = false;
    }

    [ClientRpc]
    public void ActivateShieldClientRpc()
    {
        if (IsLocalPlayer) return;

        //audio.PlayOneShot(ShieldSound);
        transform.GetChild(3).gameObject.SetActive(true);
    }

    [ClientRpc]
    public void DeactivateShieldClientRpc()
    {
        //if (IsLocalPlayer) return;

        transform.GetChild(3).gameObject.SetActive(false);
    }

    [ClientRpc]
    public void DestroyDespawnClientRpc()
    {
        gameObject.transform.localScale = new Vector3(0, 0, 0);
    }

    [ClientRpc]
    public void DestroyRespawnClientRpc()
    {
        gameObject.transform.localScale = new Vector3(1, 1, 1);
    }

    [ClientRpc]
    private void NotifyGameOverClientRpc(GameOverReason reason, ClientRpcParams clientParams)
    {
        NotifyGameOver(reason);
    }

    /// <summary>
    /// This should only be called locally, either through NotifyGameOverClientRpc or through the InvadersGame.BroadcastGameOverReason
    /// </summary>
    /// <param name="reason"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void NotifyGameOver(GameOverReason reason)
    {
        Assert.IsTrue(IsLocalPlayer);
        m_HasGameStarted = false;
        switch (reason)
        {
            case GameOverReason.None:
                Spawner.Singleton.DisplayGameOverText("You have lost! \n Unknown reason!");
                break;
            case GameOverReason.Death:
                Spawner.Singleton.DisplayGameOverText("You have lost! \n Your health was depleted!");
                break;
            case GameOverReason.Max:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
        }
    }

    void OnGUI()
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);

        // draw the name with a shadow (colored for buf)	
        GUI.color = Color.black;
        GUI.Label(new Rect(pos.x - 20, Screen.height - pos.y - 30, 100, 30), PlayerName.Value.Value);
        
        GUI.color = Color.white;
        if (RocketCounter.Value > 0) { GUI.color = Buff.GetColor(Buff.BuffType.Rocket); }

        if (TripleShotCounter.Value > 0) { GUI.color = Buff.GetColor(Buff.BuffType.Triple); }

        if (MineCounter.Value > 0) { GUI.color = Buff.GetColor(Buff.BuffType.Mine); }

        if (ShieldCounter.Value > 0) { GUI.color = Buff.GetColor(Buff.BuffType.Shield); }
        
        GUI.Label(new Rect(pos.x - 21, Screen.height - pos.y - 31, 100, 30), PlayerName.Value.Value);

        // draw health bar background
        GUI.color = Color.grey;
        GUI.DrawTexture(new Rect(pos.x - 26, Screen.height - pos.y + 20, 52, 7), m_Box);

        // draw health bar amount
        GUI.color = Color.green;
        GUI.DrawTexture(new Rect(pos.x - 25, Screen.height - pos.y + 21, (float)Health.Value / (float)initialHealth * (float)50f, 5), m_Box);
    }

    IEnumerator DeactivateShieldTimer()
    {
        yield return new WaitForSeconds(10.0f);
        transform.GetChild(3).gameObject.SetActive(false);
        DeactivateShieldClientRpc();
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2.0f);

        Health.Value = 100;
        transform.position = new Vector3(0f, -18.07f, 0f);
        GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        GetComponent<Rigidbody2D>().angularVelocity = 0;
        canShoot = true;
        canMove = true;
        gameObject.transform.localScale = new Vector3(1, 1, 1);
        DestroyRespawnClientRpc();
    }
}
