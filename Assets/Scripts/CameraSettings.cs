using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraSettings : MonoBehaviour{
    InputField m_inputField;

    void Start() {
        m_inputField = GetComponent<InputField>();
    }
    public void SetLensLength() {
        Camera cam = Camera.main;
        cam.focalLength = float.Parse(m_inputField.text);
    }
}
