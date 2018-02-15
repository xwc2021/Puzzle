using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//已經連在一起的Puzzle
public class ConnectedSet : MonoBehaviour, IPuzzleLayer
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

    public List<PuzzlePiece> list;

    void Awake()
    {
        list = new List<PuzzlePiece>();
    }

    public void Add(ConnectedSet target)
    {
        foreach (var p in target.list)
            list.Add(p);
    }

    public void Add(PuzzlePiece p)
    {
        list.Add(p);
    }

    public void BeforeMoving()
    {
        foreach (var p in list)
            p.ClearFromBucket();
    }

    public void AfterMoving()
    {
        //(1)更新Bucket

        //(2)pos重新對齊Cell

        //(3)找出可以連接的Layer
    }
}
