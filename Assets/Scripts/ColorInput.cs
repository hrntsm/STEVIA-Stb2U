using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorInput:MonoBehaviour {
    InputField inputField;
    public static Color[] memberColor = new Color[11];
    public static SaveColor saveColor = new SaveColor();
    void Start() {
        inputField = GetComponent<InputField>();
    }

    public void ChangeColor(int i) {
        memberColor[i] = GetMemberColor(inputField.text);
        inputField.GetComponent<Image>().color = memberColor[i];

        saveColor.num[i] = i;
        saveColor.rgba[i] = inputField.text;
    }

    public static Color GetMemberColor(string inputText) {
        float[] inputRGBA = Array.ConvertAll<string, float>(inputText.Split(','), float.Parse);
        Color color = new Color(inputRGBA[0], inputRGBA[1], inputRGBA[2], inputRGBA[3]);
        return (color);
    }

}
