using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    void Start()
    {
        // Only destroy GameManager if we're in the Menu Screen
        // (so the game resets when returning to menu)
        // Don't destroy it in gameplay scenes!
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MenuScreen")
        {
            if (GameManager.Gary != null)
            {
                Debug.Log("MenuScript: Destroying GameManager to reset game");
                Destroy(GameManager.Gary.gameObject);
            }
        }
    }
    public void btn_StartTheGame()
    {
        SceneManager.LoadScene("Level1");
    }


    public void btn_Instructions()
    {
        SceneManager.LoadScene("HowToPlay");
    }

    public void btn_RestartLevel()
    {
        LevelManager.Larry.ReloadLevel();
    }

    public void btn_Credits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void btn_GoToMenu()
    {
        SceneManager.LoadScene("MenuScreen");
    }

    public void btn_Quit()
    {
        //tell the app to quit
        Application.Quit();
    }
    

}