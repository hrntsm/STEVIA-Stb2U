using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stevia.VR
{
    public class Calibrator:MonoBehaviour 
    {
        [SerializeField]
        Transform cameraRig;

        [SerializeField]
        Transform hmdEye;

        [SerializeField, Range(0f, 3.0f)]
        float charaHeight;

        public void Run() 
        {
            float xPos = cameraRig.position.x;
            float yPos = cameraRig.position.y + (charaHeight - hmdEye.transform.localPosition.y);
            float zPos = cameraRig.position.z;
            cameraRig.position = new Vector3(xPos, yPos, zPos);
        }
    }
}
