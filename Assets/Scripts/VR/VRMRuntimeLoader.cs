using System;
using System.IO;
using AniLipSync.VRM;
using RootMotion.FinalIK;
using SFB;
using UnityEngine;
using UnityEngine.UI;
using VRM;

namespace VR
{
    public class VrmRuntimeLoader:MonoBehaviour
    {
        private GameObject model;

        private void Start()
        {
            LoadVrm();
        }

        private void LoadVrm() 
        {
            var extensions = new[]
            {
                new ExtensionFilter("VRM Files", "vrm", "VRM" ),
                new ExtensionFilter("All Files", "*" ),
            };
            string path = StandaloneFileBrowser.OpenFilePanel("Open VRM", "", extensions, true)[0];
            if (string.IsNullOrEmpty(path))
                return;

            byte[] bytes = File.ReadAllBytes(path);
            // なんらかの方法でByte列を得た

            var context = new VRMImporterContext();

            // GLB形式でJSONを取得しParseします
            context.ParseGlb(bytes);

            // metaを取得(todo: thumbnailテクスチャのロード)
            VRMMetaObject meta = context.ReadMeta(true);
            Debug.LogFormat("meta: title:{0}", meta.Title);
            ShowMetaInfo(meta); // UIに表示する

            // ParseしたJSONをシーンオブジェクトに変換していく
            context.LoadAsync(() => OnLoaded(context));
        }

        private void OnLoaded(VRMImporterContext context)
        {
            if (model != null)
                Destroy(model.gameObject);

            model = context.Root;
            model.transform.position = new Vector3(-10, 0, 0);
            model.transform.rotation = Quaternion.Euler(0, 270, 0);

            context.ShowMeshes();
            context.EnableUpdateWhenOffscreen();

            // VR化用に追記
            model.AddComponent<VRIK>();
            model.GetComponent<VRIK>().solver.spine.headTarget = GameObject.Find("Head_Target").transform;
            model.GetComponent<VRIK>().solver.leftArm.target = GameObject.Find("Hand_L_Target").transform;
            model.GetComponent<VRIK>().solver.rightArm.target = GameObject.Find("Hand_R_Target").transform;

            // akiraさんのQiitaよりがに股を直すように設定
            model.GetComponent<VRIK>().solver.leftLeg.swivelOffset = 15f;
            model.GetComponent<VRIK>().solver.rightLeg.swivelOffset = -15f;
            model.GetComponent<VRIK>().solver.locomotion.footDistance = 0.4f;
            model.GetComponent<VRIK>().solver.locomotion.stepThreshold = 0.3f;
            model.GetComponent<VRIK>().solver.locomotion.maxVelocity = 0.3f;
            model.GetComponent<VRIK>().solver.locomotion.rootSpeed = 30f;
            model.GetComponent<VRIK>().solver.plantFeet = false;

            //自分のカメラに頭が映らないように VRMFirstPerson の設定
            model.GetComponent<VRMFirstPerson>().Setup();
            foreach (SkinnedMeshRenderer meshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                meshRenderer.updateWhenOffscreen = true;
            }

            // リップシンクのターゲット指定
            GameObject aniLipSync = GameObject.Find("AniLipSync-VRM");
            aniLipSync.GetComponent<AnimMorphTarget>().blendShapeProxy = model.GetComponent<VRMBlendShapeProxy>();

            // まばたきをする設定
            model.AddComponent<Blinker>();
        }

        private static void ShowMetaInfo(VRMMetaObject meta)
        {
            GameObject uiObject = GameObject.Find("VRM meta");

            #region Model Infomation
            var title = uiObject.transform.Find("Model Info/Title/Meta").gameObject.GetComponent<Text>();
            var author = uiObject.transform.Find("Model Info/Auther/Meta").gameObject.GetComponent<Text>();
            var contact = uiObject.transform.Find("Model Info/Contact/Meta").gameObject.GetComponent<Text>();
            var reference = uiObject.transform.Find("Model Info/Reference/Meta").gameObject.GetComponent<Text>();
            var version = uiObject.transform.Find("Model Info/Version/Meta").gameObject.GetComponent<Text>();
            var thumbnail = uiObject.transform.Find("Model Info/Thumbnail/Meta").gameObject.GetComponent<Image>();

            title.text = meta.Title;
            author.text = meta.Author;
            contact.text = meta.ContactInformation;
            reference.text = meta.Reference;
            version.text = meta.Version;

            if (meta.Thumbnail != null) {
                var thumbnailSprite = Sprite.Create(meta.Thumbnail, new Rect(0, 0, meta.Thumbnail.width, meta.Thumbnail.height), Vector2.zero);
                thumbnail.sprite = thumbnailSprite;
            }
            #endregion

            #region Personation/Charaterization Permission
            var allowedUser = uiObject.transform.Find("License/Personation/allowedUserName/Meta").gameObject.GetComponent<Text>();
            var violentUsage = uiObject.transform.Find("License/Personation/violentUssageName/Meta").gameObject.GetComponent<Text>();
            var sexualUsage = uiObject.transform.Find("License/Personation/sexualUssageName/Meta").gameObject.GetComponent<Text>();
            var commercialUsage = uiObject.transform.Find("License/Personation/commercialUssageName/Meta").gameObject.GetComponent<Text>();
            var otherPermissionUrl = uiObject.transform.Find("License/Personation/otherPermissionUrl/Meta").gameObject.GetComponent<Text>();

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
                    throw new ArgumentOutOfRangeException();
            }
            switch (meta.ViolentUssage) {
                case UssageLicense.Disallow:
                    violentUsage.text = "不許可";
                    break;
                case UssageLicense.Allow:
                    violentUsage.text = "許可";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (meta.SexualUssage) {
                case UssageLicense.Disallow:
                    sexualUsage.text = "不許可";
                    break;
                case UssageLicense.Allow:
                    sexualUsage.text = "許可";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (meta.CommercialUssage) {
                case UssageLicense.Disallow:
                    commercialUsage.text = "不許可";
                    break;
                case UssageLicense.Allow:
                    commercialUsage.text = "許可";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            otherPermissionUrl.text = meta.OtherPermissionUrl;
            #endregion

            #region Redistribution
            var licenseName = uiObject.transform.Find("License/Redistribution/licenseName/Meta").gameObject.GetComponent<Text>();
            var otherLicenseUrl = uiObject.transform.Find("License/Redistribution/otherLicenseUrl/Meta").gameObject.GetComponent<Text>();

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
                    throw new ArgumentOutOfRangeException();
            }
            otherLicenseUrl.text = meta.OtherLicenseUrl;
            #endregion
        }
    }
}
