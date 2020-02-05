using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 参考 http://backlight1162.blog.fc2.com/blog-entry-85.html
public class ColorPicker : MonoBehaviour
{
    public Texture2D colorPicker;
    public int ImageWidth = 100;
    public int ImageHeight = 100;
    public int x = 100;
    public int y = 100;

    void OnGUI() {
        GameObject obj;
        if (GUI.RepeatButton(new Rect(x, y, ImageWidth, ImageHeight), colorPicker)) {
            Vector2 pickpos = Event.current.mousePosition;
            int aaa = Convert.ToInt32(pickpos.x);
            int bbb = Convert.ToInt32(pickpos.y);
            Color col = colorPicker.GetPixel(aaa, 41 - bbb);

            obj = GameObject.Find("ColorCheckPanel");
            obj.GetComponent<Image>().color = col;
            Debug.Log(col);
        }
    }
}
