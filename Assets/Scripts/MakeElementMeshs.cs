using System.Collections.Generic;
using System.Xml.Linq;
using System;

using UnityEngine;

public partial class STBReader : MonoBehaviour {
    void MakeSlabObjs(XDocument xdoc) {
        int[] NodeIndex = new int[4];
        string SlabName;
        int SlabNum = 0;
        var xSlabs = xdoc.Root.Descendants("StbSlab");
        GameObject Slabs = new GameObject("StbSlabs");

        foreach (var xSlab in xSlabs) {
            List<int> xSlabNodeIDs = new List<int>();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            int CountNode = 0;
            Mesh meshObj = new Mesh();

            var xNodeids = xSlab.Element("StbNodeid_List").Elements("StbNodeid");
            foreach (var xNodeid in xNodeids) {
                xSlabNodeIDs.Add((int)xNodeid.Attribute("id"));
                CountNode++;
            }

            int i = 0;
            while (i < 4) {
                // 頂点座標の取得
                NodeIndex[i] = VertexIDs.IndexOf(xSlabNodeIDs[i]);
                // Unityでの頂点座標の生成
                vertices.Add(StbNodes[NodeIndex[i]]);
                i++;
            }

            // Unityでの三角形メッシュの生成
            triangles.Add(CountNode - 4);
            triangles.Add(CountNode - 3);
            triangles.Add(CountNode - 2);
            triangles.Add(CountNode - 2);
            triangles.Add(CountNode - 1);
            triangles.Add(CountNode - 4);

            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.RecalculateNormals();

            SlabName = string.Format("Slab{0}", SlabNum);
            GameObject Slab = new GameObject(SlabName);
            Slab.AddComponent<MeshFilter>().mesh = meshObj;
            Slab.AddComponent<MeshRenderer>().material = material;
            Slab.transform.parent = Slabs.transform;

            SlabNum++;
            xSlabNodeIDs.Clear(); // foreachごとでListにAddし続けてるのでここで値をClear
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
        int LoopLength = new int();
        if (this.ElementShapeType == "H") {
            LoopLength = 4;
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
        }
        else if (this.ElementShapeType == "BOX") {
            LoopLength = 5;
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
        }
        else if (this.ElementShapeType == "Pipe") {
            Debug.Log("Pipe is not supported");
        }
        else if (this.ElementShapeType == "L") {
            LoopLength = 3;
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
        }
        else {
        }

        for (int i = 1; i < LoopLength; ++i) {
            triangles.Add(4 * i - 4);
            triangles.Add(4 * i - 3);
            triangles.Add(4 * i - 2);
            triangles.Add(4 * i - 2);
            triangles.Add(4 * i - 1);
            triangles.Add(4 * i - 4);
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
