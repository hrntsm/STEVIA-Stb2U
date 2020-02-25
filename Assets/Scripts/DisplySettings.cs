using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DisplySettings : MonoBehaviour {
    Toggle m_toggle;
    GameObject m_dispObject;
    string m_findName = "";
    bool m_hasObj = false;
    Animator animMenu;

    void Start() {
        m_toggle = GetComponent<Toggle>();
        animMenu = gameObject.GetComponent<Animator>();
    }

    public void ElementDisp(int index) {
        switch (index) {
            case 0: m_findName = "StbColumns"; break;
            case 1: m_findName = "StbColumnBar"; break;
            case 2: m_findName = "StbGirders"; break;
            case 3: m_findName = "StbGirderBar"; break;
            case 4: m_findName = "StbPosts"; break;
            case 5: m_findName = "StbPostBar"; break;
            case 6: m_findName = "StbBeams"; break;
            case 7: m_findName = "StbBeamBar"; break;
            case 8: m_findName = "StbBraces"; break;
            case 9: m_findName = "StbBraceBar"; break;
            case 10: m_findName = "StbSlabs"; break;
            case 11: m_findName = "StbSlabBar"; break;
            default: break;
        }
        if (m_hasObj == false) {
            m_dispObject = GameObject.Find(m_findName);
            m_hasObj = true;
        }
        m_dispObject.SetActive(m_toggle.isOn);
    }

    public void Hamburger() {
        if (animMenu.GetInteger("MenuOpen") == 0)
            animMenu.SetInteger("MenuOpen", 1);
        else
            animMenu.SetInteger("MenuOpen", 0);
    }
}
