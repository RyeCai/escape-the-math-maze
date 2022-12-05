using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Problem : MonoBehaviour
{

    private GameObject fps_player_obj;
    private Level level;
    private AudioSource source;
    private AudioClip win_sound;
    private TMP_InputField input;
    private Button submit;
    private GameObject panel;
    // private GameObject p;
    private int answer;
    private int solution;
    private Text question;
    private Canvas problems;
    public bool solved;
    // public GameObject panel;
    
    // Use this for initialization
    void Start()
    {
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        GameObject canvas_obj = GameObject.FindGameObjectWithTag("ProblemCanvas");
        problems = canvas_obj.GetComponent<Canvas>();
        solved = false;
        GameObject prob = GameObject.FindGameObjectWithTag("prob");
        panel = GameObject.Instantiate(prob) as GameObject;

        panel.gameObject.SetActive(true);
        Debug.Log(panel);
        panel.transform.SetParent(problems.transform,false);

        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
        panel.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "PLAYER")
        {
            panel.SetActive(true);
        }
    }

    void submitAnswer(){
        if(answer == solution){
            solved = true;
            Debug.Log("Yay");
        }else{
            Debug.Log("No");
        }
        panel.SetActive(false);

    }
    void report(){
        int.TryParse(input.text, out int a);
        answer = a;
    }
}