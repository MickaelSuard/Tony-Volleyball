using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject menuPanel;
    public GameManager gameManager;

    void Start()
    {
        Time.timeScale = 0f;
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void PlaySolo()
    {
        if (gameManager != null)
        {
            Time.timeScale = 1f;
            menuPanel.SetActive(false);
            gameManager.InitializeGame(true); 
        }
    }

    public void PlayTwoPlayers()
    {
        if (gameManager != null)
        {
            Time.timeScale = 1f;
            menuPanel.SetActive(false);
            gameManager.InitializeGame(false); 
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitter");
        Application.Quit();
    }
}