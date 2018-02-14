using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleBuilder : MonoBehaviour {

    

    //分解成幾組
    [SerializeField]
    int W=1;
    [SerializeField]
    int H=1;

    [SerializeField]
    ImgLoader imgLoader;

    [SerializeField]
    PuzzlePieceGroup puzzlePieceGroup;

    [SerializeField]
    PuzzlePieceGroup target;

    [SerializeField]
    Transform helpCorner;

    [SerializeField]
    PuzzlePiecePocket puzzlePiecePocket;

    // Use this for initialization
    void Awake () {

    }

    public void Generate()
    {
        var startPos = transform.position;
        var ImageScaleX = imgLoader.GetImageScaleX();
        var ImageScaleZ = imgLoader.GetImageScaleZ();
        var UnitSize = ScreenAdapter.UnitSize;

        Vector3 offsetX = new Vector3(ImageScaleX * UnitSize / W, 0.0f, 0.0f);
        Vector3 offsetY = new Vector3(0.0f, ImageScaleZ*UnitSize / H, 0.0f);

        Vector3 scale = new Vector3(ImageScaleX / W, 1.0f, ImageScaleZ / H);
        Vector2 uvScaleFactor =new Vector2(1.0f / W,1.0f / H);
        var uvOffsetX = 1.0f / W;
        var uvOffsetY = 1.0f / H;

        for (int y = 0; y < H; y++) 
        {
            for (int x = 0; x < W; x++)
            {
                var uvOffsetFactor = new Vector2(x * uvOffsetX, y * uvOffsetY);
                Generate(startPos + x * offsetX + y * offsetY, scale, uvScaleFactor, uvOffsetFactor, ref target);
            }
        }

        //一些前置作業
        target.ReRangePiece(W, H);
        target.InjectNeighborPiece();
        target.ResetPieceSize(ImageScaleX, ImageScaleZ);
        target.RecordPositionBeforeSouffleToPocket(W, H);
        target.InitBucket(W, H);
        target.SouffleToPocket(W, H, puzzlePiecePocket);

        target.transform.position = helpCorner.position;
    }

    
    Quaternion rot =Quaternion.Euler(90,180,0);
    void Generate(Vector3 pos, Vector3 scale,Vector2 uvScaleFactor, Vector2 uvOffsetFactor, ref PuzzlePieceGroup target)
    {
        var group =GameObject.Instantiate<PuzzlePieceGroup>(puzzlePieceGroup,pos,rot);
        group.transform.localScale = scale;
        var m = imgLoader.GetMaterial();

        var pieces =group.GetComponentsInChildren<PuzzlePiece>();
        foreach (var p in pieces)
        {
            p.SetMainTextrue(m);
        }

        group.ResetUV(uvScaleFactor, uvOffsetFactor);

        group.Give(target);
    }
}
