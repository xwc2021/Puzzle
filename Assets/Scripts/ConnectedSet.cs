using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//已經連在一起的Puzzle
public class ConnectedSet : MonoBehaviour, IPuzzleLayer
{
    public PuzzlePieceGroup group;
    public int GetPiecesCount()
    {
        return pieces.Count;
    }
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

    public List<PuzzlePiece> pieces;

    void Awake()
    {
        pieces = new List<PuzzlePiece>();
    }

    public void Add(ConnectedSet target)
    {
        foreach (var p in target.pieces)
            Add(p);

        target.pieces.Clear();
    }

    public void Add(PuzzlePiece p)
    {
        pieces.Add(p);
        p.connectedSet = this;

        //對齊到位置上
        var t = p.transform;
        t.parent = transform;
        t.localPosition = group.pos1D[p.indexInGroup];
    }

    public void BeforeMoving()
    {
        foreach (var p in pieces)
            p.ClearFromBucket();
    }

    public void AfterMoving()
    {
        //(1)更新Bucket

        //(2)pos重新對齊Cell

        //(3)找出可以連接的Layer
    }
}
