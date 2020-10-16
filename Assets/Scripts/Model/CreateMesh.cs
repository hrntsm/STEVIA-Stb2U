using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model 
{
    /// <summary>
    /// Create mesh from given data
    /// </summary>
    public class CreateMesh:MonoBehaviour 
    {
        /// <summary>
        /// Create slab mesh
        /// </summary>
        public static Mesh Slab(List<Vector3> stbNodes, int[] nodeIndex)
        {
            var meshObj = new Mesh();
            
            int meshNum = nodeIndex.Length == 4 ? 1 : 0;
            List<int> triangles = GetTriangles(meshNum);

            meshObj.vertices = nodeIndex.Select(t => stbNodes[t]).ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.RecalculateNormals();
            return (meshObj);
        }

        /// <summary>
        /// Create H section mesh
        /// </summary>
        public static Mesh H(Vector3[] vertexS, Vector3[] vertexE)
        {
            var vertices = new List<Vector3>();
            var meshObj = new Mesh();
            const int meshNum = 3;

            AddUpperFlangeVertices(vertices, vertexS, vertexE);
            AddBottomFlangeVertices(vertices, vertexS, vertexE);
            AddCenterWebVertices(vertices, vertexS, vertexE);
            List<int> triangles = GetTriangles(meshNum);

            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.RecalculateNormals();
            return meshObj;
        }

        /// <summary>
        /// Create BOX section mesh
        /// </summary>
        public static Mesh Box(Vector3[] vertexS, Vector3[] vertexE) 
        {
            var vertices = new List<Vector3>();
            var meshObj = new Mesh();
            const int meshNum = 4;

            AddUpperFlangeVertices(vertices, vertexS, vertexE);
            AddBottomFlangeVertices(vertices, vertexS, vertexE);
            AddSide1WebVertices(vertices, vertexS, vertexE);
            AddSide2WebVertices(vertices, vertexS, vertexE);
            List<int> triangles = GetTriangles(meshNum);

            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.RecalculateNormals();
            return (meshObj);
        }

        /// <summary>
        /// Create L section mesh
        /// </summary>
        public static Mesh L(Vector3[] vertexS, Vector3[] vertexE)
        {
            var vertices = new List<Vector3>();
            var meshObj = new Mesh();
            const int meshNum = 2;

            AddBottomFlangeVertices(vertices, vertexS, vertexE);
            AddSide2WebVertices(vertices, vertexS, vertexE);
            List<int> triangles = GetTriangles(meshNum);

            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.RecalculateNormals();
            return (meshObj);
        }

        /// <summary>
        /// Create L section mesh
        /// </summary>
        public static Mesh Pipe(Vector3 startPoint, Vector3 endPoint, float radius, int divNum = 24, bool isCap = false)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var meshObj = new Mesh();
            float dx = endPoint.x - startPoint.x;
            float dy = endPoint.y - startPoint.y;
            float dz = endPoint.z - startPoint.z;
            float angleX = -1f * Mathf.Atan2(dx, dy);
            float angleZ = -1f * Mathf.Atan2(dz, dy);

            var i = 0;
            float baseAngle = 2f * Mathf.PI / divNum;
            float startX, startY, startZ;
            float endX, endY, endZ;

            while (i < divNum)
            {
                startX = startPoint.x + radius * Mathf.Cos(baseAngle * i) * Mathf.Cos(angleX);
                startY = startPoint.y + radius * (Mathf.Cos(baseAngle * i) * Mathf.Sin(angleX) + Mathf.Sin(baseAngle * i) * Mathf.Sin(angleZ));
                startZ = startPoint.z + radius * Mathf.Sin(baseAngle * i) * Mathf.Cos(angleZ);

                endX = endPoint.x + radius * Mathf.Cos(baseAngle * i) * Mathf.Cos(angleX);
                endY = endPoint.y + radius * (Mathf.Cos(baseAngle * i) * Mathf.Sin(angleX) + Mathf.Sin(baseAngle * i) * Mathf.Sin(angleZ));
                endZ = endPoint.z + radius * Mathf.Sin(baseAngle * i) * Mathf.Cos(angleZ);

                vertices.Add(new Vector3(startX, startY, startZ));
                vertices.Add(new Vector3(endX, endY, endZ));

                if (i != divNum - 1) 
                {
                    triangles.Add(2 * i);
                    triangles.Add(2 * i + 1);
                    triangles.Add(2 * i + 2);
                    triangles.Add(2 * i + 2);
                    triangles.Add(2 * i + 1);
                    triangles.Add(2 * i + 3);
                }
                else 
                {
                    triangles.Add(2 * i);
                    triangles.Add(2 * i + 1);
                    triangles.Add(0);
                    triangles.Add(0);
                    triangles.Add(2 * i + 1);
                    triangles.Add(1);
                }
                i++;
            }

            if (isCap)
            {
                i = 0;
                while (i < divNum) 
                {
                    startX = startPoint.x + radius * Mathf.Cos(baseAngle * i) * Mathf.Cos(angleX);
                    startY = startPoint.y + radius * (Mathf.Cos(baseAngle * i) * Mathf.Sin(angleX) + Mathf.Sin(baseAngle * i) * Mathf.Sin(angleZ));
                    startZ = startPoint.z + radius * Mathf.Sin(baseAngle * i) * Mathf.Cos(angleZ);

                    endX = endPoint.x + radius * Mathf.Cos(baseAngle * i) * Mathf.Cos(angleX);
                    endY = endPoint.y + radius * (Mathf.Cos(baseAngle * i) * Mathf.Sin(angleX) + Mathf.Sin(baseAngle * i) * Mathf.Sin(angleZ));
                    endZ = endPoint.z + radius * Mathf.Sin(baseAngle * i) * Mathf.Cos(angleZ);

                    vertices.Add(new Vector3(startX, startY, startZ));
                    vertices.Add(new Vector3(endX, endY, endZ));
                    i++;
                }
                vertices.Add(startPoint);
                vertices.Add(endPoint);

                i = 0;
                while (i < divNum - 1)
                {
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
        /// Get triangles vertex information
        /// </summary>
        private static List<int> GetTriangles(int meshNum)
        {
            var triangles = new List<int>();
            if (meshNum == 0)
            {
                triangles.Add(0);
                triangles.Add(1);
                triangles.Add(2);
            }
            else 
            {
                for (var i = 1; i <= meshNum; ++i) 
                {
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

        private static void AddUpperFlangeVertices(ICollection<Vector3> vertices, IReadOnlyList<Vector3> vertexS, IReadOnlyList<Vector3> vertexE) 
        {
            vertices.Add(vertexS[3]);
            vertices.Add(vertexS[5]);
            vertices.Add(vertexE[5]);
            vertices.Add(vertexE[3]);
        }

        private static void AddBottomFlangeVertices(ICollection<Vector3> vertices, IReadOnlyList<Vector3> vertexS, IReadOnlyList<Vector3> vertexE)
        {
            vertices.Add(vertexS[0]);
            vertices.Add(vertexS[2]);
            vertices.Add(vertexE[2]);
            vertices.Add(vertexE[0]);
        }

        private static void AddCenterWebVertices(ICollection<Vector3> vertices, IReadOnlyList<Vector3> vertexS, IReadOnlyList<Vector3> vertexE) 
        {
            vertices.Add(vertexS[4]);
            vertices.Add(vertexS[1]);
            vertices.Add(vertexE[1]);
            vertices.Add(vertexE[4]);
        }

        private static void AddSide1WebVertices(ICollection<Vector3> vertices, IReadOnlyList<Vector3> vertexS, IReadOnlyList<Vector3> vertexE) 
        {
            vertices.Add(vertexS[3]);
            vertices.Add(vertexS[0]);
            vertices.Add(vertexE[0]);
            vertices.Add(vertexE[3]);
        }

        private static void AddSide2WebVertices(ICollection<Vector3> vertices, IReadOnlyList<Vector3> vertexS, IReadOnlyList<Vector3> vertexE)
        {
            vertices.Add(vertexS[5]);
            vertices.Add(vertexS[2]);
            vertices.Add(vertexE[2]);
            vertices.Add(vertexE[5]);
        }

        public static void Combine(GameObject parent, Color meshColor, string shaderName) 
        {
            MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
            var combine = new CombineInstance[meshFilters.Length];
            var count = 0;
            while (count < meshFilters.Length) 
            {
                combine[count].mesh = meshFilters[count].sharedMesh;
                combine[count].transform = meshFilters[count].transform.localToWorldMatrix;
                meshFilters[count].gameObject.SetActive(false);
                count++;
            }
            parent.AddComponent<MeshFilter>().mesh.CombineMeshes(combine);
            parent.AddComponent<MeshRenderer>().material = new Material(Shader.Find(shaderName))
            {
                color = meshColor
            };
            parent.transform.gameObject.SetActive(true);
        }
    }
}
