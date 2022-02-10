using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleBuilder : MonoBehaviour
{

    //分解成幾組
    [SerializeField]
    int W = 1;
    [SerializeField]
    int H = 1;

    [SerializeField]
    PuzzlePieceGroup puzzlePieceGroup;

    [SerializeField]
    PuzzlePieceGroup target;

    [SerializeField]
    Transform helpCorner;

    [SerializeField]
    PuzzlePiecePocket puzzlePiecePocket;

    public void Generate()
    {
        var startPos = transform.position;
        var ImageScaleX = Bootstrap.getInstance().getImageScaleX();
        var ImageScaleZ = Bootstrap.getInstance().getImageScaleZ();
        var UnitSize = ScreenAdapter.UnitSize;

        Vector3 offsetX = new Vector3(ImageScaleX * UnitSize / W, 0.0f, 0.0f);
        Vector3 offsetY = new Vector3(0.0f, ImageScaleZ * UnitSize / H, 0.0f);

        Vector3 scale = new Vector3(ImageScaleX / W, 1.0f, ImageScaleZ / H);
        Vector2 uvScaleFactor = new Vector2(1.0f / W, 1.0f / H);
        var uvOffsetX = 1.0f / W;
        var uvOffsetY = 1.0f / H;

        // 目前的做法，是用W*H個6*4的拼圖組成1個大拼圖
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                var uvOffsetFactor = new Vector2(x * uvOffsetX, y * uvOffsetY);
                GeneratePiece(startPos + x * offsetX + y * offsetY, scale, uvScaleFactor, uvOffsetFactor, target);
            }
        }

        // 一些前置作業
        target.reRangeAndMarkPieceInfo(W, H);
        target.InjectNeighborPieceInfo();
        target.setDebugInfoPieceSize(ImageScaleX, ImageScaleZ);
        target.recordPieceRealCenter(W, H);
        target.InitBucket(W, H);
        target.SouffleToPocket(W, H, puzzlePiecePocket);

        target.transform.position = helpCorner.position;
    }


    Quaternion rot = Quaternion.Euler(90, 180, 0);
    void GeneratePiece(Vector3 pos, Vector3 scale, Vector2 uvScaleFactor, Vector2 uvOffsetFactor, PuzzlePieceGroup target)
    {
        var group = GameObject.Instantiate<PuzzlePieceGroup>(puzzlePieceGroup, pos, rot);
        group.transform.localScale = scale;
        group.setPieceTexture(Bootstrap.getInstance().GetMaterial().mainTexture);
        group.resetPieceUV(uvScaleFactor, uvOffsetFactor);
        group.transfer(target);
    }
}
