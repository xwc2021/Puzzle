using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool  {

    //找出位在那個小格
    public static int GetIndexOfCell(float x, float cell)
    {
        return (int)((x - (x % cell)) / cell);
    }
}
