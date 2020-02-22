using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBar : MonoBehaviour {

    public static void Column(int index, Vector3 nodeStart, Vector3 nodeEnd, float width, float hight) {
        // かぶり、鉄筋径はとりあえずで設定
        float kaburi = 50 / 1000f;
        float bandD = 10 / 1000f;
        float mainD = 25 / 1000f;
        float barSpace = Mathf.Max(1.5f * mainD, 25 / 1000f); // 鉄筋のあき
        float bandSpace = 2 * kaburi + bandD;
        float main1Space = bandSpace + bandD + mainD;
        float main2Space = main1Space + 2 * (mainD + barSpace);

        Vector3[,] hoopPos = GetCornerPoint(nodeStart, nodeEnd, width - bandSpace, hight - bandSpace);
        Vector3[,] main1Pos = GetCornerPoint(nodeStart, nodeEnd, width - main1Space, hight - main1Space);
        Vector3[,] mainX2Pos = GetCornerPoint(nodeStart, nodeEnd, width - main1Space, hight - main2Space);
        Vector3[,] mainY2Pos = GetCornerPoint(nodeStart, nodeEnd, width - main2Space, hight - main1Space);

        MakeHoop(hoopPos, bandD, index);
        MakeColumnMainBar(main1Pos, mainX2Pos, mainY2Pos, barSpace, mainD, index);
    }

    public static void Beam(int index, Vector3 nodeStart, Vector3 nodeEnd, float width, float hight) {
        // かぶり、鉄筋径はとりあえずで設定
        float kaburi = 50 / 1000f;
        float bandD = 10 / 1000f;
        float mainD = 25 / 1000f;
        float barSpace = Mathf.Max(1.5f * mainD, 25 / 1000f); // 鉄筋のあき
        float bandSpace = 2 * kaburi + bandD;
        float main1Space = bandSpace + bandD + mainD;
        float main2Space = main1Space + 2 * (mainD + barSpace);
        float main3Space = main2Space + 2 * (mainD + barSpace);

        Vector3[,] strupPos = GetCornerPoint(nodeStart, nodeEnd, width - bandSpace, hight - bandSpace);
        Vector3[,] main1Pos = GetCornerPoint(nodeStart, nodeEnd, width - main1Space, hight - main1Space);
        Vector3[,] main2Pos = GetCornerPoint(nodeStart, nodeEnd, width - main1Space, hight - main2Space);
        Vector3[,] main3Pos = GetCornerPoint(nodeStart, nodeEnd, width - main1Space, hight - main3Space);
        MakeStrup(strupPos, bandD, index);
        //MakeBeamMainBar(main1Pos, main2Pos, main3Pos, barSpace, mainD, index);
    }

    static int[] GetMainBarInfo(int index) {
        int[] mainBar = new int[5];

        for (int i = 0; i < 5; i++) {
            mainBar[i] = STBReader.m_xRcColumnBar[index][i];
        }
        return (mainBar);
    }

    static Vector3[,] GetCornerPoint(Vector3 nodeStart, Vector3 nodeEnd, float width, float hight) {
        //  Z        4 - 3
        //  ^        | 0 |
        //  o >  X   1 - 2
        Vector3[,] cornerPoint = new Vector3[2, 5];
        Vector3 node = nodeStart;
        float dx = nodeEnd.x - nodeStart.x;
        float dy = nodeEnd.y - nodeStart.y;
        float dz = nodeEnd.z - nodeStart.z;
        float angleX = -1f * Mathf.Atan2(dx, dy) * Mathf.Rad2Deg;
        float angleZ = -1f * Mathf.Atan2(dz, dy) * Mathf.Rad2Deg;

        for (int i = 0; i < 2; i++) {
            cornerPoint[i, 0] = node;
            cornerPoint[i, 1] = new Vector3(node.x - width / 2f * Mathf.Cos(angleX),
                                            node.y - width / 2f * Mathf.Sin(angleX) - hight / 2f * Mathf.Sin(angleZ),
                                            node.z - hight / 2f * Mathf.Cos(angleZ)
                                            );
            cornerPoint[i, 2] = new Vector3(node.x + width / 2f * Mathf.Cos(angleX),
                                            node.y + width / 2f * Mathf.Sin(angleX) + hight / 2f * Mathf.Sin(angleZ),
                                            node.z - hight / 2f * Mathf.Cos(angleZ)
                                            );
            cornerPoint[i, 3] = new Vector3(node.x + width / 2f * Mathf.Cos(angleX),
                                            node.y + width / 2f * Mathf.Sin(angleX) + hight / 2f * Mathf.Sin(angleZ),
                                            node.z + hight / 2f * Mathf.Cos(angleZ)
                                            );
            cornerPoint[i, 4] = new Vector3(node.x - width / 2f * Mathf.Cos(angleX),
                                            node.y - width / 2f * Mathf.Sin(angleX) - hight / 2f * Mathf.Sin(angleZ),
                                            node.z + hight / 2f * Mathf.Cos(angleZ)
                                            );
            node = nodeEnd;
        }
        return (cornerPoint);
    }

    static Vector3[,] GetBandPos(Vector3[,] cornerPos, int dirXNum, int dirYNum) {
        Vector3[,] hoopPos = new Vector3[2, 2 * (dirXNum + dirYNum)];
        // dir_X
        for (int i = 0; i < dirXNum; i++) {
            for (int j = 0; j < 2; j++) {
                if (i == 0) {
                    hoopPos[j, 2 * i] = cornerPos[j, 1];
                    hoopPos[j, 2 * i + 1] = cornerPos[j, 2];
                }
                else if (i == dirXNum - 1) {
                    hoopPos[j, 2 * i] = cornerPos[j, 4];
                    hoopPos[j, 2 * i + 1] = cornerPos[j, 3];
                }
                else {
                    hoopPos[j, 2 * i] = Vector3.Lerp(cornerPos[j, 1], cornerPos[j, 4], 1f / (dirXNum - 1) * i);
                    hoopPos[j, 2 * i + 1] = Vector3.Lerp(cornerPos[j, 2], cornerPos[j, 3], 1f / (dirXNum - 1) * i);
                }
            }
        }
        // dir_Y
        for (int i = dirXNum; i < dirXNum + dirYNum; i++) {
            for (int j = 0; j < 2; j++) {
                if (i == 0) {
                    hoopPos[j, 2 * i] = cornerPos[j, 1];
                    hoopPos[j, 2 * i + 1] = cornerPos[j, 4];
                }
                else if (i == dirXNum + dirYNum - 1) {
                    hoopPos[j, 2 * i] = cornerPos[j, 2];
                    hoopPos[j, 2 * i + 1] = cornerPos[j, 3];
                }
                else {
                    hoopPos[j, 2 * i] = Vector3.Lerp(cornerPos[j, 1], cornerPos[j, 2], 1f / (dirYNum - 1) * (i - dirXNum));
                    hoopPos[j, 2 * i + 1] = Vector3.Lerp(cornerPos[j, 4], cornerPos[j, 3], 1f / (dirYNum - 1) * (i - dirXNum));
                }
            }
        }
        return (hoopPos);
    }

    static void MakeHoop(Vector3[,] cornerPos, float bandD, int index) {
        float pitch = STBReader.m_xRcColumnBar[index][5] / 1000f;
        int dirXNum = STBReader.m_xRcColumnBar[index][6];
        int dirYNum = STBReader.m_xRcColumnBar[index][7];
        int sumBar = dirXNum + dirYNum;
        float distance = Vector3.Distance(cornerPos[0, 0], cornerPos[1, 0]);
        List<Vector3> vertex = new List<Vector3>();
        int i = 0;

        Vector3[,] hoopPos = GetBandPos(cornerPos, dirXNum, dirYNum);

        while ((pitch * i) / distance < 1) {
            for (int j = 0; j < 2 * sumBar; j++) {
                vertex.Add(Vector3.Lerp(hoopPos[0, j], hoopPos[1, j], (float)(pitch * i) / distance));
            }
            for (int j = 0; j < sumBar; j++) {
                Mesh meshObj = CreateMesh.Pipe(vertex[2 * j + (i * 2 * sumBar)], vertex[2 * j + 1 + (i * 2 * sumBar)], bandD / 2f, 12, true);
                GameObject element = new GameObject("hoop");
                element.AddComponent<MeshFilter>().mesh = meshObj;
                element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader")) {
                    color = new Color(1, 0, 1, 1)
                };
            }
            i++;
        }
    }

    static void MakeStrup(Vector3[,] cornerPos, float bandD, int index) {
        float pitch = STBReader.m_xRcBeamBar[index][6] / 1000f;
        int strupNum = STBReader.m_xRcBeamBar[index][7];
        int sumBar = strupNum + 2;
        float distance = Vector3.Distance(cornerPos[0, 0], cornerPos[1, 0]);
        List<Vector3> vertex = new List<Vector3>();
        int i = 0;

        Vector3[,] strupPos = GetBandPos(cornerPos, 2, strupNum);

        while ((pitch * i) / distance < 1) {
            for (int j = 0; j < 2 * sumBar; j++) {
                vertex.Add(Vector3.Lerp(strupPos[0, j], strupPos[1, j], (float)(pitch * i) / distance));
            }
            for (int j = 0; j < strupNum; j++) {
                Mesh meshObj = CreateMesh.Pipe(vertex[2 * j + (i * 2 * sumBar)], vertex[2 * j + 1 + (i * 2 * sumBar)], bandD / 2f, 12, true);
                GameObject element = new GameObject("Strup");
                element.AddComponent<MeshFilter>().mesh = meshObj;
                element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader")) {
                    color = new Color(1, 0, 1, 1)
                };
            }
            i++;
        }
    }

    static void MakeColumnMainBar(Vector3[,] mainPos, Vector3[,] mainX2Pos, Vector3[,] mainY2Pos, float barSpace, float mainD, int index) {
        int[] mainBarNum = GetMainBarInfo(index);
        bool[] hasMain2 = { false, false }; // {Main2_X, Main2_Y}
        if (mainBarNum[2] > 1)
            hasMain2[0] = true;
        if (mainBarNum[3] > 1)
            hasMain2[1] = true;

        for (int i = 1; i < 5; i++) {
            // コーナーの主筋
            Mesh meshObj = CreateMesh.Pipe(mainPos[0, i], mainPos[1, i], mainD / 2f, 12, true);
            GameObject element = new GameObject("main");
            element.AddComponent<MeshFilter>().mesh = meshObj;
            element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader")) {
                color = new Color(1, 1, 0, 1)
            };
        }

        float posX1Ratio = 1f / (mainBarNum[0] - 1);
        float posY1Ratio = 1f / (mainBarNum[1] - 1);
        float posX2Ratio = 1f / (mainBarNum[2] - 1);
        float posY2Ratio = 1f / (mainBarNum[3] - 1);
        float distanceX = Vector3.Distance(mainPos[0, 1], mainPos[0, 2]);
        float distanceY = Vector3.Distance(mainPos[0, 2], mainPos[0, 3]);
        int barCount = 0;
        List<Vector3> vertex = new List<Vector3>();

        if (hasMain2[1]) {
            // 寄せ筋の作成
            vertex.Add(Vector3.Lerp(mainPos[0, 1], mainPos[0, 2], (barSpace + mainD) / distanceX));
            vertex.Add(Vector3.Lerp(mainPos[1, 1], mainPos[1, 2], (barSpace + mainD) / distanceX));
            vertex.Add(Vector3.Lerp(mainPos[0, 3], mainPos[0, 4], (barSpace + mainD) / distanceX));
            vertex.Add(Vector3.Lerp(mainPos[1, 3], mainPos[1, 4], (barSpace + mainD) / distanceX));
            vertex.Add(Vector3.Lerp(mainPos[0, 1], mainPos[0, 2], 1f - (barSpace + mainD) / distanceX));
            vertex.Add(Vector3.Lerp(mainPos[1, 1], mainPos[1, 2], 1f - (barSpace + mainD) / distanceX));
            vertex.Add(Vector3.Lerp(mainPos[0, 3], mainPos[0, 4], 1f - (barSpace + mainD) / distanceX));
            vertex.Add(Vector3.Lerp(mainPos[1, 3], mainPos[1, 4], 1f - (barSpace + mainD) / distanceX));
            barCount += 4;
            // 1st_X
            for (int j = 2; j <= mainBarNum[0] - 3; j++) {
                vertex.Add(Vector3.Lerp(mainPos[0, 1], mainPos[0, 2], posX1Ratio * j));
                vertex.Add(Vector3.Lerp(mainPos[1, 1], mainPos[1, 2], posX1Ratio * j));
                vertex.Add(Vector3.Lerp(mainPos[0, 3], mainPos[0, 4], posX1Ratio * j));
                vertex.Add(Vector3.Lerp(mainPos[1, 3], mainPos[1, 4], posX1Ratio * j));
                barCount += 2;
            }
            // 2nd_X
            for (int j = 1; j <= mainBarNum[2] - 2; j++) {
                vertex.Add(Vector3.Lerp(mainX2Pos[0, 1], mainX2Pos[0, 2], posX2Ratio * j));
                vertex.Add(Vector3.Lerp(mainX2Pos[1, 1], mainX2Pos[1, 2], posX2Ratio * j));
                vertex.Add(Vector3.Lerp(mainX2Pos[0, 3], mainX2Pos[0, 4], posX2Ratio * j));
                vertex.Add(Vector3.Lerp(mainX2Pos[1, 3], mainX2Pos[1, 4], posX2Ratio * j));
                barCount += 2;
            }
        }
        else {
            for (int j = 1; j <= mainBarNum[0] - 2; j++) {
                vertex.Add(Vector3.Lerp(mainPos[0, 1], mainPos[0, 2], posX1Ratio * j));
                vertex.Add(Vector3.Lerp(mainPos[1, 1], mainPos[1, 2], posX1Ratio * j));
                vertex.Add(Vector3.Lerp(mainPos[0, 3], mainPos[0, 4], posX1Ratio * j));
                vertex.Add(Vector3.Lerp(mainPos[1, 3], mainPos[1, 4], posX1Ratio * j));
                barCount += 2;
            }
        }
        if (hasMain2[0]) {
            // 寄せ筋の作成
            vertex.Add(Vector3.Lerp(mainPos[0, 2], mainPos[0, 3], (barSpace + mainD) / distanceY));
            vertex.Add(Vector3.Lerp(mainPos[1, 2], mainPos[1, 3], (barSpace + mainD) / distanceY));
            vertex.Add(Vector3.Lerp(mainPos[0, 4], mainPos[0, 1], (barSpace + mainD) / distanceY));
            vertex.Add(Vector3.Lerp(mainPos[1, 4], mainPos[1, 1], (barSpace + mainD) / distanceY));
            vertex.Add(Vector3.Lerp(mainPos[0, 2], mainPos[0, 3], 1f - (barSpace + mainD) / distanceY));
            vertex.Add(Vector3.Lerp(mainPos[1, 2], mainPos[1, 3], 1f - (barSpace + mainD) / distanceY));
            vertex.Add(Vector3.Lerp(mainPos[0, 4], mainPos[0, 1], 1f - (barSpace + mainD) / distanceY));
            vertex.Add(Vector3.Lerp(mainPos[1, 4], mainPos[1, 1], 1f - (barSpace + mainD) / distanceY));
            barCount += 4;
            // 1st_Y
            for (int j = 2; j <= mainBarNum[0] - 3; j++) {
                vertex.Add(Vector3.Lerp(mainPos[0, 2], mainPos[0, 3], posY1Ratio * j));
                vertex.Add(Vector3.Lerp(mainPos[1, 2], mainPos[1, 3], posY1Ratio * j));
                vertex.Add(Vector3.Lerp(mainPos[0, 4], mainPos[0, 1], posY1Ratio * j));
                vertex.Add(Vector3.Lerp(mainPos[1, 4], mainPos[1, 1], posY1Ratio * j));
                barCount += 2;
            }
            // 2nd_Y
            for (int j = 1; j <= mainBarNum[3] - 2; j++) {
                vertex.Add(Vector3.Lerp(mainY2Pos[0, 2], mainY2Pos[0, 3], posY2Ratio * j));
                vertex.Add(Vector3.Lerp(mainY2Pos[1, 2], mainY2Pos[1, 3], posY2Ratio * j));
                vertex.Add(Vector3.Lerp(mainY2Pos[0, 4], mainY2Pos[0, 1], posY2Ratio * j));
                vertex.Add(Vector3.Lerp(mainY2Pos[1, 4], mainY2Pos[1, 1], posY2Ratio * j));
                barCount += 2;
            }
        }
        else {
            for (int j = 1; j <= mainBarNum[1] - 2; j++) {
                vertex.Add(Vector3.Lerp(mainPos[0, 2], mainPos[0, 3], posY1Ratio * j));
                vertex.Add(Vector3.Lerp(mainPos[1, 2], mainPos[1, 3], posY1Ratio * j));
                vertex.Add(Vector3.Lerp(mainPos[0, 4], mainPos[0, 1], posY1Ratio * j));
                vertex.Add(Vector3.Lerp(mainPos[1, 4], mainPos[1, 1], posY1Ratio * j));
                barCount += 2;
            }
        }
        for (int i = 0; i < barCount; i++) {
            Mesh meshObj = CreateMesh.Pipe(vertex[2 * i], vertex[2 * i + 1], mainD / 2f, 12, true);
            GameObject element = new GameObject("main");
            element.AddComponent<MeshFilter>().mesh = meshObj;
            element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/CulloffSurfaceShader")) {
                color = new Color(1, 1, 0, 1)
            };
        }
    }
}
