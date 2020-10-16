using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DisplaySettings:MonoBehaviour
    {
        private Toggle toggle;
        private GameObject dispObject;
        private string findName = string.Empty;
        private bool hasObj;
        private Animator animMenu;
        private static readonly int MenuOpen = Animator.StringToHash("MenuOpen");

        private void Start() 
        {
            toggle = GetComponent<Toggle>();
            animMenu = gameObject.GetComponent<Animator>();
        }

        public void ElementDisp(int index) 
        {
            switch (index)
            {
                case 0: findName = "StbColumns"; break;
                case 1: findName = "StbColumnBar"; break;
                case 2: findName = "StbGirders"; break;
                case 3: findName = "StbGirderBar"; break;
                case 4: findName = "StbPosts"; break;
                case 5: findName = "StbPostBar"; break;
                case 6: findName = "StbBeams"; break;
                case 7: findName = "StbBeamBar"; break;
                case 8: findName = "StbBraces"; break;
                case 9: findName = "StbBraceBar"; break;
                case 10: findName = "StbSlabs"; break;
                case 11: findName = "StbSlabBar"; break;
            }
            if (hasObj == false) 
            {
                GameObject stbData = GameObject.Find("StbData");
                dispObject = stbData.transform.Find(findName).gameObject;
                hasObj = true;
            }
            dispObject.SetActive(toggle.isOn);
        }

        public static void BarOff()
        {
            for (var index = 0; index < 6; index++) 
            {
                string barName;
                switch (index) 
                {
                    case 0: barName = "StbColumnBar"; break;
                    case 1: barName = "StbGirderBar"; break;
                    case 2: barName = "StbPostBar"; break;
                    case 3: barName = "StbBeamBar"; break;
                    case 4: barName = "StbBraceBar"; break;
                    case 5: barName = "StbSlabBar"; break;
                    default: barName = ""; break;
                }
                GameObject barObject = GameObject.Find(barName);
                barObject.SetActive(false);
            }
        }

        public void Hamburger()
        {
            animMenu.SetInteger(MenuOpen, animMenu.GetInteger(MenuOpen) == 0 ? 1 : 0);
        }
    }
}
