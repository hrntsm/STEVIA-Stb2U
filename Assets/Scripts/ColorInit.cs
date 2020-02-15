using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorInit : MonoBehaviour {

    void Start() {
        if (PlayerPrefs.HasKey("UserColorData"))
            InitMemberColor(true);
            //InitMemberColor(false);
        else
            InitMemberColor(false);
    }

    void InitMemberColor(bool init) {
        int i = 0;

        if (init) {
            string json = PlayerPrefs.GetString(SaveColorData._SaveKey);
            print(json);
            SaveColor loadColor = JsonUtility.FromJson<SaveColor>(json);
            while (i < 11) {
                string inputText = loadColor.rgba[i];
                ColorInput.m_memberColor[i] = ColorInput.GetMemberColor(inputText);
                ColorInput.m_saveColor.num[i] = loadColor.num[i];
                ColorInput.m_saveColor.rgba[i] = loadColor.rgba[i];
                i++;
            }
        }
        else {
            while (i < 11) {
                ColorInput.m_memberColor[i] = new Color(1, 1, 1, 1);
                ColorInput.m_saveColor.num[i] = i;
                ColorInput.m_saveColor.rgba[i] = "1,1,1,1";
                i++;
            }
        }
    }
}
