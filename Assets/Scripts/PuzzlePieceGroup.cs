using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePieceGroup : MonoBehaviour {

    const int pieceCount = 24;
    const int rowCount = 4;
    const int columnCount = 6;

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
                        map1D[newIndex] = pieces[index];
                    }
                }
                ++group;//走完24片
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

    public void ResetPieceSize(int W, int H,float ImageScaleX,float ImageScaleZ)
    {
        newColumnCount = W * columnCount;
        newRowCount = H * rowCount;

        var hWidth = 0.5f* ImageScaleX*ScreenAdapter.UnitSize/ newColumnCount;
        var hHeight = 0.5f * ImageScaleZ* ScreenAdapter.UnitSize / newRowCount;
        var hMin = Mathf.Min(hWidth, hHeight);
        var pieces = GetComponentsInChildren<PuzzlePiece>();
        
        foreach (var p in pieces)
            p.ResetSize(hMin);
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
            puzzlePiecePocket.AddToPocket(nowPiece);
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
        var targetTransform =target.transform;

        var pieces = GetComponentsInChildren<Transform>();
        for (var i = 0; i < pieces.Length; i++)
        {
            var p = pieces[i];

            //排除自己
            if(p!=transform)
                p.parent = targetTransform;
        }

        Destroy(gameObject);
    }
}
