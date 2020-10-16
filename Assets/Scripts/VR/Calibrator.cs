using UnityEngine;

namespace VR
{
    public class Calibrator:MonoBehaviour 
    {
        [SerializeField] private Transform cameraRig;

        [SerializeField] private Transform hmdEye;

        [SerializeField, Range(0f, 3.0f)] private float charaHeight;

        public void Run() 
        {
            Vector3 position = cameraRig.position;
            float xPos = position.x;
            float yPos = position.y + (charaHeight - hmdEye.transform.localPosition.y);
            float zPos = position.z;
            position = new Vector3(xPos, yPos, zPos);
            cameraRig.position = position;
        }
    }
}
