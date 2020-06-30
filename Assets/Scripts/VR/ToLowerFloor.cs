using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace Stevia.VR 
{
    public class ToLowerFloor:MonoBehaviour 
    {
        [SerializeField]
        GameObject _gameObject;
        
        /// <summary>
        /// コントローラーでトリガーがひかれているか
        /// </summary>
        bool _isClicked;

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
            int floorNum = _gameObject.GetComponent<Dropdown>().value;
            if (_isClicked && floorNum > 0)
                _gameObject.GetComponent<Dropdown>().value -= 1;
        }
    }
}