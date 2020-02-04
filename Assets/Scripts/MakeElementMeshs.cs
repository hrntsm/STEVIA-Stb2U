using System.Collections.Generic;
using System.Xml.Linq;
using System;

using UnityEngine;

public partial class STBReader : MonoBehaviour {
    /// <summary>
    /// Make Slab GameObjects
    /// </summary>
    /// <param name="xdoc">stbdata</param>
    static void MakeSlabObjs(XDocument xdoc) {
        int[] NodeIndex = new int[4];
        string SlabName;
        int SlabNum = 0;
        var xSlabs = xdoc.Root.Descendants("StbSlab");
        GameObject Slabs = new GameObject("StbSlabs");

        foreach (var xSlab in xSlabs) {
            List<int> xSlabNodeIDs = new List<int>();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            Mesh meshObj = new Mesh();

            var xNodeids = xSlab.Element("StbNodeid_List").Elements("StbNodeid");
            foreach (var xNodeid in xNodeids) {
                xSlabNodeIDs.Add((int)xNodeid.Attribute("id"));
            }
            int i = 0;
            while (i < 4) {
                NodeIndex[i] = VertexIDs.IndexOf(xSlabNodeIDs[i]);
                vertices.Add(StbNodes[NodeIndex[i]]);
                i++;
            }
            triangles = GetTriangles(triangles, 1);
            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.RecalculateNormals();

            SlabName = string.Format("Slab{0}", SlabNum);
            GameObject Slab = new GameObject(SlabName);
            Slab.AddComponent<MeshFilter>().mesh = meshObj;
            Slab.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader")) {
                color = new Color(1.0f, 1.0f, 1.0f, 1.0f)
            };
            Slab.transform.parent = Slabs.transform;

            SlabNum++;
            xSlabNodeIDs.Clear(); // foreachごとでListにAddし続けてるのでここで値をClear
        }
    }

    static void MakeElementMesh(XDocument xdoc, string xDateTag, string ElementStructureType) {
        Vector3 NodeStart, NodeEnd;
        float   ElementHight = 0;
        float   ElementWidth = 0;
        int     ElementNum = 0;
        int     NodeIndexStart, NodeIndexEnd, xNodeStart, xNodeEnd, xElementIdSection,
                StbSecIndex, ElementIdSection;
        var     xElements = xdoc.Root.Descendants(xDateTag);
        string  ElementShape, xElementKind;
        string  ElementShapeType = "";

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
                    ElementHight = xSecRcBeamDepth[StbSecIndex] / 1000f;
                    ElementWidth = xSecRcBeamWidth[StbSecIndex] / 1000f;
                }
                else if (ElementStructureType == "Column") {
                    StbSecIndex = xSecRcColumnId.IndexOf(xElementIdSection);
                    ElementHight = xSecRcColumnDepth[StbSecIndex] / 1000f;
                    ElementWidth = xSecRcColumnWidth[StbSecIndex] / 1000f;
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
                else
                    ElementShape = "";
                // 断面形状（HxB）の取得の取得
                StbSecIndex = xStbSecSteelName.IndexOf(ElementShape);
                ElementHight = xStbSecSteelParamA[StbSecIndex] / 1000f;
                ElementWidth = xStbSecSteelParamB[StbSecIndex] / 1000f;
                ElementShapeType = xStbSecSteelType[StbSecIndex];
            }

            // 始点と終点から梁断面サーフェスの作成
            ElementShapeMesh = MakeElementsMeshFromVertex(NodeStart, NodeEnd, ElementHight, ElementWidth, ElementShapeType, ElementStructureType, ElementNum, Elements);
            ElementNum++;
        }
        ElementShapeMesh.Clear();
    }

    static public List<Mesh> MakeElementsMeshFromVertex(Vector3 NodeStart, Vector3 NodeEnd, float ElementHight, float ElementWidth, string ElementShapeType, string ElementStructureType, int ElementNum, GameObject Elements) {
        float ElementAngleY, ElementAngleZ;
        Vector3[] VertexS = new Vector3[6];
        Vector3[] VertexE = new Vector3[6];

        // 部材のアングルの確認
        ElementAngleY = -1 * (float)Math.Atan((NodeEnd.y - NodeStart.y) / (NodeEnd.x - NodeStart.x));
        ElementAngleZ = -1 * (float)Math.Atan((NodeEnd.z - NodeStart.z) / (NodeEnd.x - NodeStart.x));

        // 描画用点の作成
        // 梁は部材天端の中心が起点に対して、柱・ブレースは部材芯が起点なので場合分け
        // NodeStart側   
        //  Y        S3 - S4 - S5 
        //  ^        |    |    |  
        //  o >  X   S0 - S1 - S2
        if (ElementStructureType == "Beam") {
            VertexS[0] = new Vector3(NodeStart.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeStart.y - ElementHight,
                                     NodeStart.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
            VertexS[1] = new Vector3(NodeStart.x,
                                     NodeStart.y - ElementHight,
                                     NodeStart.z
                                     );
            VertexS[2] = new Vector3(NodeStart.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeStart.y - ElementHight,
                                     NodeStart.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
            VertexS[3] = new Vector3(NodeStart.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeStart.y,
                                     NodeStart.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
            VertexS[4] = NodeStart;
            VertexS[5] = new Vector3(NodeStart.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeStart.y,
                                     NodeStart.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
        }
        else if (ElementStructureType == "Column") {
            VertexS[0] = new Vector3(NodeStart.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                     NodeStart.y - (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                     NodeStart.z - (ElementHight / 2)
                                     );
            VertexS[1] = new Vector3(NodeStart.x,
                                     NodeStart.y,
                                     NodeStart.z + (ElementHight / 2)
                                     );
            VertexS[2] = new Vector3(NodeStart.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                     NodeStart.y + (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                     NodeStart.z - (ElementHight / 2)
                                     );
            VertexS[3] = new Vector3(NodeStart.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                     NodeStart.y - (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                     NodeStart.z + (ElementHight / 2)
                                     );
            VertexS[4] = new Vector3(NodeStart.x,
                                     NodeStart.y,
                                     NodeStart.z - (ElementHight / 2)
                                     );
            VertexS[5] = new Vector3(NodeStart.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                     NodeStart.y + (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                     NodeStart.z + (ElementHight / 2)
                                     );
        }
        else if (ElementStructureType == "Brace") {
            VertexS[0] = new Vector3(NodeStart.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeStart.y - (ElementWidth / 2),
                                     NodeStart.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
            VertexS[1] = new Vector3(NodeStart.x,
                                     NodeStart.y - (ElementWidth / 2),
                                     NodeStart.z
                                     );
            VertexS[2] = new Vector3(NodeStart.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeStart.y - (ElementWidth / 2),
                                     NodeStart.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
            VertexS[3] = new Vector3(NodeStart.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeStart.y + (ElementWidth / 2),
                                     NodeStart.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
            VertexS[4] = new Vector3(NodeStart.x,
                                     NodeStart.y + (ElementWidth / 2),
                                     NodeStart.z
                                     );
            VertexS[5] = new Vector3(NodeStart.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeStart.y + (ElementWidth / 2),
                                     NodeStart.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
        }
        // NodeEnd側
        //  Y        E3 - E4 - E5
        //  ^        |    |    |
        //  o >  X   E0 - E1 - E2
        if (ElementStructureType == "Beam") {
            VertexE[0] = new Vector3(NodeEnd.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeEnd.y - ElementHight,
                                     NodeEnd.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
            VertexE[1] = new Vector3(NodeEnd.x,
                                     NodeEnd.y - ElementHight,
                                     NodeEnd.z
                                     );
            VertexE[2] = new Vector3(NodeEnd.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeEnd.y - ElementHight,
                                     NodeEnd.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
            VertexE[3] = new Vector3(NodeEnd.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeEnd.y,
                                     NodeEnd.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
            VertexE[4] = NodeEnd;
            VertexE[5] = new Vector3(NodeEnd.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeEnd.y,
                                     NodeEnd.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
        }
        else if (ElementStructureType == "Column") {
            VertexE[0] = new Vector3(NodeEnd.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                     NodeEnd.y - (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                     NodeEnd.z - (ElementHight / 2)
                                     );
            VertexE[1] = new Vector3(NodeEnd.x,
                                     NodeEnd.y,
                                     NodeEnd.z + (ElementHight / 2)
                                     );
            VertexE[2] = new Vector3(NodeEnd.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                     NodeEnd.y + (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                     NodeEnd.z - (ElementHight / 2)
                                     );
            VertexE[3] = new Vector3(NodeEnd.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                     NodeEnd.y - (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                     NodeEnd.z + (ElementHight / 2)
                                     );
            VertexE[4] = new Vector3(NodeEnd.x,
                                     NodeEnd.y,
                                     NodeEnd.z - (ElementHight / 2)
                                     );
            VertexE[5] = new Vector3(NodeEnd.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleY),
                                     NodeEnd.y + (ElementWidth / 2) * (float)Math.Cos(ElementAngleY),
                                     NodeEnd.z + (ElementHight / 2)
                                     );
        }
        else if (ElementStructureType == "Brace") {
            VertexE[0] = new Vector3(NodeEnd.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeEnd.y - (ElementWidth / 2),
                                     NodeEnd.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
            VertexE[1] = new Vector3(NodeEnd.x,
                                     NodeEnd.y - (ElementWidth / 2),
                                     NodeEnd.z
                                     );
            VertexE[2] = new Vector3(NodeEnd.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeEnd.y - (ElementWidth / 2),
                                     NodeEnd.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
            VertexE[3] = new Vector3(NodeEnd.x + (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeEnd.y + (ElementWidth / 2),
                                     NodeEnd.z + (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
            VertexE[4] = new Vector3(NodeEnd.x,
                                     NodeEnd.y + (ElementWidth / 2),
                                     NodeEnd.z
                                     );
            VertexE[5] = new Vector3(NodeEnd.x - (ElementWidth / 2) * (float)Math.Sin(ElementAngleZ),
                                     NodeEnd.y + (ElementWidth / 2),
                                     NodeEnd.z - (ElementWidth / 2) * (float)Math.Cos(ElementAngleZ)
                                     );
        }

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        Mesh meshObj = new Mesh();
        int MeshNum = 0;
        if (ElementShapeType == "H") {
            MeshNum = 3;
            AddUpperFlangeVertices(vertices, VertexS, VertexE);
            AddBottomFlangeVertices(vertices, VertexS, VertexE);
            AddCenterWebVertices(vertices, VertexS, VertexE);
        }
        else if (ElementShapeType == "BOX") {
            MeshNum = 4;
            AddUpperFlangeVertices(vertices, VertexS, VertexE);
            AddBottomFlangeVertices(vertices, VertexS, VertexE);
            AddSide1WebVertices(vertices, VertexS, VertexE);
            AddSide2WebVertices(vertices, VertexS, VertexE);
        }
        else if (ElementShapeType == "Pipe") {
            Debug.Log("Pipe is not supported");
        }
        else if (ElementShapeType == "L") {
            MeshNum = 2;
            AddBottomFlangeVertices(vertices, VertexS, VertexE);
            AddSide2WebVertices(vertices, VertexS, VertexE);
        }
        triangles = GetTriangles(triangles, MeshNum);
        meshObj.vertices = vertices.ToArray();
        meshObj.triangles = triangles.ToArray();
        meshObj.RecalculateNormals();

        string ElementName = string.Format(ElementStructureType + "{0}", ElementNum);
        GameObject Element = new GameObject(ElementName);
        Element.AddComponent<MeshFilter>().mesh = meshObj;
        Element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader")) {
            color = new Color(1.0f, 1.0f, 1.0f, 1.0f)
        }; ;
        Element.transform.parent = Elements.transform;

        return ElementShapeMesh;
    }

    /// <summary>
    /// Get triangles vertex infomation
    /// </summary>
    /// <param name="triangles"></param>
    /// <param name="LoopLength"></param>
    static List<int> GetTriangles(List<int> triangles, int MeshNum) {
        for (int i = 1; i <= MeshNum; ++i) {
            triangles.Add(4 * i - 4);
            triangles.Add(4 * i - 3);
            triangles.Add(4 * i - 2);
            triangles.Add(4 * i - 2);
            triangles.Add(4 * i - 1);
            triangles.Add(4 * i - 4);
        }
        return (triangles);
    }

    static List<Vector3> AddUpperFlangeVertices(List<Vector3> vertices, Vector3[] VertexS, Vector3[] VertexE) {
        vertices.Add(VertexS[3]);
        vertices.Add(VertexS[5]);
        vertices.Add(VertexE[5]);
        vertices.Add(VertexE[3]);
        return (vertices);
    }
    static List<Vector3> AddBottomFlangeVertices(List<Vector3> vertices, Vector3[] VertexS, Vector3[] VertexE) {
        vertices.Add(VertexS[0]);
        vertices.Add(VertexS[2]);
        vertices.Add(VertexE[2]);
        vertices.Add(VertexE[0]);
        return (vertices);
    }
    static List<Vector3> AddCenterWebVertices(List<Vector3> vertices, Vector3[] VertexS, Vector3[] VertexE) {
        vertices.Add(VertexS[4]);
        vertices.Add(VertexS[1]);
        vertices.Add(VertexE[1]);
        vertices.Add(VertexE[4]);
        return (vertices);
    }
    static List<Vector3> AddSide1WebVertices(List<Vector3> vertices, Vector3[] VertexS, Vector3[] VertexE) {
        vertices.Add(VertexS[3]);
        vertices.Add(VertexS[0]);
        vertices.Add(VertexE[0]);
        vertices.Add(VertexE[3]);
        return (vertices);
    }
    static List<Vector3> AddSide2WebVertices(List<Vector3> vertices, Vector3[] VertexS, Vector3[] VertexE) {
        vertices.Add(VertexS[5]);
        vertices.Add(VertexS[2]);
        vertices.Add(VertexE[2]);
        vertices.Add(VertexE[5]);
        return (vertices);
    }
}
