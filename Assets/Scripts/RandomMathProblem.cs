using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMathProblem : MonoBehaviour
{
    //public List<int> operands;
    //public List<string> operators;
    public int operator1;
    public int operator2;
    public string operand;
    public int answer;
    string[] operator_types = { "+", "-", "*", "/" };

    int num_operands;
    // Start is called before the first frame update
    void Start()
    {
        bool hardest_problem = gameObject.tag == "door";
        operator1 = hardest_problem ? Random.Range(3, 20) : Random.Range(2, 9);
        operator2 = hardest_problem ? Random.Range(3, 20) : Random.Range(2, 9);
        operand = operator_types[Random.Range(0, 3)];

        switch(operand)
        {
            case "+":
                answer = operator1 + operator2;
                break;
            case "-":
                answer = operator1 - operator2;
                break;
            case "*":
                answer = operator1 * operator2;
                break;
            case "/":
                //First operator becomes multiplication of generated numbers
                answer = operator1;
                operator1 *= operator2;
                break;
        }

        // If we want to do multiple operators/operands
        //int cur_operand;
        //string cur_operator;
        //num_operands = hardest_problem ? Random.Range(3, 4) : Random.Range(2, 3);
        //for (int i = 0; i < num_operands; i++)
        //{
        //    cur_operand = hardest_problem ? Random.Range(3, 20) : Random.Range(2, 9);
        //    operands.Add(cur_operand);
        //    if (i < num_operands - 1)
        //    {
        //        cur_operator = operator_types[Random.Range(0, 2)];
        //        operators.Add(cur_operator);
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
