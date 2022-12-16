using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NumPad : MonoBehaviour
{
    public TMP_InputField answer_text;
    // Start is called before the first frame update

    // Update is called once per frame
    public void NumberPadClick(string input)
    {
        if (input == "back" && answer_text.text != "")
        {
            answer_text.text = answer_text.text.Remove(answer_text.text.Length - 1);
        } else if (input != "back")
        {
            answer_text.text += input;
        }
    }
}
