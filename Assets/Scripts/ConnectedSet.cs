using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 已經連在一起的Puzzle
public class ConnectedSet : MonoBehaviour, IPuzzleLayer
{
    //拉動時的參考Piece
    public static PuzzlePiece pieceForAlign;

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

    static int bucketOffsetX;
    static int bucketOffsetY;
    public void AfterMoving()
    {
        //取得所在Cell
        var worldPos = pieceForAlign.transform.position;
        var localPosInGroup = group.transform.InverseTransformPoint(worldPos);
        int x, y;
        group.GetAlignCell(localPosInGroup, out x, out y);

        //(1)pos重新對齊Cell
        var diff = group.GetDiffAlightPieceToCell(localPosInGroup, x, y);
        transform.localPosition += diff;

        //因為ConnectedSet不一定會剛好和Group對齊
        //所以要使用Offset來修正
        bucketOffsetX = x - pieceForAlign.xIndexInGroup;
        bucketOffsetY = y - pieceForAlign.yIndexInGroup;

        //(2)所有Piece更新位在那一個Bucket
        foreach (var p in pieces)
        {
            var targetX = p.xIndexInGroup + bucketOffsetX;
            var targetY = p.yIndexInGroup + bucketOffsetY;

            if (group.IsValidIndex(targetX, targetY))
                group.InjectToBucket(p, targetX, targetY);
        }

        //(3)找出可以相連的Layer
        FindConnectLayerAndMerge(x, y);
    }

    void FindConnectLayerAndMerge(int x, int y)
    {
        var set = new HashSet<IPuzzleLayer>();

        foreach (var p in pieces)
        {
            if (p != pieceForAlign)
            {
                var targetX = p.xIndexInGroup + bucketOffsetX;
                var targetY = p.yIndexInGroup + bucketOffsetY;

                if (group.IsValidIndex(targetX, targetY))
                    p.FindConnectLayer(targetX, targetY, set);
            }
            else
                p.FindConnectLayer(x, y, set);
        }

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
}
