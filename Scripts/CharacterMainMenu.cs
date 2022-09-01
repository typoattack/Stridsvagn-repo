using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterMainMenu : MonoBehaviour
{
    private int gameMode;
    public int tankSelector = 0;
    private int currentTank;
    private int totalTanks = 8;
    private float detectDistance;
    private int tankHP;
    private int tankInitialHP;

    public int[] tankLocks = { 1, 0, 0, 0, 0, 0, 0, 0 };
    public int[] tankThresholds = { 0, 20, 50, 100, 150, 200, 0, 0 };
    public int[] campaignThresholds = { 1, 1, 1, 1, 1, 1, 0, 0 };
    public int[] noLivesLostCampaign = { 1, 1, 1, 1, 1, 1, 1, 0 };
    public int killsCampaign;
    private int deathlessRun;

    public Text lockText;


    void Start()
    {
        if (!PlayerPrefs.HasKey("gameMode"))
        {
            PlayerPrefs.SetInt("gameMode", 3);
        }
        else
        {
            gameMode = PlayerPrefs.GetInt("gameMode");
        }

        if (!PlayerPrefs.HasKey("tankModel"))
        {
            PlayerPrefs.SetInt("tankModel", 0);
        }
        else
        {
            tankSelector = PlayerPrefs.GetInt("tankModel");
        }

        if (!PlayerPrefs.HasKey("tankHP"))
        {
            PlayerPrefs.SetInt("tankHP", 10);
        }
        else
        {
            tankHP = PlayerPrefs.GetInt("tankHP");
        }

        if (!PlayerPrefs.HasKey("tankInitialHP"))
        {
            PlayerPrefs.SetInt("tankInitialHP", 10);
        }
        else
        {
            tankInitialHP = PlayerPrefs.GetInt("tankInitialHP");
        }

        if (!PlayerPrefs.HasKey("detectDistance"))
        {
            PlayerPrefs.SetFloat("detectDistance", 14.0f);
        }
        else
        {
            detectDistance = PlayerPrefs.GetFloat("detectDistance");
        }

        if (!PlayerPrefs.HasKey("tankLocks"))
        {
            PlayerPrefsX.SetIntArray("tankLocks", tankLocks);
        }
        else
        {
            tankLocks = PlayerPrefsX.GetIntArray("tankLocks");
        }

        if (!PlayerPrefs.HasKey("tankThresholds"))
        {
            PlayerPrefsX.SetIntArray("tankThresholds", tankThresholds);
        }
        else
        {
            tankThresholds = PlayerPrefsX.GetIntArray("tankThresholds");
        }

        if (!PlayerPrefs.HasKey("campaignThresholds"))
        {
            PlayerPrefsX.SetIntArray("campaignThresholds", campaignThresholds);
        }
        else
        {
            campaignThresholds = PlayerPrefsX.GetIntArray("campaignThresholds");
        }

        if (!PlayerPrefs.HasKey("noLivesLostCampaign"))
        {
            PlayerPrefsX.SetIntArray("noLivesLostCampaign", noLivesLostCampaign);
        }
        else
        {
            noLivesLostCampaign = PlayerPrefsX.GetIntArray("noLivesLostCampaign");
        }

        if (!PlayerPrefs.HasKey("killsCampaign"))
        {
            PlayerPrefs.SetInt("killsCampaign", 0);
        }
        else
        {
            killsCampaign = PlayerPrefs.GetInt("killsCampaign");
        }

        if (!PlayerPrefs.HasKey("deathlessRun"))
        {
            PlayerPrefs.SetInt("deathlessRun", 1);
        }
        else
        {
            deathlessRun = PlayerPrefs.GetInt("deathlessRun");
        }

        checkLocks();
        currentTank = tankSelector;
        displayButton(currentTank);
    }

    void Update()
    {
        //tankSelector = tankSelector % totalTanks;
        if (currentTank != tankSelector)
        {
            checkLocks();
            currentTank = tankSelector;
            displayButton(currentTank);
        }
    }

    public void AddToAbilities()
    {
        tankSelector++;
        if (tankSelector == totalTanks) tankSelector = 0;
    }

    public void SubtractFromAbilities()
    {
        tankSelector--;
        if (tankSelector < 0) tankSelector = totalTanks - 1;
    }

    public void ConfirmAbility()
    {
        PlayerPrefs.SetInt("tankModel", tankSelector);

        if (tankSelector == 1 || tankSelector == 2 || tankSelector == 7) PlayerPrefs.SetFloat("detectDistance", 10.0f);
        else PlayerPrefs.SetFloat("detectDistance", 14.0f);

        if (tankSelector == 3 || tankSelector == 4)
        {
            PlayerPrefs.SetInt("tankHP", 15);
            PlayerPrefs.SetInt("tankInitialHP", 15);
        }
        else if (tankSelector == 5 || tankSelector == 7)
        {
            PlayerPrefs.SetInt("tankHP", 20);
            PlayerPrefs.SetInt("tankInitialHP", 20);
        }
        else
        {
            PlayerPrefs.SetInt("tankHP", 10);
            PlayerPrefs.SetInt("tankInitialHP", 10);
        }
        }

    public void ExitToMenu()
    {
        Crate.numCrates = 0;
        SceneManager.LoadScene("MenuSP", LoadSceneMode.Single);
    }

    public void GoToLevelSelect()
    {
        Crate.numCrates = 0;
        if (gameMode == 3) SceneManager.LoadScene("LevelSelectSP", LoadSceneMode.Single);
        if (gameMode == 4)
        {
            deathlessRun = 1;
            PlayerPrefs.SetInt("deathlessRun", deathlessRun);
            SceneManager.LoadScene("Main", LoadSceneMode.Single);
        }
        if (gameMode == 5) SceneManager.LoadScene("Tutorial", LoadSceneMode.Single);
    }

    void displayButton(int tankLock)
    {
        if (tankLocks[tankLock] == 0)
        {
            gameObject.transform.GetChild(2).gameObject.SetActive(false);
            gameObject.transform.GetChild(4).gameObject.SetActive(true);
            if (noLivesLostCampaign[tankLock] == 0) lockText.text = "Beat campaign without losing a life to unlock";
            else if (campaignThresholds[tankLock] == 0) lockText.text = "Beat campaign to unlock";
            else
            {
                int killsRemaining = tankThresholds[tankLock] - killsCampaign;
                lockText.text = "Destroy " + killsRemaining + " tanks in campaign to unlock";
            }
        }
        else
        {
            gameObject.transform.GetChild(2).gameObject.SetActive(true);
            gameObject.transform.GetChild(4).gameObject.SetActive(false);
            lockText.text = "";
        }
        
    }

    private void checkLocks()
    {
        for (int i = 0; i < totalTanks; i++)
        {
            if (killsCampaign >= tankThresholds[i] && campaignThresholds[i] == 1 && noLivesLostCampaign[i] == 1)
            {
                tankLocks[i] = 1;
            }
            else tankLocks[i] = 0;
        }
    }

    public void debugKills(int kills)
    {
        killsCampaign = kills;
        PlayerPrefs.SetInt("killsCampaign", killsCampaign);
    }

    public void debugCampaign(int campaignClear)
    {
        campaignThresholds[6] = campaignThresholds[7] = campaignClear;
        PlayerPrefsX.SetIntArray("campaignThresholds", campaignThresholds);
    }

    public void debugNLL(int noDeaths)
    {
        noLivesLostCampaign[7] = noDeaths;
        PlayerPrefsX.SetIntArray("noLivesLostCampaign", noLivesLostCampaign);
    }
}