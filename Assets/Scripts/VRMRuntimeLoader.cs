using System.IO;
using System.Collections;
using System.Collections.Generic;
using VRM;
using UnityEngine;
using SFB;
using RootMotion.FinalIK;

public class VRMRuntimeLoader : MonoBehaviour {
    GameObject m_model;

    // Start is called before the first frame update
    void Start() {
        LoadVRM();
    }

    // Update is called once per frame
    void Update() {

    }

    void LoadVRM() {
        var extensions = new[] {
            new ExtensionFilter("VRM Files", "vrm", "VRM" ),
            new ExtensionFilter("All Files", "*" ),
        };
        string path = StandaloneFileBrowser.OpenFilePanel("Open VRM", "", extensions, true)[0]; if (string.IsNullOrEmpty(path)) {
            return;
        }

        var bytes = File.ReadAllBytes(path);
        // なんらかの方法でByte列を得た

        var context = new VRMImporterContext();

        // GLB形式でJSONを取得しParseします
        context.ParseGlb(bytes);

        // metaを取得(todo: thumbnailテクスチャのロード)
        var meta = context.ReadMeta();
        Debug.LogFormat("meta: title:{0}", meta.Title);

        // ParseしたJSONをシーンオブジェクトに変換していく
        context.LoadAsync(() => OnLoaded(context));
    }

    void OnLoaded(VRMImporterContext context) {
        if (m_model != null) {
            GameObject.Destroy(m_model.gameObject);
        }

        m_model = context.Root;
        m_model.transform.position = new Vector3(-10, 0, 0);
        m_model.transform.rotation = Quaternion.Euler(0, 270, 0);

        context.ShowMeshes();
        context.EnableUpdateWhenOffscreen();

        // VR化用に追記
        m_model.AddComponent<VRIK>();
        m_model.GetComponent<VRM.VRMFirstPerson>().Setup();
        foreach (var renderer in GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
            renderer.updateWhenOffscreen = true;
        }
    }
}
