using UnityEngine;
using UnityEngine.SceneManagement;

public class StartPlaying : MonoBehaviour
{
    // Start is called before the first frame update
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
