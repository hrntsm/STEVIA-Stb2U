using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorInit : MonoBehaviour {
    void Start() {
        if (PlayerPrefs.HasKey("UserColorData"))
            InitMemberColor(true);
        else
            InitMemberColor(false);
    }

    void InitMemberColor(bool init) {
        int i = 0;
        if (init) {
            string json = PlayerPrefs.GetString(SaveColorData._SaveKey);
            SaveColor loadColor = JsonUtility.FromJson<SaveColor>(json);
            while (i < 11) {
                string inputText = loadColor.rgba[i];
                ColorInput.memberColor[i] = ColorInput.GetMemberColor(inputText);
                ColorInput.saveColor.num[i] = loadColor.num[i];
                ColorInput.saveColor.rgba[i] = loadColor.rgba[i];
                i++;
            }
        }
        else {
            while (i < 11) {
                ColorInput.memberColor[i] = new Color(1, 1, 1, 1);
                ColorInput.saveColor.num[i] = i;
                ColorInput.saveColor.rgba[i] = "1,1,1,1";
                i++;
            }
        }
    }
}
