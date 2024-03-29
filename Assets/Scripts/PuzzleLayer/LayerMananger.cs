using System.Collections.Generic;
using UnityEngine;

// 負責深度排序的工作
public class LayerMananger
{
    static LayerMananger instance;
    public static LayerMananger GetInstance()
    {
        if (instance == null)
            instance = new LayerMananger();

        return instance;
    }

    float layerDepthOffset = 1;

    List<IPuzzleLayer> layers;
    private LayerMananger()
    {
        layers = new List<IPuzzleLayer>();
    }

    public void add(IPuzzleLayer layer)
    {
        if (layers.Count == 0)
        {
            layer.SetLayerIndex(0);
            layers.Add(layer);
        }
        else
            upToDownInsert(layer);

        refreshLayerDepth();
    }

    public void remove(IPuzzleLayer layer)
    {
        var i = layer.GetLayerIndex();
        layers.RemoveAt(i);
        layer.SetLayerIndex(Tool.NullIndex);

        refreshLayerDepth();
    }

    public void update(IPuzzleLayer layer, bool downToUp = true)
    {
        var i = layer.GetLayerIndex();
        layers.RemoveAt(i);

        // 從下往上插入會比較快
        if (downToUp)
            downToUpInsert(layer);
        else
            upToDownInsert(layer);

        refreshLayerDepth();
    }

    public void moveToTop(IPuzzleLayer layer)
    {
        update(layer, false);
    }

    void downToUpInsert(IPuzzleLayer layer)
    {
        for (var i = 0; i < layers.Count; ++i)
        {
            var L = layers[i];
            if (layer.GetPiecesCount() > L.GetPiecesCount()) // 比這層大，就插入
            {
                layers.Insert(i, layer);
                return;
            }
        }

        layers.Insert(layers.Count, layer);
    }

    void upToDownInsert(IPuzzleLayer layer)
    {
        var startIndex = layers.Count - 1;
        for (var i = startIndex; i >= 0; --i)
        {
            var L = layers[i];
            if (layer.GetPiecesCount() <= L.GetPiecesCount()) // 比這層小，就插入
            {
                var insetIndex = i + 1;
                layers.Insert(insetIndex, layer);
                return;
            }
        }

        //比所有的都大
        var head = 0;
        layers.Insert(head, layer);
    }

    //這裡還可以優化：不用全部更新
    public void refreshLayerDepth()
    {
        //Debug.Log("Layer count=" + layers.Count);
        float depth_start = 0.0f;
        for (var i = 0; i < layers.Count; ++i)
        {
            var layer = layers[i];
            layer.SetLayerIndex(i);
            var t = layer.GetTransform();
            var pos = t.localPosition;
            t.localPosition = new Vector3(pos.x, depth_start, pos.z);

            depth_start += layerDepthOffset;
        }
    }

    public void merge(HashSet<IPuzzleLayer> set, PuzzlePieceGroup group)
    {
        var layerManager = LayerMananger.GetInstance();
        var layers = new List<IPuzzleLayer>(set);

        // print("before sort");
        // foreach (var e in layers)
        //     print(e.GetLayerIndex());

        // 從depth小排到大
        layers.Sort((a, b) =>
        {
            return a.GetLayerIndex() - b.GetLayerIndex();
        });

        // print("after sort");
        // foreach (var e in layers)
        //     print(e.GetLayerIndex());

        // 取出含有最多piece的Layer，把所有piece都給它
        var theChosenOne = layers[0];

        // 全部set都只有1塊piece
        if (theChosenOne.GetPiecesCount() == 1)
        {
            var p = theChosenOne as PuzzlePiece;

            // 建立connectedSet，全部piece都加進來
            var cs = group.createConnectedSet(p);
            for (var i = layers.Count - 1; i >= 0; --i) //從最上層開始
            {
                var L = layers[i];
                cs.add(L as PuzzlePiece);
                layerManager.remove(L);
            }

            layerManager.add(cs);
            return;
        }

        var nowCS = theChosenOne as ConnectedSet;
        // 把其他piece都加進來
        for (var i = layers.Count - 1; i >= 1; --i) //從最上層開始
        {
            var L = layers[i];

            if (L.GetPiecesCount() == 1)
            {
                nowCS.add(L as PuzzlePiece);
                layerManager.remove(L);
            }
            else
            {
                var cs = L as ConnectedSet;
                nowCS.add(cs);
                layerManager.remove(L);
                Object.Destroy(cs.gameObject);//刪除connectedSet
            }
        }
        layerManager.update(theChosenOne);
    }
}