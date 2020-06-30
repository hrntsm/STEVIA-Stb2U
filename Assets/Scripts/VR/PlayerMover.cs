using System.Collections;
using System.Collections.Generic;
using Stevia.Model;
using UnityEngine;
using UnityEngine.UI;

using Valve.VR;

namespace Stevia.VR
{
    public class PlayerMover:MonoBehaviour 
    {
        SteamVR_Input_Sources _srcRight = SteamVR_Input_Sources.RightHand;
        SteamVR_Input_Sources _srcLeft = SteamVR_Input_Sources.LeftHand;
        SteamVR_Action_Boolean _actionBoolean;
        SteamVR_Action_Vector2 _actionVector2;
        GameObject _vrmObject;

        bool _clickRight;
        float _tpadRightX;
        float _tpadRightY;

        bool _clickLeft;
        float _tpadLeftX;
        float _tpadLeftY;

        [SerializeField]
        float _moveSpeed = 2.0f;

        [SerializeField]
        float _rotateSpeed = 15.0f;
        
        void Start()
        {
            _actionBoolean = SteamVR_Actions._default.Teleport;
            _actionVector2 = SteamVR_Actions._default.tpad;
        }
        
        void Update()
        {
            if (!_vrmObject)
                _vrmObject = GameObject.Find("VRM");
            
            // 右手input
            _clickRight = _actionBoolean.GetState(_srcRight);
            _tpadRightX = _actionVector2.GetAxis(_srcRight).x;
            _tpadRightY = _actionVector2.GetAxis(_srcRight).y;
            
            // 左手input
            _clickLeft = _actionBoolean.GetState(_srcLeft);
            _tpadLeftX = _actionVector2.GetAxis(_srcLeft).x;
            _tpadLeftY = _actionVector2.GetAxis(_srcLeft).y;

            // 右のタッチパッドは移動を割り当て
            // VRMのforworadにすることで、向かっている正面をキーの前ボタンと対応させた。
            if (_clickRight && _tpadRightY > 0 && _tpadRightX < 0.7f && _tpadRightX > -0.7f) 
                transform.position += _vrmObject.transform.forward * (Time.deltaTime * _moveSpeed);
            
            if (_clickRight && _tpadRightY < 0 && _tpadRightX < 0.7f && _tpadRightX > -0.7f)
                transform.position -= _vrmObject.transform.forward * (Time.deltaTime * _moveSpeed);
            
            if (_clickRight && _tpadRightX < 0 && _tpadRightY < 0.7f && _tpadRightY > -0.7f)
                transform.position -= _vrmObject.transform.right * (Time.deltaTime * _moveSpeed);
            
            if (_clickRight && _tpadRightX > 0 && _tpadRightY < 0.7f && _tpadRightY > -0.7f)
                transform.position += _vrmObject.transform.right * (Time.deltaTime * _moveSpeed);
            

            // 左のタッチパッドは回転を割り当て
            // RotateAroundにVRMを入れることで、VRM中心に回転するように設定
            if (_clickLeft && _tpadLeftX < 0 && _tpadLeftY < 0.5f && _tpadLeftY > -0.5f)
                transform.RotateAround(_vrmObject.transform.position, transform.up, -Time.deltaTime * _rotateSpeed);

            if (_clickLeft && _tpadLeftX > 0 && _tpadLeftY < 0.5f && _tpadLeftY > -0.5f)
                transform.RotateAround(_vrmObject.transform.position, transform.up, Time.deltaTime * _rotateSpeed);
        }

        public void ChangeStory(Dropdown dropdown) 
        {
            var position = gameObject.transform.position;
            float xPos = position.x;
            float yPos = STBReader._storys.Height[dropdown.value];
            float zPos = position.z;
            position = new Vector3(xPos, yPos, zPos);
            gameObject.transform.position = position;

            var vrmPos = _vrmObject.transform.position;
            float xPosVrm = vrmPos.x;
            float yPosVrm = yPos;
            float zPosVrm = vrmPos.z;
            vrmPos = new Vector3(xPosVrm, yPosVrm, zPosVrm);
            _vrmObject.transform.position = vrmPos;
        }
    }
}
