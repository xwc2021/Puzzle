using System.Collections.Generic;
using UnityEngine;

// 已經連在一起的Puzzle
public class ConnectedSet : MonoBehaviour, IPuzzleLayer
{
    public List<PuzzlePiece> pieces;
    void Awake()
    {
        pieces = new List<PuzzlePiece>();
    }

    public int GetPiecesCount()
    {
        return pieces.Count;
    }
    public Transform GetTransform()
    {
        return transform;
    }

    /* 索引相關 */
    public int layerIndex = Tool.NullIndex;
    public int GetLayerIndex()
    {
        return layerIndex;
    }
    public void SetLayerIndex(int value)
    {
        layerIndex = value;
    }

    /* 移動相關 */
    public static PuzzlePiece pieceForAlign; // 拉動時的參考Piece

    public void BeforeMoving()
    {
        foreach (var p in pieces)
            PuzzlePieceGroup.Instance.removeFromBucket(p);
    }

    static int bucketOffsetX;
    static int bucketOffsetY;
    public void AfterMoving()
    {
        var group = PuzzlePieceGroup.Instance;

        // 取得所在Cell
        var worldPos = pieceForAlign.transform.position;
        var localPosInGroup = group.transform.InverseTransformPoint(worldPos);
        int x, y;
        group.getAlignCell(localPosInGroup, out x, out y);

        // (1)pos重新對齊Cell
        var diff = group.getDiffAlightPieceToCell(localPosInGroup, x, y);
        transform.localPosition += diff;

        //因為ConnectedSet不一定會剛好和Group對齊
        //所以要使用Offset來修正
        bucketOffsetX = x - pieceForAlign.xIndexInFull;
        bucketOffsetY = y - pieceForAlign.yIndexInFull;

        //(2)所有Piece更新位在那一個Bucket
        foreach (var p in pieces)
        {
            var targetX = p.xIndexInFull + bucketOffsetX;
            var targetY = p.yIndexInFull + bucketOffsetY;

            if (group.IsValidIndex(targetX, targetY))
                group.injectToBucket(p, targetX, targetY);
        }

        //(3)找出可以相連的Layer
        findConnectLayerAndMerge(x, y);
    }

    /* 合併相關 */
    void findConnectLayerAndMerge(int x, int y)
    {
        var group = PuzzlePieceGroup.Instance;
        var set = new HashSet<IPuzzleLayer>();

        foreach (var p in pieces)
        {
            if (p != pieceForAlign)
            {
                var targetX = p.xIndexInFull + bucketOffsetX;
                var targetY = p.yIndexInFull + bucketOffsetY;

                if (group.IsValidIndex(targetX, targetY))
                    p.FindConnectLayer(targetX, targetY, set);
            }
            else
                p.FindConnectLayer(x, y, set);
        }

        //沒有找到任何相鄰Layer
        if (set.Count == 0)
        {
            LayerMananger.GetInstance().refreshLayerDepth();
            return;
        }

        //Merge Layer
        set.Add(this);//把自己也加進去
        LayerMananger.GetInstance().merge(set, group);
    }

    public void add(ConnectedSet target)
    {
        foreach (var p in target.pieces)
            add(p);

        target.pieces.Clear();
    }

    public void add(PuzzlePiece p)
    {
        pieces.Add(p);
        p.connectedSet = this;

        //對齊到位置上
        var t = p.transform;
        t.parent = transform;
        t.localPosition = PuzzlePieceGroup.Instance.pieceRealCenter[p.index1DInFull];
    }
}