using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerMover:MonoBehaviour {

    private SteamVR_Input_Sources source;
    private SteamVR_Action_Boolean actionBoolean;
    private SteamVR_Action_Vector2 actionVector2;

    private bool click;
    private float tpad_x;
    private float tpad_y;

    private float mv_sp = 2.0f;

    // Use this for initialization
    void Start() {
        actionBoolean = SteamVR_Actions._default.Teleport;
        actionVector2 = SteamVR_Actions._default.tpad;
    }

    // Update is called once per frame
    void Update() {
        click = actionBoolean.GetState(source);
        tpad_x = actionVector2.GetAxis(source).x;
        tpad_y = actionVector2.GetAxis(source).y;

        // TODO 正面に向かって進むようにする。VRMのforworadにすればよい？
        if (Input.GetKey(KeyCode.W) || (click && tpad_y > 0 && tpad_x < 0.7f && tpad_x > -0.7f)) {
            transform.position += transform.forward * Time.deltaTime * mv_sp;
        }
        if (Input.GetKey(KeyCode.S) || (click && tpad_y < 0 && tpad_x < 0.7f && tpad_x > -0.7f)) {
            transform.position -= transform.forward * Time.deltaTime * mv_sp;
        }
        if (Input.GetKey(KeyCode.A) || (click && tpad_x < 0 && tpad_y < 0.7f && tpad_y > -0.7f)) {
            transform.position -= transform.right * Time.deltaTime * mv_sp;
        }
        if (Input.GetKey(KeyCode.D) || (click && tpad_x > 0 && tpad_y < 0.7f && tpad_y > -0.7f)) {
            transform.position += transform.right * Time.deltaTime * mv_sp;
        }
    }
}
