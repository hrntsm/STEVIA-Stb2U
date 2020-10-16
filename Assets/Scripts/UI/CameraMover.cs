/* Special thanks Nekomasu-San
 * https://qiita.com/Nekomasu/items/f195db36a2516e0dd460
 */

using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class CameraMover:MonoBehaviour 
    {
        // WASD：前後左右の移動
        // QE：上昇・降下
        // 右ドラッグ：カメラの回転
        // 左ドラッグ：前後左右の移動
        // スペース：カメラ操作の有効・無効の切り替え
        // P：回転を実行時の状態に初期化する

        //カメラの移動量
        [FormerlySerializedAs("_positionStep")] [SerializeField, Range(0.1f, 10.0f)]
        private float positionStep = 2.0f;

        //マウス感度
        [FormerlySerializedAs("_mouseSensitive")] [SerializeField, Range(30.0f, 150.0f)]
        private float mouseSensitive = 90.0f;

        //カメラ操作の有効無効
        private bool cameraMoveActive = true;
        //カメラのtransform  
        private Transform camTransform;
        //マウスの始点 
        private Vector3 startMousePos;
        //カメラ回転の始点情報
        private Vector3 presentCamRotation;

        private Vector3 presentCamPos;
        //初期状態 Rotation
        private Quaternion initialCamRotation;
        //UIメッセージの表示
        private bool uiMessageActive;

        private void Start()
        {
            GameObject obj = gameObject;
            camTransform = obj.transform;

            //初期回転の保存
            initialCamRotation = obj.transform.rotation;
        }

        private void Update()
        {
            CamControlIsActive(); //カメラ操作の有効無効

            if (!cameraMoveActive)
                return;
            ResetCameraRotation(); //回転角度のみリセット
            CameraRotationMouseControl(); //カメラの回転 マウス
            CameraSlideMouseControl(); //カメラの縦横移動 マウス
            CameraPositionKeyControl(); //カメラのローカル移動 キー
        }

        //カメラ操作の有効無効
        private void CamControlIsActive()
        {
            if (!Input.GetKeyDown(KeyCode.Space))
                return;
            cameraMoveActive = !cameraMoveActive;
            if (uiMessageActive == false)
                StartCoroutine(DisplayUiMessage());
        }

        //回転を初期状態にする
        private void ResetCameraRotation() 
        {
            if (Input.GetKeyDown(KeyCode.P))
                gameObject.transform.rotation = initialCamRotation;
        }

        //カメラの回転 マウス
        private void CameraRotationMouseControl() 
        {
            if (Input.GetMouseButtonDown(0))
            {
                startMousePos = Input.mousePosition;
                Vector3 eulerAngles = camTransform.transform.eulerAngles;
                presentCamRotation.x = eulerAngles.x;
                presentCamRotation.y = eulerAngles.y;
            }

            if (!Input.GetMouseButton(0))
                return;
            //(移動開始座標 - マウスの現在座標) / 解像度 で正規化
            float x = (startMousePos.x - Input.mousePosition.x) / Screen.width;
            float y = (startMousePos.y - Input.mousePosition.y) / Screen.height;

            //回転開始角度 ＋ マウスの変化量 * マウス感度
            float eulerX = presentCamRotation.x + y * mouseSensitive;
            float eulerY = presentCamRotation.y - x * mouseSensitive;

            camTransform.rotation = Quaternion.Euler(eulerX, eulerY, 0);
        }

        //カメラの移動 マウス
        private void CameraSlideMouseControl() 
        {
            if (Input.GetMouseButtonDown(1)) 
            {
                startMousePos = Input.mousePosition;
                presentCamPos = camTransform.position;
            }

            if (!Input.GetMouseButton(1))
                return;
            //(移動開始座標 - マウスの現在座標) / 解像度 で正規化
            float x = (startMousePos.x - Input.mousePosition.x) / Screen.width;
            float y = (startMousePos.y - Input.mousePosition.y) / Screen.height;

            x *= positionStep;
            y *= positionStep;

            Vector3 velocity = camTransform.rotation * new Vector3(x, y, 0);
            velocity += presentCamPos;
            camTransform.position = velocity;
        }

        //カメラのローカル移動 キー
        private void CameraPositionKeyControl()
        {
            Vector3 campos = camTransform.position;

            if (Input.GetKey(KeyCode.D))
                campos += camTransform.right * (Time.deltaTime * positionStep);
            if (Input.GetKey(KeyCode.A))
                campos -= camTransform.right * (Time.deltaTime * positionStep);
            if (Input.GetKey(KeyCode.E))
                campos += camTransform.up * (Time.deltaTime * positionStep);
            if (Input.GetKey(KeyCode.Q))
                campos -= camTransform.up * (Time.deltaTime * positionStep);
            if (Input.GetKey(KeyCode.W))
                campos += camTransform.forward * (Time.deltaTime * positionStep);
            if (Input.GetKey(KeyCode.S))
                campos -= camTransform.forward * (Time.deltaTime * positionStep);

            camTransform.position = campos;
        }

        //UIメッセージの表示
        private IEnumerator DisplayUiMessage()
        {
            uiMessageActive = true;
            float time = 0;
            while (time < 2) 
            {
                time = time + Time.deltaTime;
                yield return null;
            }
            uiMessageActive = false;
        }

        private void OnGUI()
        {
            if (uiMessageActive == false)
                return;
            GUI.color = Color.black;
            switch (cameraMoveActive)
            {
                case true:
                    GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height - 30, 100, 20), "カメラ操作 有効");
                    break;
                case false:
                    GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height - 30, 100, 20), "カメラ操作 無効");
                    break;
            }
        }
    }
}
