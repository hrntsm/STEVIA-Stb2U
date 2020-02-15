using System.Collections.Generic;
using System.Xml.Linq;
using SFB;
using UnityEngine;

public partial class STBReader:MonoBehaviour {
    private List<Vector3> m_stbNodes = new List<Vector3>();
    private List<int> m_vertexIDs = new List<int>();
    private int i;

    private List<int> m_xRcColumnId = new List<int>();
    private List<int> m_xRcColumnDepth = new List<int>();
    private List<int> m_xRcColumnWidth = new List<int>();
    private List<int> m_xStColumnId = new List<int>();
    private List<string> m_xStColumnShape = new List<string>();
    private List<int> m_xRcBeamId = new List<int>();
    private List<int> m_xRcBeamDepth = new List<int>();
    private List<int> m_xRcBeamWidth = new List<int>();
    private List<int> m_xStBeamId = new List<int>();
    private List<string> m_xStBeamShape = new List<string>();
    private List<int> m_xStBraceId = new List<int>();
    private List<string> m_xStBraceShape = new List<string>();
    private List<string> m_xSteelName = new List<string>();
    private List<float> m_xSteelParamA = new List<float>();
    private List<float> m_xSteelParamB = new List<float>();
    private List<string> m_xSteelType = new List<string>();
    private List<Mesh> m_shapeMesh = new List<Mesh>();

    // Start is called before the first frame update
    void Start() {
        XDocument xDoc = GetStbFileData();
        GetStbNodes(xDoc, m_stbNodes, m_vertexIDs);
        MakeSlabObjs(xDoc);
        // StbSecColumn_RC の取得
        var xSecRcColumns = xDoc.Root.Descendants("StbSecColumn_RC");
        foreach (var xSecRcColumn in xSecRcColumns) {
            m_xRcColumnId.Add((int)xSecRcColumn.Attribute("id"));
            var xSecFigure = xSecRcColumn.Element("StbSecFigure");

            // 子要素が StbSecRect か StbSecCircle を判定
            if (xSecFigure.Element("StbSecRect") != null) {
                m_xRcColumnDepth.Add((int)xSecFigure.Element("StbSecRect").Attribute("DY"));
                m_xRcColumnWidth.Add((int)xSecFigure.Element("StbSecRect").Attribute("DX"));
            }
            else {
                m_xRcColumnDepth.Add((int)xSecFigure.Element("StbSecCircle").Attribute("D"));
                m_xRcColumnWidth.Add(0); // Circle と判定用に width は 0
            }
        }
        // StbSecColumn_S の取得
        var xSecSColumns = xDoc.Root.Descendants("StbSecColumn_S");
        foreach (var xSecSColumn in xSecSColumns) {
            m_xStColumnId.Add((int)xSecSColumn.Attribute("id"));
            m_xStColumnShape.Add((string)xSecSColumn.Element("StbSecSteelColumn").Attribute("shape"));
        }
        // StbSecBeam_RC の取得
        var xSecRcBeams = xDoc.Root.Descendants("StbSecBeam_RC");
        foreach (var xSecRcBeam in xSecRcBeams) {
            m_xRcBeamId.Add((int)xSecRcBeam.Attribute("id"));
            var xSecFigure = xSecRcBeam.Element("StbSecFigure");

            // 子要素が StbSecHaunch か StbSecStraight を判定
            if (xSecFigure.Element("StbSecHaunch") != null) {
                m_xRcBeamDepth.Add((int)xSecFigure.Element("StbSecHaunch").Attribute("depth_center"));
                m_xRcBeamWidth.Add((int)xSecFigure.Element("StbSecHaunch").Attribute("width_center"));
            }
            else {
                m_xRcBeamDepth.Add((int)xSecFigure.Element("StbSecStraight").Attribute("depth"));
                m_xRcBeamWidth.Add((int)xSecFigure.Element("StbSecStraight").Attribute("width"));
            }
        }
        // StbSecBeam_S の取得
        var xSecSBeams = xDoc.Root.Descendants("StbSecBeam_S");
        foreach (var xSecSBeam in xSecSBeams) {
            m_xStBeamId.Add((int)xSecSBeam.Attribute("id"));
            m_xStBeamShape.Add((string)xSecSBeam.Element("StbSecSteelBeam").Attribute("shape"));
        }
        // StbSecBrace_S の取得
        var xSecSBraces = xDoc.Root.Descendants("StbSecBrace_S");
        foreach (var xSecSBrace in xSecSBraces) {
            m_xStBraceId.Add((int)xSecSBrace.Attribute("id"));
            m_xStBraceShape.Add((string)xSecSBrace.Element("StbSecSteelBrace").Attribute("shape"));
        }
        // S断面形状の取得
        i = 0;
        string[,] SteelSecName = GetSteelSecNameArray();
        while (i < SteelSecName.GetLength(0)) {
            GetStbSteelSection(xDoc, SteelSecName[i, 0], SteelSecName[i, 1]);
            i++;
        } 
        // 断面の生成
        i = 0;
        string[,] MemberName = GetMemberNameArray();
        while (i < MemberName.GetLength(0)) {
            MakeElementMesh(xDoc, MemberName[i, 0], MemberName[i, 1]);
            i++;
        }
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

    void GetStbNodes(XDocument xdoc, List<Vector3> StbNodes, List<int> VertexIDs) {
        float xPos, yPos, zPos;
        int nodeID;
        var xNodes = xdoc.Root.Descendants("StbNode");

        foreach (var xNode in xNodes) {
            // unity は 1 が 1m なので1000で割ってる
            xPos = (float) xNode.Attribute("x") / 1000;
            yPos = (float) xNode.Attribute("z") / 1000; // unityは Z-Up
            zPos = (float) xNode.Attribute("y") / 1000;
            nodeID = (int) xNode.Attribute("id");

            StbNodes.Add(new Vector3(xPos, yPos, zPos));
            VertexIDs.Add(nodeID);
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
            {"StbGirder", "Girder"},
            {"StbPost", "Post"},
            {"StbBeam", "Beam"},
            {"StbBrace", "Brace"}
        };
        return (MemberNameArray);
    }
}
