using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    int bucketIndex= Tool.NullIndex;
    public void SetBucketIndex(int bucketIndex) { this.bucketIndex = bucketIndex; }

    int nowIndexInPocket= Tool.NullIndex;
    public void SetInPucketIndex(int nowIndexInPocket) { this.nowIndexInPocket = nowIndexInPocket; }

    bool inPocket = false;
    public void SetInPucket(bool inPocket) { this.inPocket = inPocket; }

    PuzzlePiecePocket pocket;
    public void SetPocket(PuzzlePiecePocket pocket) { this.pocket = pocket; }

    PuzzlePieceGroup group;
    public void SetGroup(PuzzlePieceGroup group) {
        this.group = group;
    }

    Transform GetParentTransform()
    {
        return transform.parent;
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

        var localDelta = GetParentTransform().InverseTransformVector(delta);
        //print(localDelta);
        transform.localPosition = oldLocalPos + localDelta;

        var nowX = transform.position.x;
        if (nowX < pocket.GetBorder())//臨界點
        {
            if (inPocket)
                pocket.RemoveFromPocket(this);
        }
        else
        {
            //轉換回Pocket的local space
            var pos = transform.position;
            pos = pocket.transform.InverseTransformPoint(pos);
            var localZ = pos.z;

            var nowIndex = pocket.GetInsertIndex(localZ, inPocket);
            //print(nowIndex);
            if (!inPocket)//不在口袋裡，就加進去
            {
                pocket.AddToPocket(nowIndex, this,false);
            }
            else
            {
                if(nowIndexInPocket!= nowIndex)
                    pocket.SwapInPocket(nowIndexInPocket, nowIndex);//交換
            }
        }    
    }

    void OnMouseUp()
    {
        if (inPocket)
        {
            transform.parent = pocket.transform;
            pocket.RefreshPocket();//口袋重新對齊
            group.ClearBucket(bucketIndex, this);//從桶子中移掉
        }
        else
        {
            transform.parent = group.transform;//放回group

            //重新對齊Cell
            bucketIndex = group.AlightPieceToBucket(bucketIndex, this);
            
        }
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
    public void MemoryOldScale (){ oldScale = transform.localScale; }

    public void SetScaleInPocket(Vector3 scale)
    {
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
