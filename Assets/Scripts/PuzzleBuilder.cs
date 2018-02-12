using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleBuilder : MonoBehaviour {

    const int pieceCount = 24;
    const int rowCount = 4;
    const int columnCount = 6;

    const float UnitSize = 10;

    //分解成幾組
    [SerializeField]
    int W=1;
    [SerializeField]
    int H=1;

    [SerializeField]
    ImgLoader imgLoader;

    [SerializeField]
    PuzzlePieceGroup puzzlePieceGroup;
    public PuzzlePiece[] puzzlePieces;

    PuzzlePiece GetPiece(int column,int row )
    {
        Debug.Assert(row < rowCount);
        Debug.Assert(column < columnCount);

        var index = column + row * columnCount;
        return puzzlePieces[index];
    }


    // Use this for initialization
    void Awake () {
        //get 24 pieces
        puzzlePieces=puzzlePieceGroup.GetComponentsInChildren<PuzzlePiece>();
       
    }

    PuzzlePieceGroup target;
    public void Generate()
    {
        var startPos = transform.position;
        var ratio = imgLoader.GetRatio();

        Vector3 offsetX = new Vector3(ratio*UnitSize / W, 0.0f, 0.0f);
        Vector3 offsetY = new Vector3(0.0f, UnitSize / H, 0.0f);

        Vector3 scale = new Vector3(ratio / W, 1.0f, 1.0f / H);
        print(scale);

        target=null;
        for (int y = 0; y < H; y++) 
        {
            for (int x = 0; x < W; x++)
            {
                Generate(startPos + x * offsetX + y * offsetY, scale, ref target);
            }
        }
    }

    
    Quaternion rot =Quaternion.Euler(90,180,0);
    void Generate(Vector3 pos, Vector3 scale, ref PuzzlePieceGroup target)
    {
        var group =GameObject.Instantiate<PuzzlePieceGroup>(puzzlePieceGroup,pos,rot);
        group.transform.localScale = scale;
        var m = imgLoader.GetMaterial();

        var pieces =group.GetComponentsInChildren<PuzzlePiece>();
        foreach (var p in pieces)
        {
            p.SetMaterial(m);
        }

        if (target == null)
            target = group; 
        else
            group.Give(target);
    }
}
