using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Valve.VR;

namespace VR 
{
    public class MenuChoice:MonoBehaviour
    {
        [FormerlySerializedAs("_gameObject")] [SerializeField]
        private List<GameObject> objList;

        private bool isClicked;
        private bool isActiveVR = true;

        private const SteamVR_Input_Sources SrcAny = SteamVR_Input_Sources.Any;
        private SteamVR_Action_Boolean actionBoolean;

        private void Start()
        {
            actionBoolean = SteamVR_Actions._default.InteractUI;
        }

        private void Update()
        {
            isClicked = actionBoolean.GetState(SrcAny);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isActiveVR && isClicked)
            {
                foreach (GameObject obj in objList)
                    obj.GetComponent<Toggle>().isOn = false;

                isActiveVR = false;
            }
            else if ((isActiveVR == false) && isClicked)
            {
                foreach (GameObject obj in objList)
                    obj.GetComponent<Toggle>().isOn = true;

                isActiveVR = true;
            }
        }
    }
}