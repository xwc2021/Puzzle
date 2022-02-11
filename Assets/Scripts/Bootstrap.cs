using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 負責載入圖檔
/// </summary>
public class Bootstrap : MonoBehaviour
{

    [SerializeField]
    string url = "https://www.kpopn.com/upload/42ec1fe31f45347f8843.jpg";

    [SerializeField]
    PuzzleBuilder puzzleBuilder;

    [SerializeField]
    ScreenAdapter screenAdapter;

    [SerializeField]
    PuzzlePiecePocket puzzlePiecePocket;

    float imageRatio;

    Material m_Material;
    public Material GetMaterial() { return m_Material; }

    static Bootstrap instance;
    public static Bootstrap getInstance()
    {
        return instance;
    }

    private void Awake()
    {
        // 綁定
        Bootstrap.instance = this;

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
            setSquareSize(tex.width, tex.height);

            puzzleBuilder.Generate();
            puzzlePiecePocket.snapToRight();
        }
    }

    void setSquareSize(float w, float h)
    {
        // 先把mesh(原本是正方形)縮放到圖片的長寬比
        imageRatio = (float)w / h;
        // transform.localScale = new Vector3(imageRatio, 1, 1);

        // 圖片可能大於銀幕，所以要再scale一次
        var scale = screenAdapter.getScaleToFitScreen(imageRatio);

        // 再縮小
        scale *= screenAdapter.addtionalScale;
        transform.localScale = new Vector3(imageRatio * scale, 1, 1 * scale);

    }

    public float getImageScaleX() { return transform.localScale.x; }
    public float getImageScaleZ() { return transform.localScale.z; }
}
