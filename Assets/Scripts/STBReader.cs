using System.Collections.Generic;
using System.Xml.Linq;
using SFB;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Stevia.STB;
using Stevia.STB.Model;
using Stevia.STB.Model.Member;
using Stevia.STB.Model.Section;


namespace Stevia {

    public partial class STBReader:MonoBehaviour {
        [SerializeField]
        Material _material;
        [SerializeField]
        Dropdown _dropdown;

        List<string> _xStName = new List<string>();
        List<float> _xStParamA = new List<float>();
        List<float> _xStParamB = new List<float>();
        List<string> _xStType = new List<string>();
        List<Mesh> _shapeMesh = new List<Mesh>();

        public static StbNodes _nodes;
        public static StbStorys _storys;
        public static StbSlabs _slabs;
        public static StbWalls _walls;
        public static StbSecColRC _secColumnRC;
        public static StbSecBeamRC _secBeamRC;
        public static StbSecColumnS _secColumnS;
        public static StbSecBeamS _secBeamS;
        public static StbSecBraceS _secBraceS;
        public static StbSecSteel _stbSecSteel;

        void Start() {
            XDocument xDoc = GetStbFileData();

            // 2回以上の起動を想定してここで初期化して各データを読み込み
            Init();
            Load(xDoc);

            // VRモードの場合、ドロップダウンリストに階情報を追加
            if (SceneManager.GetActiveScene().name == "Stb2U4VR") {
                foreach (var name in _storys.Name) {
                    _dropdown.options.Add(new Dropdown.OptionData { text = "階：" + name + " へ移動" });
                }
                _dropdown.RefreshShownValue();
            }

            // TODO stb読み込み関連とほかの処理は分離する。

            // S断面形状の取得
            string[,] SteelSecName = GetSteelSecNameArray();
            for (int i = 0; i < SteelSecName.GetLength(0); i++) {
                GetStbSteelSection(xDoc, SteelSecName[i, 0], SteelSecName[i, 1]);
            }

            // meshの生成
            MakeSlab(_slabs);
            MakeWall(_walls);
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

        void Init() {
            _nodes = new StbNodes();
            _storys = new StbStorys();
            _slabs = new StbSlabs();
            _walls = new StbWalls();
            _secColumnRC = new StbSecColRC();
            _secBeamRC = new StbSecBeamRC();
            _secColumnS = new StbSecColumnS();
            _secBeamS = new StbSecBeamS();
            _secBraceS = new StbSecBraceS();
            _stbSecSteel = new StbSecSteel();
        }

        void Load(XDocument xDoc) {
            var members = new List<StbData>() {
                _nodes, _storys, _slabs, _walls,
                _secColumnRC, _secColumnS, _secBeamRC, _secBeamS, _secBraceS, _stbSecSteel
            };

            foreach (var member in members) {
                member.Load(xDoc);
            }
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
