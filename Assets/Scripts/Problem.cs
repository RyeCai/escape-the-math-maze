using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 
using System.Globalization;
using TMPro;

public class Problem : MonoBehaviour
{

    private GameObject fps_player_obj;
    private Level level;
    private AudioSource source;
    private AudioClip win_sound;
    public TMP_InputField input;
    private TMP_Text placehold; 
    public Button submit;
    private Canvas panel;
    // private GameObject p;
    private int answer;
    private int solution;
    public TMP_Text question;
    private Canvas problems;
    private bool solved;
    // public GameObject panel;
    
    // Use this for initialization
    private int operand1;
    private int operand2;
    private string arith_operator;
    string[] arith_operator_types = { "+", "-", "*", "/" };
    private bool touching;
    private Color gray = new Color(50, 50, 50, 128);
    private Color incorrect = new Color(255, 0, 0, 128);
    void Start()
    {
        Canvas[] panels = GameObject.FindObjectsOfType<Canvas>(true);
        foreach (Canvas pan in panels)
        {
            if (pan.tag == "prob")
            {
                panel = pan;
                break;
            }
        }
        placehold = input.placeholder.GetComponent<TMP_Text>();
        touching = false;
        fps_player_obj = GameObject.FindGameObjectWithTag("PLAYER");
        //GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        //level = level_obj.GetComponent<Level>();
        //GameObject canvas_obj = GameObject.FindGameObjectWithTag("ProblemCanvas");
        //problems = canvas_obj.GetComponent<Canvas>();
        solved = false;
        //panel = GameObject.Instantiate(prob) as GameObject;

        //panel.gameObject.SetActive(true);
        //panel.transform.SetParent(problems.transform,false);

        //Generate random math problem
        bool hardest_problem = gameObject.tag == "door";
        operand1 = hardest_problem ? Random.Range(3, 20) : Random.Range(2, 9);
        operand2 = hardest_problem ? Random.Range(3, 20) : Random.Range(2, 9);
        arith_operator = arith_operator_types[Random.Range(0, 3)];

        switch (arith_operator)
        {
            case "+":
                solution = operand1 + operand2;
                break;
            case "-":
                int tempOp1 = operand1;
                int tempOp2 = operand2;
                operand1 = Mathf.Max(tempOp1, tempOp2);
                operand2 = Mathf.Min(tempOp1, tempOp2);
                solution = operand1 - operand2;
                break;
            case "*":
                solution = operand1 * operand2;
                arith_operator = "×";
                break;
            case "/":
                //First operand becomes multiplication of generated numbers
                solution = operand1;
                operand1 *= operand2;
                arith_operator = "÷";
                break;
        }

        //if (level == null)
        //{
        //    Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
        //    return;
        //}
        
        panel.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "PLAYER" && !solved)
        {
            //Debug.Log("oop");
            touching = true;
            question.text = operand1.ToString() + arith_operator + operand2.ToString();
            panel.gameObject.SetActive(true);
            input.ActivateInputField();
            placehold.text = "Answer";
            placehold.color = gray;
            input.text = "";
        }
    }

    //Decreases number of remaining problems if solved correctly
    private void SubmitAnswer(){
        
        if (answer == solution){
            solved = true;
            gameObject.GetComponent<Renderer>().material.color = Color.green;
            panel.gameObject.SetActive(false);
            touching = false;
        } else{
            input.text = "";
            placehold.text = "Incorrect";
            placehold.color = incorrect;
        }
    }

    private void Update()
    {
        if (panel.gameObject.activeSelf && touching)
        {
            float distance = (gameObject.transform.position - fps_player_obj.gameObject.transform.position).magnitude;
            bool correctForm = int.TryParse(input.text, out int a);
            answer = correctForm ? a : answer;
            submit.onClick.RemoveAllListeners();
            submit.onClick.AddListener(SubmitAnswer);
            if (distance > 2 || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                panel.gameObject.SetActive(false);
                touching = false;  
            } else if (Input.GetKeyUp(KeyCode.Return))
            {
                SubmitAnswer();
            }
        }
    }

}