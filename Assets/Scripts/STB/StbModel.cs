using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stevia.STB.Model {
    /// <summary>
    /// 位置・断面情報（節点・部材・階・軸）
    /// </summary>
    public class StbModel {
        // TODO 一括でStbModelに属するものを読み込めるようにする
        //public void LoadAll(XDocument stbData) {
        //}
    }

    /// <summary>
    /// 節点（複数） 各節点を管理
    /// </summary>
    public class StbNodes {
        public List<int> Id { get; } = new List<int>();
        public List<float> X { get; } = new List<float>();
        public List<float> Y { get; } = new List<float>();
        public List<float> Z { get; } = new List<float>();
        public List<KindsNode> Kind { get; } = new List<KindsNode>();
        public List<int> IdMember { get; } = new List<int>();
        public List<Vector3> Vertex { get; } = new List<Vector3>();

        public void Load(XDocument stbDoc) {
            int index = 0;
            var stbNodes = stbDoc.Root.Descendants("StbNode");
            foreach (var stbNode in stbNodes) {
                // 必須コード
                Id.Add((int)stbNode.Attribute("id"));
                X.Add((float)stbNode.Attribute("x") / 1000f);
                Y.Add((float)stbNode.Attribute("z") / 1000f); // Y-Up対応
                Z.Add((float)stbNode.Attribute("y") / 1000f);
                
                // 必須ではないコード
                if (stbNode.Attribute("id_member") != null) {
                    IdMember.Add((int)stbNode.Attribute("id_member"));
                }
                else {
                    IdMember.Add(-1);
                }
                switch ((string)stbNode.Attribute("kind")) {
                    case "ON_BEAM":
                        Kind.Add(KindsNode.ON_BEAM); break;
                    case "ON_COLUMN":
                        Kind.Add(KindsNode.ON_COLUMN); break;
                    case "ON_GRID":
                        Kind.Add(KindsNode.ON_GRID); break;
                    case "ON_CANTI":
                        Kind.Add(KindsNode.ON_CANTI); break;
                    case "ON_SLAB":
                        Kind.Add(KindsNode.ON_SLAB); break;
                    case "OTHER":
                        Kind.Add(KindsNode.OTHER); break;
                    default:
                        break;
                }

                // StbNodeにはない追加した属性
                Vertex.Add(new Vector3((float)X[index], (float)Y[index], (float)Z[index]));
                index++;
            }
        }

        public enum KindsNode {
            ON_BEAM,
            ON_COLUMN,
            ON_GRID,
            ON_CANTI,
            ON_SLAB,
            OTHER
        }
    }

    /// <summary>
    /// 節点IDリスト
    /// </summary>
    public class StbNodeIdList {
        public List<int> Load(XElement stbElem) {
            List<int> idList = new List<int>();

            var xNodeIds = stbElem.Element("StbNodeid_List").Elements("StbNodeid");
            foreach (var xNodeId in xNodeIds) {
                idList.Add((int)xNodeId.Attribute("id"));
            }
            return idList;
        }
    }

    /// <summary>
    /// 軸情報
    /// </summary>
    public class StbAxes {
    }


    /// <summary>
    /// 階情報（複数）
    /// </summary>
    public class StbStorys {
        public List<int> Id { get; } = new List<int>();
        public List<string> Name { get; } = new List<string>();
        public List<float> Height { get; } = new List<float>();
        public List<KindsStory> Kind { get; } = new List<KindsStory>();
        public List<int> IdDependens { get; } = new List<int>();
        public List<string> StrengthConcrete { get; } = new List<string>();
        public List<List<int>> NodeIdList { get; } = new List<List<int>>();

        public void Load(XDocument stbData) {
            var stbStorys = stbData.Root.Descendants("StbStory");
            foreach (var stbStory in stbStorys) {
                // 必須コード
                Id.Add((int)stbStory.Attribute("id"));
                Height.Add((float)stbStory.Attribute("height") / 1000f);
                switch ((string)stbStory.Attribute("kind")) {
                    case "GENERAL":
                        Kind.Add(KindsStory.GENERAL); break;
                    case "BASEMENT":
                        Kind.Add(KindsStory.BASEMENT); break;
                    case "ROOF":
                        Kind.Add(KindsStory.ROOF); break;
                    case "PENTHOUSE":
                        Kind.Add(KindsStory.PENTHOUSE); break;
                    case "ISOLATION":
                        Kind.Add(KindsStory.ISOLATION); break;
                    default:
                        break;
                }
                
                // 必須ではないコード
                // リストの長さが合うように、空の場合はstring.Enpty
                if (stbStory.Attribute("name") != null) {
                    Name.Add((string)stbStory.Attribute("name"));
                }
                else {
                    Name.Add(string.Empty);
                }
                if (stbStory.Attribute("concrete_strength") != null) {
                    StrengthConcrete.Add((string)stbStory.Attribute("concrete_strength"));
                }
                else {
                    StrengthConcrete.Add(string.Empty);
                }

                // TODO
                // 所属節点の読み込み　List<List<int>> NodeIdList　の Set 部分の作成
            }
        }

        public enum KindsStory { 
            GENERAL,
            BASEMENT,
            ROOF,
            PENTHOUSE,
            ISOLATION
        }
    }

    /// <summary>
    /// 断面情報
    /// </summary>
    public class StbSections {
    }

    /// <summary>
    /// 継手情報
    /// </summary>
    public class StbJoints {
    }

    /// <summary>
    /// 床組（複数）
    /// </summary>
    public class StbSlabFrames {
    }

    public enum KindsStructure {
        RC,
        S,
        SRC,
        CFT
    }
}
