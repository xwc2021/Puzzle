﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 單塊拚圖
public class PuzzlePiece : MonoBehaviour, IPuzzleLayer
{
    public int GetPiecesCount()
    {
        return 1;
    }
    public int layerIndex = PuzzleBucket.NullIndex;
    public int GetLayerIndex()
    {
        return layerIndex;
    }
    public void SetLayerIndex(int value)
    {
        layerIndex = value;
    }
    public Transform GetTransform()
    {
        return transform;
    }

    //CreateConnectedSet會使用bucketIndex來計算diff
    public int bucketIndex = PuzzleBucket.NullIndex;

    public int xIndexInGroup;
    public int yIndexInGroup;
    public int index1DInGroup;

    //鄰接資訊
    public Vector2[] NeighborOffset;

    public int nowIndexInPocket = PuzzleBucket.NullIndex;
    public bool inPocket = false;

    bool IsMyNeighbor(int x, int y, PuzzlePiece p)
    {
        var b1 = p.xIndexInGroup == (xIndexInGroup + x);
        var b2 = p.yIndexInGroup == (yIndexInGroup + y);
        return b1 && b2;
    }

    PuzzlePiecePocket pocket;
    public void SetPocket(PuzzlePiecePocket pocket) { this.pocket = pocket; }

    PuzzlePieceGroup group;
    public void SetGroup(PuzzlePieceGroup group)
    {
        this.group = group;
    }

    Transform GetParentTransform()
    {
        //不管現在的paretn是Pocket、Group、ConnectedSet都沒差(反正它們軸向都一樣)
        return transform.parent;
    }

    public ConnectedSet connectedSet;

    public void FindConnectLayer(int x, int y, HashSet<IPuzzleLayer> set)
    {
        for (var i = 0; i < NeighborOffset.Length; ++i)
        {
            var offset = NeighborOffset[i];
            var offsetX = (int)offset.x;
            var offsetY = (int)offset.y;
            var Pieces = group.GetBucketPieces(x + offsetX, y + offsetY);
            if (Pieces == null)
                continue;

            for (var k = 0; k < Pieces.Length; ++k)
            {
                var p = Pieces[k];
                if (IsMyNeighbor(offsetX, offsetY, p))//找到相鄰的了
                {
                    //已經相接在一起，就跳過
                    if (p.connectedSet == connectedSet && connectedSet != null)
                        continue;

                    var findOne = (p.connectedSet == null) ? p as IPuzzleLayer : p.connectedSet as IPuzzleLayer;

                    if (!set.Contains(findOne))//有可能findOne已經在set裡了
                        set.Add(findOne);

                    break;
                }
            }
        }
    }

    void FindConnectLayerAndMerge(int x, int y)
    {
        var set = new HashSet<IPuzzleLayer>();
        FindConnectLayer(x, y, set);

        //沒有找到任何相鄰Layer
        if (set.Count == 0)
        {
            LayerMananger.GetInstance().RefreshLayerDepth();
            return;
        }

        //Merge Layer
        set.Add(this);//把自己也加進去
        LayerMananger.GetInstance().Merge(set, group);
    }

    public void ClearFromBucket()
    {
        //移出桶子
        group.RemoveFromBucket(this);
    }

    public void BeforeMoving()
    {

        if (connectedSet != null)
            connectedSet.BeforeMoving();
        else
        {
            //從Bucket裡清除
            ClearFromBucket();

            //從Layer移除：這樣才能放到最上面
            if (layerIndex != PuzzleBucket.NullIndex)
                LayerMananger.GetInstance().Remove(this);
        }

    }

    public void AfterMoving()
    {
        if (connectedSet != null)
        {
            connectedSet.AfterMoving();
        }
        else
        {
            //取得所在Cell
            int x, y;
            group.GetAlignCell(transform.localPosition, out x, out y);

            //(1)pos重新對齊Cell
            group.AlightPieceToCell(this, x, y);

            //(2)更新位在那一個Bucket
            group.InjectToBucket(this, x, y);

            //還不屬於Layer
            if (GetLayerIndex() == PuzzleBucket.NullIndex)
                LayerMananger.GetInstance().Add(this);

            //(3)找出可以相連的Layer
            FindConnectLayerAndMerge(x, y);
        }
    }

    Vector3 oldLocalPos;
    Vector3 beginDragPos;
    bool onMoving = false;

    static Transform MovingTarget;

    void StartMoving()
    {
        onMoving = true;
        group.nowMovingPiece = this;

        MovingTarget = (connectedSet == null) ? transform : connectedSet.transform;
        ConnectedSet.pieceForAlign = (connectedSet == null) ? null : this;

        oldLocalPos = MovingTarget.localPosition;
        beginDragPos = Input.mousePosition;

        BeforeMoving();
    }

    Vector3 ScreenVectorToWorld(Vector3 v)
    {
        //1個ScreenAdapter.UnitSize就是Screen.height
        v = v / Screen.height;
        v = v * ScreenAdapter.UnitSquareSize;
        return v;
    }

    void MovingPiece()
    {
        //https://docs.unity3d.com/ScriptReference/Input-mousePosition.html
        //這裡是Sreen的delta
        var delta = (Input.mousePosition - beginDragPos);
        delta = ScreenVectorToWorld(delta);

        var localDelta = GetParentTransform().InverseTransformVector(delta);
        MovingTarget.localPosition = oldLocalPos + localDelta;

        var nowX = transform.position.x;
        if (nowX < pocket.GetBorder())//臨界點
        {
            if (inPocket)
                pocket.RemoveFromPocket(this);
        }
        else
        {
            //已經拼好的，就不能放回口袋
            if (connectedSet != null)
                return;

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
            if (connectedSet == null)
                transform.parent = group.transform;//放回group

            AfterMoving();

        }
    }

    static bool isMouseDown = false;
    //點選後移動
    void OnMouseDown()
    {
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

    public void ResetUV(Vector2 uvScaleFactor, Vector2 uvOffsetFactor)
    {
        var mesh = GetComponent<MeshFilter>().mesh;
        var uvs = mesh.uv;
        for (var i = 0; i < uvs.Length; i++)
        {
            var uv = uvs[i];//這已經是複本了!!
            uvs[i].Set(uv.x * uvScaleFactor.x + uvOffsetFactor.x, uv.y * uvScaleFactor.y + uvOffsetFactor.y);
        }
        mesh.uv = uvs;
    }

    public void SetMainTextrue(Texture tex)
    {
        var render = GetComponent<Renderer>();
        render.material.mainTexture = tex;
    }

    Vector3 oldScale;
    public void MemoryOldScale() { oldScale = transform.localScale; }

    public void SetScaleInPocket(Vector3 scale)
    {
        transform.localScale = scale;
    }
    public void ResetScale()
    {
        transform.localScale = oldScale;
    }
}
