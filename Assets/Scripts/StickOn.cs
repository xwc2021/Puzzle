using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickOn : MonoBehaviour {

    bool isStickOn = false;
    Vector3 memoryPos;
    public void BeginStickOn()
    {
        memoryPos = transform.position;
        isStickOn = true;
    }
	
	
	void Update () {
        if(isStickOn)
            transform.position = memoryPos;

    }
}
