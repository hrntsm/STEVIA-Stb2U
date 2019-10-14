using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

using SFB;

using UnityEngine;

public class STBReader : MonoBehaviour
{
    // Unity 関連の引数定義
    // メッシュ表示コンポーネント
    private MeshRenderer meshRenderer;
    // メッシュに設定するマテリアル
    public Material material;

    public string path;
    private string  ElementShape, xElementKind, ElementShapeType;
    private int NodeID, 
    NodeIndex_i, NodeIndex_j, NodeIndex_k, NodeIndex_l, NodeIndexStart, NodeIndexEnd,
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
    private List<float> xStbSecSteelParamA = new List<float>();
    private List<float> xStbSecSteelParamB = new List<float>();
    private List<string> xStbSecSteelType = new List<string>();
    private List<Mesh> ElementShapeMesh = new List<Mesh>();
    private Mesh[] ElementShapeMeshArray;
    
    // Start is called before the first frame update
    void Start() {

        // Open file with filter
        var extensions = new[] {
            new ExtensionFilter("ST-Bridge Files", "stb", "STB" ),
            new ExtensionFilter("All Files", "*" ),
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true)[0];

        var xdoc = XDocument.Load(paths);
        //var xdoc = XDocument.Load("D:/Document/unity/StbUnity/StbUnity/Assets/StbData/S.STB");

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
        GameObject Slabs = new GameObject("StbSlabs");
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

        // 断面の生成
        MakeElementMesh(xdoc, "StbColumn", "Column");
        MakeElementMesh(xdoc, "StbGirder", "Beam");
        MakeElementMesh(xdoc, "StbPost", "Column");
        MakeElementMesh(xdoc, "StbBeam", "Beam");
        MakeElementMesh(xdoc, "StbBrace", "Brace");
    }

    // Update is called once per frame
    void Update() {
    }

    void GetStbSteelSection(XDocument xdoc, string xDateTag, string SectionType) {
        if(SectionType == "Pipe") {
            var xSteelSections = xdoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                xStbSecSteelName.Add((string)xSteelSection.Attribute("name"));
                xStbSecSteelParamA.Add((float)xSteelSection.Attribute("D"));
                xStbSecSteelParamB.Add((float)xSteelSection.Attribute("t"));
                xStbSecSteelType.Add(SectionType);
            }
        }
        else if (SectionType == "Bar") {
            var xSteelSections = xdoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                xStbSecSteelName.Add((string)xSteelSection.Attribute("name"));
                xStbSecSteelParamA.Add((float)xSteelSection.Attribute("R"));
                xStbSecSteelParamB.Add(0);
                xStbSecSteelType.Add(SectionType);
            }
        }
        else if (SectionType == "NotSupport") {
        }
        else {
            var xSteelSections = xdoc.Root.Descendants(xDateTag);
            foreach (var xSteelSection in xSteelSections) {
                xStbSecSteelName.Add((string)xSteelSection.Attribute("name"));
                xStbSecSteelParamA.Add((float)xSteelSection.Attribute("A"));
                xStbSecSteelParamB.Add((float)xSteelSection.Attribute("B"));
                xStbSecSteelType.Add(SectionType);
            }
        }
    }

    void MakeElementMesh(XDocument xdoc, string xDateTag, string ElementStructureType) {
        ElementShapeMesh.Clear();

        var xElements = xdoc.Root.Descendants(xDateTag);
        int ElementNum = 0;
        GameObject Elements = new GameObject(xDateTag + "s");
        foreach (var xElement in xElements) {
            if (ElementStructureType == "Beam" || ElementStructureType == "Brace") {
                xNodeStart = (int)xElement.Attribute("idNode_start");
                xNodeEnd = (int)xElement.Attribute("idNode_end");
            }
            else {
                xNodeStart = (int)xElement.Attribute("idNode_bottom");
                xNodeEnd = (int)xElement.Attribute("idNode_top");
            }
            xElementIdSection = (int)xElement.Attribute("id_section");
            xElementKind = (string)xElement.Attribute("kind_structure");

            // 始点と終点の座標取得
            NodeIndexStart = VertexIDs.IndexOf(xNodeStart);
            NodeIndexEnd = VertexIDs.IndexOf(xNodeEnd);
            NodeStart = StbNodes[NodeIndexStart];
            NodeEnd = StbNodes[NodeIndexEnd];

            if (xElementKind == "RC") {
                // 断面形状名（shape) と 断面形状（HxB）の取得の取得
                if (ElementStructureType == "Beam") {
                    StbSecIndex = xSecRcBeamId.IndexOf(xElementIdSection);
                    ElementHight = xSecRcBeamDepth[StbSecIndex] / 1000;
                    ElementWidth = xSecRcBeamWidth[StbSecIndex] / 1000;
                }
                else if (ElementStructureType == "Column") {
                    StbSecIndex = xSecRcColumnId.IndexOf(xElementIdSection);
                    ElementHight = xSecRcColumnDepth[StbSecIndex] / 1000;
                    ElementWidth = xSecRcColumnWidth[StbSecIndex] / 1000;
                }

                if (ElementWidth == 0) {
                    ElementShapeType = "Pipe";
                }
                else {
                    ElementShapeType = "BOX";
                }
            }
            else if (xElementKind == "S") {
                // 断面形状名（shape）の取得の取得
                if (ElementStructureType == "Beam") {
                    ElementIdSection = xSecSBeamId.IndexOf(xElementIdSection);
                    ElementShape = xSecSBeamShape[ElementIdSection];
                }
                else if (ElementStructureType == "Column") {
                    ElementIdSection = xSecSColumnId.IndexOf(xElementIdSection);
                    ElementShape = xSecSColumnShape[ElementIdSection];
                }
                else if (ElementStructureType == "Brace") {
                    ElementIdSection = xSecSBraceId.IndexOf(xElementIdSection);
                    ElementShape = xSecSBraceShape[ElementIdSection];
                }
                // 断面形状（HxB）の取得の取得
                StbSecIndex = xStbSecSteelName.IndexOf(ElementShape);
                ElementHight = xStbSecSteelParamA[StbSecIndex] / 1000;
                ElementWidth = xStbSecSteelParamB[StbSecIndex] / 1000;
                ElementShapeType = xStbSecSteelType[StbSecIndex];
            }

            // 始点と終点から梁断面サーフェスの作成
            ElementShapeMesh = MakeElementsMeshFromVertex(NodeStart, NodeEnd, ElementHight, ElementWidth, ElementShapeType, ElementStructureType, ElementNum, Elements);
            ElementNum = ElementNum + 1;
        }
        // ElementShapeMeshArray = ElementShapeMesh.ToArray;
        // return ElementShapeMeshArray;
    } 

    public List<Mesh> MakeElementsMeshFromVertex(Vector3 NodeStart, Vector3 NodeEnd, float ElementHight, float ElementWidth, string ElementShapeType, string ElementStructureType, int ElementNum, GameObject Elements) {

            // 部材のアングルの確認
            ElementAngleY = -1 * (float)Math.Atan((NodeEnd.y - NodeStart.y) / (NodeEnd.x - NodeStart.x));
            ElementAngleZ = -1 * (float)Math.Atan((NodeEnd.z - NodeStart.z) / (NodeEnd.x - NodeStart.x));

            // 描画用点の作成
            // 梁は部材天端の中心が起点に対して、柱・ブレースは部材芯が起点なので場合分け
            // NodeStart側   
            //  Y        S4 - S5 - S6 
            //  ^        |    |    |  
            //  o >  X   S1 - S2 - S3
            if (ElementStructureType == "Beam") {
                VertexS1 = new Vector3(NodeStart.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeStart.y - ElementHight,
                                       NodeStart.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
                VertexS2 = new Vector3(NodeStart.x,
                                       NodeStart.y - ElementHight,
                                       NodeStart.z
                                       );
                VertexS3 = new Vector3(NodeStart.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeStart.y - ElementHight,
                                       NodeStart.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
                VertexS4 = new Vector3(NodeStart.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeStart.y,
                                       NodeStart.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
                VertexS5 = NodeStart;
                VertexS6 = new Vector3(NodeStart.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeStart.y,
                                       NodeStart.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
            }
            else if (ElementStructureType == "Column") {
                VertexS1 = new Vector3(NodeStart.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                       NodeStart.y - (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                       NodeStart.z - (ElementHight / 2)
                                       );
                VertexS2 = new Vector3(NodeStart.x,
                                       NodeStart.y,
                                       NodeStart.z + (ElementHight / 2)
                                       );
                VertexS3 = new Vector3(NodeStart.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                       NodeStart.y + (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                       NodeStart.z - (ElementHight / 2)
                                       );
                VertexS4 = new Vector3(NodeStart.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                       NodeStart.y - (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                       NodeStart.z + (ElementHight / 2)
                                       );
                VertexS5 = new Vector3(NodeStart.x,
                                       NodeStart.y,
                                       NodeStart.z - (ElementHight / 2)
                                       );
                VertexS6 = new Vector3(NodeStart.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                       NodeStart.y + (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                       NodeStart.z + (ElementHight / 2)
                                       );
            }
            else if (ElementStructureType == "Brace") {
                VertexS1 = new Vector3(NodeStart.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeStart.y - (ElementWidth / 2),
                                       NodeStart.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
                VertexS2 = new Vector3(NodeStart.x,
                                       NodeStart.y - (ElementWidth / 2),
                                       NodeStart.z
                                       );
                VertexS3 = new Vector3(NodeStart.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeStart.y - (ElementWidth / 2),
                                       NodeStart.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
                VertexS4 = new Vector3(NodeStart.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeStart.y + (ElementWidth / 2),
                                       NodeStart.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
                VertexS5 = new Vector3(NodeStart.x,
                                       NodeStart.y + (ElementWidth / 2),
                                       NodeStart.z
                                       );
                VertexS6 = new Vector3(NodeStart.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeStart.y + (ElementWidth / 2),
                                       NodeStart.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
            }
            // NodeEnd側
            //  Y        E4 - E5 - E6
            //  ^        |    |    |
            //  o >  X   E1 - E2 - E3
            if (ElementStructureType == "Beam") {
                VertexE1 = new Vector3(NodeEnd.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeEnd.y - ElementHight,
                                       NodeEnd.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
                VertexE2 = new Vector3(NodeEnd.x,
                                       NodeEnd.y - ElementHight,
                                       NodeEnd.z
                                       );
                VertexE3 = new Vector3(NodeEnd.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeEnd.y - ElementHight,
                                       NodeEnd.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
                VertexE4 = new Vector3(NodeEnd.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeEnd.y,
                                       NodeEnd.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
                VertexE5 = NodeEnd;
                VertexE6 = new Vector3(NodeEnd.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeEnd.y,
                                       NodeEnd.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
            }
            else if (ElementStructureType == "Column") {
                VertexE1 = new Vector3(NodeEnd.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                       NodeEnd.y - (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                       NodeEnd.z - (ElementHight / 2)
                                       );
                VertexE2 = new Vector3(NodeEnd.x,
                                       NodeEnd.y,
                                       NodeEnd.z + (ElementHight / 2)
                                       );
                VertexE3 = new Vector3(NodeEnd.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                       NodeEnd.y + (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                       NodeEnd.z - (ElementHight / 2)
                                       );
                VertexE4 = new Vector3(NodeEnd.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                       NodeEnd.y - (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                       NodeEnd.z + (ElementHight / 2)
                                       );
                VertexE5 = new Vector3(NodeEnd.x,
                                       NodeEnd.y,
                                       NodeEnd.z - (ElementHight / 2)
                                       );
                VertexE6 = new Vector3(NodeEnd.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                       NodeEnd.y + (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                       NodeEnd.z + (ElementHight / 2)
                                       );
            }
            else if (ElementStructureType == "Brace") {
                VertexE1 = new Vector3(NodeEnd.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeEnd.y - (ElementWidth / 2),
                                       NodeEnd.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
                VertexE2 = new Vector3(NodeEnd.x,
                                       NodeEnd.y - (ElementWidth / 2),
                                       NodeEnd.z
                                       );
                VertexE3 = new Vector3(NodeEnd.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeEnd.y - (ElementWidth / 2),
                                       NodeEnd.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
                VertexE4 = new Vector3(NodeEnd.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeEnd.y + (ElementWidth / 2),
                                       NodeEnd.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
                VertexE5 = new Vector3(NodeEnd.x,
                                       NodeEnd.y + (ElementWidth / 2),
                                       NodeEnd.z
                                       );
                VertexE6 = new Vector3(NodeEnd.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                       NodeEnd.y + (ElementWidth / 2),
                                       NodeEnd.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                       );
            }

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            Mesh meshObj = new Mesh();
            if (this.ElementShapeType == "H") {
                // make upper flange
                vertices.Add(VertexS4);
                vertices.Add(VertexS6);
                vertices.Add(VertexE6);
                vertices.Add(VertexE4);
                // make bottom flange
                vertices.Add(VertexS1);
                vertices.Add(VertexS3);
                vertices.Add(VertexE3);
                vertices.Add(VertexE1);
                // make web 
                vertices.Add(VertexS5);
                vertices.Add(VertexS2);
                vertices.Add(VertexE2);
                vertices.Add(VertexE5);

                for(int i = 1; i < 4; ++i){
                    triangles.Add(4*i-4);
                    triangles.Add(4*i-3);
                    triangles.Add(4*i-2);
                    triangles.Add(4*i-2);
                    triangles.Add(4*i-1);
                    triangles.Add(4*i-4);
                }

            }
            else if (this.ElementShapeType == "BOX") {
                // make upper flange
                vertices.Add(VertexS4);
                vertices.Add(VertexS6);
                vertices.Add(VertexE6);
                vertices.Add(VertexE4);
                // make bottom flange
                vertices.Add(VertexS1);
                vertices.Add(VertexS3);
                vertices.Add(VertexE3);
                vertices.Add(VertexE1);
                // make web 1
                vertices.Add(VertexS4);
                vertices.Add(VertexS1);
                vertices.Add(VertexE1);
                vertices.Add(VertexE4);
                // make web 2
                vertices.Add(VertexS6);
                vertices.Add(VertexS3);
                vertices.Add(VertexE3);
                vertices.Add(VertexE6);

                for(int i = 1; i < 5; ++i){
                    triangles.Add(4*i-4);
                    triangles.Add(4*i-3);
                    triangles.Add(4*i-2);
                    triangles.Add(4*i-2);
                    triangles.Add(4*i-1);
                    triangles.Add(4*i-4);
                }
            }
            else if (this.ElementShapeType == "Pipe") {
                Debug.Log("Pipe is not supported");
                // LineCurve PipeCurve = new LineCurve(NodeStart, NodeEnd);
                // ElementShapeMesh.Add(Brep.CreatePipe(PipeCurve, ElementHight / 2.0, false, 0, false, GH_Component.DocumentTolerance(), GH_Component.DocumentAngleTolerance())[0]);
            }
            else if (this.ElementShapeType == "L") {
                // make bottom flange
                vertices.Add(VertexS1);
                vertices.Add(VertexS3);
                vertices.Add(VertexE3);
                vertices.Add(VertexE1);
                // make web
                vertices.Add(VertexS6);
                vertices.Add(VertexS3);
                vertices.Add(VertexE3);
                vertices.Add(VertexE6);

                for(int i = 1; i < 3; ++i){
                    triangles.Add(4*i-4);
                    triangles.Add(4*i-3);
                    triangles.Add(4*i-2);
                    triangles.Add(4*i-2);
                    triangles.Add(4*i-1);
                    triangles.Add(4*i-4);
                }
            }
            else {
            }
            
            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            
            meshObj.RecalculateNormals();
            
            string ElementName = string.Format(ElementStructureType + "{0}", ElementNum);
            GameObject Element = new GameObject(ElementName);
            Element.AddComponent<MeshFilter>().mesh = meshObj;
            Element.AddComponent<MeshRenderer>().material = material;
            Element.transform.parent = Elements.transform;

            return ElementShapeMesh;
        }
}
