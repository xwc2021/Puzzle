using System.Collections.Generic;
using UnityEngine;

// PuzzlePieceGroup底下可能會放PuzzlePiece或ConnectedSet(相連的Piece)
public class PuzzlePieceGroup : MonoBehaviour
{
    const int pieceCount = 24;
    const int rowCount = 4;
    const int columnCount = 6;

    public PuzzlePiece nowMovingPiece;

    /* 索引Tool相關*/

    int GetIndex1D(int column, int row)
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

    /* Create Piece相關 */

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

    public void transferPieceTo(PuzzlePieceGroup target)
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

    public PuzzlePiece[] map1D;
    int newRowCount;
    int newColumnCount;

    public void reRangeAndMarkPieceInfo(int W, int H)
    {
        newColumnCount = W * columnCount;
        newRowCount = H * rowCount;
        map1D = new PuzzlePiece[W * H * pieceCount];

        //如果W=2 H=3，會有6個Group
        //口口  G4 G5
        //口口  G2 G3
        //口口  G0 G1
        //把這6個Group的piece，重新映射到一個一維陣列，並標名(nX,nY):index
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
                        var newIndex = GetIndex1D(nX, nY);
                        var nowPiece = map1D[newIndex] = pieces[index];
                        nowPiece.name = "(" + nX + "," + nY + "):" + index + ',' + newIndex;//rename
                        nowPiece.xIndexInFull = nX;
                        nowPiece.yIndexInFull = nY;
                        nowPiece.index1DInFull = newIndex;

                        ++index; // 1個1個取出就行了
                    }
                }
            }
        }
    }

    //建立相鄰資訊
    public void InjectNeighborPieceInfo()
    {
        var temp = new List<Vector2>();
        for (var y = 0; y < newRowCount; ++y)
        {
            for (var x = 0; x < newColumnCount; ++x)
            {
                temp.Clear();
                var index = GetIndex1D(x, y);
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

    float pieceWidth;
    float pieceHeight;
    float hPieceWidth;
    float hPieceHeight;

    public void setDebugInfoPieceSize(float ImageScaleX, float ImageScaleZ)
    {
        pieceWidth = ImageScaleX * ScreenAdapter.UnitSquareSize / newColumnCount;
        pieceHeight = ImageScaleZ * ScreenAdapter.UnitSquareSize / newRowCount;
        hPieceWidth = 0.5f * pieceWidth;
        hPieceHeight = 0.5f * pieceHeight;
        var pieces = GetComponentsInChildren<PuzzlePiece>();

        foreach (var p in pieces)
            p.ResetSize(hPieceWidth, hPieceHeight);
    }

    // 因為拼圖的模型是從3D建模軟體來的
    // 所以每片拼圖的中心位置，不是剛好位移一個(-hPieceWidth, 0,-hPieceHeight)
    // pieceRealCenter會記下每片拼圖真正的中心位置
    public Vector3[] pieceRealCenter;
    public void recordPieceRealCenter(int W, int H)
    {
        var count = W * H * pieceCount;
        pieceRealCenter = new Vector3[count];

        for (var i = 0; i < map1D.Length; ++i)
            pieceRealCenter[i] = map1D[i].transform.localPosition;
    }

    public void SouffleToPocket(int W, int H, PuzzlePiecePocket puzzlePiecePocket)
    {
        var count = W * H * pieceCount;
        var indexList = new List<int>();

        // 產生號碼牌(index)
        for (var i = 0; i < count; ++i)
            indexList.Add(i);

        while (indexList.Count > 0)
        {
            // 搖出號碼牌
            int i = Random.Range(0, indexList.Count);
            var removeIndex = indexList[i];
            indexList.RemoveAt(i);

            // 把號碼牌對映的Piece加到口袋(Pocket)
            var nowPiece = map1D[removeIndex];
            puzzlePiecePocket.AddToPocket(0, nowPiece, true);
        }
    }

    /* Bucket相關 */
    // 桶子可以接水，這裡的桶子是用來裝拼圖(空間索引)
    PuzzleBucket[] bucketCells;
    public void InitBucket(int W, int H)
    {
        var count = W * H * pieceCount;
        bucketCells = new PuzzleBucket[count];

        for (var i = 0; i < bucketCells.Length; ++i)
            bucketCells[i] = new PuzzleBucket();
    }

    public void InjectToBucket(PuzzlePiece p, int xIndex, int yIndex)
    {
        //放到桶子裡
        var i = GetIndex1D(xIndex, yIndex);
        p.bucketIndex = i;
        bucketCells[i].Add(p);
    }

    public void RemoveFromBucket(PuzzlePiece p)
    {
        if (p.bucketIndex == PuzzleBucket.NullIndex)
            return;

        bucketCells[p.bucketIndex].Remove(p);//這裡是O(n) 
        p.bucketIndex = PuzzleBucket.NullIndex;
    }

    public PuzzlePiece[] GetBucketPieces(int column, int row)
    {
        if (!IsValidIndex(column, row))
            return null;

        var i = GetIndex1D(column, row);
        return bucketCells[i].GetTotal();
    }

    /* Cell相關 */
    public void GetAlignCell(Vector3 localPos, out int xIndex, out int yIndex)
    {
        //找出xIndex,zIndex
        float x = localPos.x;
        float z = localPos.z;
        xIndex = PuzzleBucket.GetIndexOfCell(x, -pieceWidth);
        yIndex = PuzzleBucket.GetIndexOfCell(z, -pieceHeight);

        xIndex = Mathf.Clamp(xIndex, 0, newColumnCount - 1);
        yIndex = Mathf.Clamp(yIndex, 0, newRowCount - 1);
        //print(xIndex + "," + zIndex);
    }

    public void AlightPieceToCell(PuzzlePiece piece, int xIndex, int yIndex)
    {
        var newIndex = GetIndex1D(xIndex, yIndex);

        //更新拼圖pos
        piece.transform.localPosition = pieceRealCenter[newIndex];
    }

    public Vector3 GetDiffAlightPieceToCell(Vector3 localPos, int xIndex, int yIndex)
    {
        var newIndex = GetIndex1D(xIndex, yIndex);
        return pieceRealCenter[newIndex] - localPos;
    }

    /* 建立ConnectedSet相關 */
    [SerializeField]
    ConnectedSet templateConnectedSet;

    public ConnectedSet CreateConnectedSet(PuzzlePiece p)
    {
        //找出目前所在的Cell和原始的Cell的diff
        var diff = pieceRealCenter[p.bucketIndex] - pieceRealCenter[p.index1DInFull];

        var cs = GameObject.Instantiate<ConnectedSet>(templateConnectedSet);
        var t = cs.transform;
        t.parent = transform;
        t.localPosition = Vector3.zero + diff;

        cs.group = this;
        return cs;
    }
}