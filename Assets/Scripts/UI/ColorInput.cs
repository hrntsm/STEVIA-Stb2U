using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class ColorInput:MonoBehaviour 
    {
        private InputField inputField;
        [FormerlySerializedAs("_num")] public int num;
        public static readonly Color[] MemberColor = new Color[11];
        public static readonly SaveColor SaveColor = new SaveColor();

        private void Start()
        {
            inputField = GetComponent<InputField>();

            if (!PlayerPrefs.HasKey("UserColorData"))
                return;
            string json = PlayerPrefs.GetString(SaveColorData.saveKey);
            var loadColor = JsonUtility.FromJson<SaveColor>(json);
            inputField.GetComponent<Image>().color = GetMemberColor(loadColor.rgba[num]);
        }

        public void ChangeColor(int i)
        {
            MemberColor[i] = GetMemberColor(inputField.text);
            inputField.GetComponent<Image>().color = MemberColor[i];
            SaveColor.num[i] = i;
            SaveColor.rgba[i] = inputField.text;
        }

        public static Color GetMemberColor(string inputText)
        {
            float[] inputRgba = Array.ConvertAll<string, float>(inputText.Split(','), float.Parse);
            var color = new Color(inputRgba[0], inputRgba[1], inputRgba[2], inputRgba[3]);
            return (color);
        }
    }
}
