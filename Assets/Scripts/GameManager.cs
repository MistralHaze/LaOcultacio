using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour {

    //Time
    float timer;

    //Labels
    public Text timeLabel;
    public Text tokensLabel;
    public Text currentTokensLabel;
    public Text scoreLabel;
    public Text scoreCalculatedLabel;

    public GameObject youWin;
    public GameObject youLose;
    public GameObject pause;

    //Buttons
    public GameObject repeatLevelButton;
    //public GameObject nextLevelButton;
    public GameObject menuButton;

    public bool gameRunning = true;
    int collectedTokens = 0;


    void Start()
    {
        //Initialize Timers
        timer = 0;
        Time.timeScale = 1;
    }

    void Update()
    {
        if (gameRunning)
            RunTimer();
    }

    void RunTimer( )
    {
        timer += Time.deltaTime;


        //timeLabel.text = timer.ToString().Substring(0, 4);
        timeLabel.text = Mathf.Round(timer).ToString();

    }

    public void playerDead()
    {
        youLose.SetActive(true);
        menuButton.SetActive(true);
        repeatLevelButton.SetActive(true);
        gameRunning = false;
        Time.timeScale = 0;
    }

    public void pauseGame()
    {
        if (gameRunning)
        {
            pause.SetActive(true);
            menuButton.SetActive(true);
            repeatLevelButton.SetActive(true);
            gameRunning = false;
            Time.timeScale = 0;
        }
        else
        {
            pause.SetActive(false);
            menuButton.SetActive(false);
            repeatLevelButton.SetActive(false);
            gameRunning = true;
            Time.timeScale = 1;
        }
    }

    public void winGame()
    {
        youWin.SetActive(true);
        //nextLevelButton.SetActive(true);
        repeatLevelButton.SetActive(true);
        repeatLevelButton.SetActive(true);
        scoreLabel.enabled = true;
        scoreCalculatedLabel.enabled = true;
        int score = (int) timer * 10 + collectedTokens*500;
        scoreCalculatedLabel.text = score.ToString();
        gameRunning = false;
        Time.timeScale = 0;
    }

    public void levelRepeat()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void goToMenu()
    {
        gameRunning = true;
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
    public void nextLevel()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }
    public void tokenCollected()
    {
        collectedTokens++;
        currentTokensLabel.text = collectedTokens.ToString();
    }

}
