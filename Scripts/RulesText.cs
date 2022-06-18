using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RulesText : MonoBehaviour
{

    public Text rulesText;
    public Text title;

    private void Start()
    {
        SetRulesText();
    }

    private void FixedUpdate()
    {
        if (Input.GetButton("Fire1"))
            SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    private void SetRulesText()
    {
        title.text = "Tank Destroyer";
        rulesText.text = "Rules:\r\n" +
                         "Sticks control treads\r\n" +
                         "Right trigger to shoot\r\n" +
                         "Press space to play again at any point\r\n" +
                         "Shoot to start\r\n" +
                         "Music: The Army Goes Rolling Along--Chiptune version";
    }
}
