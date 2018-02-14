using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiecePocket : MonoBehaviour {

    [SerializeField]
    Transform border;

    public float GetBorder() { return border.position.x; }

    [SerializeField]
    StickOn stickOn;

    float scale = 0.35f;
    Vector3 nowPos = Vector3.zero;
    const float span = 1.5f;
    Vector3 spanV3 = new Vector3(0,0, span);
    public void AddToPocket(int index,PuzzlePiece p,bool attach)
    {
        if (pieceList.Contains(p))
            return;

        //attach
        if (attach)
        {
            var pTransform = p.transform;
            pTransform.parent = transform;
        }
        
        pieceList.Insert(index,p);
        p.SetPocket(this);
        p.SetInPucket(true);
        p.SetScaleInPocket(new Vector3(scale, scale, scale));

        RefreshPocket();
    }

    //交換
    public void SwapInPocket(int a, int b)
    {
        var target = pieceList[b];
        pieceList.Remove(target);
        pieceList.Insert(a, target);

        RefreshPocket();
    }

    public int GetInsertIndex(float y,bool inPocket)
    {
        //拼圖  y    y-hSpan (y向下為+)
        //      一
        //       I    一
        //口    一     I span
        //       I    一
        //口    一     I span
        var hSpan = 0.5f * span;
        var yIndex =Tool.GetIndexOfCell(y- hSpan, span);
        //如果本來不在Pocket裡，可以放到尾巴，所以會多1個位置可放
        yIndex = Mathf.Clamp(yIndex, 0, inPocket ? pieceList.Count - 1 : pieceList.Count);
        return yIndex;
    }

    public void RemoveFromPocket(PuzzlePiece p)
    {
        if (!pieceList.Contains(p))
            return;

        pieceList.Remove(p);
        p.SetInPucket(false);
        p.SetInPucketIndex(Tool.NullIndex);
        p.ResetScale();

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
