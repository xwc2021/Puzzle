using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    int nowIndexInPocket=-1;
    public void SetInPucketIndex(int nowIndexInPocket) { this.nowIndexInPocket = nowIndexInPocket; }

    bool inPocket = false;
    public void SetInPucket(bool inPocket) { this.inPocket = inPocket; }

    PuzzlePiecePocket pocket;
    public void SetPocket(PuzzlePiecePocket pocket) { this.pocket = pocket; }

    Transform group;
    Transform GetGroupTransform()
    {
        if(group==null)
            group=transform.parent;

        return group;
    }

    Vector3 oldLocalPos;
    Vector3 beginDragPos;
    void OnMouseDown()
    {
        oldLocalPos = transform.localPosition;
        beginDragPos = Input.mousePosition;
    }

    Vector3 ScreenVectorToWorld(Vector3 v)
    {
        //1個ScreenAdapter.UnitSize就是Screen.height
        v = v / Screen.height;
        v = v * ScreenAdapter.UnitSize;
        return v;
    }

    void OnMouseDrag()
    {
        //https://docs.unity3d.com/ScriptReference/Input-mousePosition.html
        //這裡是Sreen的delta
        var delta = (Input.mousePosition - beginDragPos);
        delta = ScreenVectorToWorld(delta);

        var localDelta = GetGroupTransform().InverseTransformVector(delta);
        //print(localDelta);
        transform.localPosition = oldLocalPos + localDelta;

        var nowX = transform.localPosition.x;
        if (nowX > pocket.GetBorder())//臨界點
        {
            if (inPocket)
                pocket.RemoveFromPocket(this);

        }
        else
        {
            var nowZ = transform.localPosition.z;
            var nowIndex = pocket.GetInsertIndex(nowZ);
            print(nowIndex);
            if (!inPocket)//不再口袋裡，就加進去
            {
                pocket.AddToPocket(nowIndex, this);
            }
            else
            {
                if(nowIndexInPocket!= nowIndex)
                    pocket.SwapInPocket(nowIndexInPocket, nowIndex);
            }
            
        }
            
    }

    void OnMouseUp()
    {
        pocket.RefreshPocket();
    }

    //Just For Debug
    public float hWidth;
    public float hHeight;
    public void ResetSize(float hWidth, float hHeight)
    {
        this.hWidth = hWidth;
        this.hHeight = hHeight;
    }

    public void ResetUV (Vector2 uvScaleFactor, Vector2 uvOffsetFactor) {
        var mesh = GetComponent<MeshFilter>().mesh;
        var uvs =mesh.uv;
        for (var i = 0; i < uvs.Length; i++)
        {
            var uv = uvs[i];//這已經是複本了!!
            uvs[i].Set(uv.x* uvScaleFactor.x+ uvOffsetFactor.x, uv.y* uvScaleFactor.y+ uvOffsetFactor.y);
        }
        mesh.uv = uvs;
    }

    public void SetMainTextrue(Material m)
    {
        var render = GetComponent<Renderer>();
        render.material.mainTexture = m.mainTexture;
    }

    Vector3 oldScale;
    public void SetScaleInPocket(Vector3 scale)
    {
        oldScale = transform.localScale;
        transform.localScale = scale;
    }
    public void ResetScale()
    {
        transform.localScale = oldScale;
    }

    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position + hHeight * Vector3.up,Color.yellow);
        Debug.DrawLine(transform.position, transform.position + hWidth * Vector3.left, Color.yellow);
    }
}
