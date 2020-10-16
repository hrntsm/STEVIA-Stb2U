using System;
using System.Collections.Generic;
using Stevia.STB.Model;
using Stevia.STB.Model.Member;
using Stevia.STB.Model.Section;
using UI;
using UnityEngine;

namespace Model
{
    public partial class StbReader
    {
        /// <summary>
        /// スラブのオブジェクトの作成
        /// </summary>
        private void MakeSlab(StbSlabs stbSlabs)
        {
            var slabNum = 0;

            var slabs = new GameObject("StbSlabs");
            var slabBar = new GameObject("StbSlabBar");
            slabs.transform.parent = GameObject.Find("StbData").transform;
            slabBar.transform.parent = GameObject.Find("StbData").transform;

            foreach (List<int> nodeIds in stbSlabs.NodeIdList)
            {
                var nodeIndex = new int[nodeIds.Count];

                for (var i = 0; i < nodeIds.Count; i++)
                {
                    nodeIndex[i] = StbReader.Nodes.Id.IndexOf(nodeIds[i]);
                }
                Mesh meshObj = CreateMesh.Slab(StbReader.Nodes.Vertex, nodeIndex);

                var slabName = $"Slab{slabNum}";
                var slab = new GameObject(slabName);
                slab.AddComponent<MeshFilter>().mesh = meshObj;
                slab.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader"))
                {
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
        private void MakeWall(StbWalls stbWalls) 
        {
            var wallNum = 0;

            var walls = new GameObject("StbWalls");
            var wallBar = new GameObject("StbWallBar");
            walls.transform.parent = GameObject.Find("StbData").transform;
            wallBar.transform.parent = GameObject.Find("StbData").transform;

            foreach (List<int> nodeIds in stbWalls.NodeIdList)
            {
                var nodeIndex = new int[nodeIds.Count];

                for (var i = 0; i < nodeIds.Count; i++)
                {
                    nodeIndex[i] = StbReader.Nodes.Id.IndexOf(nodeIds[i]);
                }
                Mesh meshObj = CreateMesh.Slab(StbReader.Nodes.Vertex, nodeIndex);

                var wallName = $"Wall{wallNum}";
                var wall = new GameObject(wallName);
                wall.AddComponent<MeshFilter>().mesh = meshObj;
                wall.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader"))
                {
                    color = GetMemberColor(KindsStructure.RC, FrameType.Wall)
                };
                wall.transform.parent = walls.transform;
                wallNum++;
            }
        }

        private void MakeFrame(IEnumerable<StbFrame> stbFrames)
        {
            var height = 0f;
            var width = 0f;
            var secIndex = 0;
            var shape = string.Empty;
            var shapeType = ShapeTypes.H;

            foreach (StbFrame stbFrame in stbFrames)
            {
                var elements = new GameObject(stbFrame.Tag + "s");
                var barObj = new GameObject(stbFrame.Tag + "Bar");
                elements.transform.parent = GameObject.Find("StbData").transform;
                barObj.transform.parent = GameObject.Find("StbData").transform;

                for (var eNum = 0; eNum < stbFrame.Id.Count; eNum++) 
                {
                    int idSection = stbFrame.IdSection[eNum];
                    KindsStructure kind = stbFrame.KindStructure[eNum] ;

                    // 始点と終点の座標取得
                    int nodeIndexStart = StbReader.Nodes.Id.IndexOf(stbFrame.IdNodeStart[eNum]);
                    int nodeIndexEnd = StbReader.Nodes.Id.IndexOf(stbFrame.IdNodeEnd[eNum]);
                    Vector3 nodeStart = StbReader.Nodes.Vertex[nodeIndexStart];
                    Vector3 nodeEnd = StbReader.Nodes.Vertex[nodeIndexEnd];

                    switch (kind)
                    {
                        case KindsStructure.RC:
                        {
                            switch (stbFrame.FrameType) 
                            {
                                case FrameType.Column:
                                case FrameType.Post:
                                    secIndex = StbReader.SecColumnRc.Id.IndexOf(idSection);
                                    height = StbReader.SecColumnRc.Height[secIndex];
                                    width = StbReader.SecColumnRc.Width[secIndex];
                                    break;
                                case FrameType.Girder:
                                case FrameType.Beam:
                                    secIndex = StbReader.SecBeamRc.Id.IndexOf(idSection);
                                    height = StbReader.SecBeamRc.Depth[secIndex];
                                    width = StbReader.SecBeamRc.Width[secIndex];
                                    break;
                                case FrameType.Brace:
                                    break;
                                case FrameType.Slab:
                                    break;
                                case FrameType.Wall:
                                    break;
                                case FrameType.Any:
                                    break;
                            }
                            shapeType = height == 0 ? ShapeTypes.Pipe : ShapeTypes.BOX;
                            break;
                        }
                        case KindsStructure.S:
                        {
                            int idShape;
                            switch (stbFrame.FrameType)
                            {
                                case FrameType.Column:
                                case FrameType.Post:
                                    idShape = StbReader.SecColumnS.Id.IndexOf(idSection);
                                    shape = StbReader.SecColumnS.Shape[idShape];
                                    break;
                                case FrameType.Girder:
                                case FrameType.Beam:
                                    idShape = StbReader.SecBeamS.Id.IndexOf(idSection);
                                    shape = StbReader.SecBeamS.Shape[idShape];
                                    break;
                                case FrameType.Brace:
                                    idShape = StbReader.SecBraceS.Id.IndexOf(idSection);
                                    shape = StbReader.SecBraceS.Shape[idShape];
                                    break;
                                case FrameType.Slab:
                                    break;
                                case FrameType.Wall:
                                    break;
                                case FrameType.Any:
                                    break;
                            }
                            secIndex = StbReader.StbSecSteel.Name.IndexOf(shape);
                            height = StbReader.StbSecSteel.A[secIndex] / 1000f;
                            width = StbReader.StbSecSteel.B[secIndex] / 1000f;
                            shapeType = StbReader.StbSecSteel.ShapeType[secIndex];
                            break;
                        }
                        case KindsStructure.SRC:
                            break;
                        case KindsStructure.CFT:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    FrameVertex2Mesh(nodeStart, nodeEnd, height, width, shapeType, stbFrame.FrameType, eNum, elements, kind);
                    
                    // 配筋の作成
                    if (kind != KindsStructure.RC)
                        continue;
                    if (shapeType == ShapeTypes.BOX)
                    {
                        switch (stbFrame.FrameType)
                        {
                            case FrameType.Column:
                            case FrameType.Post:
                                CreateBar.Column(secIndex, nodeStart, nodeEnd, width, height, barObj, eNum);
                                break;
                            case FrameType.Girder:
                            case FrameType.Beam:
                                CreateBar.Beam(secIndex, nodeStart, nodeEnd, width, height, barObj, eNum);
                                break;
                            case FrameType.Brace:
                                break;
                            case FrameType.Slab:
                                break;
                            case FrameType.Wall:
                                break;
                            case FrameType.Any:
                                break;
                        }
                    }
                }
            }
        }

        private void FrameVertex2Mesh(Vector3 nodeStart, Vector3 nodeEnd, float height, float width, ShapeTypes shapeType,
                                      FrameType frameType, int eNum, GameObject elements, KindsStructure kind)
        {
            var vertexS = new Vector3[6];
            var vertexE = new Vector3[6];
            var meshObj = new Mesh();

            float dx = nodeEnd.x - nodeStart.x;
            float dy = nodeEnd.y - nodeStart.y;
            float dz = nodeEnd.z - nodeStart.z;
            float angleY = -1f * Mathf.Atan2(dy, dx);
            float angleZ = -1f * Mathf.Atan2(dz, dx);

            // 梁は部材天端の中心が起点に対して、柱・ブレースは部材芯が起点なので場合分け
            switch (frameType)
            {
                case FrameType.Column:
                case FrameType.Post:
                    vertexS = GetColumnVertex(nodeStart, width, height, angleY);
                    vertexE = GetColumnVertex(nodeEnd, width, height, angleY);
                    break;
                case FrameType.Girder:
                case FrameType.Beam:
                    vertexS = GetGirderVertex(nodeStart, width, height, angleZ);
                    vertexE = GetGirderVertex(nodeEnd, width, height, angleZ);
                    break;
                case FrameType.Brace:
                    vertexS = GetBraceVertex(nodeStart, width, height, angleZ);
                    vertexE = GetBraceVertex(nodeEnd, width, height, angleZ);
                    break;
                case FrameType.Slab:
                    break;
                case FrameType.Wall:
                    break;
                case FrameType.Any:
                    break;
            }
            switch (shapeType)
            {
                case ShapeTypes.H:
                    meshObj = CreateMesh.H(vertexS, vertexE); break;
                case ShapeTypes.BOX:
                    meshObj = CreateMesh.Box(vertexS, vertexE); break;
                case ShapeTypes.Pipe:
                    meshObj = CreateMesh.Pipe(nodeStart, nodeEnd, width / 2); break;
                case ShapeTypes.L:
                    meshObj = CreateMesh.L(vertexS, vertexE); break;
                case ShapeTypes.T:
                    break;
                case ShapeTypes.C:
                    break;
                case ShapeTypes.FB:
                    break;
                case ShapeTypes.Bar:
                    break;
            }

            string elemName = string.Format(frameType + "{0}", eNum);
            var element = new GameObject(elemName);
            element.AddComponent<MeshFilter>().mesh = meshObj;
            element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader"))
            {
                color = GetMemberColor(kind, frameType)
            };
            element.transform.parent = elements.transform;
        }

        private static Color GetMemberColor(KindsStructure kind, FrameType frameType)
        {
            var unexpected = new Color(1, 0, 1, 1);

            switch (kind)
            {
                case KindsStructure.RC:
                    switch (frameType) {
                        case FrameType.Column:
                            return ColorInput.MemberColor[0];
                        case FrameType.Post:
                            return ColorInput.MemberColor[1];
                        case FrameType.Girder:
                            return ColorInput.MemberColor[2];
                        case FrameType.Beam:
                            return ColorInput.MemberColor[3];
                        case FrameType.Brace:
                            return ColorInput.MemberColor[4];
                        case FrameType.Slab:
                            return ColorInput.MemberColor[5];
                        case FrameType.Wall:
                            return ColorInput.MemberColor[5];
                        default:
                            return unexpected;
                    }
                case KindsStructure.S:
                    switch (frameType)
                    {
                        case FrameType.Column:
                            return ColorInput.MemberColor[6];
                        case FrameType.Post:
                            return ColorInput.MemberColor[7];
                        case FrameType.Girder:
                            return ColorInput.MemberColor[8];
                        case FrameType.Beam: 
                            return ColorInput.MemberColor[9];
                        case FrameType.Brace:
                            return ColorInput.MemberColor[10];
                        default:
                            return unexpected;
                    }
                default:
                    return unexpected;
            }
        }

        private static Vector3[] GetGirderVertex(Vector3 node, float width, float height, float angle)
        {
            //  Y        3 - 4 - 5 
            //  ^        |   |   |  
            //  o >  X   0 - 1 - 2
            var vertex = new Vector3[6];

            vertex[0] = new Vector3(node.x + width / 2 * (float)Math.Sin(angle),
                                    node.y - height,
                                    node.z + width / 2 * (float)Math.Cos(angle)
                                    );
            vertex[1] = new Vector3(node.x,
                                    node.y - height,
                                    node.z
                                    );
            vertex[2] = new Vector3(node.x - width / 2 * (float)Math.Sin(angle),
                                    node.y - height,
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

        Vector3[] GetColumnVertex(Vector3 node, float width, float height, float angle) 
        {
            //  Y        3 - 4 - 5 
            //  ^        |   |   |  
            //  o >  X   0 - 1 - 2
            Vector3[] vertex = new Vector3[6];

            vertex[0] = new Vector3(node.x - width / 2 * (float)Math.Sin(angle),
                                    node.y - width / 2 * (float)Math.Cos(angle),
                                    node.z - height / 2
                                    );
            vertex[1] = new Vector3(node.x,
                                    node.y,
                                    node.z + height / 2
                                    );
            vertex[2] = new Vector3(node.x + width / 2 * (float)Math.Sin(angle),
                                    node.y + width / 2 * (float)Math.Cos(angle),
                                    node.z - height / 2
                                    );
            vertex[3] = new Vector3(node.x - width / 2 * (float)Math.Sin(angle),
                                    node.y - width / 2 * (float)Math.Cos(angle),
                                    node.z + height / 2
                                    );
            vertex[4] = new Vector3(node.x,
                                    node.y,
                                    node.z - height / 2
                                    );
            vertex[5] = new Vector3(node.x + width / 2 * (float)Math.Sin(angle),
                                    node.y + width / 2 * (float)Math.Cos(angle),
                                    node.z + height / 2
                                    );
            return (vertex);
        }

        private static Vector3[] GetBraceVertex(Vector3 node, float width, float height, float angle)
        {
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
