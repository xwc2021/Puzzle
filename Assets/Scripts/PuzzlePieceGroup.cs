using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePieceGroup : MonoBehaviour {

    public void Give(PuzzlePieceGroup target)
    {
        var targetTransform =target.transform;

        var pieces = GetComponentsInChildren<Transform>();
        for (var i = 0; i < pieces.Length; i++)
        {
            var p = pieces[i];

            //排除自己
            if(p!=transform)
                p.parent = targetTransform;
        }

        Destroy(gameObject);
    }
}
