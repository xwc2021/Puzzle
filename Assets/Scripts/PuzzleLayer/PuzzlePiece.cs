using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 單塊拚圖
public class PuzzlePiece : MonoBehaviour, IPuzzleLayer
{
    public int GetPiecesCount()
    {
        return 1;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    Transform GetParentTransform()
    {
        //不管現在的parent是Pocket、Group、ConnectedSet都沒差(反正它們軸向都一樣)
        return transform.parent;
    }

    /* Debug相關 */
    public float hWidth;
    public float hHeight;
    public void ResetSize(float hWidth, float hHeight)
    {
        this.hWidth = hWidth;
        this.hHeight = hHeight;
    }

    /* 顯示相關 */
    public void ResetUV(Vector2 uvScaleFactor, Vector2 uvOffsetFactor)
    {
        var mesh = GetComponent<MeshFilter>().mesh;
        var uvs = mesh.uv;
        for (var i = 0; i < uvs.Length; i++)
        {
            var uv = uvs[i];//這已經是複本了!!
            uvs[i].Set(uv.x * uvScaleFactor.x + uvOffsetFactor.x, uv.y * uvScaleFactor.y + uvOffsetFactor.y);
        }
        mesh.uv = uvs;
    }

    public void SetMainTextrue(Texture tex)
    {
        var render = GetComponent<Renderer>();
        render.material.mainTexture = tex;
    }

    /* scale相關 */
    Vector3 oldScale;
    public void memoryScale() { oldScale = transform.localScale; }
    public void recoverScale()
    {
        transform.localScale = oldScale;
    }

    public void setScaleInPocket(Vector3 scale)
    {
        transform.localScale = scale;
    }

    /* Pocket相關 */
    public int nowIndexInPocket = Tool.NullIndex;
    public bool inPocket = false;

    /* 索引相關 */
    public int bucketIndex = Tool.NullIndex;
    public int xIndexInFull;
    public int yIndexInFull;
    public int index1DInFull;
    public int layerIndex = Tool.NullIndex;
    public int GetLayerIndex()
    {
        return layerIndex;
    }
    public void SetLayerIndex(int value)
    {
        layerIndex = value;
    }

    /* 鄰接相關 */
    public Vector2[] NeighborOffset;

    bool IsMyNeighbor(int x, int y, PuzzlePiece p)
    {
        var b1 = p.xIndexInFull == (xIndexInFull + x);
        var b2 = p.yIndexInFull == (yIndexInFull + y);
        return b1 && b2;
    }

    /* Drag相關 */

    static bool isMouseDown = false;
    static Transform MovingTarget;

    Vector3 oldLocalPos;
    Vector3 beginDragPos;
    bool onMoving = false;
    public ConnectedSet connectedSet; // 已經和別的piece相連成connectedSet，就會指向該塊connectedSet

    //點選後移動
    void OnMouseDown()
    {
        isMouseDown = true;
        startMoving();
    }
    void OnMouseDrag() { movingPiece(); }
    void OnMouseUp()
    {
        isMouseDown = false;
        stopMoving();
    }

    // 改善從口袋滑出拼圖的手感
    // 沒點中拼圖，但滑過拼圖，還是可以接動
    void OnMouseOver()
    {
        if (!isMouseDown)
            return;

        // 已經在moving其他的拼圖
        if (PuzzlePiece.MovingTarget)
            return;

        startMoving();
    }
    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position + hHeight * Vector3.up, Color.yellow);
        Debug.DrawLine(transform.position, transform.position + hWidth * Vector3.left, Color.yellow);

        isMouseDown = Input.GetMouseButton(0);

        if (onMoving)
        {
            if (isMouseDown)
                movingPiece();
            else
                stopMoving();
        }
    }

    void startMoving()
    {
        BeforeMoving();

        MovingTarget = (connectedSet == null) ? transform : connectedSet.transform;
        ConnectedSet.pieceForAlign = (connectedSet == null) ? null : this;

        // 記下位置
        oldLocalPos = MovingTarget.localPosition;
        beginDragPos = Input.mousePosition;

        onMoving = true;
    }

    public void BeforeMoving()
    {
        if (connectedSet != null)
            connectedSet.BeforeMoving();
        else
        {
            // 從Bucket裡清除
            PuzzlePieceGroup.Instance.removeFromBucket(this);

            // 放到最上面，並從Layer移除
            if (layerIndex != Tool.NullIndex)
            {
                var instance = LayerMananger.GetInstance();
                instance.moveToTop(this);
                instance.remove(this);
            }
        }
    }

    Vector3 screenVectorToWorld(Vector3 v)
    {
        // 1 Screen.height = 1 ScreenAdapter.UnitSquareSize
        v = v / Screen.height;
        v = v * ScreenAdapter.UnitSquareSize;
        return v;
    }

    void movingPiece()
    {
        //https://docs.unity3d.com/ScriptReference/Input-mousePosition.html
        //這裡是Sreen的delta
        var delta = (Input.mousePosition - beginDragPos);
        delta = screenVectorToWorld(delta);

        var localDelta = GetParentTransform().InverseTransformVector(delta);
        MovingTarget.localPosition = oldLocalPos + localDelta;

        var pocket = PuzzlePiecePocket.Instance;
        var nowX = transform.position.x;
        if (nowX < pocket.GetBorder()) // group區
        {
            if (inPocket)
                pocket.removeFromPocket(this);
        }
        else // Pocket區
        {
            // 已經拼好的，就不能放回口袋
            if (connectedSet != null)
                return;

            //轉換回Pocket的local space
            var pos = transform.position;
            pos = PuzzlePiecePocket.Instance.transform.InverseTransformPoint(pos);
            var localZ = pos.z;

            var nowIndex = pocket.getInsertIndex(localZ, inPocket);
            if (!inPocket) // 不在口袋裡，就加進去
                pocket.addToPocket(nowIndex, this, false);
            else
            {
                if (nowIndexInPocket != nowIndex)
                    pocket.swapInPocket(nowIndexInPocket, nowIndex);//交換
            }
        }
    }

    void stopMoving()
    {
        onMoving = false;
        PuzzlePiece.MovingTarget = null;
        if (inPocket) // 口袋區
        {
            var pocket = PuzzlePiecePocket.Instance;
            transform.parent = pocket.transform;
            pocket.refreshPocket();
            PuzzlePieceGroup.Instance.removeFromBucket(this);
        }
        else
        {
            if (connectedSet == null)
                transform.parent = PuzzlePieceGroup.Instance.transform;

            AfterMoving();
        }
    }

    public void AfterMoving()
    {
        if (connectedSet != null)
        {
            connectedSet.AfterMoving();
        }
        else
        {
            // 取得所在Cell
            int x, y;
            var group = PuzzlePieceGroup.Instance;
            group.getAlignCell(transform.localPosition, out x, out y);

            // (1)pos重新對齊Cell
            group.snapPieceToCell(this, x, y);

            // (2)更新位在那一個Bucket
            group.injectToBucket(this, x, y);

            // (3)設為Layer
            if (GetLayerIndex() == Tool.NullIndex)
                LayerMananger.GetInstance().add(this);

            // (4)找出可以相連的Layer
            findConnectLayerAndMerge(x, y);
        }
    }

    /* 合併相關 */
    void findConnectLayerAndMerge(int x, int y)
    {
        var set = new HashSet<IPuzzleLayer>();
        findConnectLayer(x, y, set);

        //沒有找到任何相鄰Layer
        if (set.Count == 0)
        {
            LayerMananger.GetInstance().refreshLayerDepth();
            return;
        }

        //Merge Layer
        set.Add(this);//把自己也加進去
        LayerMananger.GetInstance().merge(set, PuzzlePieceGroup.Instance);
    }

    public void findConnectLayer(int x, int y, HashSet<IPuzzleLayer> set)
    {
        for (var i = 0; i < NeighborOffset.Length; ++i)
        {
            var offset = NeighborOffset[i];
            var offsetX = (int)offset.x;
            var offsetY = (int)offset.y;
            var Pieces = PuzzlePieceGroup.Instance.getBucketPieces(x + offsetX, y + offsetY);
            if (Pieces == null)
                continue;

            for (var k = 0; k < Pieces.Length; ++k)
            {
                var p = Pieces[k];
                if (IsMyNeighbor(offsetX, offsetY, p))//找到相鄰的了
                {
                    //已經相接在一起，就跳過
                    if (p.connectedSet == connectedSet && connectedSet != null)
                        continue;

                    var findOne = (p.connectedSet == null) ? p as IPuzzleLayer : p.connectedSet as IPuzzleLayer;

                    if (!set.Contains(findOne))//有可能findOne已經在set裡了
                        set.Add(findOne);

                    break;
                }
            }
        }
    }
}