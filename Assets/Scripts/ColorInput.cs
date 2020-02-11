using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorInput:MonoBehaviour {
    InputField inputField;
    string inputText;
    public static Color[] memberColor = new Color[9];
    // Start is called before the first frame update
    void Start() {
        inputField = GetComponent<InputField>();
        InitMemberColor();
    }

    public void ChangeColor(int i) {
        inputText = inputField.text;
        float[] rgba = Array.ConvertAll<string, float>(inputText.Split(','), float.Parse);
        memberColor[i] = new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
        inputField.GetComponent<Image>().color = memberColor[i];
    }

    void InitMemberColor() {
        int i = 0;
        while (i < 9) {
            memberColor[i] = new Color(1, 1, 1, 1);
            i++;
        }
    }

}
