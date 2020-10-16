using System.Collections.Generic;
using System.Xml.Linq;
using SFB;
using Stevia.STB;
using Stevia.STB.Model;
using Stevia.STB.Model.Member;
using Stevia.STB.Model.Section;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Model
{
    public partial class StbReader:MonoBehaviour
    {
        [FormerlySerializedAs("_material")] [SerializeField]
        private Material material;
        [FormerlySerializedAs("_dropdown")] [SerializeField]
        private Dropdown dropdown;

        public static StbNodes Nodes;
        public static StbStorys Stories;
        public static StbColumns Columns;
        public static StbPosts Posts;
        public static StbGirders Girders;
        public static StbBeams Beams;
        public static StbBraces Braces;
        public static StbSlabs Slabs;
        public static StbWalls Walls;
        public static StbSecColRC SecColumnRc;
        public static StbSecBeamRC SecBeamRc;
        public static StbSecColumnS SecColumnS;
        public static StbSecBeamS SecBeamS;
        public static StbSecBraceS SecBraceS;
        public static StbSecSteel StbSecSteel;

        private void Start()
        {
            XDocument xDoc = GetStbFileData();

            // 2回以上の起動を想定してここで初期化して各データを読み込み
            Init();
            Load(xDoc);

            // VRモードの場合、ドロップダウンリストに階情報を追加
            if (SceneManager.GetActiveScene().name == "SteviaVR")
            {
                foreach (string storyName in Stories.Name)
                {
                    dropdown.options.Add(new Dropdown.OptionData { text = "階：" + storyName + " へ移動" });
                }
                dropdown.RefreshShownValue();
            }

            // meshの生成
            MakeMesh();

            // 配筋表示は最初はオフにする
            DisplaySettings.BarOff();
        }

        private static XDocument GetStbFileData() 
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

        private static void Init() 
        {
            Nodes = new StbNodes();
            Stories = new StbStorys();
            Columns = new StbColumns();
            Posts = new StbPosts();
            Girders = new StbGirders();
            Beams = new StbBeams();
            Braces = new StbBraces();
            Slabs = new StbSlabs();
            Walls = new StbWalls();
            SecColumnRc = new StbSecColRC();
            SecBeamRc = new StbSecBeamRC();
            SecColumnS = new StbSecColumnS();
            SecBeamS = new StbSecBeamS();
            SecBraceS = new StbSecBraceS();
            StbSecSteel = new StbSecSteel();
        }

        private static void Load(XDocument xDoc)
        {
            var members = new List<StbData>
            {
                Nodes, Stories,
                Columns, Posts, Girders, Beams, Braces, Slabs, Walls,
                SecColumnRc, SecColumnS, SecBeamRc, SecBeamS, SecBraceS, StbSecSteel
            };

            foreach (StbData member in members)
            {
                member.Load(xDoc);
            }
        }

        private void MakeMesh() {
            var stbFrames = new List<StbFrame>
            {
                Columns, Posts, Girders, Beams, Braces
            };

            MakeSlab(Slabs);
            MakeWall(Walls);
            MakeFrame(stbFrames);
        }
    }
}
