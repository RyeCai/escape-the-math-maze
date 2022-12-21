using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WinScreen : MonoBehaviour
{
    public Button Menu;
    public Button Submit;
    public TMP_InputField input;
    private string name;
    // Start is called before the first frame update
    void Start()
    {
        input.onValueChanged.AddListener(delegate{report();});
        Submit.onClick.AddListener(delegate{submitScore();});
        Menu.onClick.AddListener(delegate{ProcessButtonInput("MainMenu");});
    }

    void ProcessButtonInput(string scene){
            SceneManager.LoadScene(scene);
    }

    void report(){
        name = input.text;
    }
    void submitScore(){
        StaticData.name = name;
    }
}
