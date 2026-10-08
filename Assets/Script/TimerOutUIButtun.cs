using UnityEngine;
using UnityEngine.SceneManagement;

public class TimerOutUIButtun : MonoBehaviour
{
    public int indexNumber;

   
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void LoadSceneByIndex()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(indexNumber);
    }
    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}
