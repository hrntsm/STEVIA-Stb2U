using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SFB;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Stevia.STB;
using Stevia.STB.Model;
using Stevia.STB.Model.Member;
using Stevia.STB.Model.Section;
using Stevia.UI;


namespace Stevia.Model
{
    public partial class STBReader:MonoBehaviour
    {
        [SerializeField] Material _material;
        [SerializeField] Dropdown _dropdown;

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

        void Start()
        {
            XDocument xDoc = GetStbFileData();

            // 2回以上の起動を想定してここで初期化して各データを読み込み
            Init();
            Load(xDoc);

            // VRモードの場合、ドロップダウンリストに階情報を追加
            if (SceneManager.GetActiveScene().name == "SteviaVR")
            {
                foreach (var name in _storys.Name)
                {
                    _dropdown.options.Add(new Dropdown.OptionData { text = "階：" + name + " へ移動" });
                }
                _dropdown.RefreshShownValue();
            }

            // meshの生成
            MakeMesh();

            // 配筋表示は最初はオフにする
            DisplySettings.BarOff();
        }

        XDocument GetStbFileData() 
        {
            var extensions = new[]
            {
                new ExtensionFilter("ST-Bridge Files", "stb", "STB" ),
                new ExtensionFilter("All Files", "*" ),
            };
            string paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true)[0];
            XDocument xDoc = XDocument.Load(paths);
            return (xDoc);
        }

        void Init() 
        {
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

        void Load(XDocument xDoc)
        {
            var members = new List<StbData>()
            {
                _nodes, _storys,
                _columns, _posts, _girders, _beams, _braces, _slabs, _walls,
                _secColumnRC, _secColumnS, _secBeamRC, _secBeamS, _secBraceS, _stbSecSteel
            };

            foreach (var member in members)
            {
                member.Load(xDoc);
            }
        }

        void MakeMesh() {
            List<StbFrame> stbFrames = new List<StbFrame>()
            {
                _columns, _posts, _girders, _beams, _braces
            };

            MakeSlab(_slabs);
            MakeWall(_walls);
            MakeFrame(stbFrames);
        }
    }
}
