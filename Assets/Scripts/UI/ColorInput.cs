using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Stevia.UI
{
    public class ColorInput:MonoBehaviour 
    {
        InputField _inputField;
        public int _num;
        public static Color[] _memberColor = new Color[11];
        public static SaveColor _saveColor = new SaveColor();

        void Start()
        {
            _inputField = GetComponent<InputField>();

            if (PlayerPrefs.HasKey("UserColorData"))
            {
                string json = PlayerPrefs.GetString(SaveColorData._SaveKey);
                SaveColor loadColor = JsonUtility.FromJson<SaveColor>(json);
                _inputField.GetComponent<Image>().color = GetMemberColor(loadColor.rgba[_num]);
            }
        }

        public void ChangeColor(int i)
        {
            _memberColor[i] = GetMemberColor(_inputField.text);
            _inputField.GetComponent<Image>().color = _memberColor[i];
            _saveColor.num[i] = i;
            _saveColor.rgba[i] = _inputField.text;
        }

        public static Color GetMemberColor(string inputText)
        {
            float[] inputRGBA = Array.ConvertAll<string, float>(inputText.Split(','), float.Parse);
            Color color = new Color(inputRGBA[0], inputRGBA[1], inputRGBA[2], inputRGBA[3]);
            return (color);
        }
    }
}
