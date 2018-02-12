using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePieceGroup : MonoBehaviour {

    const int pieceCount = 24;
    const int rowCount = 4;
    const int columnCount = 6;

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
