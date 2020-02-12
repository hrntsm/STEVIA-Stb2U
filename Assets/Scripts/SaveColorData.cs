using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveColorData: MonoBehaviour{
    public const string _SaveKey = "UserColorData";

    public void Save() {
        string json = JsonUtility.ToJson(ColorInput.saveColor);
        PlayerPrefs.SetString(_SaveKey, json);
    }
}

[System.Serializable]
public class SaveColor {
    public int[] num = new int[11];
    public string[] rgba = new string[11];
}
