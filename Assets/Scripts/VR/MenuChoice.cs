using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace Stevia.VR 
{
    public class MenuChoice:MonoBehaviour
    {
        [SerializeField]
        List<GameObject> _gameObject;

        bool _isClicked;
        bool _isActiveVR = true;

        SteamVR_Input_Sources _srcAny = SteamVR_Input_Sources.Any;
        SteamVR_Action_Boolean _actionBoolean;
        
        void Start()
        {
            _actionBoolean = SteamVR_Actions._default.InteractUI;
        }

        private void Update()
        {
            _isClicked = _actionBoolean.GetState(_srcAny);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isActiveVR && _isClicked)
            {
                foreach (var obj in _gameObject)
                    obj.GetComponent<Toggle>().isOn = false;

                _isActiveVR = false;
            }
            else if ((_isActiveVR == false) && _isClicked)
            {
                foreach (var obj in _gameObject)
                    obj.GetComponent<Toggle>().isOn = true;

                _isActiveVR = true;
            }
        }
    }
}