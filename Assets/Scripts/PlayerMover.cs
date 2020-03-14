using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Stevia {

    public class PlayerMover:MonoBehaviour {

        SteamVR_Input_Sources _source;
        SteamVR_Action_Boolean _actionBoolean;
        SteamVR_Action_Vector2 _actionVector2;
        GameObject _vrmObject;

        bool _click;
        float _tpadX;
        float _tpadY;

        float _moveSpeed = 2.0f;
        
        void Start() {
            _actionBoolean = SteamVR_Actions._default.Teleport;
            _actionVector2 = SteamVR_Actions._default.tpad;
        }
        
        void Update() {
            if (!_vrmObject) {
                _vrmObject = GameObject.Find("VRM");
            }
            _click = _actionBoolean.GetState(_source);
            _tpadX = _actionVector2.GetAxis(_source).x;
            _tpadY = _actionVector2.GetAxis(_source).y;

            // VRMのforworadにすることで、向かっている正面をキーの前ボタンと対応させた。
            //if (Input.GetKey(KeyCode.W) || (_click && _tpadY > 0 && _tpadX < 0.7f && _tpadX > -0.7f)) {
            if (_click && _tpadY > 0 && _tpadX < 0.5f && _tpadX > -0.5f) {
                transform.position += _vrmObject.transform.forward * Time.deltaTime * _moveSpeed;
            }
            //if (Input.GetKey(KeyCode.S) || (_click && _tpadY < 0 && _tpadX < 0.7f && _tpadX > -0.7f)) {
            if (_click && _tpadY < 0 && _tpadX < 0.5f && _tpadX > -0.5f) {
                transform.position -= _vrmObject.transform.forward * Time.deltaTime * _moveSpeed;
            }
            //if (Input.GetKey(KeyCode.A) || (_click && _tpadX < 0 && _tpadY < 0.7f && _tpadY > -0.7f)) {
            if (_click && _tpadX < 0 && _tpadY < 0.5f && _tpadY > -0.5f) {
                transform.position -= _vrmObject.transform.right * Time.deltaTime * _moveSpeed;
            }
            //if (Input.GetKey(KeyCode.D) || (_click && _tpadX > 0 && _tpadY < 0.7f && _tpadY > -0.7f)) {
            if (_click && _tpadX > 0 && _tpadY < 0.5f && _tpadY > -0.5f) {
                transform.position += _vrmObject.transform.right * Time.deltaTime * _moveSpeed;
            }
        }
    }
}
