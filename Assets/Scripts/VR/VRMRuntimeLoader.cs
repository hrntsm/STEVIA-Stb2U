using System.IO;
using System.Collections;
using System.Collections.Generic;
using VRM;
using UnityEngine;
using UnityEngine.UI;
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
            string path = StandaloneFileBrowser.OpenFilePanel("Open VRM", "", extensions, true)[0];
            if (string.IsNullOrEmpty(path)) {
                return;
            }

            var bytes = File.ReadAllBytes(path);
            // なんらかの方法でByte列を得た

            var context = new VRMImporterContext();

            // GLB形式でJSONを取得しParseします
            context.ParseGlb(bytes);

            // metaを取得(todo: thumbnailテクスチャのロード)
            var meta = context.ReadMeta(true);
            Debug.LogFormat("meta: title:{0}", meta.Title);
            ShowMetaInfo(meta); // UIに表示する

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

            // キャリブレーションなくしたので足は地面につける
            _model.GetComponent<VRIK>().solver.plantFeet = true;

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

        void ShowMetaInfo(VRMMetaObject meta) {
            var uiObject = GameObject.Find("VRM meta");

            #region Model Infomation
            Text title = uiObject.transform.Find("Model Info/Title/Meta").gameObject.GetComponent<Text>();
            Text auther = uiObject.transform.Find("Model Info/Auther/Meta").gameObject.GetComponent<Text>();
            Text contact = uiObject.transform.Find("Model Info/Contact/Meta").gameObject.GetComponent<Text>();
            Text reference = uiObject.transform.Find("Model Info/Reference/Meta").gameObject.GetComponent<Text>();
            Text version = uiObject.transform.Find("Model Info/Version/Meta").gameObject.GetComponent<Text>();
            Image thumbnail = uiObject.transform.Find("Model Info/Thumbnail/Meta").gameObject.GetComponent<Image>();

            title.text = meta.Title;
            auther.text = meta.Author;
            contact.text = meta.ContactInformation;
            reference.text = meta.Reference;
            version.text = meta.Version;

            if (meta.Thumbnail != null) {
                Sprite thumbnail_sprite = Sprite.Create(meta.Thumbnail, new Rect(0, 0, meta.Thumbnail.width, meta.Thumbnail.height), Vector2.zero);
                thumbnail.sprite = thumbnail_sprite;
            }
            #endregion

            #region Personation/Charaterization Permission
            Text allowedUser = uiObject.transform.Find("License/Personation/allowedUserName/Meta").gameObject.GetComponent<Text>();
            Text violentUssage = uiObject.transform.Find("License/Personation/violentUssageName/Meta").gameObject.GetComponent<Text>();
            Text sexualUssage = uiObject.transform.Find("License/Personation/sexualUssageName/Meta").gameObject.GetComponent<Text>();
            Text commercialUssage = uiObject.transform.Find("License/Personation/commercialUssageName/Meta").gameObject.GetComponent<Text>();
            Text otherPermissionUrl = uiObject.transform.Find("License/Personation/otherPermissionUrl/Meta").gameObject.GetComponent<Text>();

            switch (meta.AllowedUser) {
                case AllowedUser.OnlyAuthor:
                    allowedUser.text = "アバター作者にのみ";
                        break;
                case AllowedUser.ExplicitlyLicensedPerson:
                    allowedUser.text = "明確に許可された人のみ";
                    break;
                case AllowedUser.Everyone:
                    allowedUser.text = "全員に許可";
                    break;
                default:
                    break;
            }
            switch (meta.ViolentUssage) {
                case UssageLicense.Disallow:
                    violentUssage.text = "不許可";
                    break;
                case UssageLicense.Allow:
                    violentUssage.text = "許可";
                    break;
                default:
                    break;
            }
            switch (meta.SexualUssage) {
                case UssageLicense.Disallow:
                    sexualUssage.text = "不許可";
                    break;
                case UssageLicense.Allow:
                    sexualUssage.text = "許可";
                    break;
                default:
                    break;
            }
            switch (meta.CommercialUssage) {
                case UssageLicense.Disallow:
                    commercialUssage.text = "不許可";
                    break;
                case UssageLicense.Allow:
                    commercialUssage.text = "許可";
                    break;
                default:
                    break;
            }
            otherPermissionUrl.text = meta.OtherPermissionUrl;
            #endregion

            #region Redistribution
            Text licenseName = uiObject.transform.Find("License/Redistribution/licenseName/Meta").gameObject.GetComponent<Text>();
            Text otherLicenseUrl = uiObject.transform.Find("License/Redistribution/otherLicenseUrl/Meta").gameObject.GetComponent<Text>();

            switch (meta.LicenseType) {
                case LicenseType.Redistribution_Prohibited:
                    licenseName.text = "再配布禁止";
                    break;
                case LicenseType.CC0:
                    licenseName.text = "著作権放棄(CC0)";
                    break;
                case LicenseType.CC_BY:
                    licenseName.text = "Creative Commons CC BYライセンス(CC_BY)";
                    break;
                case LicenseType.CC_BY_NC:
                    licenseName.text = "Creative Commons CC BY NCライセンス(CC_BY_NC)";
                    break;
                case LicenseType.CC_BY_SA:
                    licenseName.text = "Creative Commons CC BY NCライセンス(CC_BY_SA)";
                    break;
                case LicenseType.CC_BY_NC_SA:
                    licenseName.text = "Creative Commons CC BY NCライセンス(CC_BY_NC_SA)";
                    break;
                case LicenseType.CC_BY_ND:
                    licenseName.text = "Creative Commons CC BY NCライセンス(CC_BY_ND)";
                    break;
                case LicenseType.CC_BY_NC_ND:
                    licenseName.text = "Creative Commons CC BY NCライセンス(CC_BY_NC_ND)";
                    break;
                case LicenseType.Other:
                    licenseName.text = "その他";
                    break;
                default:
                    break;
            }
            otherLicenseUrl.text = meta.OtherLicenseUrl;
            #endregion
        }
    }
}
