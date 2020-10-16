using Model;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Valve.VR;

namespace VR
{
    public class ToUpperFloor:MonoBehaviour
    {
        [FormerlySerializedAs("_gameObject")] [SerializeField]
        private GameObject obj;

        private bool isClicked;

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
            int floorNum = obj.GetComponent<Dropdown>().value;
            if (isClicked && floorNum < StbReader.Stories.Height.Count - 1)
                obj.GetComponent<Dropdown>().value += 1;
        }
    }
}