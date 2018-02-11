﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImgLoader : MonoBehaviour {

    [SerializeField]
    string url = "https://p2.bahamut.com.tw/B/2KU/89/0001526689.JPG";

    [SerializeField]
    PuzzleBuilder puzzleBuilder;

    float ratio;
    public float GetRatio() { return ratio; }

    Material m_Material;
    public Material GetMaterial() { return m_Material; }

    private void Awake()
    {
        var m_Renderer = GetComponent<Renderer>();
        m_Material = m_Renderer.material;
    }

    IEnumerator Start()
    {
        // Start a download of the given URL
        using (WWW www = new WWW(url))
        {
            // Wait for download to complete
            yield return www;

            // assign texture
            var tex = www.texture;
            //print($"{tex.width},{tex.height}");
            m_Material.mainTexture = tex;
            ResetImageSize(tex.width, tex.height);

            puzzleBuilder.Generate();
        } 
    }

    void ResetImageSize(float w,float h)
    {
        ratio = (float)w / h;
        transform.localScale = new Vector3(ratio, 1, 1);   
    }
}
