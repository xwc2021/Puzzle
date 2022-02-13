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
        // 綁定
        PuzzlePieceGroup.Instance = target;
        PuzzlePiecePocket.Instance = puzzlePiecePocket;

        // 準備生成Group需要的資料
        var startPos = transform.position;
        var ImageScaleX = Bootstrap.getInstance().getImageScaleX();
        var ImageScaleZ = Bootstrap.getInstance().getImageScaleZ();
        var UnitSquareSize = ScreenAdapter.UnitSquareSize;

        Vector3 offsetX = new Vector3(ImageScaleX * UnitSquareSize / W, 0.0f, 0.0f);
        Vector3 offsetY = new Vector3(0.0f, ImageScaleZ * UnitSquareSize / H, 0.0f);

        Vector3 scale = new Vector3(ImageScaleX / W, 1.0f, ImageScaleZ / H);
        Vector2 uvScaleFactor = new Vector2(1.0f / W, 1.0f / H);
        var uvOffsetX = 1.0f / W;
        var uvOffsetY = 1.0f / H;

        // 如果W=2 H=3，會有6個Group
        // 口口  G4 G5
        // 口口  G2 G3
        // 口口  G0 G1
        // 把這6個Group的piece transfer到target
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                var uvOffsetFactor = new Vector2(x * uvOffsetX, y * uvOffsetY);
                GeneratePieceGroupAndTransferPiece(startPos + x * offsetX + y * offsetY, scale, uvScaleFactor, uvOffsetFactor, target);
            }
        }

        // Create Piece 相關
        target.reRangeAndMarkPieceInfo(W, H);
        target.InjectNeighborPieceInfo();
        target.setDebugInfoPieceSize(ImageScaleX, ImageScaleZ);
        target.recordPieceRealCenter(W, H);
        target.initBucket(W, H);
        target.souffleToPocket(W, H, puzzlePiecePocket);

        target.transform.position = helpCorner.position;
    }


    Quaternion rot = Quaternion.Euler(90, 180, 0);
    void GeneratePieceGroupAndTransferPiece(Vector3 pos, Vector3 scale, Vector2 uvScaleFactor, Vector2 uvOffsetFactor, PuzzlePieceGroup target)
    {
        var group = GameObject.Instantiate<PuzzlePieceGroup>(puzzlePieceGroup, pos, rot);
        group.transform.localScale = scale;
        group.setPieceTexture(Bootstrap.getInstance().GetMaterial().mainTexture);
        group.resetPieceUV(uvScaleFactor, uvOffsetFactor);
        group.transferPieceTo(target);
    }
}
