using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Stevia.STB.Model.Member {
    /// <summary>
    /// 柱・梁・スラブ・壁などの部材情報
    /// </summary>
    public class StbMember {
        // TODO 一括でStbMemberに属するものを読み込めるようにする
        // public void LoadAll(XDocument stbData) {
        // }
    }

    /// <summary>
    /// 柱情報（複数）
    /// </summary>
    public class StbColumns {
    }

    /// <summary>
    /// 間柱情報（複数）
    /// </summary>
    public class StbPosts {
    }

    /// <summary>
    /// 大梁情報（複数）
    /// </summary>
    public class StbGirders {
    }

    /// <summary>
    /// 小梁情報（複数）
    /// </summary>
    public class StbBeams {
    }

    /// <summary>
    /// ブレース情報（複数）
    /// </summary>
    public class StbBraces {
    }

    /// <summary>
    /// スラブ情報（複数）
    /// </summary>
    public class StbSlabs {
        public List<int> Id { get; } = new List<int>();
        public List<string> Name { get; } = new List<string>();
        public List<string> IdSection { get; } = new List<string>();
        public List<KindsStructure> KindStructure { get; } = new List<KindsStructure>();
        public List<KindsSlab> KindSlab { get; } = new List<KindsSlab>();
        public List<double> Level { get; } = new List<double>();
        public List<double> ThicknessExUpper { get; } = new List<double>();
        public List<double> ThicknessExBottom { get; } = new List<double>();
        public List<DirsLoad> DirLoad { get; } = new List<DirsLoad>();
        public List<double> AngleLoad { get; } = new List<double>();
        public List<bool> IsFoundation { get; } = new List<bool>();
        public List<TypesHanch> TypeHaunch { get; } = new List<TypesHanch>();
        public List<List<int>> NodeIdList { get; } = new List<List<int>>();

        public void LoadData(XDocument stbDoc) {
            int index = 0;
            var stbSlabs = stbDoc.Root.Descendants("StbSlab");
            foreach (var stbSlab in stbSlabs) {
                // 必須コード
                Id.Add((int)stbSlab.Attribute("id"));
                Name.Add((string)stbSlab.Attribute("name"));
                IdSection.Add((string)stbSlab.Attribute("id_section"));
                KindStructure.Add(KindsStructure.RC); // スラブはRCのみ

                // 必須ではないコード
                if ((string)stbSlab.Attribute("kind_slab") == "NORMAL") {
                    KindSlab.Add(KindsSlab.NORMAL);
                }
                else {
                    KindSlab.Add(KindsSlab.CANTI);
                }
                if (stbSlab.Attribute("level") != null) {
                    Level.Add((double)stbSlab.Attribute("level") / 1000d);
                }
                else {
                    Level.Add(0d);
                }

                // TODO
                // 必須ではないコードは未実装多め

                // 子要素 StbNodeid_List
                NodeIdList.Add(StbNodeIdList.Load(stbSlab));
                index++;
            }
        }

        public enum KindsSlab {
            NORMAL,
            CANTI
        }

        public enum DirsLoad {
            OneWay,
            TwoWay
        }

        public enum TypesHanch {
            BOTH,
            TOP,
            BOTTOM
        }
    }
    // TODO スラブは子要素がいくつかあるので注意（開口とか）

    /// <summary>
    /// 壁情報（複数）
    /// </summary>
    public class StbWalls {
    }
}
