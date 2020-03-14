using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Stevia {

    public class CameraSettings:MonoBehaviour {
        InputField _inputField;

        void Start() {
            _inputField = GetComponent<InputField>();
        }

        public void SetLensLength() {
            Camera cam = Camera.main;
            cam.focalLength = float.Parse(_inputField.text);
        }
    }
}