using System.Collections.Generic;
using UnityEngine;

// 畫面右方的口袋
public class PuzzlePiecePocket : MonoBehaviour
{
    public static PuzzlePiecePocket Instance;

    List<PuzzlePiece> pieceList;
    private void Awake()
    {
        pieceList = new List<PuzzlePiece>();
    }

    /* border右邊代表進入Pocket */
    [SerializeField]
    Transform border;
    public float GetBorder() { return border.position.x; }

    public void snapToRight()
    {
        var offset = -1;
        var pos = transform.position;
        pos.Set(ScreenAdapter.getHalfScreenWidth() + offset, pos.y, pos.z);
        transform.position = pos;
    }

    float scaleForInPocketPiece = 0.35f;
    const float span = 1.5f;
    Vector3 spanV3 = new Vector3(0, 0, span);
    public void addToPocket(int index, PuzzlePiece p, bool attach)
    {
        //attach
        if (attach)
        {
            var pTransform = p.transform;
            pTransform.parent = transform;
        }

        pieceList.Insert(index, p);
        p.inPocket = true;
        p.setScaleInPocket(new Vector3(scaleForInPocketPiece, scaleForInPocketPiece, scaleForInPocketPiece));

        refreshPocket();
    }

    public void removeFromPocket(PuzzlePiece p)
    {
        pieceList.Remove(p);
        p.inPocket = false;
        p.nowIndexInPocket = Tool.NullIndex;
        p.recoverScale();

        refreshPocket();
    }

    //交換
    public void swapInPocket(int a, int b)
    {
        var target = pieceList[b];
        pieceList.Remove(target);
        pieceList.Insert(a, target);

        refreshPocket();
    }

    public int getInsertIndex(float y, bool inPocket)
    {
        //拼圖  y    y-hSpan (y向下為+)
        //      一
        //       I    一
        //口    一     I (span)
        //       I    一
        //口    一     I (span)

        var hSpan = 0.5f * span;
        var yIndex = Tool.GetIndexOfCell(y - hSpan, span);
        return Mathf.Clamp(yIndex, 0, maxInsertIndex(inPocket)); ;
    }

    int maxInsertIndex(bool inPocket)
    {
        // 如果本來不在Pocket裡，可以放到尾巴，所以會多1個位置可放
        return inPocket ? pieceList.Count - 1 : pieceList.Count;
    }

    public void refreshPocket()
    {
        Vector3 topPos = Vector3.zero;
        for (var i = 0; i < pieceList.Count; ++i)
        {
            topPos += spanV3;
            var p = pieceList[i];
            p.transform.localPosition = topPos;
            p.nowIndexInPocket = i;
        }
    }
}