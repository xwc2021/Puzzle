using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenAdapter : MonoBehaviour
{

    [SerializeField]
    public float addtionalScale = 0.6f;// 放進銀幕了，可以再縮放一次

    public float getScaleToFitScreen(float imgRatio)
    {
        var screenRatio = getScreenRatio();
        //比銀幕大，就縮小到可以放進銀幕
        var adapterScale = (imgRatio > screenRatio) ? screenRatio / imgRatio : 1.0f;

        return adapterScale;
    }

    public static float getScreenRatio()
    {
        return (float)Screen.width / Screen.height;
    }

    public static float UnitSize
    {
        // CameraSize的2倍
        get => Camera.main.orthographicSize * 2;
    }

    public static float getHalfScreenWidth()
    {
        return 0.5f * UnitSize * getScreenRatio();
    }
}
