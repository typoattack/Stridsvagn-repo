using System;
using System.Collections.Generic;
using Unity.Netcode;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public enum StridsvagnObjectType
{
    Enemy = 1,
    DestructibleWall,
    Bullet,
    Max
}

[Flags]
public enum UpdateEnemiesResultFlags : byte
{
    None = 0x0000,
    Max
}

public enum GameOverReason : byte
{
    None = 0,
    Death = 1,
    Max,
}

public class Spawner : NetworkBehaviour
{
    [SerializeField]
    NetworkObjectPool m_ObjectPool;

    [Header("Boundaries")]
    [SerializeField]
    private int xMax;
    [SerializeField]
    private int xMin;
    [SerializeField]
    private int yMax;
    [SerializeField]
    private int yMin;

    [Header("GameMode Settings")]
    [SerializeField]
    private int m_AmountPickups = 10;
    [SerializeField]
    private int m_AmountEnemies = 1;
    
    [Header("Prefab settings")]
    [SerializeField]
    private GameObject m_PowerupPrefab;
    [SerializeField]
    private GameObject m_ObstaclePrefab;
    [SerializeField]
    private GameObject m_ObstacleLongPrefab;
    [SerializeField]
    private GameObject m_ObstacleTallPrefab;
    [SerializeField]
    private GameObject m_WallDestructiblePrefab;
    [SerializeField]
    private GameObject m_WallDestructibleLongPrefab;
    [SerializeField]
    private GameObject m_enemyBasicPrefab;
    [SerializeField]
    private GameObject m_enemyMovePrefab;
    [SerializeField]
    private GameObject m_enemyHPPrefab;
    [SerializeField]
    private GameObject m_enemyHPMoveSLPrefab;
    [SerializeField]
    private GameObject m_enemyTriplePrefab;
    [SerializeField]
    private GameObject m_enemyTripleMoveSLPrefab;
    

    [Header("UI Settings")]
    public TMP_Text gameTimerText;
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public TMP_Text gameOverText;

    private bool HaveDestructibleWallsBeenSpawned = false;
    private bool HaveEnemiesBeenSpawned = false;

    static int[] s_Obstacles = new int[]
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0,
        0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
        0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    };

    [SerializeField]
    [Tooltip("Time Remaining until the game starts")]
    private float m_DelayedStartTime = 5.0f;

    private List<EnemyControllerNetwork> m_Enemies = new List<EnemyControllerNetwork>();

    //These help to simplify checking server vs client
    //[NSS]: This would also be a great place to add a state machine and use networked vars for this
    private bool m_ClientGameOver;
    private bool m_ClientGameStarted;
    private bool m_ClientStartCountdown;

    private NetworkVariable<bool> m_CountdownStarted = new NetworkVariable<bool>(false);

    // the timer should only be synced at the beginning
    // and then let the client to update it in a predictive manner
    private bool m_ReplicatedTimeSent = false;
    private float m_TimeRemaining;

    public static Spawner Singleton { get; private set; }

    public NetworkVariable<bool> hasGameStarted { get; } = new NetworkVariable<bool>(false);

    public NetworkVariable<bool> isGameOver { get; } = new NetworkVariable<bool>(false);

    private void Awake()
    {
        Assert.IsNull(Singleton, $"Multiple instances of {nameof(Spawner)} detected. This should not happen.");
        Singleton = this;

        OnSingletonReady?.Invoke();

        if (IsServer)
        {
            hasGameStarted.Value = false;

            //Set our time remaining locally
            m_TimeRemaining = m_DelayedStartTime;

            //Set for server side
            m_ReplicatedTimeSent = false;
        }
        else
        {
            //We do a check for the client side value upon instantiating the class (should be zero)
            Debug.LogFormat("Client side we started with a timer value of {0}", m_TimeRemaining);
        }
    }

    

    /// <summary>
    ///     Update
    ///     MonoBehaviour Update method
    /// </summary>
    private void Update()
    {
        //Is the game over?
        if (IsCurrentGameOver()) return;

        //Update game timer (if the game hasn't started)
        UpdateGameTimer();

        //If we are a connected client, then don't update the enemies (server side only)
        if (!IsServer) return;

        //If we are the server and the game has started, then update the enemies
        if (HasGameStarted()) UpdateNetworkObjects();
    }

    /// <summary>
    ///     OnDestroy
    ///     Clean up upon destruction of this class
    /// </summary>
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            m_Enemies.Clear();
        }
    }

    internal static event Action OnSingletonReady;
    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            m_ClientGameOver = false;
            m_ClientStartCountdown = false;
            m_ClientGameStarted = false;

            m_CountdownStarted.OnValueChanged += (oldValue, newValue) =>
            {
                m_ClientStartCountdown = newValue;
                Debug.LogFormat("Client side we were notified the start count down state was {0}", newValue);
            };

            hasGameStarted.OnValueChanged += (oldValue, newValue) =>
            {
                m_ClientGameStarted = newValue;
                gameTimerText.gameObject.SetActive(!m_ClientGameStarted);
                Debug.LogFormat("Client side we were notified the game started state was {0}", newValue);
            };

            isGameOver.OnValueChanged += (oldValue, newValue) =>
            {
                m_ClientGameOver = newValue;
                Debug.LogFormat("Client side we were notified the game over state was {0}", newValue);
            };
        }

        //Both client and host/server will set the scene state to "ingame" which places the PlayerControl into the SceneTransitionHandler.SceneStates.INGAME
        //and in turn makes the players visible and allows for the players to be controlled.
        SceneTransitionHandler.sceneTransitionHandler.SetSceneState(SceneTransitionHandler.SceneStates.Ingame);

        base.OnNetworkSpawn();
        SpawnObstacles();
    }

    /// <summary>
    ///     ShouldStartCountDown
    ///     Determines when the countdown should start
    /// </summary>
    /// <returns>true or false</returns>
    private bool ShouldStartCountDown()
    {
        //If the game has started, then don't both with the rest of the count down checks.
        if (HasGameStarted()) return false;
        if (IsServer)
        {
            m_CountdownStarted.Value = SceneTransitionHandler.sceneTransitionHandler.AllClientsAreLoaded();

            //While we are counting down, continually set the m_ReplicatedTimeRemaining.Value (client should only receive the update once)
            if (m_CountdownStarted.Value && !m_ReplicatedTimeSent)
            {
                SetReplicatedTimeRemainingClientRPC(m_DelayedStartTime);
                m_ReplicatedTimeSent = true;
            }

            return m_CountdownStarted.Value;
        }

        return m_ClientStartCountdown;
    }

    /// <summary>
    ///     We want to send only once the Time Remaining so the clients
    ///     will deal with updating it. For that, we use a ClientRPC
    /// </summary>
    /// <param name="delayedStartTime"></param>
    [ClientRpc]
    private void SetReplicatedTimeRemainingClientRPC(float delayedStartTime)
    {
        // See the ShouldStartCountDown method for when the server updates the value
        if (m_TimeRemaining == 0)
        {
            Debug.LogFormat("Client side our first timer update value is {0}", delayedStartTime);
            m_TimeRemaining = delayedStartTime;
        }
        else
        {
            Debug.LogFormat("Client side we got an update for a timer value of {0} when we shouldn't", delayedStartTime);
        }
    }

    /// <summary>
    ///     IsCurrentGameOver
    ///     Returns whether the game is over or not
    /// </summary>
    /// <returns>true or false</returns>
    private bool IsCurrentGameOver()
    {
        if (IsServer)
            return isGameOver.Value;
        return m_ClientGameOver;
    }

    /// <summary>
    ///     HasGameStarted
    ///     Determine whether the game has started or not
    /// </summary>
    /// <returns>true or false</returns>
    private bool HasGameStarted()
    {
        if (IsServer)
            return hasGameStarted.Value;
        return m_ClientGameStarted;
    }

    /// <summary>
    ///     Client side we try to predictively update the gameTimer
    ///     as there shouldn't be a need to receive another update from the server
    ///     We only got the right m_TimeRemaining value when we started so it will be enough
    /// </summary>
    /// <returns> True when m_HasGameStared is set </returns>
    private void UpdateGameTimer()
    {
        if (!ShouldStartCountDown()) return;
        if (!HasGameStarted() && m_TimeRemaining > 0.0f)
        {
            m_TimeRemaining -= Time.deltaTime;

            if (IsServer && m_TimeRemaining <= 0.0f) // Only the server should be updating this
            {
                m_TimeRemaining = 0.0f;
                hasGameStarted.Value = true;
                OnGameStarted();
            }

            if (m_TimeRemaining > 0.1f)
                gameTimerText.SetText("{0}", Mathf.FloorToInt(m_TimeRemaining));
        }
    }

    /// <summary>
    ///     OnGameStarted
    ///     Only invoked by the server, this hides the timer text and initializes the enemies and level
    /// </summary>
    private void OnGameStarted()
    {
        gameTimerText.gameObject.SetActive(false);
        //SpawnObstacles();
        SpawnDestructibleWalls();
        SpawnEnemies();
    }

    // Update is called once per frame
    void UpdateNetworkObjects()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (!HaveDestructibleWallsBeenSpawned)
        {
            SpawnDestructibleWalls();
        }
        if (!HaveEnemiesBeenSpawned)
        {
            SpawnEnemies();
        }
        
        if (PickupNetwork.numPickups < m_AmountPickups)
        {
            Vector3 pos = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), 0);
            var hits = Physics2D.OverlapCircleAll(pos, 2.0f);
            while (hits.Length > 0)
            {
                pos = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), 0);
                hits = Physics2D.OverlapCircleAll(pos, 2.0f);
            }

            GameObject powerUp = m_ObjectPool.GetNetworkObject(m_PowerupPrefab).gameObject;
            powerUp.transform.position = pos;
            powerUp.GetComponent<PickupNetwork>().buffType.Value = (Buff.BuffType)Random.Range(0, (int)Buff.BuffType.Last);
            powerUp.GetComponent<NetworkObject>().Spawn(true);
        }
        
    }

    public void SetScore(int score)
    {
        scoreText.SetText("0{0}", score);
    }

    public void SetLives(int lives)
    {
        livesText.SetText("0{0}", lives);
    }

    public void DisplayGameOverText(string message)
    {
        if (gameOverText)
        {
            gameOverText.SetText(message);
            gameOverText.gameObject.SetActive(true);
        }
    }

    public void SetGameEnd(GameOverReason reason)
    {
        Assert.IsTrue(IsServer, "SetGameEnd should only be called server side!");

        // We should only end the game if all the player's are dead
        if (reason != GameOverReason.Death)
        {
            this.isGameOver.Value = true;
            BroadcastGameOverClientRpc(reason); // Notify our clients!
            return;
        }

        foreach (NetworkClient networkedClient in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = networkedClient.PlayerObject;
            if (playerObject == null) continue;

            // We should just early out if any of the player's are still alive
            if (playerObject.GetComponent<TankControllerNetwork>().IsAlive)
                return;
        }

        this.isGameOver.Value = true;
    }

    [ClientRpc]
    public void BroadcastGameOverClientRpc(GameOverReason reason)
    {
        var localPlayerObject = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject;
        Assert.IsNotNull(localPlayerObject);

        if (localPlayerObject.TryGetComponent<TankControllerNetwork>(out var tankControllerNetwork))
            tankControllerNetwork.NotifyGameOver(reason);
    }

    public void RegisterSpawnableObject(StridsvagnObjectType stridsvagnObjectType, GameObject gameObject)
    {
        Assert.IsTrue(IsClient);

        switch (stridsvagnObjectType)
        {
            case StridsvagnObjectType.Enemy:
                {
                    gameObject.TryGetComponent<EnemyControllerNetwork>(out var enemyControllerNetwork);
                    Assert.IsTrue(enemyControllerNetwork != null);
                    if (!m_Enemies.Contains(enemyControllerNetwork))
                        m_Enemies.Add(enemyControllerNetwork);
                    break;
                }
            default:
                Assert.IsTrue(false);
                break;
        }
    }
    public void UnregisterSpawnableObject(StridsvagnObjectType stridsvagnObjectType, GameObject gameObject)
    {
        Assert.IsTrue(IsServer);

        switch (stridsvagnObjectType)
        {
            case StridsvagnObjectType.Enemy:
                {
                    gameObject.TryGetComponent<EnemyControllerNetwork>(out var enemyControllerNetwork);
                    Assert.IsTrue(enemyControllerNetwork != null);
                    if (m_Enemies.Contains(enemyControllerNetwork))
                        m_Enemies.Remove(enemyControllerNetwork);
                    break;
                }
            default:
                Assert.IsTrue(false);
                break;
        }
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneTransitionHandler.sceneTransitionHandler.ExitAndLoadStartMenu();
    }

    void SpawnDestructibleWalls()
    {
        int y = -1;
        int x = 0;
        for (int i = 0; i < s_Obstacles.Length; i++)
        {
            if (i % (xMax + 1) == 0)
            {
                y = y + 1;
                x = 0;
            }
            else
            {
                x = x + 1;
            }

            if (s_Obstacles[i] == 2)
            {
                int choice = Random.Range(0, 2);
                GameObject go = new GameObject();
                if (choice == 0) go = Instantiate(m_WallDestructiblePrefab);
                else go = Instantiate(m_WallDestructibleLongPrefab);
                go.transform.position = new Vector3(xMin + x * 2, yMin + y * 2, 0);
                go.transform.rotation = Quaternion.identity;
                go.GetComponent<NetworkObject>().Spawn(true);
            }
        }
        HaveDestructibleWallsBeenSpawned = true;
    }

    void SpawnObstacles()
    {
        // Obstacles are not networked we just spawn them as static objects on each peer

        int y = -1;
        int x = 0;
        for (int i = 0; i < s_Obstacles.Length; i++)
        {
            if (i % (xMax + 1) == 0)
            {
                y = y + 1;
                x = 0;
            }
            else
            {
                x = x + 1;
            }

            if (s_Obstacles[i] == 1)
            {
                int choice = 1;//Random.Range(0, 3);
                switch (choice)
                {
                    default:
                        Instantiate(m_ObstaclePrefab, new Vector3(xMin + x * 2, yMin + y * 2, 0), Quaternion.identity);
                        break;
                    case 1:
                        Instantiate(m_ObstacleLongPrefab, new Vector3(xMin + x * 2, yMin + y * 2, 0), Quaternion.identity);
                        break;
                    case 2:
                        Instantiate(m_ObstacleTallPrefab, new Vector3(xMin + x * 2, yMin + y * 2, 0), Quaternion.identity);
                        break;
                }
                
            }
        }

    }

    void SpawnEnemies()
    {
        for (int i = 0; i < m_AmountEnemies; i++)
        {
            Vector3 pos = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), 0);
            var hits = Physics2D.OverlapCircleAll(pos, 2.0f);
            while (hits.Length > 0)
            {
                pos = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), 0);
                hits = Physics2D.OverlapCircleAll(pos, 2.0f);
            }

            GameObject enemyToSpawn = new GameObject();
            int choice = Random.Range(2, 6);
            switch (choice)
            {
                default:
                case 1:
                    //enemyToSpawn = m_ObjectPool.GetNetworkObject(m_enemyBasicPrefab).gameObject;
                    enemyToSpawn = Instantiate(m_enemyBasicPrefab);
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                    choice = Random.Range(0, 2);
                    if (choice == 0) enemyToSpawn = Instantiate(m_enemyBasicPrefab);
                    else enemyToSpawn = Instantiate(m_enemyMovePrefab);
                    break;
                case 6:
                    enemyToSpawn = Instantiate(m_enemyHPPrefab);
                    break;
                case 7:
                    enemyToSpawn = Instantiate(m_enemyHPMoveSLPrefab);
                    break;
                case 8:
                    enemyToSpawn = Instantiate(m_enemyTriplePrefab);
                    break;
                case 9:
                    enemyToSpawn = Instantiate(m_enemyTripleMoveSLPrefab);
                    break;
                
            }
            enemyToSpawn.transform.position = pos;
            enemyToSpawn.GetComponent<NetworkObject>().Spawn(true);
            EnemyControllerNetwork.numEnemies++;
        }
        HaveEnemiesBeenSpawned = true;
    }
}
