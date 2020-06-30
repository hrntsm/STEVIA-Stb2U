using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stevia.Model
{
    public class CreateBar:MonoBehaviour
    {
        static int[] GetColMainNum(int index)
        {
            int[] mainBar = new int[5];

            for (int i = 0; i < 5; i++)
                mainBar[i] = STBReader._secColumnRC.BarList[index][i];
            return (mainBar);
        }

        static int[] GetBeamMainNum(int index)
        {
            int[] mainBar = new int[5];

            for (int i = 0; i < 5; i++)
                mainBar[i] = STBReader._secBeamRC.BarList[index][i];
            return (mainBar);
        }


        public static void Column(int index, Vector3 nodeStart, Vector3 nodeEnd, float width, float hight, GameObject parent, int elemNum)
        {
            // かぶり、鉄筋径はとりあえずで設定
            float kaburi = 50 / 1000f;
            float bandD = 10 / 1000f;
            float mainD = 25 / 1000f;
            float barSpace = Mathf.Max(1.5f * mainD, 25 / 1000f); // 鉄筋のあき
            float bandSpace = 2 * kaburi + bandD;
            float main1Space = bandSpace + bandD + mainD;
            float main2Space = main1Space + 2 * (mainD + barSpace);

            Vector3[,] hoopPos = GetColumnCorner(nodeStart, nodeEnd, width - bandSpace, hight - bandSpace);
            Vector3[,] main1Pos = GetColumnCorner(nodeStart, nodeEnd, width - main1Space, hight - main1Space);
            Vector3[,] mainX2Pos = GetColumnCorner(nodeStart, nodeEnd, width - main1Space, hight - main2Space);
            Vector3[,] mainY2Pos = GetColumnCorner(nodeStart, nodeEnd, width - main2Space, hight - main1Space);

            string name = string.Format("Bar" + "{0}", elemNum);
            GameObject barObj = new GameObject(name);
            barObj.transform.parent = parent.transform;

            CreateBar.Hoop(hoopPos, bandD, index, barObj);
            CreateBar.ColumnMainBar(main1Pos, mainX2Pos, mainY2Pos, barSpace, mainD, index, barObj);
        }

        static Vector3[,] GetColumnCorner(Vector3 nodeStart, Vector3 nodeEnd, float width, float hight)
        {
            //  Z        4 - 3
            //  ^        | 0 |
            //  o >  X   1 - 2
            Vector3[,] cornerPoint = new Vector3[2, 5];
            Vector3 node = nodeStart;
            float dx = nodeEnd.x - nodeStart.x;
            float dy = nodeEnd.y - nodeStart.y;
            float dz = nodeEnd.z - nodeStart.z;
            float angleX = -1f * Mathf.Atan2(dx, dy);
            float angleZ = -1f * Mathf.Atan2(dz, dy);

            for (int i = 0; i < 2; i++)
            {
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

        static void Hoop(Vector3[,] cornerPos, float bandD, int index, GameObject parent)
        {
            // メッシュ結合用に親のオブジェクト作成
            var hoops = new GameObject("Hoops");
            hoops.transform.parent = parent.transform;

            float pitch = STBReader._secColumnRC.BarList[index][5] / 1000f;
            int dirXNum = STBReader._secColumnRC.BarList[index][6];
            int dirYNum = STBReader._secColumnRC.BarList[index][7];
            int sumBar = dirXNum + dirYNum;
            float distance = Vector3.Distance(cornerPos[0, 0], cornerPos[1, 0]);
            List<Vector3> vertex = new List<Vector3>();
            int i = 0;


            Vector3[,] hoopPos = GetBandPos(cornerPos, dirXNum, dirYNum);

            while ((pitch * i) / distance < 1) 
            {
                for (int j = 0; j < 2 * sumBar; j++)
                {
                    vertex.Add(Vector3.Lerp(hoopPos[0, j], hoopPos[1, j], (float)(pitch * i) / distance));
                }
                for (int j = 0; j < sumBar; j++) 
                {
                    Mesh meshObj = CreateMesh.Pipe(vertex[2 * j + (i * 2 * sumBar)], vertex[2 * j + 1 + (i * 2 * sumBar)], bandD / 2f, 12, true);
                    GameObject element = new GameObject("hoop");
                    element.AddComponent<MeshFilter>().mesh = meshObj;
                    element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"))
                    {
                        color = new Color(1, 0, 1, 1)
                    };
                    element.transform.parent = hoops.transform;
                }
                i++;
            }

            // ドローコール削減のため、作成したフープを一つのメッシュに結合
            var color = new Color(1, 0, 1, 1);
            CreateMesh.Conbine(hoops, color, "Standard");
        }

        static void ColumnMainBar(Vector3[,] mainPos, Vector3[,] mainX2Pos, Vector3[,] mainY2Pos, float barSpace, float mainD, int index, GameObject parent)
        {
            // メッシュ結合用に親のオブジェクト作成
            var mainBars = new GameObject("MainBars");
            mainBars.transform.parent = parent.transform;

            int[] mainBarNum = GetColMainNum(index);
            bool[] hasMain2 = { false, false }; // {Main2_X, Main2_Y}
            if (mainBarNum[2] > 1)
                hasMain2[0] = true;
            if (mainBarNum[3] > 1)
                hasMain2[1] = true;

            for (int i = 1; i < 5; i++) 
            {
                // コーナーの主筋
                Mesh meshObj = CreateMesh.Pipe(mainPos[0, i], mainPos[1, i], mainD / 2f, 12, true);
                GameObject element = new GameObject("MainBar");
                element.AddComponent<MeshFilter>().mesh = meshObj;
                element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) 
                {
                    color = new Color(1, 1, 0, 1)
                };
                element.transform.parent = mainBars.transform;
            }

            float posX1Ratio = 1f / (mainBarNum[0] - 1);
            float posY1Ratio = 1f / (mainBarNum[1] - 1);
            float posX2Ratio = 1f / (mainBarNum[2] - 1);
            float posY2Ratio = 1f / (mainBarNum[3] - 1);
            float distanceX = Vector3.Distance(mainPos[0, 1], mainPos[0, 2]);
            float distanceY = Vector3.Distance(mainPos[0, 2], mainPos[0, 3]);
            int barCount = 0;
            List<Vector3> vertex = new List<Vector3>();

            if (hasMain2[1])
            {
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
                for (int j = 2; j <= mainBarNum[0] - 3; j++) 
                {
                    vertex.Add(Vector3.Lerp(mainPos[0, 1], mainPos[0, 2], posX1Ratio * j));
                    vertex.Add(Vector3.Lerp(mainPos[1, 1], mainPos[1, 2], posX1Ratio * j));
                    vertex.Add(Vector3.Lerp(mainPos[0, 3], mainPos[0, 4], posX1Ratio * j));
                    vertex.Add(Vector3.Lerp(mainPos[1, 3], mainPos[1, 4], posX1Ratio * j));
                    barCount += 2;
                }
                // 2nd_X
                for (int j = 1; j <= mainBarNum[2] - 2; j++) 
                {
                    vertex.Add(Vector3.Lerp(mainX2Pos[0, 1], mainX2Pos[0, 2], posX2Ratio * j));
                    vertex.Add(Vector3.Lerp(mainX2Pos[1, 1], mainX2Pos[1, 2], posX2Ratio * j));
                    vertex.Add(Vector3.Lerp(mainX2Pos[0, 3], mainX2Pos[0, 4], posX2Ratio * j));
                    vertex.Add(Vector3.Lerp(mainX2Pos[1, 3], mainX2Pos[1, 4], posX2Ratio * j));
                    barCount += 2;
                }
            }
            else
            {
                for (int j = 1; j <= mainBarNum[0] - 2; j++) 
                {
                    vertex.Add(Vector3.Lerp(mainPos[0, 1], mainPos[0, 2], posX1Ratio * j));
                    vertex.Add(Vector3.Lerp(mainPos[1, 1], mainPos[1, 2], posX1Ratio * j));
                    vertex.Add(Vector3.Lerp(mainPos[0, 3], mainPos[0, 4], posX1Ratio * j));
                    vertex.Add(Vector3.Lerp(mainPos[1, 3], mainPos[1, 4], posX1Ratio * j));
                    barCount += 2;
                }
            }
            if (hasMain2[0]) 
            {
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
                for (int j = 2; j <= mainBarNum[0] - 3; j++)
                {
                    vertex.Add(Vector3.Lerp(mainPos[0, 2], mainPos[0, 3], posY1Ratio * j));
                    vertex.Add(Vector3.Lerp(mainPos[1, 2], mainPos[1, 3], posY1Ratio * j));
                    vertex.Add(Vector3.Lerp(mainPos[0, 4], mainPos[0, 1], posY1Ratio * j));
                    vertex.Add(Vector3.Lerp(mainPos[1, 4], mainPos[1, 1], posY1Ratio * j));
                    barCount += 2;
                }
                // 2nd_Y
                for (int j = 1; j <= mainBarNum[3] - 2; j++)
                {
                    vertex.Add(Vector3.Lerp(mainY2Pos[0, 2], mainY2Pos[0, 3], posY2Ratio * j));
                    vertex.Add(Vector3.Lerp(mainY2Pos[1, 2], mainY2Pos[1, 3], posY2Ratio * j));
                    vertex.Add(Vector3.Lerp(mainY2Pos[0, 4], mainY2Pos[0, 1], posY2Ratio * j));
                    vertex.Add(Vector3.Lerp(mainY2Pos[1, 4], mainY2Pos[1, 1], posY2Ratio * j));
                    barCount += 2;
                }
            }
            else
            {
                for (int j = 1; j <= mainBarNum[1] - 2; j++)
                {
                    vertex.Add(Vector3.Lerp(mainPos[0, 2], mainPos[0, 3], posY1Ratio * j));
                    vertex.Add(Vector3.Lerp(mainPos[1, 2], mainPos[1, 3], posY1Ratio * j));
                    vertex.Add(Vector3.Lerp(mainPos[0, 4], mainPos[0, 1], posY1Ratio * j));
                    vertex.Add(Vector3.Lerp(mainPos[1, 4], mainPos[1, 1], posY1Ratio * j));
                    barCount += 2;
                }
            }
            for (int i = 0; i < barCount; i++)
            {
                Mesh meshObj = CreateMesh.Pipe(vertex[2 * i], vertex[2 * i + 1], mainD / 2f, 12, true);
                GameObject element = new GameObject("mainBars");
                element.AddComponent<MeshFilter>().mesh = meshObj;
                element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) 
                {
                    color = new Color(1, 1, 0, 1)
                };
                element.transform.parent = mainBars.transform;
            }

            // ドローコール削減のため、作成したフープを一つのメッシュに結合
            var color = new Color(1, 1, 0, 1);
            CreateMesh.Conbine(mainBars, color, "Standard");
        }

        public static void Beam(int index, Vector3 nodeStart, Vector3 nodeEnd, float width, float hight, GameObject parent, int elemNum)
        {
            // かぶり、鉄筋径はとりあえずで設定
            float kaburi = 50 / 1000f;
            float bandD = 10 / 1000f;
            float mainD = 25 / 1000f;
            float barSpace = Mathf.Max(1.5f * mainD, 25 / 1000f); // 鉄筋のあき
            float bandSpace = 2 * kaburi + bandD;
            float main1Space = bandSpace + bandD + mainD;
            float main2Space = main1Space + 2 * (mainD + barSpace);
            float main3Space = main2Space + 2 * (mainD + barSpace);

            Vector3[,] strupPos = GetBeamCorner(nodeStart, nodeEnd, width - bandSpace, hight, bandSpace / 2);
            Vector3[,] main1Pos = GetBeamCorner(nodeStart, nodeEnd, width - main1Space, hight, main1Space / 2);
            Vector3[,] main2Pos = GetBeamCorner(nodeStart, nodeEnd, width - main1Space, hight, main2Space / 2);
            Vector3[,] main3Pos = GetBeamCorner(nodeStart, nodeEnd, width - main1Space, hight, main3Space / 2);

            string name = string.Format("Bar" + "{0}", elemNum);
            GameObject barObj = new GameObject(name);
            barObj.transform.parent = parent.transform;

            CreateBar.Stirrup(strupPos, bandD, index, barObj);
            CreateBar.BeamMainBar(main1Pos, main2Pos, main3Pos, barSpace, mainD, index, barObj);
        }

        static Vector3[,] GetBeamCorner(Vector3 nodeStart, Vector3 nodeEnd, float width, float height, float space) 
        {
            //  Z        4 - 0 - 3
            //  ^        |       |
            //  o >  X   1 - - - 2
            Vector3[,] cornerPoint = new Vector3[2, 5];
            Vector3 node = nodeStart;
            float dx = nodeEnd.x - nodeStart.x;
            float dy = nodeEnd.y - nodeStart.y;
            float dz = nodeEnd.z - nodeStart.z;
            float angleY = -1f * Mathf.Atan2(dy, dx);
            float angleZ = -1f * Mathf.Atan2(dz, dx);

            for (int i = 0; i < 2; i++) 
            {
                cornerPoint[i, 0] = node;
                cornerPoint[i, 1] = new Vector3(node.x + width / 2f * Mathf.Sin(angleZ),
                                                node.y - height + space,
                                                node.z + width / 2f * Mathf.Cos(angleZ)
                                                );
                cornerPoint[i, 2] = new Vector3(node.x - width / 2f * Mathf.Sin(angleZ),
                                                node.y - height + space,
                                                node.z - width / 2f * Mathf.Cos(angleZ)
                                                );
                cornerPoint[i, 3] = new Vector3(node.x - width / 2f * Mathf.Sin(angleZ),
                                                node.y - space,
                                                node.z - width / 2f * Mathf.Cos(angleZ)
                                                );
                cornerPoint[i, 4] = new Vector3(node.x + width / 2f * Mathf.Sin(angleZ),
                                                node.y - space,
                                                node.z + width / 2f * Mathf.Cos(angleZ)
                                                );
                node = nodeEnd;
            }
            return (cornerPoint);
        }

        static void Stirrup(Vector3[,] cornerPos, float bandD, int index, GameObject parent)
        {
            // メッシュ結合用に親のオブジェクト作成
            var stirrups = new GameObject("Stirrups");
            stirrups.transform.parent = parent.transform;

            float pitch = STBReader._secBeamRC.BarList[index][6] / 1000f;
            int strupNum = STBReader._secBeamRC.BarList[index][7];
            int sumBar = strupNum + 2;
            float distance = Vector3.Distance(cornerPos[0, 0], cornerPos[1, 0]);
            List<Vector3> vertex = new List<Vector3>();
            int i = 0;

            Vector3[,] stirrupPos = GetBandPos(cornerPos, 2, strupNum);

            while ((pitch * i) / distance < 1)
            {
                for (int j = 0; j < 2 * sumBar; j++) 
                {
                    vertex.Add(Vector3.Lerp(stirrupPos[0, j], stirrupPos[1, j], (float)(pitch * i) / distance));
                }
                for (int j = 0; j < sumBar; j++)
                {
                    Mesh meshObj = CreateMesh.Pipe(vertex[2 * j + (i * 2 * sumBar)], vertex[2 * j + 1 + (i * 2 * sumBar)], bandD / 2f, 12, true);
                    GameObject element = new GameObject("Stirrup");
                    element.AddComponent<MeshFilter>().mesh = meshObj;
                    element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"))
                    {
                        color = new Color(0, 0, 1, 1)
                    };
                    element.transform.parent = stirrups.transform;
                }
                i++;
            }

            // ドローコール削減のため、作成したフープを一つのメッシュに結合
            var color = new Color(0, 0, 1, 1);
            CreateMesh.Conbine(stirrups, color, "Standard");
        }

        static void BeamMainBar(Vector3[,] main1Pos, Vector3[,] main2Pos, Vector3[,] main3Pos, float barSpace, float mainD, int index, GameObject parent)
        {
            // メッシュ結合用に親のオブジェクト作成
            var mainBars = new GameObject("MainBars");
            mainBars.transform.parent = parent.transform;

            int[] mainBarNum = GetBeamMainNum(index);
            bool[] hasMain = new bool[6]; // {Main1_Top, Main1_bottom, Main2_Top, Main2_bottom, Main3_Top, Main3_bottom}
            float distance = Vector3.Distance(main1Pos[0, 1], main1Pos[0, 2]);
            int barCount = 0;
            List<Vector3> vertex = new List<Vector3>();

            for (int i = 0; i < 5; i++) 
            {
                if (mainBarNum[i] > 1)
                    hasMain[i] = true;
                else
                    hasMain[i] = false;
            }

            if (hasMain[0])
            {
                float posRatio = 1f / (mainBarNum[0] - 1);

                for (int j = 0; j < mainBarNum[0]; j++) 
                {
                    vertex.Add(Vector3.Lerp(main1Pos[0, 1], main1Pos[0, 2], posRatio * j));
                    vertex.Add(Vector3.Lerp(main1Pos[1, 1], main1Pos[1, 2], posRatio * j));
                    barCount++;
                }
            }
            if (hasMain[1]) 
            {
                float posRatio = 1f / (mainBarNum[1] - 1);

                for (int j = 0; j < mainBarNum[1]; j++) 
                {
                    vertex.Add(Vector3.Lerp(main1Pos[0, 3], main1Pos[0, 4], posRatio * j));
                    vertex.Add(Vector3.Lerp(main1Pos[1, 3], main1Pos[1, 4], posRatio * j));
                    barCount++;
                }
            }
            if (hasMain[2])
            {
                float posRatio = 1f / (mainBarNum[2] - 1);

                for (int j = 0; j < mainBarNum[2]; j++)
                {
                    vertex.Add(Vector3.Lerp(main2Pos[0, 1], main2Pos[0, 2], posRatio * j));
                    vertex.Add(Vector3.Lerp(main2Pos[1, 1], main2Pos[1, 2], posRatio * j));
                    barCount++;
                }
            }
            if (hasMain[3]) 
            {
                float posRatio = 1f / (mainBarNum[3] - 1);

                for (int j = 0; j < mainBarNum[3]; j++)
                {
                    vertex.Add(Vector3.Lerp(main2Pos[0, 3], main2Pos[0, 4], posRatio * j));
                    vertex.Add(Vector3.Lerp(main2Pos[1, 3], main2Pos[1, 4], posRatio * j));
                    barCount++;
                }
            }
            if (hasMain[4])
            {
                float posRatio = 1f / (mainBarNum[4] - 1);

                for (int j = 0; j < mainBarNum[4]; j++)
                {
                    vertex.Add(Vector3.Lerp(main3Pos[0, 1], main3Pos[0, 2], posRatio * j));
                    vertex.Add(Vector3.Lerp(main3Pos[1, 1], main3Pos[1, 2], posRatio * j));
                    barCount++;
                }
            }
            if (hasMain[5])
            {
                float posRatio = 1f / (mainBarNum[5] - 1);

                for (int j = 0; j < mainBarNum[5]; j++) 
                {
                    vertex.Add(Vector3.Lerp(main3Pos[0, 3], main3Pos[0, 4], posRatio * j));
                    vertex.Add(Vector3.Lerp(main3Pos[1, 3], main3Pos[1, 4], posRatio * j));
                    barCount++;
                }
            }

            for (int i = 0; i < barCount; i++) 
            {
                Mesh meshObj = CreateMesh.Pipe(vertex[2 * i], vertex[2 * i + 1], mainD / 2f, 12, true);
                GameObject element = new GameObject("MainBar");
                element.AddComponent<MeshFilter>().mesh = meshObj;
                element.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) {
                    color = new Color(0, 1, 0, 1)
                };
                element.transform.parent = mainBars.transform;
            }

            // ドローコール削減のため、作成したフープを一つのメッシュに結合
            var color = new Color(0, 1, 0, 1);
            CreateMesh.Conbine(mainBars, color, "Standard");
        }

        static Vector3[,] GetBandPos(Vector3[,] cornerPos, int dirXNum, int dirYNum) 
        {
            Vector3[,] bandPos = new Vector3[2, 2 * (dirXNum + dirYNum)];
            // dir_X
            for (int i = 0; i < dirXNum; i++) 
            {
                for (int j = 0; j < 2; j++) 
                {
                    if (i == 0) 
                    {
                        bandPos[j, 2 * i] = cornerPos[j, 1];
                        bandPos[j, 2 * i + 1] = cornerPos[j, 2];
                    }
                    else if (i == dirXNum - 1)
                    {
                        bandPos[j, 2 * i] = cornerPos[j, 4];
                        bandPos[j, 2 * i + 1] = cornerPos[j, 3];
                    }
                    else
                    {
                        bandPos[j, 2 * i] = Vector3.Lerp(cornerPos[j, 1], cornerPos[j, 4], 1f / (dirXNum - 1) * i);
                        bandPos[j, 2 * i + 1] = Vector3.Lerp(cornerPos[j, 2], cornerPos[j, 3], 1f / (dirXNum - 1) * i);
                    }
                }
            }
            // dir_Y
            for (int i = dirXNum; i < dirXNum + dirYNum; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (i == 0) 
                    {
                        bandPos[j, 2 * i] = cornerPos[j, 1];
                        bandPos[j, 2 * i + 1] = cornerPos[j, 4];
                    }
                    else if (i == dirXNum + dirYNum - 1)
                    {
                        bandPos[j, 2 * i] = cornerPos[j, 2];
                        bandPos[j, 2 * i + 1] = cornerPos[j, 3];
                    }
                    else 
                    {
                        bandPos[j, 2 * i] = Vector3.Lerp(cornerPos[j, 1], cornerPos[j, 2], 1f / (dirYNum - 1) * (i - dirXNum));
                        bandPos[j, 2 * i + 1] = Vector3.Lerp(cornerPos[j, 4], cornerPos[j, 3], 1f / (dirYNum - 1) * (i - dirXNum));
                    }
                }
            }
            return (bandPos);
        }
    }
}
