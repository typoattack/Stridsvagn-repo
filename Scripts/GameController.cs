using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private Scene currentScene;
    private int gameMode;

    public GameObject enemy;
    public GameObject enemyMoveStraightLine;
    public GameObject enemyHP;
    public GameObject enemyHPMoveSL;
    public GameObject enemyTriple;
    public GameObject enemyTripleMoveSL;
    public GameObject powerup;

    public GameObject wall;
    public GameObject wallDestructible;
    public Transform targetSpawn;
    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;
    public bool sameSpawnAreaTanksAndWalls;
    public float xMinWalls;
    public float xMaxWalls;
    public float yMinWalls;
    public float yMaxWalls;
    public int maxObjects;
    [HideInInspector] public int objectsToSpawn;
    public int pickupsToSpawn;
    private int decider;
    public bool proceduralTanks;
    public bool proceduralWalls;

    public Text gameText;
    public Text enemiesLeftText;
    [HideInInspector] public static int enemyCount;
    [HideInInspector] public static int allyCount;

    public static Vector3 spawnPoint;
    public int xSpawn;
    public int ySpawn;
    public int zSpawn;

    public GameObject pauseMenu;
    private bool canPause;
    public GameObject gameOverMenu;
    private bool hasGameStarted = false;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("gameMode"))
        {
            PlayerPrefs.SetInt("gameMode", 3);
        }
        else
        {
            gameMode = PlayerPrefs.GetInt("gameMode");
        }
        currentScene = SceneManager.GetActiveScene();
        allyCount = 0;
        enemyCount = 0;
        objectsToSpawn = Random.Range(maxObjects / 2, maxObjects + 1);
        SetText();
        if (sameSpawnAreaTanksAndWalls)
        {
            xMinWalls = xMin;
            xMaxWalls = xMax;
            yMinWalls = yMin;
            yMaxWalls = yMax;
        }
        SpawnTargets(objectsToSpawn);
        spawnPoint = new Vector3(xSpawn, ySpawn, zSpawn);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            canPause = false;
            hasGameStarted = false;
        }
        else
        {
            canPause = true;
            hasGameStarted = true;
        }
    }

    private void Update()
    {
        SetText();

        if ((allyCount > 0 && enemyCount <= 0) && hasGameStarted)
        {
            //Debug.Log("AllyCount = " + allyCount + ", EnemyCount = " + enemyCount);
            activateGameOverMenu(2, gameMode);
        }

        if (allyCount <= 0 && hasGameStarted)
        {
            //Debug.Log("AllyCount = " + allyCount + ", EnemyCount = " + enemyCount);
            activateGameOverMenu(1, gameMode);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(currentScene.name, LoadSceneMode.Single);
        }

        if (Input.GetKeyDown("escape"))
        {
            pauseGame();
        }

        if (Input.GetKeyDown("0"))
        {
            SceneManager.LoadScene(2, LoadSceneMode.Single);
        }

        if (Input.GetKeyDown("1"))
        {
            SceneManager.LoadScene(3, LoadSceneMode.Single);
        }

        if (Input.GetKeyDown("2"))
        {
            SceneManager.LoadScene(4, LoadSceneMode.Single);
        }

        if (Input.GetKeyDown("3"))
        {
            SceneManager.LoadScene(5, LoadSceneMode.Single);
        }

        if (Input.GetKeyDown("4"))
        {
            SceneManager.LoadScene(6, LoadSceneMode.Single);
        }

        if (Pickup.numPickups <= pickupsToSpawn)
        {
            Vector2 pos = new Vector2(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
            targetSpawn.position = pos;
            var hits = Physics2D.OverlapCircleAll(pos, 2.0f, 31 << 6);
            while (hits.Length > 0)
            {
                pos = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), 0);
                hits = Physics2D.OverlapCircleAll(pos, 2.0f, 31 << 6);
            }

            Instantiate(powerup, pos, transform.rotation);
            Debug.Log(Pickup.numPickups);
        }
    }

    private void SpawnTargets(int spawnables)
    {
        if (proceduralWalls)
        {
            for (int x = (int)xMinWalls; x <= (int)xMaxWalls; x += 2)
            {
                for (int y = (int)yMinWalls; y <= (int)yMaxWalls; y++)
                {
                    decider = Random.Range(0, 10);
                    Vector2 pos = new Vector2(x, y);
                    if (decider >= 9)
                    {
                        decider = Random.Range(0, 2);
                        if (decider == 0) Instantiate(wall, pos, transform.rotation);
                        else Instantiate(wallDestructible, pos, transform.rotation);
                    }
                }
            }
        }
        
        if (proceduralTanks)
        {
            while (spawnables > 0)
            {
                decider = Random.Range(0, 10);

                // Defines the min and max ranges for x and y
                Vector2 pos = new Vector2(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
                targetSpawn.position = pos;
                var hits = Physics2D.OverlapCircleAll(pos, 2.0f, 31 << 6);
                while (hits.Length > 0)
                {
                    pos = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), 0);
                    hits = Physics2D.OverlapCircleAll(pos, 2.0f, 31 << 6);
                }

                // Creates the random object at the random 2D position.
                switch (decider)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        {
                            decider = Random.Range(0, 2);
                            if (decider == 0) Instantiate(enemy, pos, transform.rotation);
                            else Instantiate(enemyMoveStraightLine, pos, transform.rotation);
                        }
                        break;
                    case 6:
                        Instantiate(enemyHP, pos, transform.rotation);
                        break;
                    case 7:
                        Instantiate(enemyHPMoveSL, pos, transform.rotation);
                        break;
                    case 8:
                        Instantiate(enemyTriple, pos, transform.rotation);
                        break;
                    case 9:
                        Instantiate(enemyTripleMoveSL, pos, transform.rotation);
                        break;
                }

                spawnables--;

                // If I wanted to get the result of instantiate and fiddle with it, I might do this instead:
                //GameObject newGoods = (GameObject)Instantiate(goodsPrefab, pos)
                //newgoods.something = somethingelse;
            }
        }
    }
    void SetText()
    {
        /*
        if (allyCount > 0 && enemyCount > 0)
            gameText.text = "";
        else if (allyCount > 0 && enemyCount <= 0)
            gameText.text = "Your side won!\r\n Press space to play again";
        else
            gameText.text = "Your side was destroyed.\r\n Press space to play again";
        */

        gameText.text = "";

        enemiesLeftText.text = "Enemies: " + enemyCount.ToString() + "\r\n" +
                               "Allies: " + allyCount.ToString();
    }

    public void pauseGame()
    {
        if (canPause)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0.0f;
        }
    }

    public void unpauseGame()
    {
        Time.timeScale = 1.0f;
    }

    public void activateGameOverMenu(int outcome, int levelButton)
    {
        canPause = false;
        gameOverMenu.SetActive(true);
        gameOverMenu.transform.GetChild(outcome).gameObject.SetActive(false);
        if (gameMode != 4 || (gameMode == 4 && outcome == 2 && SceneManager.GetActiveScene().buildIndex != 7))
        {
            gameOverMenu.transform.GetChild(levelButton).gameObject.SetActive(true);
        }
    }

    public void reloadScene()
    {
        unpauseGame();
        hasGameStarted = false;
        Pickup.numPickups = 0;
        SceneManager.LoadScene(currentScene.name, LoadSceneMode.Single);
    }

    public void loadScene(int sceneNumber)
    {
        unpauseGame();
        hasGameStarted = false;
        Pickup.numPickups = 0;
        SceneManager.LoadScene(sceneNumber, LoadSceneMode.Single);
    }
}
