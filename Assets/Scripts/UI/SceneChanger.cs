using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI 
{
    public class SceneChanger:MonoBehaviour 
    {
        public void Scene2Stb2U4Desktop()
        {
            SceneManager.LoadScene("SteviaDesktop");
        }

        public void Scene2Stb2U4VR()
        {
            SceneManager.LoadScene("SteviaVR");
        }

        public void Scene2Start()
        {
            SceneManager.LoadScene("Start");
        }
    }
}