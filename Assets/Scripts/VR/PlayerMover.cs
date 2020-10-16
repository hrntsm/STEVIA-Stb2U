using Model;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Valve.VR;

namespace VR
{
    public class PlayerMover:MonoBehaviour 
    {
        private const SteamVR_Input_Sources SrcRight = SteamVR_Input_Sources.RightHand;
        private const SteamVR_Input_Sources SrcLeft = SteamVR_Input_Sources.LeftHand;
        private SteamVR_Action_Boolean actionBoolean;
        private SteamVR_Action_Vector2 actionVector2;
        private GameObject vrmObject;

        private bool clickRight;
        private float tpadRightX;
        private float tpadRightY;

        private bool clickLeft;
        private float tpadLeftX;
        private float tpadLeftY;

        [FormerlySerializedAs("_moveSpeed")] [SerializeField]
        private float moveSpeed = 2.0f;

        [FormerlySerializedAs("_rotateSpeed")] [SerializeField]
        private float rotateSpeed = 15.0f;

        private void Start()
        {
            actionBoolean = SteamVR_Actions._default.Teleport;
            actionVector2 = SteamVR_Actions._default.tpad;
        }

        private void Update()
        {
            if (!vrmObject)
                vrmObject = GameObject.Find("VRM");
            
            // 右手input
            clickRight = actionBoolean.GetState(SrcRight);
            tpadRightX = actionVector2.GetAxis(SrcRight).x;
            tpadRightY = actionVector2.GetAxis(SrcRight).y;
            
            // 左手input
            clickLeft = actionBoolean.GetState(SrcLeft);
            tpadLeftX = actionVector2.GetAxis(SrcLeft).x;
            tpadLeftY = actionVector2.GetAxis(SrcLeft).y;

            // 右のタッチパッドは移動を割り当て
            // VRMのforwardにすることで、向かっている正面をキーの前ボタンと対応させた。
            if (clickRight && tpadRightY > 0 && tpadRightX < 0.7f && tpadRightX > -0.7f) 
                transform.position += vrmObject.transform.forward * (Time.deltaTime * moveSpeed);
            
            if (clickRight && tpadRightY < 0 && tpadRightX < 0.7f && tpadRightX > -0.7f)
                transform.position -= vrmObject.transform.forward * (Time.deltaTime * moveSpeed);
            
            if (clickRight && tpadRightX < 0 && tpadRightY < 0.7f && tpadRightY > -0.7f)
                transform.position -= vrmObject.transform.right * (Time.deltaTime * moveSpeed);
            
            if (clickRight && tpadRightX > 0 && tpadRightY < 0.7f && tpadRightY > -0.7f)
                transform.position += vrmObject.transform.right * (Time.deltaTime * moveSpeed);
            

            // 左のタッチパッドは回転を割り当て
            // RotateAroundにVRMを入れることで、VRM中心に回転するように設定
            if (clickLeft && tpadLeftX < 0 && tpadLeftY < 0.5f && tpadLeftY > -0.5f)
                transform.RotateAround(vrmObject.transform.position, transform.up, -Time.deltaTime * rotateSpeed);

            if (clickLeft && tpadLeftX > 0 && tpadLeftY < 0.5f && tpadLeftY > -0.5f)
                transform.RotateAround(vrmObject.transform.position, transform.up, Time.deltaTime * rotateSpeed);
        }

        public void ChangeStory(Dropdown dropdown) 
        {
            GameObject obj = gameObject;
            Vector3 position = obj.transform.position;
            float xPos = position.x;
            float yPos = StbReader.Stories.Height[dropdown.value];
            float zPos = position.z;
            position = new Vector3(xPos, yPos, zPos);
            obj.transform.position = position;

            Vector3 vrmPos = vrmObject.transform.position;
            float xPosVrm = vrmPos.x;
            float yPosVrm = yPos;
            float zPosVrm = vrmPos.z;
            vrmPos = new Vector3(xPosVrm, yPosVrm, zPosVrm);
            vrmObject.transform.position = vrmPos;
        }
    }
}
