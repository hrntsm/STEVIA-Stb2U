using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorInput:MonoBehaviour {
    InputField inputField;
    public static Color[] memberColor = new Color[11];
    public static SaveColor sCol = new SaveColor();
    // Start is called before the first frame update
    void Start() {
        inputField = GetComponent<InputField>();
        // Get the color setting of the past
        if (PlayerPrefs.HasKey("UserColorData"))
            InitMemberColor(true);
            //InitMemberColor(false);
        else
            InitMemberColor(false);
    }

    public void ChangeColor(int i) {
        memberColor[i] = GetMemberColor(inputField.text);
        inputField.GetComponent<Image>().color = memberColor[i];

        sCol.num[i] = i;
        sCol.rgba[i] = inputField.text;
    }

    void InitMemberColor(bool init) {
        int i = 0;
        if (init) {
            string json = PlayerPrefs.GetString(SaveColorData._SaveKey);
            print(json);
            SaveColor saveColor = JsonUtility.FromJson<SaveColor>(json);
            while (i < 11) {
                string inputText = saveColor.rgba[i];
                memberColor[i] = GetMemberColor(inputText);
                sCol.num[i] = saveColor.num[i];
                sCol.rgba[i] = saveColor.rgba[i];
                i++;
            }
        }
        else {
            while (i < 11) {
                memberColor[i] = new Color(1, 1, 1, 1);
                sCol.num[i] = i;
                sCol.rgba[i] = "1,1,1,1";
                i++;
            }
        }
    }

    Color GetMemberColor(string inputText) {
        float[] inputRGBA = Array.ConvertAll<string, float>(inputText.Split(','), float.Parse);
        Color color = new Color(inputRGBA[0], inputRGBA[1], inputRGBA[2], inputRGBA[3]);
        return (color);
    }

}
