using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseScreen : MonoBehaviour
{

    public Button Restart;
    public Button MainMenu;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        List<Vector3> startPositions = new List<Vector3>();
        List<GameObject> powerUps = new List<GameObject>();
        List<GameObject> problems = new List<GameObject>();
        foreach (GameObject go in gameObjects)
        {
            if (go.tag == "Enemy" || go.tag == "PLAYER")
            {
                startPositions.Add(go.transform.position);
            }
            if(go.tag == "Invisible"){
                powerUps.Add(go);
            }
            if(go.name.Contains("Gift_Box")){
                problems.Add(go);
            }
        }
        MainMenu.onClick.AddListener(delegate{ProcessButtonInput("MainMenu");});
        Restart.onClick.AddListener(delegate{reset(startPositions, powerUps, problems);});
    }

    private void reset(List<Vector3> startPositions, List<GameObject> powerUps, List<GameObject> problems){
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        int i = 0;
        foreach (GameObject go in gameObjects){
            if (go.tag == "Enemy" || go.tag == "PLAYER"){
                go.transform.position = startPositions[i];
                i++;
            }
            // if (go.name.Contains("Gift_Box")){
            //     go.solved = false;
            // }
        }
        foreach(GameObject go in powerUps){
            GameObject goo = go;
        }
        StaticData.health = 3;
        StaticData.invisible = false;
        this.enabled = false;


    }
    void ProcessButtonInput(string scene){
            SceneManager.LoadScene(scene);
    }
}
