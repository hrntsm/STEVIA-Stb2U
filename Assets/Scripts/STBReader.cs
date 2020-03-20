using System.Collections.Generic;
using System.Xml.Linq;
using SFB;
using UnityEngine;
using Stevia.STB.Model;
using Stevia.STB.Model.Member;
using Stevia.STB.Model.Section;


namespace Stevia {

    public partial class STBReader:MonoBehaviour {
        [SerializeField]
        Material _material;

        List<string> _xStName = new List<string>();
        List<float> _xStParamA = new List<float>();
        List<float> _xStParamB = new List<float>();
        List<string> _xStType = new List<string>();
        List<Mesh> _shapeMesh = new List<Mesh>();

        public static StbNodes _nodes = new StbNodes();
        public static StbStorys _storys = new StbStorys();
        public static StbSlabs _slabs = new StbSlabs();
        public static StbSecColRC _secColumnRC = new StbSecColRC();
        public static StbSecBeamRC _secBeamRC = new StbSecBeamRC();
        public static StbSecColumnS _secColumnS = new StbSecColumnS();
        public static StbSecBeamS _secBeamS = new StbSecBeamS();
        public static StbSecBraceS _secBraceS = new StbSecBraceS();
        public static StbSecSteel _stbSecSteel = new StbSecSteel();

        void Start() {
            // stbデータの読み込み
            XDocument xDoc = GetStbFileData();
            _nodes.Load(xDoc);
            _storys.Load(xDoc);
            _slabs.Load(xDoc);
            _secColumnRC.Load(xDoc);
            _secColumnS.Load(xDoc);
            _secBeamRC.Load(xDoc);
            _secBeamS.Load(xDoc);
            _secBraceS.Load(xDoc);
            _stbSecSteel.Load(xDoc);

            // TODO stb読み込み関連とほかの処理は分離する。

            // S断面形状の取得
            string[,] SteelSecName = GetSteelSecNameArray();
            for (int i = 0; i < SteelSecName.GetLength(0); i++) {
                GetStbSteelSection(xDoc, SteelSecName[i, 0], SteelSecName[i, 1]);
            }

            // meshの生成
            MakeSlabObjs(_slabs);
            string[,] memberName = GetMemberNameArray();
            for (int i = 0; i < memberName.GetLength(0); i++) {
                MakeElementMesh(xDoc, memberName[i, 0], memberName[i, 1]);
            }

            // 配筋表示は最初はオフにする
            DisplySettings.BarOff();
        }

        XDocument GetStbFileData() {
            var extensions = new[] {
            new ExtensionFilter("ST-Bridge Files", "stb", "STB" ),
            new ExtensionFilter("All Files", "*" ),
        };
            string paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true)[0];
            XDocument xDoc = XDocument.Load(paths);
            return (xDoc);
        }

        string[,] GetSteelSecNameArray() {
            string[,] steelSecNameArray = new string[7, 2] {
                {"StbSecRoll-H", "H"},
                {"StbSecBuild-H", "H"},
                {"StbSecRoll-BOX", "BOX"},
                {"StbSecBuild-BOX", "BOX"},
                {"StbSecPipe", "Pipe"},
                {"StbSecRoll-L", "L"},
                {"StbSecRoll-Bar", "Bar"}
            };
            return (steelSecNameArray);
        }

        string[,] GetMemberNameArray() {
            string[,] memberNameArray = new string[5, 2] {
                {"StbColumn", "Column"},
                {"StbGirder", "Girder"},
                {"StbPost", "Post"},
                {"StbBeam", "Beam"},
                {"StbBrace", "Brace"}
            };
            return (memberNameArray);
        }
    }
}
