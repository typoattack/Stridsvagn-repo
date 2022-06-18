using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuControlSP : MonoBehaviour
{
    private int gameMode; // 3 == free play, 4 == campaign
    private int livesRemaining;
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

        if (!PlayerPrefs.HasKey("livesLeft"))
        {
            PlayerPrefs.SetInt("livesLeft", 4);
        }
        else
        {
            livesRemaining = PlayerPrefs.GetInt("livesLeft");
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("gameMode", 3);
        PlayerPrefs.SetInt("livesLeft", 4);
        SceneManager.LoadScene("CharacterSelectSP", LoadSceneMode.Single);
    }

    public void StartCampaign()
    {
        PlayerPrefs.SetInt("gameMode", 4);
        PlayerPrefs.SetInt("livesLeft", 4);
        SceneManager.LoadScene("CharacterSelectSP", LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene("Tutorial", LoadSceneMode.Single);
    }
}
