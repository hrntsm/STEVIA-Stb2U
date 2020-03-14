using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Stevia {

    public class DisplySettings:MonoBehaviour {

        Toggle _toggle;
        GameObject _dispObject;
        string _findName = "";
        bool _hasObj = false;
        Animator _animMenu;

        void Start() {
            _toggle = GetComponent<Toggle>();
            _animMenu = gameObject.GetComponent<Animator>();
        }

        public void ElementDisp(int index) {
            switch (index) {
                case 0: _findName = "StbColumns"; break;
                case 1: _findName = "StbColumnBar"; break;
                case 2: _findName = "StbGirders"; break;
                case 3: _findName = "StbGirderBar"; break;
                case 4: _findName = "StbPosts"; break;
                case 5: _findName = "StbPostBar"; break;
                case 6: _findName = "StbBeams"; break;
                case 7: _findName = "StbBeamBar"; break;
                case 8: _findName = "StbBraces"; break;
                case 9: _findName = "StbBraceBar"; break;
                case 10: _findName = "StbSlabs"; break;
                case 11: _findName = "StbSlabBar"; break;
                default: break;
            }
            if (_hasObj == false) {
                _dispObject = GameObject.Find(_findName);
                _hasObj = true;
            }
            _dispObject.SetActive(_toggle.isOn);
        }

        public void Hamburger() {
            if (_animMenu.GetInteger("MenuOpen") == 0)
                _animMenu.SetInteger("MenuOpen", 1);
            else
                _animMenu.SetInteger("MenuOpen", 0);
        }
    }
}
