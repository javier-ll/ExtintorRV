using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar escenas

public class LevelManager : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("JuegoFinal");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadResults()
    {
        SceneManager.LoadScene("Resultados");
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}