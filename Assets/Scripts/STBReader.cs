using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using UnityEngine;
using UnityEngine.UI;

public class STBReader : MonoBehaviour
{
    // Unity 関連の引数定義
    // メッシュ表示コンポーネント
    private MeshRenderer meshRenderer;
    // メッシュに設定するマテリアル
    public Material material;

    public string path;
    private string  ElementShape, xElementKind, ElementShapeType;
    private int NodeID, NodeIndex_i, NodeIndex_j, NodeIndex_k, NodeIndex_l, NodeIndexStart, NodeIndexEnd,
                xNodeStart, xNodeEnd, xElementIdSection,
                StbSecIndex,  ElementIdSection;
    private float xPos, yPos, zPos, ElementAngleY, ElementAngleZ, ElementHight, ElementWidth;
    private Vector3 NodeStart, NodeEnd, 
                     VertexS1, VertexS2, VertexS3, VertexS4, VertexS5, VertexS6,
                     VertexE1, VertexE2, VertexE3, VertexE4, VertexE5, VertexE6;
    private List<Vector3> StbNodes = new List<Vector3>();
    private List<int> VertexIDs = new List<int>();
    private List<int> xSlabNodeIDs = new List<int>();
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
    private List<double> xStbSecSteelParamA = new List<double>();
    private List<double> xStbSecSteelParamB = new List<double>();
    private List<string> xStbSecSteelType = new List<string>();
    // private List<Brep> RhinoSlabs = new List<Brep>();
    // private List<Brep> ElementShapeBrep = new List<Brep>();
    // private Brep[] RhinoColumns, RhinoGirders, RhinoPosts, RhinoBeams, SteelBraces, ElementShapeBrepArray;
    
    // Start is called before the first frame update
    void Start() {

        var xdoc = XDocument.Load(path);

        // StbNode の取得
        var xNodes = xdoc.Root.Descendants("StbNode");
        foreach (var xNode in xNodes) {
            // unity は 1 が 1m なので1000で割ってる
            xPos = (float)xNode.Attribute("x")/1000;
            yPos = (float)xNode.Attribute("z")/1000; // unityは Z-Up
            zPos = (float)xNode.Attribute("y")/1000;
            NodeID = (int)xNode.Attribute("id");
            
            StbNodes.Add(new Vector3(xPos, yPos, zPos));
            VertexIDs.Add(NodeID);
        }
        
        // StbSlabs の取得
        var xSlabs = xdoc.Root.Descendants("StbSlab");
        int SlabNum = 0;
        GameObject Slabs = new GameObject("Slabs");
        foreach (var xSlab in xSlabs) {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            int CountNode = 0;
            Mesh meshObj = new Mesh();

            var xNodeids = xSlab.Element("StbNodeid_List")
                                .Elements("StbNodeid");
            foreach (var xNodeid in xNodeids) {
                xSlabNodeIDs.Add((int)xNodeid.Attribute("id"));
                CountNode = CountNode + 1;
            }

            NodeIndex_i = VertexIDs.IndexOf(xSlabNodeIDs[0]);
            NodeIndex_j = VertexIDs.IndexOf(xSlabNodeIDs[1]);
            NodeIndex_k = VertexIDs.IndexOf(xSlabNodeIDs[2]);
            NodeIndex_l = VertexIDs.IndexOf(xSlabNodeIDs[3]);

            xSlabNodeIDs.Clear(); // foreachごとでListにAddし続けてるのでここで値をClear

            // Unityでの頂点座標の生成
            vertices.Add(StbNodes[NodeIndex_i]);
            vertices.Add(StbNodes[NodeIndex_j]);
            vertices.Add(StbNodes[NodeIndex_k]);
            vertices.Add(StbNodes[NodeIndex_l]);
            // Unityでの三角形メッシュの生成
            triangles.Add(CountNode-4);
            triangles.Add(CountNode-3);
            triangles.Add(CountNode-2);
            triangles.Add(CountNode-2);
            triangles.Add(CountNode-1);
            triangles.Add(CountNode-4);

            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();

            meshObj.RecalculateNormals();

            string SlabName = string.Format("Slab{0}", SlabNum);
            SlabNum = SlabNum + 1;
            GameObject Slab = new GameObject(SlabName);
            Slab.AddComponent<MeshFilter>().mesh = meshObj;
            Slab.AddComponent<MeshRenderer>().material = material;
            Slab.transform.parent = Slabs.transform;
        }

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
                xSecRcColumnWidth.Add( 0 ); // Circle と判定用に width は 0
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
        GetStbSteelSection(xdoc, "StbSecRoll-H", "H");
        GetStbSteelSection(xdoc, "StbSecBuild-H", "H");
        GetStbSteelSection(xdoc, "StbSecRoll-BOX", "BOX");
        GetStbSteelSection(xdoc, "StbSecBuild-BOX", "BOX");
        GetStbSteelSection(xdoc, "StbSecPipe", "Pipe");
        GetStbSteelSection(xdoc, "StbSecRoll-L", "L");
        GetStbSteelSection(xdoc, "StbSecRoll-Bar", "Bar");
    }

    // Update is called once per frame
    void Update() {
    }

    void GetStbSteelSection(XDocument xdoc, string xDateTag, string SectionType) {
        if(SectionType == "Pipe") {
            var xSteelSections = xdoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                xStbSecSteelName.Add((string)xSteelSection.Attribute("name"));
                xStbSecSteelParamA.Add((double)xSteelSection.Attribute("D"));
                xStbSecSteelParamB.Add((double)xSteelSection.Attribute("t"));
                xStbSecSteelType.Add(SectionType);
            }
        }
        else if (SectionType == "Bar") {
            var xSteelSections = xdoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                xStbSecSteelName.Add((string)xSteelSection.Attribute("name"));
                xStbSecSteelParamA.Add((double)xSteelSection.Attribute("R"));
                xStbSecSteelParamB.Add(0.0);
                xStbSecSteelType.Add(SectionType);
            }
        }
        else if (SectionType == "NotSupport") {
        }
        else {
            var xSteelSections = xdoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                xStbSecSteelName.Add((string)xSteelSection.Attribute("name"));
                xStbSecSteelParamA.Add((double)xSteelSection.Attribute("A"));
                xStbSecSteelParamB.Add((double)xSteelSection.Attribute("B"));
                xStbSecSteelType.Add(SectionType);
            }
        }
    }

    void CreateMesh(Mesh mesh, Vector3[] vertices, int[] triangles) {
        // 最初にメッシュをクリアする
        mesh.Clear();
        // 頂点の設定
        mesh.vertices = vertices;
        // 三角形メッシュの設定
        mesh.triangles = triangles;
        // Boundsの再計算
        mesh.RecalculateBounds ();
        // NormalMapの再計算
        mesh.RecalculateNormals ();
    }
}
