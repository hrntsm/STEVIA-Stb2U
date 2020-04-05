using System.Collections.Generic;
using System.Xml.Linq;
using System;

using UnityEngine;

using Stevia.STB.Model;
using Stevia.STB.Model.Member;

namespace Stevia {

    public partial class STBReader:MonoBehaviour {
        /// <summary>
        /// スラブのオブジェクトの作成
        /// </summary>
        void MakeSlab(StbSlabs stbSlabs) {
            int slabNum = 0;

            GameObject slabs = new GameObject("StbSlabs");
            GameObject slabBar = new GameObject("StbSlabBar");
            slabs.transform.parent = GameObject.Find("StbData").transform;
            slabBar.transform.parent = GameObject.Find("StbData").transform;

            foreach (var NodeIds in stbSlabs.NodeIdList) {
                int[] nodeIndex = new int[NodeIds.Count];
                Mesh meshObj = new Mesh();

                for (int i = 0; i < NodeIds.Count; i++) {
                    nodeIndex[i] = _nodes.Id.IndexOf(NodeIds[i]);
                }
                meshObj = CreateMesh.Slab(_nodes.Vertex, nodeIndex);

                var slabName = string.Format("Slab{0}", slabNum);
                GameObject slab = new GameObject(slabName);
                slab.AddComponent<MeshFilter>().mesh = meshObj;
                slab.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader")) {
                    color = GetMemberColor(KindsStructure.RC, FrameType.Slab)
                };
                slab.transform.localPosition = new Vector3(0, (float)stbSlabs.Level[slabNum], 0);
                slab.transform.parent = slabs.transform;
                slabNum++;
            }
        }

        /// <summary>
        /// 壁のオブジェクトの作成
        /// </summary>
        void MakeWall(StbWalls stbWalls) {
            int wallNum = 0;

            GameObject walls = new GameObject("StbWalls");
            GameObject wallBar = new GameObject("StbWallBar");
            walls.transform.parent = GameObject.Find("StbData").transform;
            wallBar.transform.parent = GameObject.Find("StbData").transform;

            foreach (var NodeIds in stbWalls.NodeIdList) {
                int[] nodeIndex = new int[NodeIds.Count];
                Mesh meshObj = new Mesh();

                for (int i = 0; i < NodeIds.Count; i++) {
                    nodeIndex[i] = _nodes.Id.IndexOf(NodeIds[i]);
                }
                meshObj = CreateMesh.Slab(_nodes.Vertex, nodeIndex);

                var wallName = string.Format("Wall{0}", wallNum);
                GameObject wall = new GameObject(wallName);
                wall.AddComponent<MeshFilter>().mesh = meshObj;
                wall.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader")) {
                    color = GetMemberColor(KindsStructure.RC, FrameType.Wall)
                };
                wall.transform.parent = walls.transform;
                wallNum++;
            }
        }

        void MakeFrame(List<StbFrame> stbFrames) {
            Vector3 nodeStart, nodeEnd;
            float height = 0;
            float width = 0;
            int secIndex = 0;
            int nodeIndexStart, nodeIndexEnd, idShape;
            string shape;
            string shapeType = "";

            foreach (var stbFrame in stbFrames) {

                GameObject elements = new GameObject(stbFrame.Tag + "s");
                GameObject barObj = new GameObject(stbFrame.Tag + "Bar");
                elements.transform.parent = GameObject.Find("StbData").transform;
                barObj.transform.parent = GameObject.Find("StbData").transform;

                for (int eNum = 0; eNum < stbFrame.Id.Count; eNum++) {
                    var idSection = stbFrame.IdSection[eNum];
                    var xKind = stbFrame.KindStructure[eNum] ;

                    // 始点と終点の座標取得
                    nodeIndexStart = _nodes.Id.IndexOf(stbFrame.IdNodeStart[eNum]);
                    nodeIndexEnd = _nodes.Id.IndexOf(stbFrame.IdNodeEnd[eNum]);
                    nodeStart = _nodes.Vertex[nodeIndexStart];
                    nodeEnd = _nodes.Vertex[nodeIndexEnd];

                    if (xKind == KindsStructure.RC) {
                        switch (stbFrame.FrameType) {
                            case FrameType.Column:
                            case FrameType.Post:
                                secIndex = _secColumnRC.Id.IndexOf(idSection);
                                height = _secColumnRC.Height[secIndex];
                                width = _secColumnRC.Width[secIndex];
                                break;
                            case FrameType.Girder:
                            case FrameType.Beam:
                                secIndex = _secBeamRC.Id.IndexOf(idSection);
                                height = _secBeamRC.Depth[secIndex];
                                width = _secBeamRC.Width[secIndex];
                                break;
                            default:
                                break;
                        }
                        if (height == 0)
                            shapeType = "Pipe";
                        else
                            shapeType = "BOX";
                    }
                    else if (xKind == KindsStructure.S) {
                        switch (stbFrame.FrameType) {
                            case FrameType.Column:
                            case FrameType.Post:
                                idShape = _secColumnS.Id.IndexOf(idSection);
                                shape = _secColumnS.Shape[idShape];
                                break;
                            case FrameType.Girder:
                            case FrameType.Beam:
                                idShape = _secBeamS.Id.IndexOf(idSection);
                                shape = _secBeamS.Shape[idShape];
                                break;
                            case FrameType.Brace:
                                idShape = _secBraceS.Id.IndexOf(idSection);
                                shape = _secBraceS.Shape[idShape];
                                break;
                            default:
                                shape = "";
                                break;
                        }
                        secIndex = _xStName.IndexOf(shape);
                        height = _xStParamA[secIndex] / 1000f;
                        width = _xStParamB[secIndex] / 1000f;
                        shapeType = _xStType[secIndex];
                    }
                    _shapeMesh = MakeElementsMeshFromVertex(nodeStart, nodeEnd, height, width, shapeType, stbFrame.FrameType, eNum, elements, xKind);
                    // 配筋の作成
                    if (xKind == KindsStructure.RC) {
                        if (shapeType == "BOX") {
                            switch (stbFrame.FrameType) {
                                case FrameType.Column:
                                case FrameType.Post:
                                    CreateBar.Column(secIndex, nodeStart, nodeEnd, width, height, barObj, eNum);
                                    break;
                                case FrameType.Girder:
                                case FrameType.Beam:
                                    CreateBar.Beam(secIndex, nodeStart, nodeEnd, width, height, barObj, eNum);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            _shapeMesh.Clear();
        }

        public List<Mesh> MakeElementsMeshFromVertex(Vector3 nodeStart, Vector3 nodeEnd, float hight, float width, string shapeType, FrameType FrameType, int eNum, GameObject elements, KindsStructure kind) {
            Vector3[] vertexS = new Vector3[6];
            Vector3[] vertexE = new Vector3[6];
            Mesh meshObj = new Mesh();

            float dx = nodeEnd.x - nodeStart.x;
            float dy = nodeEnd.y - nodeStart.y;
            float dz = nodeEnd.z - nodeStart.z;
            float angleY = -1f * Mathf.Atan2(dy, dx);
            float angleZ = -1f * Mathf.Atan2(dz, dx);

            // 梁は部材天端の中心が起点に対して、柱・ブレースは部材芯が起点なので場合分け
            switch (FrameType) {
                case FrameType.Column:
                case FrameType.Post:
                    vertexS = GetColumnVertex(nodeStart, width, hight, angleY);
                    vertexE = GetColumnVertex(nodeEnd, width, hight, angleY);
                    break;
                case FrameType.Girder:
                case FrameType.Beam:
                    vertexS = GetGirderVertex(nodeStart, width, hight, angleZ);
                    vertexE = GetGirderVertex(nodeEnd, width, hight, angleZ);
                    break;
                case FrameType.Brace:
                    vertexS = GetBraceVertex(nodeStart, width, hight, angleZ);
                    vertexE = GetBraceVertex(nodeEnd, width, hight, angleZ);
                    break;
                default: break;
            }
            switch (shapeType) {
                case "H":
                    meshObj = CreateMesh.H(vertexS, vertexE); break;
                case "BOX":
                    meshObj = CreateMesh.BOX(vertexS, vertexE); break;
                case "Pipe":
                    meshObj = CreateMesh.Pipe(nodeStart, nodeEnd, width / 2); break;
                case "L":
                    meshObj = CreateMesh.L(vertexS, vertexE); break;
                default: break;
            }

            string name = string.Format(FrameType + "{0}", eNum);
            GameObject element = new GameObject(name);
            element.AddComponent<MeshFilter>().mesh = meshObj;
            element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader")) {
                color = GetMemberColor(kind, FrameType)
            };
            element.transform.parent = elements.transform;

            return _shapeMesh;
        }

        Color GetMemberColor(KindsStructure kind, FrameType FrameType) {
            Color unexpected = new Color(1, 0, 1, 1);

            if (kind == KindsStructure.RC) {
                switch (FrameType) {
                    case FrameType.Column: return ColorInput._memberColor[0];
                    case FrameType.Post: return ColorInput._memberColor[1];
                    case FrameType.Girder: return ColorInput._memberColor[2];
                    case FrameType.Beam: return ColorInput._memberColor[3];
                    case FrameType.Brace: return ColorInput._memberColor[4];
                    case FrameType.Slab: return ColorInput._memberColor[5];
                    case FrameType.Wall: return ColorInput._memberColor[5];
                    default: return unexpected;
                }
            }
            else if (kind == KindsStructure.S) {
                switch (FrameType) {
                    case FrameType.Column: return ColorInput._memberColor[6];
                    case FrameType.Post: return ColorInput._memberColor[7];
                    case FrameType.Girder: return ColorInput._memberColor[8];
                    case FrameType.Beam: return ColorInput._memberColor[9];
                    case FrameType.Brace: return ColorInput._memberColor[10];
                    default: return unexpected;
                }
            }
            else return unexpected;
        }

        Vector3[] GetGirderVertex(Vector3 node, float width, float hight, float angle) {
            //  Y        3 - 4 - 5 
            //  ^        |   |   |  
            //  o >  X   0 - 1 - 2
            Vector3[] vertex = new Vector3[6];

            vertex[0] = new Vector3(node.x + width / 2 * (float)Math.Sin(angle),
                                    node.y - hight,
                                    node.z + width / 2 * (float)Math.Cos(angle)
                                    );
            vertex[1] = new Vector3(node.x,
                                    node.y - hight,
                                    node.z
                                    );
            vertex[2] = new Vector3(node.x - width / 2 * (float)Math.Sin(angle),
                                    node.y - hight,
                                    node.z - width / 2 * (float)Math.Cos(angle)
                                    );
            vertex[3] = new Vector3(node.x + width / 2 * (float)Math.Sin(angle),
                                    node.y,
                                    node.z + width / 2 * (float)Math.Cos(angle)
                                    );
            vertex[4] = node;
            vertex[5] = new Vector3(node.x - width / 2 * (float)Math.Sin(angle),
                                    node.y,
                                    node.z - width / 2 * (float)Math.Cos(angle)
                                    );
            return (vertex);
        }

        Vector3[] GetColumnVertex(Vector3 node, float width, float hight, float angle) {
            //  Y        3 - 4 - 5 
            //  ^        |   |   |  
            //  o >  X   0 - 1 - 2
            Vector3[] vertex = new Vector3[6];

            vertex[0] = new Vector3(node.x - width / 2 * (float)Math.Sin(angle),
                                    node.y - width / 2 * (float)Math.Cos(angle),
                                    node.z - hight / 2
                                    );
            vertex[1] = new Vector3(node.x,
                                    node.y,
                                    node.z + hight / 2
                                    );
            vertex[2] = new Vector3(node.x + width / 2 * (float)Math.Sin(angle),
                                    node.y + width / 2 * (float)Math.Cos(angle),
                                    node.z - hight / 2
                                    );
            vertex[3] = new Vector3(node.x - width / 2 * (float)Math.Sin(angle),
                                    node.y - width / 2 * (float)Math.Cos(angle),
                                    node.z + hight / 2
                                    );
            vertex[4] = new Vector3(node.x,
                                    node.y,
                                    node.z - hight / 2
                                    );
            vertex[5] = new Vector3(node.x + width / 2 * (float)Math.Sin(angle),
                                    node.y + width / 2 * (float)Math.Cos(angle),
                                    node.z + hight / 2
                                    );
            return (vertex);
        }

        Vector3[] GetBraceVertex(Vector3 node, float width, float hight, float angle) {
            //  Y        3 - 4 - 5 
            //  ^        |   |   |  
            //  o >  X   0 - 1 - 2
            Vector3[] vertex = new Vector3[6];

            vertex[0] = new Vector3(node.x + width / 2 * (float)Math.Sin(angle),
                                    node.y - width / 2,
                                    node.z + width / 2 * (float)Math.Cos(angle)
                                    );
            vertex[1] = new Vector3(node.x,
                                    node.y - width / 2,
                                    node.z
                                    );
            vertex[2] = new Vector3(node.x - width / 2 * (float)Math.Sin(angle),
                                    node.y - width / 2,
                                    node.z - width / 2 * (float)Math.Cos(angle)
                                    );
            vertex[3] = new Vector3(node.x + width / 2 * (float)Math.Sin(angle),
                                    node.y + width / 2,
                                    node.z + width / 2 * (float)Math.Cos(angle)
                                    );
            vertex[4] = new Vector3(node.x,
                                    node.y + width / 2,
                                    node.z
                                    );
            vertex[5] = new Vector3(node.x - width / 2 * (float)Math.Sin(angle),
                                    node.y + width / 2,
                                    node.z - width / 2 * (float)Math.Cos(angle)
                                    );
            return (vertex);
        }
    }
}
