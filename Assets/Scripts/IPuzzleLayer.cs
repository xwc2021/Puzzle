using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPuzzleLayer
{
    int GetLayerIndex();
    void SetLayerIndex(int value);
    Transform GetTransform();
}
