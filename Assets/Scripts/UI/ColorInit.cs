using UnityEngine;

namespace UI
{
    public class ColorInit:MonoBehaviour
    {
        private void Start()
        {
            InitMemberColor(PlayerPrefs.HasKey("UserColorData"));
        }

        private static void InitMemberColor(bool init)
        {
            var i = 0;

            if (init)
            {
                string json = PlayerPrefs.GetString(SaveColorData.saveKey);
                print(json);
                var loadColor = JsonUtility.FromJson<SaveColor>(json);
                while (i < 11)
                {
                    string inputText = loadColor.rgba[i];
                    ColorInput.MemberColor[i] = ColorInput.GetMemberColor(inputText);
                    ColorInput.SaveColor.num[i] = loadColor.num[i];
                    ColorInput.SaveColor.rgba[i] = loadColor.rgba[i];
                    i++;
                }
            }
            else
            {
                while (i < 11)
                {
                    ColorInput.MemberColor[i] = new Color(0.5f, 0.5f, 0.5f, 1);
                    ColorInput.SaveColor.num[i] = i;
                    ColorInput.SaveColor.rgba[i] = "0.5,0.5,0.5,1";
                    i++;
                }
            }
        }
    }
}
