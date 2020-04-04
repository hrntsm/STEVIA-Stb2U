using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Stevia.STB.Model.Member {
    public class StbMembers {
        public List<int> Id { get; } = new List<int>();
        public List<string> Name { get; } = new List<string>();
        public List<string> IdSection { get; } = new List<string>();
        public List<KindsStructure> KindStructure { get; } = new List<KindsStructure>();

        public virtual void Load(XDocument stbData) {
        }
    }

    public class StbColumns:StbMembers {
        public string Tag { get; } = "StbColumn";
        public List<int> IdNodeStart { get; } = new List<int>();
        public List<int> IdNodeEnd { get; } = new List<int>();
        public List<float> Rotate { get; } = new List<float>();

        public override void Load(XDocument stbData) {
            var stbElems = stbData.Root.Descendants(Tag);
            foreach (var stbElem in stbElems) {
                Id.Add((int)stbElem.Attribute("id"));
                Name.Add((string)stbElem.Attribute("name"));
                IdSection.Add((string)stbElem.Attribute("id_section"));
                IdNodeStart.Add((int)stbElem.Attribute("idNode_bottom"));
                IdNodeEnd.Add((int)stbElem.Attribute("idNode_top"));
                Rotate.Add((float)stbElem.Attribute("rotate"));

                switch ((string)stbElem.Attribute("kind_structure")) {
                    case "RC":
                        KindStructure.Add(KindsStructure.RC);
                        break;
                    case "S":
                        KindStructure.Add(KindsStructure.S);
                        break;
                    case "SRC":
                        KindStructure.Add(KindsStructure.SRC);
                        break;
                    case "CFT":
                        KindStructure.Add(KindsStructure.CFT);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 間柱情報（複数）
    /// </summary>
    public class StbPosts:StbMembers {
        public string Tag { get; } = "StbPost";
        public List<int> IdNodeStart { get; } = new List<int>();
        public List<int> IdNodeEnd { get; } = new List<int>();
        public List<float> Rotate { get; } = new List<float>();

        public override void Load(XDocument stbData) {
            var stbElems = stbData.Root.Descendants(Tag);
            foreach (var stbElem in stbElems) {
                Id.Add((int)stbElem.Attribute("id"));
                Name.Add((string)stbElem.Attribute("name"));
                IdSection.Add((string)stbElem.Attribute("id_section"));
                IdNodeStart.Add((int)stbElem.Attribute("idNode_bottom"));
                IdNodeEnd.Add((int)stbElem.Attribute("idNode_top"));
                Rotate.Add((float)stbElem.Attribute("rotate"));

                switch ((string)stbElem.Attribute("kind_structure")) {
                    case "RC":
                        KindStructure.Add(KindsStructure.RC);
                        break;
                    case "S":
                        KindStructure.Add(KindsStructure.S);
                        break;
                    case "SRC":
                        KindStructure.Add(KindsStructure.SRC);
                        break;
                    case "CFT":
                        KindStructure.Add(KindsStructure.CFT);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 大梁情報（複数）
    /// </summary>
    public class StbGirders:StbMembers {
        public string Tag { get; } = "StbGirder";
        public List<int> IdNodeStart { get; } = new List<int>();
        public List<int> IdNodeEnd { get; } = new List<int>();
        public List<float> Rotate { get; } = new List<float>();
        public List<double> Level { get; } = new List<double>();

        public override void Load(XDocument stbData) {
            var stbElems = stbData.Root.Descendants(Tag);
            foreach (var stbElem in stbElems) {
                Id.Add((int)stbElem.Attribute("id"));
                Name.Add((string)stbElem.Attribute("name"));
                IdSection.Add((string)stbElem.Attribute("id_section"));
                IdNodeStart.Add((int)stbElem.Attribute("idNode_start"));
                IdNodeEnd.Add((int)stbElem.Attribute("idNode_end"));
                Rotate.Add((float)stbElem.Attribute("rotate"));

                switch ((string)stbElem.Attribute("kind_structure")) {
                    case "RC":
                        KindStructure.Add(KindsStructure.RC);
                        break;
                    case "S":
                        KindStructure.Add(KindsStructure.S);
                        break;
                    case "SRC":
                        KindStructure.Add(KindsStructure.SRC);
                        break;
                    case "CFT":
                        KindStructure.Add(KindsStructure.CFT);
                        break;
                    default:
                        break;
                }

                if (stbElem.Attribute("level") != null) {
                    Level.Add((double)stbElem.Attribute("level") / 1000d);
                }
                else {
                    Level.Add(0d);
                }
            }
        }
    }

    /// <summary>
    /// 小梁情報（複数）
    /// </summary>
    public class StbBeams:StbMembers {
        public string Tag { get; } = "StbBeam";
        public List<int> IdNodeStart { get; } = new List<int>();
        public List<int> IdNodeEnd { get; } = new List<int>();
        public List<float> Rotate { get; } = new List<float>();
        public List<double> Level { get; } = new List<double>();

        public override void Load(XDocument stbData) {
            var stbElems = stbData.Root.Descendants(Tag);
            foreach (var stbElem in stbElems) {
                Id.Add((int)stbElem.Attribute("id"));
                Name.Add((string)stbElem.Attribute("name"));
                IdSection.Add((string)stbElem.Attribute("id_section"));
                IdNodeStart.Add((int)stbElem.Attribute("idNode_start"));
                IdNodeEnd.Add((int)stbElem.Attribute("idNode_end"));
                Rotate.Add((float)stbElem.Attribute("rotate"));

                switch ((string)stbElem.Attribute("kind_structure")) {
                    case "RC":
                        KindStructure.Add(KindsStructure.RC);
                        break;
                    case "S":
                        KindStructure.Add(KindsStructure.S);
                        break;
                    case "SRC":
                        KindStructure.Add(KindsStructure.SRC);
                        break;
                    case "CFT":
                        KindStructure.Add(KindsStructure.CFT);
                        break;
                    default:
                        break;
                }

                if (stbElem.Attribute("level") != null) {
                    Level.Add((double)stbElem.Attribute("level") / 1000d);
                }
                else {
                    Level.Add(0d);
                }
            }
        }
    }

    /// <summary>
    /// ブレース情報（複数）
    /// </summary>
    public class StbBraces:StbMembers {
        public string Tag { get; } = "StbBraces";
        public List<int> IdNodeStart { get; } = new List<int>();
        public List<int> IdNodeEnd { get; } = new List<int>();
        public List<float> Rotate { get; } = new List<float>();

        public override void Load(XDocument stbData) {
            var stbElems = stbData.Root.Descendants(Tag);
            foreach (var stbElem in stbElems) {
                Id.Add((int)stbElem.Attribute("id"));
                Name.Add((string)stbElem.Attribute("name"));
                IdSection.Add((string)stbElem.Attribute("id_section"));
                IdNodeStart.Add((int)stbElem.Attribute("idNode_start"));
                IdNodeEnd.Add((int)stbElem.Attribute("idNode_end"));
                Rotate.Add((float)stbElem.Attribute("rotate"));

                switch ((string)stbElem.Attribute("kind_structure")) {
                    case "RC":
                        KindStructure.Add(KindsStructure.RC);
                        break;
                    case "S":
                        KindStructure.Add(KindsStructure.S);
                        break;
                    case "SRC":
                        KindStructure.Add(KindsStructure.SRC);
                        break;
                    case "CFT":
                        KindStructure.Add(KindsStructure.CFT);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// スラブ情報（複数）
    /// </summary>
    public class StbSlabs:StbMembers {
        public List<KindsSlab> KindSlab { get; } = new List<KindsSlab>();
        public List<double> Level { get; } = new List<double>();
        public List<double> ThicknessExUpper { get; } = new List<double>();
        public List<double> ThicknessExBottom { get; } = new List<double>();
        public List<DirsLoad> DirLoad { get; } = new List<DirsLoad>();
        public List<double> AngleLoad { get; } = new List<double>();
        public List<bool> IsFoundation { get; } = new List<bool>();
        public List<TypesHanch> TypeHaunch { get; } = new List<TypesHanch>();
        public List<List<int>> NodeIdList { get; } = new List<List<int>>();

        public override void Load(XDocument stbDoc) {
            var stbElems = stbDoc.Root.Descendants("StbSlab");
            foreach (var stbElem in stbElems) {
                // 必須コード
                Id.Add((int)stbElem.Attribute("id"));
                Name.Add((string)stbElem.Attribute("name"));
                IdSection.Add((string)stbElem.Attribute("id_section"));
                KindStructure.Add(KindsStructure.RC); // スラブはRCのみ

                // 必須ではないコード
                if (stbElem.Attribute("kind_slab") != null) {
                    if ((string)stbElem.Attribute("kind_slab") == "NORMAL") {
                        KindSlab.Add(KindsSlab.NORMAL);
                    }
                    else {
                        KindSlab.Add(KindsSlab.CANTI);
                    }
                }
                if (stbElem.Attribute("level") != null) {
                    Level.Add((double)stbElem.Attribute("level") / 1000d);
                }
                else {
                    Level.Add(0d);
                }

                // 子要素 StbNodeid_List
                var stbNodeIdList = new StbNodeIdList();
                NodeIdList.Add(stbNodeIdList.Load(stbElem));
            }
        }
    }

    /// <summary>
    /// 壁情報（複数）
    /// </summary>
    public class StbWalls:StbMembers {
        public List<List<int>> NodeIdList { get; } = new List<List<int>>();

        public override void Load(XDocument stbDoc) {
            var stbSlabs = stbDoc.Root.Descendants("StbWall");
            foreach (var stbSlab in stbSlabs) {
                // 必須コード
                Id.Add((int)stbSlab.Attribute("id"));
                Name.Add((string)stbSlab.Attribute("name"));
                IdSection.Add((string)stbSlab.Attribute("id_section"));
                KindStructure.Add(KindsStructure.RC); // 壁はRCのみ

                // 子要素 StbNodeid_List
                var stbNodeIdList = new StbNodeIdList();
                NodeIdList.Add(stbNodeIdList.Load(stbSlab));
            }
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
