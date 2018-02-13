using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiecePocket : MonoBehaviour {

    [SerializeField]
    Transform border;

    public float GetBorder() { return border.localPosition.x; }

    [SerializeField]
    StickOn stickOn;

    float scale = 0.35f;
    Vector3 nowPos = Vector3.zero;
    const float span = 1.5f;
    Vector3 spanV3 = new Vector3(0,0, span);
    public void AddToPocket(int index,PuzzlePiece p)
    {
        if (pieceList.Contains(p))
            return;

        //attach & scale
        var pTransform = p.transform;
        pTransform.parent=transform;
        p.SetScaleInPocket(new Vector3(scale, scale, scale));

        pieceList.Insert(index,p);
        p.SetPocket(this);
        p.SetInPucket(true);

        //print(pieceList.Count);
        RefreshPocket();
    }

    public void SwapInPocket(int a, int b)
    {
        var target = pieceList[b];
        pieceList.Remove(target);
        pieceList.Insert(a, target);
        RefreshPocket();
    }

    public int GetInsertIndex(float y)
    {
        var yIndex =Tool.GetIndexOfCell(y, span);
        yIndex = Mathf.Clamp(yIndex, 0, pieceList.Count - 1);
        return yIndex;
    }

    public void RemoveFromPocket(PuzzlePiece p)
    {
        if (!pieceList.Contains(p))
            return;

        pieceList.Remove(p);
        p.SetInPucket(false);
        p.ResetScale();
        //print(pieceList.Count);
        RefreshPocket();
    }

    public void RefreshPocket()
    {
        Vector3 nowPos = Vector3.zero;
        for (var i = 0; i < pieceList.Count; ++i)
        {
            nowPos += spanV3;
            var p = pieceList[i];
            p.transform.localPosition = nowPos;
            p.SetInPucketIndex(i);
        }
            
    }

    List<PuzzlePiece> pieceList;
    private void Awake()
    {
        pieceList = new List<PuzzlePiece>();
    }

    public void SetPosition()
    {
        var offset = -1;
        var pos = transform.position;
        pos.Set(ScreenAdapter.GetHalfScreenWidth()+ offset, pos.y, pos.z);
        transform.position = pos;

        stickOn.BeginStickOn();
    }
}
