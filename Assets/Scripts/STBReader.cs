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
        public static StbColumns _columns;
        public static StbPosts _posts;
        public static StbGirders _girders;
        public static StbBeams _beams;
        public static StbBraces _braces;
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

            List<StbFrame> stbFrames = new List<StbFrame>() {
                _columns, _posts, _girders, _beams, _braces
            };
            MakeFrame(stbFrames);

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
            _columns = new StbColumns();
            _posts = new StbPosts();
            _girders = new StbGirders();
            _beams = new StbBeams();
            _braces = new StbBraces();
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
                _nodes, _storys,
                _columns, _posts, _girders, _beams, _braces, _slabs, _walls,
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
    }
}
