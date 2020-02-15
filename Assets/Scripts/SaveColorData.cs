using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveColorData: MonoBehaviour{
    public const string _SaveKey = "UserColorData";

    public void Save() {
        string json = JsonUtility.ToJson(ColorInput.m_saveColor);
        print(json);
        PlayerPrefs.SetString(_SaveKey, json);
    }
}

[System.Serializable]
public class SaveColor {
    public int[] num = new int[11];
    public string[] rgba = new string[11];
    public string[] kind = { "RC", "RC", "RC", "RC", "RC", "RC", "S", "S", "S", "S", "S" };
    public string[] type = { "Column", "Post", "Girder", "Beam", "Brace", "Slab", "Column", "Post", "Girder", "Beam", "Brace" };
}
