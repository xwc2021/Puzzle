using System.Collections.Generic;
using UnityEngine;

// 負責深度排序的工作
public class LayerMananger
{
    float depth = 0;
    float span = 1;

    List<IPuzzleLayer> layers;

    private LayerMananger()
    {
        layers = new List<IPuzzleLayer>();
    }

    static LayerMananger instance;
    public static LayerMananger GetInstance()
    {
        if (instance == null)
            instance = new LayerMananger();

        return instance;
    }

    public void Update(IPuzzleLayer layer)
    {
        var i = layer.GetLayerIndex();
        layers.RemoveAt(i);

        //從下往上更新會比較快
        UpToFit(layer);

        RefreshLayerDepth();
    }

    public void Add(IPuzzleLayer layer)
    {
        var startIndex = layers.Count - 1;

        if (startIndex >= 0)
            DownToFit(layer, startIndex);
        else
        {
            layer.SetLayerIndex(0);
            layers.Add(layer);
        }

        RefreshLayerDepth();
    }

    void UpToFit(IPuzzleLayer layer)
    {
        for (var i = 0; i < layers.Count; ++i)
        {
            var L = layers[i];
            if (layer.GetPiecesCount() > L.GetPiecesCount())
            {
                layers.Insert(i, layer);
                return;
            }
        }

        layers.Insert(layers.Count, layer);
    }

    void DownToFit(IPuzzleLayer layer, int startIndex)
    {
        for (var i = startIndex; i >= 0; --i)
        {
            var L = layers[i];
            if (layer.GetPiecesCount() <= L.GetPiecesCount())
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

    public void Remove(IPuzzleLayer layer)
    {
        var i = layer.GetLayerIndex();
        layers.RemoveAt(i);
        layer.SetLayerIndex(PuzzleBucket.NullIndex);

        RefreshLayerDepth();
    }

    //這裡還可以優化：不用全部更新
    public void RefreshLayerDepth()
    {
        //Debug.Log("Layer count=" + layers.Count);
        var nowY = 0.0f;
        for (var i = 0; i < layers.Count; ++i)
        {
            var layer = layers[i];
            layer.SetLayerIndex(i);
            var t = layer.GetTransform();
            var pos = t.localPosition;
            t.localPosition = new Vector3(pos.x, nowY, pos.z);

            nowY += span;
        }
    }

    public void Merge(HashSet<IPuzzleLayer> set, PuzzlePieceGroup group)
    {
        //找出含有最多piece的Layer，把所有piece都給它
        var layers = new List<IPuzzleLayer>(set);

        //print("before sort");
        //foreach (var e in layers)
        //    print(e.GetLayerIndex());

        //從depth小排到大
        layers.Sort((a, b) =>
        {
            return a.GetLayerIndex() - b.GetLayerIndex();
        });

        //print("after sort");
        //foreach (var e in layers)
        //    print(e.GetLayerIndex());

        var theChosenOne = layers[0];
        var layerManager = LayerMananger.GetInstance();

        //全部都是piece
        if (theChosenOne.GetPiecesCount() == 1)
        {
            var p = theChosenOne as PuzzlePiece;

            //建立connectedSet，並把其他piece都加進來
            var cs = group.CreateConnectedSet(p);
            for (var i = layers.Count - 1; i >= 0; --i) //從最上層開始
            {
                var L = layers[i];
                cs.Add(L as PuzzlePiece);
                layerManager.Remove(L);
            }

            layerManager.Add(cs);
            return;
        }

        var nowCS = theChosenOne as ConnectedSet;
        //把其他Layer裡的piece加到擁有最多piece的那個Layer
        for (var i = layers.Count - 1; i >= 1; --i) //從最上層開始
        {
            var L = layers[i];

            if (L.GetPiecesCount() == 1)
            {
                nowCS.Add(L as PuzzlePiece);
                layerManager.Remove(L);
            }
            else
            {
                var cs = L as ConnectedSet;
                nowCS.Add(cs);
                layerManager.Remove(L);
                Object.Destroy(cs.gameObject);//刪除connectedSet
            }
        }
        layerManager.Update(theChosenOne);
    }
}