﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour, IPuzzleLayer
{
    public int layerIndex = Tool.NullIndex;
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

    public int bucketIndex = Tool.NullIndex;
    public int xIndexInGroup;
    public int yIndexInGroup;

    //鄰接資訊
    public Vector2[] NeighborOffset;

    public int nowIndexInPocket = Tool.NullIndex;
    public bool inPocket = false;

    bool IsMyNeighbor(int x, int y, PuzzlePiece p)
    {
        var b1=p.xIndexInGroup == (xIndexInGroup + x);
        var b2 = p.yIndexInGroup == (yIndexInGroup + y);
        return b1 && b2; 
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

    public ConnectedSet connectedSet;

    bool FindConnectLayerAndMerge()
    {
        return false;
    }

    public void ClearFromBucket()
    {
        //移出桶子
        if (bucketIndex != Tool.NullIndex)
            group.RemoveFromBucket(this);
    }

    public void BeforeMoving()
    {
        //從Bucket裡清除
        if (connectedSet != null)
            connectedSet.BeforeMoving();
        else
            ClearFromBucket();
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
            int x, z;
            group.GetAlignCell(transform.localPosition, out x, out z);

            //(1)更新Bucket
            group.InjectToBucket(this, x, z);

            //(2)pos重新對齊Cell
            bucketIndex = group.AlightPieceToCell(this, x, z);

            //(3)找出可以連接的Layer
            FindConnectLayerAndMerge();
        }
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

        BeforeMoving();
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

            AfterMoving();
            
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
