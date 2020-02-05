using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void Scene2StbOpen() {
        SceneManager.LoadScene("Stb2Unity");
    }
    public void Scene2Start() {
        SceneManager.LoadScene("Start");
    }
}
