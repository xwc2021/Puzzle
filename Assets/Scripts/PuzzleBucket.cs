
using System.Collections.Generic;

//PuzzleBucket單純用來作空間索引
public class PuzzleBucket
{
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