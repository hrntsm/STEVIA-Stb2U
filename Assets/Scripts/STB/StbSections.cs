using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Stevia.STB.Model.Section {
    /// <summary>
    /// 主柱か間柱かの柱の種別
    /// </summary>
    public enum KindsColumn {
        COLUMN,
        POST
    }

    /// <summary>
    /// 大梁か小梁かの梁種別
    /// </summary>
    public enum KindsBeam {
        GIRDER,
        BEAM
    }

    /// <summary>
    /// 柱脚形式
    /// </summary>
    public enum BaseTypes {
        /// <summary>
        /// 露出柱脚
        /// </summary>
        EXPOSE,
        /// <summary>
        /// 埋込柱脚
        /// </summary>
        EMBEDDED,
        /// <summary>
        /// 非埋込柱脚
        /// </summary>
        UNEMBEDDED, // 
        /// <summary>
        /// 根巻柱脚
        /// </summary>
        WRAP
    }

    /// <summary>
    /// 断面情報
    /// </summary>
    class StbSections {
        // TODO 一括でStbMemberに属するものを読み込めるようにする
        // public void LoadAll(XDocument stbData) {
        // }
    }

    /// <summary>
    /// RC柱断面
    /// </summary>
    public class StbSecColRC {
        /// <summary>
        /// 部材のID
        /// </summary>
        public List<int> Id { get; } = new List<int>();
        /// <summary>
        /// 部材の名前
        /// </summary>
        public List<string> Name { get; } = new List<string>();
        /// <summary>
        /// 部材が所属する階
        /// </summary>
        public List<string> Floor { get; } = new List<string>();
        /// <summary>
        /// 部材が主柱か間柱かの区別
        /// </summary>
        public List<KindsColumn> KindColumn { get; } = new List<KindsColumn>();
        /// <summary>
        /// 主筋径
        /// </summary>
        public List<string> DBarMain { get; } = new List<string>();
        /// <summary>
        /// フープ径
        /// </summary>
        public List<string> DBarBand { get; } = new List<string>();
        /// <summary>
        /// 部材幅
        /// </summary>
        public List<float> Width { get; } = new List<float>();
        /// <summary>
        /// 部材高さ
        /// </summary>
        public List<float> Height { get; } = new List<float>();
        /// <summary>
        /// 部材が矩形であるかどうか
        /// </summary>
        public List<bool> isRect { get; } = new List<bool>();
        /// <summary>
        /// 各配筋の本数をまとめたリスト
        /// </summary>
        public List<List<int>> BarList { get; } = new List<List<int>>();

        /// <summary>
        /// 与えられたstbデータからRC柱断面を取得する。
        /// </summary>
        /// <param name="stbData"></param>
        public void Load(XDocument stbData) {
            var stbRcCols = stbData.Root.Descendants("StbSecColumn_RC");
            foreach (var stbRcCol in stbRcCols) {
                // 必須コード
                Id.Add((int)stbRcCol.Attribute("id"));
                Name.Add((string)stbRcCol.Attribute("name"));
                DBarMain.Add((string)stbRcCol.Attribute("D_reinforcement_main"));
                DBarBand.Add((string)stbRcCol.Attribute("D_reinforcement_band"));

                // 必須ではないコード
                if (stbRcCol.Attribute("Floor") != null) {
                    Floor.Add((string)stbRcCol.Attribute("Floor"));
                }
                else {
                    Floor.Add(string.Empty);
                }
                if (stbRcCol.Attribute("kind_column") != null) {
                    if ((string)stbRcCol.Attribute("kind_column") == "COLUMN") {
                        KindColumn.Add(KindsColumn.COLUMN);
                    }
                    else {
                        KindColumn.Add(KindsColumn.POST);
                    }
                }
                else {
                    KindColumn.Add(KindsColumn.COLUMN);
                }

                // 子要素 StbSecFigure
                StbSecFigure.Load(stbRcCol);
                Width.Add(StbSecFigure.Width);
                Height.Add(StbSecFigure.Height);
                isRect.Add(StbSecFigure.isRect);

                // 子要素 StbSecBar_Arrangement
                StbSecBarArrangement.Load(stbRcCol, StbSecFigure.isRect);
                BarList.Add(StbSecBarArrangement.BarList);
            }
        }
    }

    /// <summary>
    /// RCとSRCの断面形状
    /// </summary>
    public class StbSecFigure {
        public static float Width { get; private set; }
        public static float Height { get; private set; }
        public static bool isRect { get; private set; }

        /// <summary>
        /// 与えられたstbデータからRC柱断面の形状を取得する。
        /// </summary>
        /// <param name="stbColumn"></param>
        public static void Load(XElement stbColumn) {
            var stbFigure = stbColumn.Element("StbSecFigure");
            if (stbFigure.Element("StbSecRect") != null) {
                Width = (float) stbFigure.Element("StbSecRect").Attribute("DX") / 1000f;
                Height = (float) stbFigure.Element("StbSecRect").Attribute("DY") / 1000f;
                isRect = true;
            }
            else if(stbFigure.Element("StbSecCircle") != null) {
                Width = (float) stbFigure.Element("StbSecCircle").Attribute("D") / 1000f;
                Height = 0f;
                isRect = false;
            }
            else {
                Width = 0f;
                Height = 0f;
                isRect = false;
            }
        }
    }

    /// <summary>
    /// 柱の配筋情報
    /// </summary>
    public class StbSecBarArrangement {
        public static List<int> BarList { get; } = new List<int>();

        public static void Load(XElement stbColumn, bool isRect) {
            string elementName;
            var stbBar = stbColumn.Element("StbSecBar_Arrangement");

            if (stbBar.Element("StbSecRect_Column_Same") != null) {
                elementName = "StbSecRect_Column_Same";
            }
            else if (stbBar.Element("StbSecRect_Column_Not_Same") != null) {
                elementName = "StbSecRect_Column_Not_Same";
            }
            else if (stbBar.Element("StbSecCircle_Column_Same") != null) {
                elementName = "StbSecCircle_Column_Same";
            }
            else if (stbBar.Element("StbSecCircle_Column_Not_Same") != null) {
                elementName = "StbSecCircle_Column_Not_Same";
            }
            else {
                BarList.AddRange(new List<int> { 2, 2, 0, 0, 4, 200, 2, 2});
                return;
            }

            var stbBarElem = stbBar.Element(elementName);

            // Main 1
            if (stbBarElem.Attribute("count_main_X_1st") != null)
                BarList.Add((int)stbBarElem.Attribute("count_main_X_1st"));
            else
                BarList.Add(0);
            if (stbBarElem.Attribute("count_main_X_1st") != null)
                BarList.Add((int)stbBarElem.Attribute("count_main_Y_1st"));
            else
                BarList.Add(0);

            // Main2
            if (stbBarElem.Attribute("count_main_X_2nd") != null)
                BarList.Add((int)stbBarElem.Attribute("count_main_X_2nd"));
            else
                BarList.Add(0);
            if (stbBarElem.Attribute("count_main_Y_2nd") != null)
                BarList.Add((int)stbBarElem.Attribute("count_main_Y_2nd"));
            else
                BarList.Add(0);

            // Main total
            if (stbBarElem.Attribute("count_main_total") != null)
                BarList.Add((int)stbBarElem.Attribute("count_main_total"));
            else
                BarList.Add(0);

            // Band
            if (stbBarElem.Attribute("pitch_band") != null)
                BarList.Add((int)stbBarElem.Attribute("pitch_band"));
            else
                BarList.Add(0);
            if (stbBarElem.Attribute("count_band_dir_X") != null)
                BarList.Add((int)stbBarElem.Attribute("count_band_dir_X"));
            else
                BarList.Add(0);
            if (stbBarElem.Attribute("count_band_dir_Y") != null)
                BarList.Add((int)stbBarElem.Attribute("count_band_dir_Y"));
            else
                BarList.Add(0);
        }
    }

    /// <summary>
    /// S柱断面
    /// </summary>
    public class StbSecColumnS {
        /// <summary>
        /// 部材のID
        /// </summary>
        public List<int> Id { get; } = new List<int>();
        /// <summary>
        /// 部材の名前
        /// </summary>
        public List<string> Name { get; } = new List<string>();
        /// <summary>
        /// 部材が所属する階
        /// </summary>
        public List<string> Floor { get; } = new List<string>();
        /// <summary>
        /// 部材が主柱か間柱かの区別
        /// </summary>
        public List<KindsColumn> KindColumn { get; } = new List<KindsColumn>();
        /// <summary>
        /// StbSecSteelの基準方向が部材座標系Xかどうか
        /// </summary>
        public List<bool> Direction { get; } = new List<bool>();
        /// <summary>
        /// 柱脚の形式
        /// </summary>
        public List<BaseTypes> BaseType { get; } = new List<BaseTypes>();
        /// <summary>
        /// 柱頭の継手のID
        /// </summary>
        public List<int> JointIdTop { get; } = new List<int>();
        /// <summary>
        /// 柱脚の継手のID
        /// </summary>
        public List<int> JointIdBottom { get; } = new List<int>();
        /// <summary>
        /// 断面形状の名称
        /// </summary>
        public List<string> Shape { get; } = new List<string>();

        /// <summary>
        /// 与えられたstbデータからS柱断面を取得する。
        /// </summary>
        /// <param name="stbData"></param>
        public void Load(XDocument stbData) {
            var stbStCols = stbData.Root.Descendants("StbSecColumn_S");
            foreach (var stbStCol in stbStCols) {
                // 必須コード
                Id.Add((int)stbStCol.Attribute("id"));
                Name.Add((string)stbStCol.Attribute("name"));

                // 必須ではないコード
                if (stbStCol.Attribute("Floor") != null) {
                    Floor.Add((string)stbStCol.Attribute("Floor"));
                }
                else {
                    Floor.Add(string.Empty);
                }
                if (stbStCol.Attribute("kind_column") != null) {
                    if ((string)stbStCol.Attribute("kind_column") == "COLUMN") {
                        KindColumn.Add(KindsColumn.COLUMN);
                    }
                    else {
                        KindColumn.Add(KindsColumn.POST);
                    }
                }
                else {
                    KindColumn.Add(KindsColumn.COLUMN);
                }
                if (stbStCol.Attribute("base_type") != null) {
                    switch ((string)stbStCol.Attribute("base_type")) {
                        case "EXPOSE":
                            BaseType.Add(BaseTypes.EXPOSE); break;
                        case "EMBEDDED":
                            BaseType.Add(BaseTypes.EMBEDDED); break;
                        case "WRAP":
                            BaseType.Add(BaseTypes.WRAP); break;
                        default:
                            break;
                    }
                }
                else {
                    BaseType.Add(BaseTypes.EXPOSE);
                }
                if (stbStCol.Attribute("direction") != null) {
                    Direction.Add((bool)stbStCol.Attribute("direction"));
                }
                else {
                    Direction.Add(true);
                }
                if (stbStCol.Attribute("joint_id_top") != null) {
                    JointIdTop.Add((int)stbStCol.Attribute("joint_id_top"));
                }
                else {
                    JointIdTop.Add(-1);
                }
                if (stbStCol.Attribute("joint_id_bottom") != null) {
                    JointIdBottom.Add((int)stbStCol.Attribute("joint_id_bottom"));
                }
                else {
                    JointIdBottom.Add(-1);
                }


                // 子要素 StbSecSteelColumn
                StbSecSteelColumn.Load(stbStCol);
                Shape.Add(StbSecSteelColumn.Shape);
            }
        }
    }

    /// <summary>
    /// 柱断面形状の名称
    /// </summary>
    public class StbSecSteelColumn {
        public static string Pos { get; private set; }
        public static string Shape { get; private set; }
        public static string StrengthMain { get; private set; }
        public static string StrengthWeb { get; private set; }

        public static void Load(XElement stbStCol) {
            var secStCol = stbStCol.Element("StbSecSteelColumn");
            // 必須コード
            Pos = (string)secStCol.Attribute("pos");
            Shape = (string)secStCol.Attribute("shape");
            StrengthMain = (string)secStCol.Attribute("strength_main");

            // 必須ではないコード
            if (secStCol.Attribute("strength_web") != null) {
                StrengthWeb = (string)secStCol.Attribute("strength_web");
            }
            else {
                StrengthWeb = string.Empty;
            }
        }
    }

    /// <summary>
    /// SRC柱断面
    /// </summary>
    public class StbSecColumnSRC {
    }

    /// <summary>
    /// CFT柱断面
    /// </summary>
    public class StbSecColumnCFT {
    }

    /// <summary>
    /// RC梁断面
    /// </summary>
    public class StbSecBeamRC {
    }

    /// <summary>
    /// S梁断面
    /// </summary>
    public class StbSecBeamS {
        /// <summary>
        /// 部材のID
        /// </summary>
        public List<int> Id { get; } = new List<int>();
        /// <summary>
        /// 部材の名前
        /// </summary>
        public List<string> Name { get; } = new List<string>();
        /// <summary>
        /// 部材が所属する階
        /// </summary>
        public List<string> Floor { get; } = new List<string>();
        /// <summary>
        /// 部材が大梁か小梁かの区別
        /// </summary>
        public List<KindsBeam> KindBeam { get; } = new List<KindsBeam>();
        /// <summary>
        /// 部材が片持ちであるかどうか
        /// </summary>
        public List<bool> isCanti { get; } = new List<bool>();
        /// <summary>
        /// 部材が外端内端であるかどうか
        /// </summary>
        public List<bool> isOutIn { get; } = new List<bool>();
        /// <summary>
        /// 始端の継手のID
        /// </summary>
        public List<int> JointIdStart { get; } = new List<int>();
        /// <summary>
        /// 終端の継手のID
        /// </summary>
        public List<int> JointIdEnd { get; } = new List<int>();
        /// <summary>
        /// 断面形状の名称
        /// </summary>
        public List<string> Shape { get; } = new List<string>();


        /// <summary>
        /// 与えられたstbデータからS梁断面を取得する。
        /// </summary>
        /// <param name="stbData"></param>
        public void Load(XDocument stbData) {
            var stbStBeams = stbData.Root.Descendants("StbSecBeam_S");
            foreach (var stbStBeam in stbStBeams) {

                // 必須コード
                Id.Add((int)stbStBeam.Attribute("id"));
                Name.Add((string)stbStBeam.Attribute("name"));

                // 必須ではないコード
                if (stbStBeam.Attribute("Floor") != null) {
                    Floor.Add((string)stbStBeam.Attribute("Floor"));
                }
                else {
                    Floor.Add(string.Empty);
                }
                if (stbStBeam.Attribute("kind_beam") != null) {
                    if ((string)stbStBeam.Attribute("kind_beam") == "GIRDER") {
                        KindBeam.Add(KindsBeam.GIRDER);
                    }
                    else {
                        KindBeam.Add(KindsBeam.BEAM);
                    }
                }
                else {
                    KindBeam.Add(KindsBeam.GIRDER);
                }
                if (stbStBeam.Attribute("isCanti") != null) {
                    isCanti.Add((bool)stbStBeam.Attribute("isCanti"));
                }
                else {
                    isCanti.Add(false);
                }
                if (stbStBeam.Attribute("isOutIn") != null) {
                    isCanti.Add((bool)stbStBeam.Attribute("isOutIn"));
                }
                else {
                    isCanti.Add(false);
                }
                if (stbStBeam.Attribute("joint_id_start") != null) {
                    JointIdStart.Add((int)stbStBeam.Attribute("joint_id_start"));
                }
                else {
                    JointIdStart.Add(-1);
                }
                if (stbStBeam.Attribute("joint_id_end") != null) {
                    JointIdEnd.Add((int)stbStBeam.Attribute("joint_id_end"));
                }
                else {
                    JointIdEnd.Add(-1);
                }

                // 子要素 StbSecSteelColumn
                StbSecSteelBeam.Load(stbStBeam);
                Shape.Add(StbSecSteelBeam.Shape);
            }
        }
    }

    /// <summary>
    /// S梁断面形状の名称
    /// </summary>
    public class StbSecSteelBeam {
        public static string Pos { get; private set; }
        public static string Shape { get; private set; }
        public static string StrengthMain { get; private set; }
        public static string StrengthWeb { get; private set; }

        public static void Load(XElement stbStBeam) {
            var secStBeam = stbStBeam.Element("StbSecSteelBeam");

            // 必須コード
            Pos = (string)secStBeam.Attribute("pos");
            Shape = (string)secStBeam.Attribute("shape");
            StrengthMain = (string)secStBeam.Attribute("strength_main");

            // 必須ではないコード
            if (secStBeam.Attribute("strength_web") != null) {
                StrengthWeb = (string)secStBeam.Attribute("strength_web");
            }
            else {
                StrengthWeb = string.Empty;
            }
        }
    }

    /// <summary>
    /// SRC梁断面
    /// </summary>
    public class StbSecBeamSRC {
    }

    /// <summary>
    /// Sブレース断面
    /// </summary>
    public class StbSecBraceS {
    }

    /// <summary>
    /// RC壁断面
    /// </summary>
    public class StbSecWallRC {
    }

    /// <summary>
    /// RCスラブ断面
    /// </summary>
    public class StbSecSlabRC {
    }

    /// <summary>
    /// RC基礎断面
    /// </summary>
    public class StbSecFoundationRC {
    }

    /// <summary>
    /// 鉄骨断面
    /// </summary>
    public class StbSecSteel {
    }
}
