using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenAdapter : MonoBehaviour {

    [SerializeField]
    float scale=0.6f;//放進銀幕了，可以再縮放一次

    public float GetScaleToFitScreen(ImgLoader imgLoader)
    {
        var imgRatio =imgLoader.GetRatio();
        var screenRatio = GetScreenRatio();

        //比銀幕大，就縮小到可以放進銀幕
        var adapterScale = (imgRatio > screenRatio)? screenRatio/ imgRatio : 1.0f;

        return adapterScale* scale;
    }

    public static float GetScreenRatio()
    {
        return (float)Screen.width / Screen.height;
    }

    public static float UnitSize = 10;

    public static float GetHalfScreenWidth()
    {
        return 0.5f * UnitSize * GetScreenRatio();
    }
}
