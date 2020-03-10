//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Valve.VR {
//    public class player:MonoBehaviour {

//        private SteamVR_ActionSet[] actionSets;
//        private SteamVR_ActionSet set;
//        private SteamVR_Action action;
//        private SteamVR_Action tpad;

//        private SteamVR_Input_Sources source;
//        private SteamVR_Action_Boolean actionBoolean;
//        private SteamVR_Action_Vector2 actionVector2;

//        private bool click;
//        private float tpad_x;
//        private float tpad_y;

//        private float mv_sp = 0.01f;

//        // Use this for initialization
//        void Start() {
//            source = SteamVR_Input_Source.GetSource(2);

//            if (actionSets == null) {
//                actionSets = SteamVR_Input_References.instance.actionSetObjects;
//            }
//            set = actionSets[0];//default
//            action = set.allActions[1];//teleport click
//            tpad = set.allActions[8];//tpad

//            actionBoolean = (SteamVR_Action_Boolean)action;
//            actionVector2 = (SteamVR_Action_Vector2)tpad;
//        }

//        // Update is called once per frame
//        void Update() {
//            click = actionBoolean.GetState(source);
//            tpad_x = actionVector2.GetAxis(source).x;
//            tpad_y = actionVector2.GetAxis(source).y;

//            if (Input.GetKey(KeyCode.W) || (click && tpad_y > 0 && tpad_x < 0.7f && tpad_x > -0.7f)) {
//                transform.position = new Vector3(transform.position.x, 0f, transform.position.z + mv_sp);
//            }
//            if (Input.GetKey(KeyCode.A) || (click && tpad_x < 0 && tpad_y < 0.7f && tpad_y > -0.7f)) {
//                transform.position = new Vector3(transform.position.x - mv_sp, 0f, transform.position.z);
//            }
//            if (Input.GetKey(KeyCode.S) || (click && tpad_y < 0 && tpad_x < 0.7f && tpad_x > -0.7f)) {
//                transform.position = new Vector3(transform.position.x, 0f, transform.position.z - mv_sp);
//            }
//            if (Input.GetKey(KeyCode.D) || (click && tpad_x > 0 && tpad_y < 0.7f && tpad_y > -0.7f)) {
//                transform.position = new Vector3(transform.position.x + mv_sp, 0f, transform.position.z);
//            }
//        }
//    }
//}

