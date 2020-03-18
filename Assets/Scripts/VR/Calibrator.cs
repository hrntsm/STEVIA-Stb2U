using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stevia.VR {

    public class Calibrator:MonoBehaviour {

        [SerializeField]
        Transform cameraRig;

        [SerializeField]
        Transform hmdEye;

        [SerializeField, Range(0f, 3.0f)]
        float charaHeight;

        public void Run() {
            float height = charaHeight - hmdEye.transform.localPosition.y;
            cameraRig.position = new Vector3(0, height, 0);
        }
    }
}
