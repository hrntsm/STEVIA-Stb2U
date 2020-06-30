using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stevia.UI
{
    public class ColorInit:MonoBehaviour
    {
        void Start()
        {
            if (PlayerPrefs.HasKey("UserColorData"))
                InitMemberColor(true);
            else
                InitMemberColor(false);
        }

        void InitMemberColor(bool init)
        {
            int i = 0;

            if (init)
            {
                string json = PlayerPrefs.GetString(SaveColorData._SaveKey);
                print(json);
                SaveColor loadColor = JsonUtility.FromJson<SaveColor>(json);
                while (i < 11)
                {
                    string inputText = loadColor.rgba[i];
                    ColorInput._memberColor[i] = ColorInput.GetMemberColor(inputText);
                    ColorInput._saveColor.num[i] = loadColor.num[i];
                    ColorInput._saveColor.rgba[i] = loadColor.rgba[i];
                    i++;
                }
            }
            else
            {
                while (i < 11)
                {
                    ColorInput._memberColor[i] = new Color(0.5f, 0.5f, 0.5f, 1);
                    ColorInput._saveColor.num[i] = i;
                    ColorInput._saveColor.rgba[i] = "0.5,0.5,0.5,1";
                    i++;
                }
            }
        }
    }
}
