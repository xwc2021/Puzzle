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
        p.SwithcScale(new Vector3(scale, scale, scale));
    }

    public void SetPosition()
    {
        var offset = -1;
        var pos = transform.position;
        pos.Set(ScreenAdapter.GetHalfScreenWidth()+ offset, pos.y, pos.z);
        transform.position = pos;
    }
}
