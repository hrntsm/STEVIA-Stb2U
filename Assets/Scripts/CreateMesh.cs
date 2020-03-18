using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stevia {
    /// <summary>
    /// Create mesh from given data
    /// </summary>
    public class CreateMesh:MonoBehaviour {
        /// <summary>
        /// Create slab mesh
        /// </summary>
        /// <param name="stbNodes"></param>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        public static Mesh Slab(List<Vector3> stbNodes, int[] nodeIndex) {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            Mesh meshObj = new Mesh();
            int meshNum;

            for (int i = 0; i < nodeIndex.Length; i++) {
                vertices.Add(stbNodes[nodeIndex[i]]);
            }
            if (nodeIndex.Length == 4)
                meshNum = 1;
            else
                meshNum = 0;
            triangles = GetTriangles(meshNum);

            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.RecalculateNormals();
            return (meshObj);
        }

        /// <summary>
        /// Create H section mesh
        /// </summary>
        /// <param name="vertexS"></param>
        /// <param name="vertexE"></param>
        /// <returns></returns>
        public static Mesh H(Vector3[] vertexS, Vector3[] vertexE) {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            Mesh meshObj = new Mesh();
            int meshNum = 3;

            AddUpperFlangeVertices(vertices, vertexS, vertexE);
            AddBottomFlangeVertices(vertices, vertexS, vertexE);
            AddCenterWebVertices(vertices, vertexS, vertexE);
            triangles = GetTriangles(meshNum);

            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.RecalculateNormals();
            return (meshObj);
        }

        /// <summary>
        /// Create BOX section mesh
        /// </summary>
        /// <param name="vertexS"></param>
        /// <param name="vertexE"></param>
        /// <returns></returns>
        public static Mesh BOX(Vector3[] vertexS, Vector3[] vertexE) {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            Mesh meshObj = new Mesh();
            int MeshNum = 4;

            AddUpperFlangeVertices(vertices, vertexS, vertexE);
            AddBottomFlangeVertices(vertices, vertexS, vertexE);
            AddSide1WebVertices(vertices, vertexS, vertexE);
            AddSide2WebVertices(vertices, vertexS, vertexE);
            triangles = GetTriangles(MeshNum);

            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.RecalculateNormals();
            return (meshObj);
        }

        /// <summary>
        /// Create L section mesh
        /// </summary>
        /// <param name="vertexS"></param>
        /// <param name="vertexE"></param>
        /// <returns></returns>
        public static Mesh L(Vector3[] vertexS, Vector3[] vertexE) {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            Mesh meshObj = new Mesh();
            int MeshNum = 2;

            AddBottomFlangeVertices(vertices, vertexS, vertexE);
            AddSide2WebVertices(vertices, vertexS, vertexE);
            triangles = GetTriangles(MeshNum);

            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.RecalculateNormals();
            return (meshObj);
        }

        /// <summary>
        /// Create L section mesh
        /// </summary>
        /// <param name="startPoint">Start point</param>
        /// <param name="endPoint">End point</param>
        /// <param name="radious">Radious</param>
        /// <param name="divNum">Divide count of circle</param>
        /// <param name="isCap">Boolean of with or without cap</param>
        /// <returns></returns>
        public static Mesh Pipe(Vector3 startPoint, Vector3 endPoint, float radious, int divNum = 24, bool isCap = false) {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            Mesh meshObj = new Mesh();
            float dx = endPoint.x - startPoint.x;
            float dy = endPoint.y - startPoint.y;
            float dz = endPoint.z - startPoint.z;
            float angleX = -1f * Mathf.Atan2(dx, dy);
            float angleZ = -1f * Mathf.Atan2(dz, dy);

            int i = 0;
            float baseAngle = 2f * Mathf.PI / divNum;
            float startX, startY, startZ;
            float endX, endY, endZ;

            while (i < divNum) {
                startX = startPoint.x + radious * Mathf.Cos(baseAngle * i) * Mathf.Cos(angleX);
                startY = startPoint.y + radious * (Mathf.Cos(baseAngle * i) * Mathf.Sin(angleX) + Mathf.Sin(baseAngle * i) * Mathf.Sin(angleZ));
                startZ = startPoint.z + radious * Mathf.Sin(baseAngle * i) * Mathf.Cos(angleZ);

                endX = endPoint.x + radious * Mathf.Cos(baseAngle * i) * Mathf.Cos(angleX);
                endY = endPoint.y + radious * (Mathf.Cos(baseAngle * i) * Mathf.Sin(angleX) + Mathf.Sin(baseAngle * i) * Mathf.Sin(angleZ));
                endZ = endPoint.z + radious * Mathf.Sin(baseAngle * i) * Mathf.Cos(angleZ);

                vertices.Add(new Vector3(startX, startY, startZ));
                vertices.Add(new Vector3(endX, endY, endZ));

                if (i != divNum - 1) {
                    triangles.Add(2 * i);
                    triangles.Add(2 * i + 1);
                    triangles.Add(2 * i + 2);
                    triangles.Add(2 * i + 2);
                    triangles.Add(2 * i + 1);
                    triangles.Add(2 * i + 3);
                }
                else {
                    triangles.Add(2 * i);
                    triangles.Add(2 * i + 1);
                    triangles.Add(0);
                    triangles.Add(0);
                    triangles.Add(2 * i + 1);
                    triangles.Add(1);
                }
                i++;
            }

            if (isCap) {
                i = 0;
                while (i < divNum) {
                    startX = startPoint.x + radious * Mathf.Cos(baseAngle * i) * Mathf.Cos(angleX);
                    startY = startPoint.y + radious * (Mathf.Cos(baseAngle * i) * Mathf.Sin(angleX) + Mathf.Sin(baseAngle * i) * Mathf.Sin(angleZ));
                    startZ = startPoint.z + radious * Mathf.Sin(baseAngle * i) * Mathf.Cos(angleZ);

                    endX = endPoint.x + radious * Mathf.Cos(baseAngle * i) * Mathf.Cos(angleX);
                    endY = endPoint.y + radious * (Mathf.Cos(baseAngle * i) * Mathf.Sin(angleX) + Mathf.Sin(baseAngle * i) * Mathf.Sin(angleZ));
                    endZ = endPoint.z + radious * Mathf.Sin(baseAngle * i) * Mathf.Cos(angleZ);

                    vertices.Add(new Vector3(startX, startY, startZ));
                    vertices.Add(new Vector3(endX, endY, endZ));
                    i++;
                }
                vertices.Add(startPoint);
                vertices.Add(endPoint);

                i = 0;
                while (i < divNum - 1) {
                    triangles.Add(2 * divNum + 2 * divNum);
                    triangles.Add(2 * divNum + 2 * i);
                    triangles.Add(2 * divNum + 2 * i + 2);
                    triangles.Add(2 * divNum + 2 * divNum + 1);
                    triangles.Add(2 * divNum + 2 * i + 3);
                    triangles.Add(2 * divNum + 2 * i + 1);
                    i++;
                }
                triangles.Add(2 * divNum + 2 * divNum);
                triangles.Add(2 * divNum + 2 * i);
                triangles.Add(2 * divNum);
                triangles.Add(2 * divNum + 2 * divNum + 1);
                triangles.Add(2 * divNum + 1);
                triangles.Add(2 * divNum + 2 * i + 1);
            }

            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.RecalculateNormals();
            return (meshObj);
        }

        /// <summary>
        /// Get triangles vertex infomation
        /// </summary>
        static List<int> GetTriangles(int meshNum) {
            List<int> triangles = new List<int>();
            if (meshNum == 0) {
                triangles.Add(0);
                triangles.Add(1);
                triangles.Add(2);
            }
            else {
                for (int i = 1; i <= meshNum; ++i) {
                    triangles.Add(4 * i - 4);
                    triangles.Add(4 * i - 3);
                    triangles.Add(4 * i - 2);
                    triangles.Add(4 * i - 2);
                    triangles.Add(4 * i - 1);
                    triangles.Add(4 * i - 4);
                }
            }
            return (triangles);
        }
        static List<Vector3> AddUpperFlangeVertices(List<Vector3> vertices, Vector3[] vertexS, Vector3[] vertexE) {
            vertices.Add(vertexS[3]);
            vertices.Add(vertexS[5]);
            vertices.Add(vertexE[5]);
            vertices.Add(vertexE[3]);
            return (vertices);
        }
        static List<Vector3> AddBottomFlangeVertices(List<Vector3> vertices, Vector3[] vertexS, Vector3[] vertexE) {
            vertices.Add(vertexS[0]);
            vertices.Add(vertexS[2]);
            vertices.Add(vertexE[2]);
            vertices.Add(vertexE[0]);
            return (vertices);
        }
        static List<Vector3> AddCenterWebVertices(List<Vector3> vertices, Vector3[] vertexS, Vector3[] vertexE) {
            vertices.Add(vertexS[4]);
            vertices.Add(vertexS[1]);
            vertices.Add(vertexE[1]);
            vertices.Add(vertexE[4]);
            return (vertices);
        }
        static List<Vector3> AddSide1WebVertices(List<Vector3> vertices, Vector3[] vertexS, Vector3[] vertexE) {
            vertices.Add(vertexS[3]);
            vertices.Add(vertexS[0]);
            vertices.Add(vertexE[0]);
            vertices.Add(vertexE[3]);
            return (vertices);
        }
        static List<Vector3> AddSide2WebVertices(List<Vector3> vertices, Vector3[] vertexS, Vector3[] vertexE) {
            vertices.Add(vertexS[5]);
            vertices.Add(vertexS[2]);
            vertices.Add(vertexE[2]);
            vertices.Add(vertexE[5]);
            return (vertices);
        }

        public static void Conbine(GameObject parent, Color meshColor, string shaderName) {
            MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            int count = 0;
            while (count < meshFilters.Length) {
                combine[count].mesh = meshFilters[count].sharedMesh;
                combine[count].transform = meshFilters[count].transform.localToWorldMatrix;
                meshFilters[count].gameObject.SetActive(false);
                count++;
            }
            parent.AddComponent<MeshFilter>().mesh.CombineMeshes(combine);
            parent.AddComponent<MeshRenderer>().material = new Material(Shader.Find(shaderName)) {
                color = meshColor
            };
            parent.transform.gameObject.SetActive(true);
        }
    }
}
