using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // Référence au panneau menu
    public GameObject menuPanel;

    // Bouton Start
    public void StartGame()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false); // Masque le menu
        }

        // Ici tu peux activer d'autres choses si besoin, comme commencer le jeu
        // Exemple : activer les joueurs ou la balle
    }

    // Bouton Quit
    public void QuitGame()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
    }
}