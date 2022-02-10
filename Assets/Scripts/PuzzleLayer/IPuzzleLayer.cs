using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPuzzleLayer
{
    int GetPiecesCount();
    void BeforeMoving();
    void AfterMoving();
    int GetLayerIndex();
    void SetLayerIndex(int value);
    Transform GetTransform();
}
