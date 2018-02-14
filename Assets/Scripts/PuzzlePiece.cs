using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour, IBucketElement
{
    public int bucketIndex = Tool.NullIndex;
    public int GetBucketIndex()
    {
        return bucketIndex;
    }
    public void SetBucketIndex(int value)
    {
        bucketIndex = value;
    }
    public Transform GetTransform()
    {
        return transform;
    }
    public int xIndexInGroup;
    public int yIndexInGroup;

    //鄰接資訊
    public Vector2[] NeighborOffset;
    public bool[] isConnected;

    public int nowIndexInPocket = Tool.NullIndex;
    public bool inPocket = false;

    bool IsMyNeighbor(int x, int y, PuzzlePiece p)
    {
        var b1=p.xIndexInGroup == (xIndexInGroup + x);
        var b2 = p.yIndexInGroup == (yIndexInGroup + y);
        return b1 && b2; 
    }

    public List<PuzzlePiece> ConnectedGroup;


    void ConnetPiece(PuzzlePiece p)
    {
        var you = p;
        var me = this;
        var IamSingle = ConnectedGroup == null;
        var YouAreSingle = you.ConnectedGroup == null;

        //孤單的兩塊，彼此相遇了
        if (IamSingle && YouAreSingle)
        {
            ConnectedGroup = new List<PuzzlePiece>();
            ConnectedGroup.Add(me);
            ConnectedGroup.Add(you);
            you.ConnectedGroup = ConnectedGroup;
            return;
        }

        //和你相遇的時候，我單身，可是你不是
        if (IamSingle && YouAreSingle==false)
        {
            you.ConnectedGroup.Add(me);
            me.ConnectedGroup = you.ConnectedGroup;
            return;
        }

        //和我相遇的時候，你單身，可是我不是
        if (IamSingle = false && YouAreSingle)
        {
            me.ConnectedGroup.Add(you);
            you.ConnectedGroup = me.ConnectedGroup;
            return;
        }

        //我不是單身，你也不是，可是我們相遇了
        if (IamSingle = false && YouAreSingle == false)
        {
            me.ConnectedGroup.AddRange(you.ConnectedGroup.ToArray());
            you.ConnectedGroup = me.ConnectedGroup;
            return;
        }
            
    }

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
    bool onMoving = false;
    
    void StartMoving()
    {
        onMoving = true;
        group.nowMovingPiece = this;

        oldLocalPos = transform.localPosition;
        beginDragPos = Input.mousePosition;

        //移出桶子
        if (bucketIndex != Tool.NullIndex)
            group.RemoveFromBucket(this);
    }

    Vector3 ScreenVectorToWorld(Vector3 v)
    {
        //1個ScreenAdapter.UnitSize就是Screen.height
        v = v / Screen.height;
        v = v * ScreenAdapter.UnitSize;
        return v;
    }

    void MovingPiece()
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
                pocket.AddToPocket(nowIndex, this, false);
            }
            else
            {
                if (nowIndexInPocket != nowIndex)
                    pocket.SwapInPocket(nowIndexInPocket, nowIndex);//交換
            }
        }
    }

    void StopMoving()
    {
        onMoving = false;
        group.nowMovingPiece = null;

        if (inPocket)
        {
            transform.parent = pocket.transform;
            pocket.RefreshPocket();//口袋重新對齊
            group.RemoveFromBucket(this);//從桶子中移掉
        }
        else
        {
            transform.parent = group.transform;//放回group

            //重新對齊Cell
            bucketIndex = group.AlightPieceToBucket(this);
        }
    }

    static bool isMouseDown = false;
    //點選後移動
    void OnMouseDown() {
        isMouseDown = true;
        StartMoving();
    }
    void OnMouseDrag() { MovingPiece(); }
    void OnMouseUp()
    {
        isMouseDown = false;
        StopMoving();
    }

    //改善從口袋滑出拼圖的手感
    void OnMouseOver()
    {
        if (!isMouseDown)//有點擊才發動
            return;

        //已經在moving其他的拼圖
        if (group.nowMovingPiece != null)
            return;

        if (onMoving)
            return;

        StartMoving();
    }
    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position + hHeight * Vector3.up, Color.yellow);
        Debug.DrawLine(transform.position, transform.position + hWidth * Vector3.left, Color.yellow);

        isMouseDown = Input.GetMouseButton(0);

        if (onMoving)
        {
            if (isMouseDown)
                MovingPiece();
            else
                StopMoving();
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
}
