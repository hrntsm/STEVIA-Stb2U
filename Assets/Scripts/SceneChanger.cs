using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Stevia {

    public class SceneChanger:MonoBehaviour {
        public void Scene2Stb2U4Desktop() {
            SceneManager.LoadScene("Stb2U4Desktop");
        }

        public void Scene2Stb2U4VR() {
            SceneManager.LoadScene("Stb2U4VR");
        }

        public void Scene2Start() {
            SceneManager.LoadScene("Start");
        }
    }
}