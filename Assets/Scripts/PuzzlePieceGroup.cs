using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBucketElement
{
    int GetBucketIndex();
    void SetBucketIndex(int value);
    Transform GetTransform();
}

class PuzzleBucket
{
    List<IBucketElement> list = new List<IBucketElement>();
    float depth=0;
    float span = 1;

    public void Add(IBucketElement element)
    {
        var t = element.GetTransform();
        var localPos = t.localPosition;
        t.localPosition = new Vector3(localPos.x, depth, localPos.z); 
        list.Add(element);

        depth += span;
    }

    public void Remove(IBucketElement element)
    {
        list.Remove(element);
        depth -= span;
    }
}

public class PuzzlePieceGroup : MonoBehaviour {

    const int pieceCount = 24;
    const int rowCount = 4;
    const int columnCount = 6;

    public PuzzlePiece nowMovingPiece;

    public PuzzlePiece[] map1D;
    int newRowCount;
    int newColumnCount;

    public void ReRangePiece(int W,int H)
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
        var group = 0;
        for (var h = 0; h < H; ++h)
        {
            for (var w = 0; w < W; ++w)
            {
                for (var y = 0; y < rowCount; ++y)
                {
                    for (var x = 0; x < columnCount; ++x)
                    {
                        var index = GetIndex(x, y, group);
                        var nX = x + w * columnCount;
                        var nY = y + h * rowCount;
                        var newIndex = GetNewIndex(nX, nY);
                        //print(nX + "," + nY);
                        var nowPiece = map1D[newIndex] = pieces[index];
                        nowPiece.name = "("+nX + "," + nY+")";//rename
                        nowPiece.xIndexInGroup = nX;
                        nowPiece.yIndexInGroup = nY;

                    }
                }
                ++group;//走完24片
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
                    temp.Add(new Vector2(1,0));

                if (IsValidIndex(x , y-1))
                    temp.Add(new Vector2(0, -1));

                if (IsValidIndex(x, y+1))
                    temp.Add(new Vector2(0, 1));

                p.NeighborOffset = temp.ToArray();
                p.isConnected = new bool[temp.Count];
            }
        }
    }

    //Before ReRangePiece
    int GetIndex(int column, int row,int group)
    {
        return column + row * columnCount + group * pieceCount;  
    }

    //After ReRangePiece
    int GetNewIndex(int column, int row)
    {
        return column + row * newColumnCount;
    }

    bool IsValidIndex(int column, int row)
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

    public void ResetPieceSize(float ImageScaleX,float ImageScaleZ)
    {
        pieceWidth =  ImageScaleX*ScreenAdapter.UnitSize/ newColumnCount;
        pieceHeight =  ImageScaleZ* ScreenAdapter.UnitSize / newRowCount;
        hPieceWidth = 0.5f * pieceWidth;
        hPieceHeight = 0.5f * pieceHeight;
        var pieces = GetComponentsInChildren<PuzzlePiece>();
        
        foreach (var p in pieces)
            p.ResetSize(hPieceWidth, hPieceHeight);
    }

    public Vector3[] pos1D;
    public void RecordPositionBeforeSouffleToPocket(int W,int H)
    {
        var count = W * H * pieceCount;
        pos1D = new Vector3[count] ;

        for (var i = 0; i < map1D.Length; ++i)
            pos1D[i] = map1D[i].transform.localPosition; 
    }

    //桶子可以接水，這裡的桶子是用來接拼圖(記錄拼圖重疊的順序)
    PuzzleBucket[] buckets;
    public void InitBucket(int W, int H)
    {
        var count = W * H * pieceCount;
        buckets = new PuzzleBucket[count];

        for (var i = 0; i < buckets.Length; ++i)
            buckets[i] = new PuzzleBucket();
    }

    //因為拼圖的模型是從3D建模軟體來的
    //所以每片拼圖的中心位置，不是剛好位移一個(-hPieceWidth, 0,-hPieceHeight)
    //可以透過pos1D取到每片拼圖真正的中心位置
    public int AlightPieceToBucket(IBucketElement element)
    {
        //找出xIndex,zIndex
        var target = element.GetTransform();
        var localPos = target.localPosition;
        float x = localPos.x;
        float z = localPos.z;
        var xIndex =Tool.GetIndexOfCell(x, -pieceWidth);
        var zIndex = Tool.GetIndexOfCell(z, -pieceHeight);

        xIndex=Mathf.Clamp(xIndex, 0, newColumnCount - 1);
        zIndex = Mathf.Clamp(zIndex, 0, newRowCount - 1);
        //print(xIndex + "," + zIndex);

        var newIndex = GetNewIndex(xIndex, zIndex);
        //print(bucketIndex + "," + i);

        //更新拼圖pos
        target.localPosition = pos1D[newIndex];

        //放到桶子裡
        buckets[newIndex].Add(element);
        return newIndex;
    }

    public void RemoveFromBucket( IBucketElement element)
    {
        var bucketIndex = element.GetBucketIndex();
        if (bucketIndex == Tool.NullIndex)
            return;

        buckets[bucketIndex].Remove(element);
        element.SetBucketIndex(Tool.NullIndex);
    }

    public void SouffleToPocket(int W, int H,PuzzlePiecePocket puzzlePiecePocket)
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
            puzzlePiecePocket.AddToPocket(0,nowPiece,true);
        } 
    }


    public void ResetUV(Vector2 uvScaleFactor,Vector2 uvOffsetFactor)
    {
        var pieces = GetComponentsInChildren<PuzzlePiece>();
        foreach (var p in pieces)
            p.ResetUV(uvScaleFactor, uvOffsetFactor);
    }

    public void Give(PuzzlePieceGroup target)
    {
        //綁定Group
        var pieces = GetComponentsInChildren<PuzzlePiece>();
        foreach (var p in pieces)
            p.SetGroup(target);

        //轉移Child
        var targetTransform =target.transform;
        var transforms = GetComponentsInChildren<Transform>();
        for (var i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];

            //排除自己
            if (t!=transform)
                t.parent = targetTransform;
        }

        //記下Scale(因為進Pocket會改變Scale)
        foreach (var p in pieces)
            p.MemoryOldScale();

        Destroy(gameObject);
    }
}
