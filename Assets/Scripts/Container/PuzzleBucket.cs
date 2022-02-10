
using System.Collections.Generic;

//PuzzleBucket單純用來作空間索引
public class PuzzleBucket
{
    //找出位在那個小格
    public static int GetIndexOfCell(float x, float cell)
    {
        return (int)((x - (x % cell)) / cell);
    }

    public readonly static int NullIndex = -1;

    List<PuzzlePiece> list = new List<PuzzlePiece>();

    public PuzzlePiece[] GetTotal()
    {
        return list.ToArray();
    }

    public void Add(PuzzlePiece p)
    {
        list.Add(p);
    }

    public void Remove(PuzzlePiece p)
    {
        list.Remove(p);
    }
}