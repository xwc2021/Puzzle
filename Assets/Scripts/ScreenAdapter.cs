using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenAdapter : MonoBehaviour {

    [SerializeField]
    float percentage=0.6f;//佔銀幕的百分比

    public float GetScaleToFitScreen(ImgLoader imgLoader)
    {
        var imgRatio =imgLoader.GetRatio();
        var screenRatio = GetScreenRatio();

        //比銀幕大，就縮小到可以放進銀幕
        var adapterScale = (imgRatio > screenRatio)?imgRatio / screenRatio:1.0f;

        return adapterScale*percentage;
    }

    float GetScreenRatio()
    {
        return (float)Screen.width / Screen.height;
    }
}
