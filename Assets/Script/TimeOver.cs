using TMPro;
using UnityEngine;

public class TimeOver : MonoBehaviour
{
    public float timeRemaining = 120f; // 2 minutes
    public TMP_Text timerText;
    public GameObject gameOverPanel;
    public GameObject controlPanal;
    


    
    public AudioSource tbMusic;
    public AudioSource bgMusic;
    public GameObject tgMusic;

    private bool timerRunning = true;

    void Update()
    {
        if (!timerRunning)
            return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimer();
        }
        else
        {
            timeRemaining = 0;
            timerRunning = false;
            GameOver();
        }

        if (timeRemaining < 60f)
        {
            
            timerText.color = Color.red; // Change color to red when less than 1 minute

            bgMusic.volume = 0.25f;
            tbMusic.volume = 0.4f;
        }
        if (timeRemaining < 90f)
        {
            tgMusic.SetActive(true);
            
        }
    }

    void UpdateTimer()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void GameOver()
    {
        gameOverPanel.SetActive(true);
        controlPanal.SetActive(false);
       

        Time.timeScale = 0f; // Pause the game
    }
}
