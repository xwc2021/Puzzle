using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePieceGroup : MonoBehaviour
{

    const int pieceCount = 24;
    const int rowCount = 4;
    const int columnCount = 6;

    [SerializeField]
    ConnectedSet templateConnectedSet;

    public ConnectedSet CreateConnectedSet(PuzzlePiece p)
    {
        //找出目前所在的Cell和原始的Cell的diff
        var diff = pos1D[p.bucketIndex] - pos1D[p.indexInGroup];

        var cs = GameObject.Instantiate<ConnectedSet>(templateConnectedSet);
        var t = cs.transform;
        t.parent = transform;
        t.localPosition = Vector3.zero + diff;

        cs.group = this;
        return cs;
    }

    public PuzzlePiece nowMovingPiece;

    public PuzzlePiece[] map1D;
    int newRowCount;
    int newColumnCount;

    public void ReRangePiece(int W, int H)
    {
        newColumnCount = W * columnCount;
        newRowCount = H * rowCount;
        map1D = new PuzzlePiece[W * H * pieceCount];

        //ReRangePiece前
        //如果W=2 H=3，會有6個group
        //口口  g4 g5
        //口口  g2 g3
        //口口  g0 g1
        //把這6個group，重新映射到一個一維陣列
        var pieces = GetComponentsInChildren<PuzzlePiece>();
        var index = 0;
        for (var h = 0; h < H; ++h)
        {
            for (var w = 0; w < W; ++w)
            {
                for (var y = 0; y < rowCount; ++y)
                {
                    for (var x = 0; x < columnCount; ++x)
                    {
                        var nX = x + w * columnCount;
                        var nY = y + h * rowCount;
                        var newIndex = GetNewIndex(nX, nY);
                        var nowPiece = map1D[newIndex] = pieces[index];
                        nowPiece.name = "(" + nX + "," + nY + "):" + index;//rename
                        nowPiece.xIndexInGroup = nX;
                        nowPiece.yIndexInGroup = nY;
                        nowPiece.indexInGroup = newIndex;

                        ++index; // 1個1個取出就行了
                    }
                }
            }
        }
    }

    //建立相鄰資訊
    public void InjectNeighborPiece()
    {
        var temp = new List<Vector2>();
        for (var y = 0; y < newRowCount; ++y)
        {
            for (var x = 0; x < newColumnCount; ++x)
            {
                temp.Clear();
                var index = GetNewIndex(x, y);
                var p = map1D[index];

                if (IsValidIndex(x - 1, y))
                    temp.Add(new Vector2(-1, 0));

                if (IsValidIndex(x + 1, y))
                    temp.Add(new Vector2(1, 0));

                if (IsValidIndex(x, y - 1))
                    temp.Add(new Vector2(0, -1));

                if (IsValidIndex(x, y + 1))
                    temp.Add(new Vector2(0, 1));

                p.NeighborOffset = temp.ToArray();
            }
        }
    }

    //After ReRangePiece
    int GetNewIndex(int column, int row)
    {
        return column + row * newColumnCount;
    }

    public bool IsValidIndex(int column, int row)
    {
        if (column < newColumnCount && column >= 0 && row < newRowCount && row >= 0)
            return true;
        else
            return false;
    }

    float pieceWidth;
    float pieceHeight;
    float hPieceWidth;
    float hPieceHeight;

    public void ResetPieceSize(float ImageScaleX, float ImageScaleZ)
    {
        pieceWidth = ImageScaleX * ScreenAdapter.UnitSize / newColumnCount;
        pieceHeight = ImageScaleZ * ScreenAdapter.UnitSize / newRowCount;
        hPieceWidth = 0.5f * pieceWidth;
        hPieceHeight = 0.5f * pieceHeight;
        var pieces = GetComponentsInChildren<PuzzlePiece>();

        foreach (var p in pieces)
            p.ResetSize(hPieceWidth, hPieceHeight);
    }

    public Vector3[] pos1D;
    public void RecordPositionBeforeSouffleToPocket(int W, int H)
    {
        var count = W * H * pieceCount;
        pos1D = new Vector3[count];

        for (var i = 0; i < map1D.Length; ++i)
            pos1D[i] = map1D[i].transform.localPosition;
    }

    public PuzzlePiece[] GetBucketPieces(int column, int row)
    {
        if (!IsValidIndex(column, row))
            return null;

        var i = GetNewIndex(column, row);
        return buckets[i].GetTotal();
    }

    //桶子可以接水，這裡的桶子是用來裝拼圖(空間索引)
    PuzzleBucket[] buckets;
    public void InitBucketAndLayer(int W, int H)
    {
        var count = W * H * pieceCount;
        buckets = new PuzzleBucket[count];

        for (var i = 0; i < buckets.Length; ++i)
            buckets[i] = new PuzzleBucket();
    }

    public void GetAlignCell(Vector3 localPos, out int xIndex, out int yIndex)
    {
        //找出xIndex,zIndex
        float x = localPos.x;
        float z = localPos.z;
        xIndex = Tool.GetIndexOfCell(x, -pieceWidth);
        yIndex = Tool.GetIndexOfCell(z, -pieceHeight);

        xIndex = Mathf.Clamp(xIndex, 0, newColumnCount - 1);
        yIndex = Mathf.Clamp(yIndex, 0, newRowCount - 1);
        //print(xIndex + "," + zIndex);
    }

    //因為拼圖的模型是從3D建模軟體來的
    //所以每片拼圖的中心位置，不是剛好位移一個(-hPieceWidth, 0,-hPieceHeight)
    //可以透過pos1D取到每片拼圖真正的中心位置
    public void AlightPieceToCell(PuzzlePiece piece, int xIndex, int yIndex)
    {
        var newIndex = GetNewIndex(xIndex, yIndex);

        //更新拼圖pos
        piece.transform.localPosition = pos1D[newIndex];
    }

    public Vector3 GetDiffAlightPieceToCell(Vector3 localPos, int xIndex, int yIndex)
    {
        var newIndex = GetNewIndex(xIndex, yIndex);
        return pos1D[newIndex] - localPos;
    }

    public void InjectToBucket(PuzzlePiece p, int xIndex, int yIndex)
    {
        //放到桶子裡
        var i = GetNewIndex(xIndex, yIndex);
        p.bucketIndex = i;
        buckets[i].Add(p);
    }

    public void RemoveFromBucket(PuzzlePiece p)
    {
        if (p.bucketIndex == Tool.NullIndex)
            return;

        buckets[p.bucketIndex].Remove(p);//這裡是O(n) 
        p.bucketIndex = Tool.NullIndex;
    }

    public void SouffleToPocket(int W, int H, PuzzlePiecePocket puzzlePiecePocket)
    {
        var count = W * H * pieceCount;
        var indexList = new List<int>();

        for (var i = 0; i < count; ++i)
            indexList.Add(i);

        while (indexList.Count > 0)
        {
            int i = Random.Range(0, indexList.Count);
            var removeIndex = indexList[i];
            indexList.RemoveAt(i);
            var nowPiece = map1D[removeIndex];
            puzzlePiecePocket.AddToPocket(0, nowPiece, true);
        }
    }

    public void setPieceTexture(Texture tex)
    {
        var pieces = GetComponentsInChildren<PuzzlePiece>();
        foreach (var p in pieces)
        {
            p.SetMainTextrue(tex);
        }
    }


    public void resetPieceUV(Vector2 uvScaleFactor, Vector2 uvOffsetFactor)
    {
        var pieces = GetComponentsInChildren<PuzzlePiece>();
        foreach (var p in pieces)
            p.ResetUV(uvScaleFactor, uvOffsetFactor);
    }

    public void transfer(PuzzlePieceGroup target)
    {
        //綁定Group
        var pieces = GetComponentsInChildren<PuzzlePiece>();
        foreach (var p in pieces)
            p.SetGroup(target);

        //轉移Child
        var targetTransform = target.transform;
        var transforms = GetComponentsInChildren<Transform>();
        for (var i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];

            //排除自己
            if (t != transform)
                t.parent = targetTransform;
        }

        //記下Scale(因為進Pocket會改變Scale)
        foreach (var p in pieces)
            p.MemoryOldScale();

        Destroy(gameObject);
    }
}
