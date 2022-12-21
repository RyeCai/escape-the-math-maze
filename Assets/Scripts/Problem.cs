using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 
using System.Globalization;
using TMPro;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;

public class Problem : MonoBehaviour
{

    private GameObject fps_player_obj;
    private MouseLook cursor;
    private Level level;
    private AudioSource source;
    private AudioClip win_sound;
    public TMP_Text question;
    public TMP_Text prompt;
    public TMP_InputField input;
    private TMP_Text placehold; 
    public Button submit;
    private Canvas panel;
    // private GameObject p;
    private int answer;
    private int solution;
    private bool solved;
    // public GameObject panel;

    // Use this for initialization
    private bool hardest_problem;
    private int operand1;
    private int operand2;
    private string arith_operator;
    string[] arith_operator_types = { "+", "-", "*", "/" };
    private bool touching;
    private Color gray = new Color(0, 0, 0, 250);
    private Color incorrect = new Color(255, 0, 0, 128);
    void Start()
    {
        Canvas[] panels = GameObject.FindObjectsOfType<Canvas>(true);
        foreach (Canvas pan in panels)
        {
            if (pan.tag == "ProblemCanvas")
            {
                panel = pan;
                break;
            }
        }

        placehold = input.placeholder.GetComponent<TMP_Text>();
        touching = false;
        fps_player_obj = GameObject.FindGameObjectWithTag("PLAYER");
        cursor = fps_player_obj.GetComponent<RigidbodyFirstPersonController>().mouseLook;
        solved = false;

        hardest_problem = gameObject.tag == "Hard";
        operand1 = hardest_problem ? Random.Range(6, 20) : Random.Range(2, 9);
        operand2 = hardest_problem ? Random.Range(6, 20) : Random.Range(2, 9);
        arith_operator = arith_operator_types[hardest_problem ? Random.Range(2, 3) : Random.Range(0, 3)];

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
        
        panel.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "PLAYER" && !solved)
        {
            touching = true;
            question.text = operand1.ToString() + " " + arith_operator +  " " + operand2.ToString();
            panel.gameObject.SetActive(true);
            input.text = "";
            input.ActivateInputField();
            placehold.text = "Answer";
            placehold.color = gray;
            cursor.SetCursorLock(false);
            if (hardest_problem)
            {
                prompt.text = "Enter the Code to your Sleigh:";
            }
        }
    }

    //Decreases number of remaining problems if solved correctly
    private void SubmitAnswer(){
        
        if (answer == solution){
            cursor.SetCursorLock(true);
            solved = true;
            panel.gameObject.SetActive(false);
            touching = false;
            if (hardest_problem)
            {
                SceneManager.LoadScene(3);
            }
            Destroy(gameObject);
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
            float distance = (gameObject.transform.position - fps_player_obj.transform.position).magnitude;
            bool correctForm = int.TryParse(input.text, out int a);
            answer = correctForm ? a : answer;
            submit.onClick.RemoveAllListeners();
            submit.onClick.AddListener(SubmitAnswer);
            if (distance > 2 || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                cursor.SetCursorLock(true);
                panel.gameObject.SetActive(false);
                touching = false;  
            } else if (Input.GetKeyUp(KeyCode.Return))
            {
                SubmitAnswer();
            }
        }
    }
}