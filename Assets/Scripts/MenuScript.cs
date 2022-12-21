using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public Button Play;
    public Button viewLeaderBoard;
    public Button closeLeaderboard;
    public Canvas leaderBoard;
    public GameObject h1;
    public Text h2;
    public Text h3;
    public Text h4;
    public Text h5;
    private string highscore1;
    private string highscore2;
    private string highscore3;
    private string highscore4;
    private string highscore5;
    // Start is called before the first frame update
    void Start()
    {
        leaderBoard.enabled = false;
        Play.onClick.AddListener(delegate{openLeaderBoard();});
        Play.onClick.AddListener(delegate{leaderBoard.enabled = false;});
        Play.onClick.AddListener(delegate{ProcessButtonInput("playground");});
    }


    void ProcessButtonInput(string scene){
            SceneManager.LoadScene(scene);
    }
    void openLeaderBoard(){
        highscore1 = "1.) " + PlayerPrefs.GetString("player1", "") + ": " + PlayerPrefs.GetFloat("highscore1", float.PositiveInfinity).ToString("0.00");
        highscore2 = "2.) " + PlayerPrefs.GetString("player2", "") + ": " + PlayerPrefs.GetFloat("highscore2", float.PositiveInfinity).ToString("0.00");
        highscore3 = "3.) " + PlayerPrefs.GetString("player3", "") + ": " + PlayerPrefs.GetFloat("highscore3", float.PositiveInfinity).ToString("0.00");
        highscore4 = "4.) " + PlayerPrefs.GetString("player4", "") + ": " + PlayerPrefs.GetFloat("highscore4", float.PositiveInfinity).ToString("0.00");
        highscore5 = "5.) " + PlayerPrefs.GetString("player5", "") + ": " + PlayerPrefs.GetFloat("highscore5", float.PositiveInfinity).ToString("0.00");
        h1.GetComponent<Text>().text = highscore1;
        h2.GetComponent<Text>().text = highscore2;
        h3.GetComponent<Text>().text = highscore3;
        h4.GetComponent<Text>().text = highscore4;
        h5.GetComponent<Text>().text = highscore5;
        leaderBoard.enabled = true;
    }
}
