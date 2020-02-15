using System.Collections.Generic;
using System.Xml.Linq;
using System;

using UnityEngine;

public partial class STBReader : MonoBehaviour {
    /// <summary>
    /// Make Slab GameObjects
    /// </summary>
    void MakeSlabObjs(XDocument xdoc) {
        int[] nodeIndex = new int[4];
        string slabName;
        int slabNum = 0;
        var xSlabs = xdoc.Root.Descendants("StbSlab");
        GameObject slabs = new GameObject("StbSlabs");

        foreach (var xSlab in xSlabs) {
            List<int> xSlabNodeIDs = new List<int>();
            Mesh meshObj = new Mesh();

            var xNodeids = xSlab.Element("StbNodeid_List").Elements("StbNodeid");
            foreach (var xNodeid in xNodeids)
                xSlabNodeIDs.Add((int)xNodeid.Attribute("id"));
            int i = 0;
            while (i < 4) {
                nodeIndex[i] = m_vertexIDs.IndexOf(xSlabNodeIDs[i]);
                i++;
            }
            meshObj = CreateMesh.Slab(m_stbNodes, nodeIndex);

            slabName = string.Format("Slab{0}", slabNum);
            GameObject Slab = new GameObject(slabName);
            Slab.AddComponent<MeshFilter>().mesh = meshObj;
            Slab.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader")) {
                color = GetMemberColor("RC", "Slab")
            };
            Slab.transform.parent = slabs.transform;

            slabNum++;
            xSlabNodeIDs.Clear(); // foreachごとでListにAddし続けてるのでここで値をClear
        }
    }

    void MakeElementMesh(XDocument xdoc, string xDateTag, string structureType) {
        Vector3 nodeStart, nodeEnd;
        float   hight = 0;
        float   Width = 0;
        int     elementNum = 0;
        int     nodeIndexStart, nodeIndexEnd, xNodeStart, xNodeEnd, xElementIdSection,
                stbSecIndex, idSection;
        var     xElements = xdoc.Root.Descendants(xDateTag);
        string  elementShape, xElementKind;
        string  shapeType = "";

        GameObject elements = new GameObject(xDateTag + "s");
        foreach (var xElement in xElements) {
            if (structureType == "Girder" || structureType == "Beam" || structureType == "Brace") {
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
            nodeIndexStart = m_vertexIDs.IndexOf(xNodeStart);
            nodeIndexEnd = m_vertexIDs.IndexOf(xNodeEnd);
            nodeStart = m_stbNodes[nodeIndexStart];
            nodeEnd = m_stbNodes[nodeIndexEnd];

            if (xElementKind == "RC") {
                // 断面形状名（shape) と 断面形状（HxB）の取得の取得
                if (structureType == "Girder" || structureType == "Beam") {
                    stbSecIndex = m_xRcBeamId.IndexOf(xElementIdSection);
                    hight = m_xRcBeamDepth[stbSecIndex] / 1000f;
                    Width = m_xRcBeamWidth[stbSecIndex] / 1000f;
                }
                else if (structureType == "Column" || structureType == "Post") {
                    stbSecIndex = m_xRcColumnId.IndexOf(xElementIdSection);
                    hight = m_xRcColumnDepth[stbSecIndex] / 1000f;
                    Width = m_xRcColumnWidth[stbSecIndex] / 1000f;
                }

                if (Width == 0) {
                    shapeType = "Pipe";
                }
                else {
                    shapeType = "BOX";
                }
            }
            else if (xElementKind == "S") {
                // 断面形状名（shape）の取得の取得
                if (structureType == "Girder" || structureType == "Beam") {
                    idSection = m_xStBeamId.IndexOf(xElementIdSection);
                    elementShape = m_xStBeamShape[idSection];
                }
                else if (structureType == "Column" || structureType == "Post") {
                    idSection = m_xStColumnId.IndexOf(xElementIdSection);
                    elementShape = m_xStColumnShape[idSection];
                }
                else if (structureType == "Brace") {
                    idSection = m_xStBraceId.IndexOf(xElementIdSection);
                    elementShape = m_xStBraceShape[idSection];
                }
                else
                    elementShape = "";
                // 断面形状（HxB）の取得の取得
                stbSecIndex = m_xSteelName.IndexOf(elementShape);
                hight = m_xSteelParamA[stbSecIndex] / 1000f;
                Width = m_xSteelParamB[stbSecIndex] / 1000f;
                shapeType = m_xSteelType[stbSecIndex];
            }

            // 始点と終点から梁断面サーフェスの作成
            m_shapeMesh = MakeElementsMeshFromVertex(nodeStart, nodeEnd, hight, Width, shapeType, structureType, elementNum, elements, xElementKind);
            elementNum++;
        }
        m_shapeMesh.Clear();
    }

    public List<Mesh> MakeElementsMeshFromVertex(Vector3 NodeStart, Vector3 NodeEnd, float ElementHight, float ElementWidth, string ShapeType, string StructureType, int ElementNum, GameObject Elements, string ElementKind) {
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
        if (StructureType == "Girder" || StructureType == "Beam") {
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
        else if (StructureType == "Column" || StructureType == "Post") {
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
        else if (StructureType == "Brace") {
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
        if (StructureType == "Girder" || StructureType == "Beam") {
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
        else if (StructureType == "Column" || StructureType == "Post") {
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
        else if (StructureType == "Brace") {
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

        Mesh meshObj = new Mesh();
        switch (ShapeType) {
            case "H":
                meshObj = CreateMesh.H(VertexS, VertexE);
                break;
            case "BOX":
                meshObj = CreateMesh.BOX(VertexS, VertexE);
                break;
            case "Pipe":
                Debug.Log("Pipe is not supported.");
                break;
            case "L":
                meshObj = CreateMesh.L(VertexS, VertexE);
                break;
            default:
                break;
        }

        string ElementName = string.Format(StructureType + "{0}", ElementNum);
        GameObject Element = new GameObject(ElementName);
        Element.AddComponent<MeshFilter>().mesh = meshObj;
        Element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader")) {
            color = GetMemberColor(ElementKind, StructureType)
        };
        Element.transform.parent = Elements.transform;

        return m_shapeMesh;
    }

    Color GetMemberColor(string ElemKind, string ElemStructType) {
        Color unexpectedColor = new Color(1, 0, 1, 1);

        if (ElemKind == "RC") {
            if (ElemStructType == "Column") return ColorInput.m_memberColor[0];
            else if (ElemStructType == "Post") return ColorInput.m_memberColor[1];
            else if (ElemStructType == "Girder") return ColorInput.m_memberColor[2];
            else if (ElemStructType == "Beam") return ColorInput.m_memberColor[3];
            else if (ElemStructType == "Brace") return ColorInput.m_memberColor[4];
            else if (ElemStructType == "Slab") return ColorInput.m_memberColor[5];
            else return unexpectedColor;
        }
        else if (ElemKind == "S") {
            if (ElemStructType == "Column") return ColorInput.m_memberColor[6];
            else if (ElemStructType == "Post") return ColorInput.m_memberColor[7];
            else if (ElemStructType == "Girder") return ColorInput.m_memberColor[8];
            else if (ElemStructType == "Beam") return ColorInput.m_memberColor[9];
            else if (ElemStructType == "Brace") return ColorInput.m_memberColor[10];
            else return unexpectedColor;
        }
        else return unexpectedColor;
    }
}
