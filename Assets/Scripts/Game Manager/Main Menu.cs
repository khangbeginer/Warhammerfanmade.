using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void play()
    {
        SceneManager.LoadScene("gameplay");



    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // Shows in Editor only
    }

}
