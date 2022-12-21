using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public Button Play;
    // Start is called before the first frame update
    void Start()
    {
        Play.onClick.AddListener(delegate{ProcessButtonInput("playground");});
    }

    void ProcessButtonInput(string scene){
            SceneManager.LoadScene(scene);
    }
}
