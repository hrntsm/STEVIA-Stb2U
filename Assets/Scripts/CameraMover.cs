/* thanks Nekomasu-San
 * https://qiita.com/Nekomasu/items/f195db36a2516e0dd460
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class CameraMover:MonoBehaviour {
    // WASD：前後左右の移動
    // QE：上昇・降下
    // 右ドラッグ：カメラの回転
    // 左ドラッグ：前後左右の移動
    // スペース：カメラ操作の有効・無効の切り替え
    // P：回転を実行時の状態に初期化する

    //カメラの移動量
    [SerializeField, Range(0.1f, 10.0f)]
    private float m_positionStep = 2.0f;

    //マウス感度
    [SerializeField, Range(30.0f, 150.0f)]
    private float m_mouseSensitive = 90.0f;

    //カメラ操作の有効無効
    private bool m_cameraMoveActive = true;
    //カメラのtransform  
    private Transform m_camTransform;
    //マウスの始点 
    private Vector3 m_startMousePos;
    //カメラ回転の始点情報
    private Vector3 m_presentCamRotation;
    private Vector3 m_presentCamPos;
    //初期状態 Rotation
    private Quaternion m_initialCamRotation;
    //UIメッセージの表示
    private bool m_uiMessageActiv;

    void Start() {
        m_camTransform = this.gameObject.transform;

        //初期回転の保存
        m_initialCamRotation = this.gameObject.transform.rotation;
    }

    void Update() {

        CamControlIsActive(); //カメラ操作の有効無効

        if (m_cameraMoveActive) {
            ResetCameraRotation(); //回転角度のみリセット
            CameraRotationMouseControl(); //カメラの回転 マウス
            CameraSlideMouseControl(); //カメラの縦横移動 マウス
            CameraPositionKeyControl(); //カメラのローカル移動 キー
        }
    }

    //カメラ操作の有効無効
    public void CamControlIsActive() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            m_cameraMoveActive = !m_cameraMoveActive;

            if (m_uiMessageActiv == false) {
                StartCoroutine(DisplayUiMessage());
            }
            Debug.Log("CamControl : " + m_cameraMoveActive);
        }
    }

    //回転を初期状態にする
    private void ResetCameraRotation() {
        if (Input.GetKeyDown(KeyCode.P)) {
            this.gameObject.transform.rotation = m_initialCamRotation;
            Debug.Log("Cam Rotate : " + m_initialCamRotation.ToString());
        }
    }

    //カメラの回転 マウス
    private void CameraRotationMouseControl() {
        if (Input.GetMouseButtonDown(0)) {
            m_startMousePos = Input.mousePosition;
            m_presentCamRotation.x = m_camTransform.transform.eulerAngles.x;
            m_presentCamRotation.y = m_camTransform.transform.eulerAngles.y;
        }

        if (Input.GetMouseButton(0)) {
            //(移動開始座標 - マウスの現在座標) / 解像度 で正規化
            float x = (m_startMousePos.x - Input.mousePosition.x) / Screen.width;
            float y = (m_startMousePos.y - Input.mousePosition.y) / Screen.height;

            //回転開始角度 ＋ マウスの変化量 * マウス感度
            float eulerX = m_presentCamRotation.x + y * m_mouseSensitive;
            float eulerY = m_presentCamRotation.y - x * m_mouseSensitive;

            m_camTransform.rotation = Quaternion.Euler(eulerX, eulerY, 0);
        }
    }

    //カメラの移動 マウス
    private void CameraSlideMouseControl() {
        if (Input.GetMouseButtonDown(1)) {
            m_startMousePos = Input.mousePosition;
            m_presentCamPos = m_camTransform.position;
        }

        if (Input.GetMouseButton(1)) {
            //(移動開始座標 - マウスの現在座標) / 解像度 で正規化
            float x = (m_startMousePos.x - Input.mousePosition.x) / Screen.width;
            float y = (m_startMousePos.y - Input.mousePosition.y) / Screen.height;

            x = x * m_positionStep;
            y = y * m_positionStep;

            Vector3 velocity = m_camTransform.rotation * new Vector3(x, y, 0);
            velocity = velocity + m_presentCamPos;
            m_camTransform.position = velocity;
        }
    }

    //カメラのローカル移動 キー
    private void CameraPositionKeyControl() {
        Vector3 campos = m_camTransform.position;

        if (Input.GetKey(KeyCode.D)) { campos += m_camTransform.right * Time.deltaTime * m_positionStep; }
        if (Input.GetKey(KeyCode.A)) { campos -= m_camTransform.right * Time.deltaTime * m_positionStep; }
        if (Input.GetKey(KeyCode.E)) { campos += m_camTransform.up * Time.deltaTime * m_positionStep; }
        if (Input.GetKey(KeyCode.Q)) { campos -= m_camTransform.up * Time.deltaTime * m_positionStep; }
        if (Input.GetKey(KeyCode.W)) { campos += m_camTransform.forward * Time.deltaTime * m_positionStep; }
        if (Input.GetKey(KeyCode.S)) { campos -= m_camTransform.forward * Time.deltaTime * m_positionStep; }

        m_camTransform.position = campos;
    }

    //UIメッセージの表示
    private IEnumerator DisplayUiMessage() {
        m_uiMessageActiv = true;
        float time = 0;
        while (time < 2) {
            time = time + Time.deltaTime;
            yield return null;
        }
        m_uiMessageActiv = false;
    }

    void OnGUI() {
        if (m_uiMessageActiv == false) { return; }
        GUI.color = Color.black;
        if (m_cameraMoveActive == true) {
            GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height - 30, 100, 20), "カメラ操作 有効");
        }

        if (m_cameraMoveActive == false) {
            GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height - 30, 100, 20), "カメラ操作 無効");
        }
    }

}
