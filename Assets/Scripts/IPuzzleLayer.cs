using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPuzzleLayer
{
    void BeforeMoving();
    void AfterMoving();
    int GetLayerIndex();
    void SetLayerIndex(int value);
    Transform GetTransform();
}
