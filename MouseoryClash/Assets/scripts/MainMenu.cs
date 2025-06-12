using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject terrenoMenu;

    public void AbrirMainMenu()
    {
        mainMenu.SetActive(true);
        terrenoMenu.SetActive(false);
    }
    public void AbrirTerrenoMenu()
    {
        mainMenu.SetActive(false);
        terrenoMenu.SetActive(true);
    }
    public void Salir()
    {
        Application.Quit();
    }

    public void JugarCastillo() {
        SceneManager.LoadScene("castleScene");
    }
    public void JugarColiseo()
    {
        SceneManager.LoadScene("coliseoScene");
    }
}
