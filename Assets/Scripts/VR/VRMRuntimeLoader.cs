using System.IO;
using System.Collections;
using System.Collections.Generic;
using VRM;
using UnityEngine;
using SFB;
using RootMotion.FinalIK;
using AniLipSync.VRM;

namespace Stevia.VR {

    public class VRMRuntimeLoader:MonoBehaviour {
        GameObject _model;

        void Start() {
            LoadVRM();
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
            if (_model != null) {
                GameObject.Destroy(_model.gameObject);
            }

            _model = context.Root;
            _model.transform.position = new Vector3(-10, 0, 0);
            _model.transform.rotation = Quaternion.Euler(0, 270, 0);

            context.ShowMeshes();
            context.EnableUpdateWhenOffscreen();

            // VR化用に追記
            _model.AddComponent<VRIK>();
            _model.GetComponent<VRIK>().solver.spine.headTarget = GameObject.Find("Head_Target").transform;
            _model.GetComponent<VRIK>().solver.leftArm.target = GameObject.Find("Hand_L_Target").transform;
            _model.GetComponent<VRIK>().solver.rightArm.target = GameObject.Find("Hand_R_Target").transform;

            // akiraさんのQiitaよりがに股を直すように設定
            _model.GetComponent<VRIK>().solver.leftLeg.swivelOffset = 15f;
            _model.GetComponent<VRIK>().solver.rightLeg.swivelOffset = -15f;
            _model.GetComponent<VRIK>().solver.locomotion.footDistance = 0.4f;
            _model.GetComponent<VRIK>().solver.locomotion.stepThreshold = 0.3f;
            _model.GetComponent<VRIK>().solver.locomotion.maxVelocity = 0.3f;
            _model.GetComponent<VRIK>().solver.locomotion.rootSpeed = 30f;
            _model.GetComponent<VRIK>().solver.plantFeet = false;

            //自分のカメラに頭が映らないように VRMFirstPerson の設定
            _model.GetComponent<VRM.VRMFirstPerson>().Setup();
            foreach (var renderer in GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
                renderer.updateWhenOffscreen = true;
            }

            // リップシンクのターゲット指定
            var aniLipSync = GameObject.Find("AniLipSync-VRM");
            aniLipSync.GetComponent<AnimMorphTarget>().blendShapeProxy = _model.GetComponent<VRMBlendShapeProxy>();

            // まばたきをする設定
            _model.AddComponent<VRM.Blinker>();
        }
    }
}
