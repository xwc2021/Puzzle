using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiecePocket : MonoBehaviour {

    float scale = 0.35f;
    Vector3 nowPos = Vector3.zero;
    Vector3 span = new Vector3(0,0,1.5f);
    public void AddToPocket(PuzzlePiece p)
    {
        nowPos += span;

        var pTransform = p.transform;
        pTransform.parent=transform;
        pTransform.localPosition = nowPos;
        pTransform.localScale = new Vector3(scale, scale, scale);
    }
}
