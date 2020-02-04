using System.Collections.Generic;
using System.Xml.Linq;
using SFB;
using UnityEngine;

public partial class STBReader:MonoBehaviour {
    // 確認済みのここで定義すべき引数
    public Material material;
    private List<Vector3> StbNodes = new List<Vector3>();
    private List<int> VertexIDs = new List<int>();
    private int i;

    // ここで定義すべきか未確認の引数
    private string ElementShape, xElementKind, ElementShapeType;
    private int NodeIndexStart, NodeIndexEnd,
                xNodeStart, xNodeEnd, xElementIdSection,
                StbSecIndex, ElementIdSection;
    private float ElementAngleY, ElementAngleZ, ElementHight, ElementWidth;
    private Vector3 NodeStart, NodeEnd,
                     VertexS1, VertexS2, VertexS3, VertexS4, VertexS5, VertexS6,
                     VertexE1, VertexE2, VertexE3, VertexE4, VertexE5, VertexE6;
    private List<int> xSecRcColumnId = new List<int>();
    private List<int> xSecRcColumnDepth = new List<int>();
    private List<int> xSecRcColumnWidth = new List<int>();
    private List<int> xSecSColumnId = new List<int>();
    private List<string> xSecSColumnShape = new List<string>();
    private List<int> xSecRcBeamId = new List<int>();
    private List<int> xSecRcBeamDepth = new List<int>();
    private List<int> xSecRcBeamWidth = new List<int>();
    private List<int> xSecSBeamId = new List<int>();
    private List<string> xSecSBeamShape = new List<string>();
    private List<int> xSecSBraceId = new List<int>();
    private List<string> xSecSBraceShape = new List<string>();
    private List<string> xStbSecSteelName = new List<string>();
    private List<float> xStbSecSteelParamA = new List<float>();
    private List<float> xStbSecSteelParamB = new List<float>();
    private List<string> xStbSecSteelType = new List<string>();
    private List<Mesh> ElementShapeMesh = new List<Mesh>();
    private Mesh[] ElementShapeMeshArray;

    // Start is called before the first frame update
    void Start() {
        // Open file with filter
        XDocument xdoc = GetStbFileData();

        // StbNode の取得
        GetStbNodes(xdoc, StbNodes, VertexIDs);

        // StbSlabs の取得とスラブの作成
        MakeSlabObjs(xdoc);

        // StbSecColumn_RC の取得
        var xSecRcColumns = xdoc.Root.Descendants("StbSecColumn_RC");
        foreach (var xSecRcColumn in xSecRcColumns) {
            xSecRcColumnId.Add((int)xSecRcColumn.Attribute("id"));
            var xSecFigure = xSecRcColumn.Element("StbSecFigure");

            // 子要素が StbSecRect か StbSecCircle を判定
            if (xSecFigure.Element("StbSecRect") != null) {
                xSecRcColumnDepth.Add((int)xSecFigure.Element("StbSecRect").Attribute("DY"));
                xSecRcColumnWidth.Add((int)xSecFigure.Element("StbSecRect").Attribute("DX"));
            }
            else {
                xSecRcColumnDepth.Add((int)xSecFigure.Element("StbSecCircle").Attribute("D"));
                xSecRcColumnWidth.Add(0); // Circle と判定用に width は 0
            }
        }

        // StbSecColumn_S の取得
        var xSecSColumns = xdoc.Root.Descendants("StbSecColumn_S");
        foreach (var xSecSColumn in xSecSColumns) {
            xSecSColumnId.Add((int)xSecSColumn.Attribute("id"));
            xSecSColumnShape.Add((string)xSecSColumn.Element("StbSecSteelColumn").Attribute("shape"));
        }

        // StbSecBeam_RC の取得
        var xSecRcBeams = xdoc.Root.Descendants("StbSecBeam_RC");
        foreach (var xSecRcBeam in xSecRcBeams) {
            xSecRcBeamId.Add((int)xSecRcBeam.Attribute("id"));
            var xSecFigure = xSecRcBeam.Element("StbSecFigure");

            // 子要素が StbSecHaunch か StbSecStraight を判定
            if (xSecFigure.Element("StbSecHaunch") != null) {
                xSecRcBeamDepth.Add((int)xSecFigure.Element("StbSecHaunch").Attribute("depth_center"));
                xSecRcBeamWidth.Add((int)xSecFigure.Element("StbSecHaunch").Attribute("width_center"));
            }
            else {
                xSecRcBeamDepth.Add((int)xSecFigure.Element("StbSecStraight").Attribute("depth"));
                xSecRcBeamWidth.Add((int)xSecFigure.Element("StbSecStraight").Attribute("width"));
            }
        }

        // StbSecBeam_S の取得
        var xSecSBeams = xdoc.Root.Descendants("StbSecBeam_S");
        foreach (var xSecSBeam in xSecSBeams) {
            xSecSBeamId.Add((int)xSecSBeam.Attribute("id"));
            xSecSBeamShape.Add((string)xSecSBeam.Element("StbSecSteelBeam").Attribute("shape"));
        }

        // StbSecBrace_S の取得
        var xSecSBraces = xdoc.Root.Descendants("StbSecBrace_S");
        foreach (var xSecSBrace in xSecSBraces) {
            xSecSBraceId.Add((int)xSecSBrace.Attribute("id"));
            xSecSBraceShape.Add((string)xSecSBrace.Element("StbSecSteelBrace").Attribute("shape"));
        }

        // S断面形状の取得
        i = 0;
        string[,] SteelSecName = GetSteelSecNameArray();
        while (i < SteelSecName.GetLength(0)) {
            GetStbSteelSection(xdoc, SteelSecName[i, 0], SteelSecName[i, 1]);
            i++;
        } 

        // 断面の生成
        i = 0;
        string[,] MemberName = GetMemberNameArray();
        while (i < MemberName.GetLength(0)) {
            MakeElementMesh(xdoc, MemberName[i, 0], MemberName[i, 1]);
            i++;
        }
    }

    XDocument GetStbFileData() {
        var extensions = new[] {
            new ExtensionFilter("ST-Bridge Files", "stb", "STB" ),
            new ExtensionFilter("All Files", "*" ),
        };
        string paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true)[0];
        XDocument xdoc = XDocument.Load(paths);
        return (xdoc);
    }

    void GetStbNodes(XDocument xdoc, List<Vector3> StbNodes, List<int> VertexIDs) {
        float xPos, yPos, zPos;
        int NodeID;
        var xNodes = xdoc.Root.Descendants("StbNode");

        foreach (var xNode in xNodes) {
            // unity は 1 が 1m なので1000で割ってる
            xPos = (float) xNode.Attribute("x") / 1000;
            yPos = (float) xNode.Attribute("z") / 1000; // unityは Z-Up
            zPos = (float) xNode.Attribute("y") / 1000;
            NodeID = (int) xNode.Attribute("id");

            StbNodes.Add(new Vector3(xPos, yPos, zPos));
            VertexIDs.Add(NodeID);
        }
    }

    string[,] GetSteelSecNameArray() {
        string[,] SteelSecNameArray = new string[7, 2] {
            {"StbSecRoll-H", "H"},
            {"StbSecBuild-H", "H"},
            {"StbSecRoll-BOX", "BOX"},
            {"StbSecBuild-BOX", "BOX"},
            {"StbSecPipe", "Pipe"},
            {"StbSecRoll-L", "L"},
            {"StbSecRoll-Bar", "Bar"}
        };
        return (SteelSecNameArray);
    }

    string[,] GetMemberNameArray() {
        string[,] MemberNameArray = new string[5, 2] {
            {"StbColumn", "Column"},
            {"StbGirder", "Beam"},
            {"StbPost", "Column"},
            {"StbBeam", "Beam"},
            {"StbBrace", "Brace"}
        };
        return (MemberNameArray);
    }
}
