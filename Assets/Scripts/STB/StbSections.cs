using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Stevia.STB.Model.Section {
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
        public List<int> Id { get; } = new List<int>();
        public List<string> Name { get; } = new List<string>();
        public List<string> Floor { get; } = new List<string>();
        public List<KindsColumn> KindColumn { get; } = new List<KindsColumn>();
        public List<string> DBarMain { get; } = new List<string>();
        public List<string> DBarBand { get; } = new List<string>();
        public List<float> Width { get; } = new List<float>();
        public List<float> Height { get; } = new List<float>();
        public List<bool> isRect { get; } = new List<bool>();
        public List<List<int>> BarList { get; } = new List<List<int>>();

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

                // 子要素 StbSecFigure
                StbSecFigure.Load(stbRcCol);
                Width.Add(StbSecFigure.Width);
                Height.Add(StbSecFigure.Height);
                isRect.Add(StbSecFigure.isRect);

                // 子要素 StbSecBar_Arrangement
                var stbBarArrange = stbRcCol.Element("StbSecBar_Arrangement");
                StbSecBarArrangement.Load(stbBarArrange, StbSecFigure.isRect);
                BarList.Add(StbSecBarArrangement.BarList);
            }
        }

        public enum KindsColumn {
            COLUMN,
            POST
        }
    }

    /// <summary>
    /// RCとSRCの断面形状
    /// </summary>
    public class StbSecFigure {
        public static float Width { get; private set; }
        public static float Height { get; private set; }
        public static bool isRect { get; private set; }

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

    public class StbSecBarArrangement {
        public static List<int> BarList { get; } = new List<int>();

        public static void Load(XElement stbBar, bool isRect) {
            string elementName;

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
