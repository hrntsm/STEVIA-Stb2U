using UnityEngine;

namespace UI
{
    public class SaveColorData:MonoBehaviour 
    {
        public const string saveKey = "UserColorData";

        public void Save()
        {
            string json = JsonUtility.ToJson(ColorInput.SaveColor);
            print(json);
            PlayerPrefs.SetString(saveKey, json);
        }
    }

    [System.Serializable]
    public class SaveColor 
    {
        public int[] num = new int[11];
        public string[] rgba = new string[11];
        public string[] kind = { "RC", "RC", "RC", "RC", "RC", "RC", "S", "S", "S", "S", "S" };
        public string[] type = { "Column", "Post", "Girder", "Beam", "Brace", "Slab", "Column", "Post", "Girder", "Beam", "Brace" };
    }
}