using UnityEngine;
using UnityEngine.UI;

namespace UI 
{
    public class CameraSettings:MonoBehaviour 
    {
        private InputField inputField;

        private void Start()
        {
            inputField = GetComponent<InputField>();
        }

        public void SetLensLength()
        {
            Camera cam = Camera.main;
            if (!(cam is null)) 
                cam.focalLength = float.Parse(inputField.text);
        }
    }
}