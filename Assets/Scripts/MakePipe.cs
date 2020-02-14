using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakePipe : MonoBehaviour {
    public int divNum;
    public float radious;
    public Vector3 startPoint, endPoint;
    public bool isCap;
    
    void Start() {
        GameObject gameObject = CreatePipeObject(divNum, radious, startPoint, endPoint, isCap);
        gameObject.name = "aaa";
    }

    public GameObject CreatePipeObject(int divNum, float radious, Vector3 startPoint, Vector3 endPoint, bool isCap = false) {
        int i = 0;
        float baseAngle = 2 * Mathf.PI / divNum;
        float startX, startY, startZ;
        float endX, endY, endZ;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        Mesh meshObj = new Mesh();
        GameObject Pipe = new GameObject();

        while (i < divNum) {
            startX = startPoint.x + radious * Mathf.Cos(baseAngle * i);
            startY = startPoint.y;
            startZ = startPoint.z + radious * Mathf.Sin(baseAngle * i);

            endX = endPoint.x + radious * Mathf.Cos(baseAngle * i);
            endY = endPoint.y;
            endZ = endPoint.z + radious * Mathf.Sin(baseAngle * i);

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
                startX = startPoint.x + radious * Mathf.Cos(baseAngle * i);
                startY = startPoint.y;
                startZ = startPoint.z + radious * Mathf.Sin(baseAngle * i);

                endX = endPoint.x + radious * Mathf.Cos(baseAngle * i);
                endY = endPoint.y;
                endZ = endPoint.z + radious * Mathf.Sin(baseAngle * i);

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
        
        Pipe.AddComponent<MeshFilter>().mesh = meshObj;
        Pipe.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) {
            color = new Color(1, 1, 1, 1)
        };
        return (Pipe);
    }
}
