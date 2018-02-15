using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedPiece : MonoBehaviour, IPuzzleLayer
{
    public int layerIndex = Tool.NullIndex;
    public int GetLayerIndex()
    {
        return layerIndex;
    }
    public void SetLayerIndex(int value)
    {
        layerIndex = value;
    }
    public Transform GetTransform()
    {
        return transform;
    }
}
