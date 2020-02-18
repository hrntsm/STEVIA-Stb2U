using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorInput:MonoBehaviour {
    InputField m_inputField;
    public int m_num;
    public static Color[] m_memberColor = new Color[11];
    public static SaveColor m_saveColor = new SaveColor();

    void Start() {
        m_inputField = GetComponent<InputField>();

        if (PlayerPrefs.HasKey("UserColorData")) {
            string json = PlayerPrefs.GetString(SaveColorData._SaveKey);
            SaveColor loadColor = JsonUtility.FromJson<SaveColor>(json);
            m_inputField.GetComponent<Image>().color = GetMemberColor(loadColor.rgba[m_num]);
        }
    }

    public void ChangeColor(int i) {
        m_memberColor[i] = GetMemberColor(m_inputField.text);
        m_inputField.GetComponent<Image>().color = m_memberColor[i];
        m_saveColor.num[i] = i;
        m_saveColor.rgba[i] = m_inputField.text;
    }

    public static Color GetMemberColor(string inputText) {
        float[] inputRGBA = Array.ConvertAll<string, float>(inputText.Split(','), float.Parse);
        Color color = new Color(inputRGBA[0], inputRGBA[1], inputRGBA[2], inputRGBA[3]);
        return (color);
    }

}
