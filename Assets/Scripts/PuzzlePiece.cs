using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour {

    MeshFilter mesh;
    Renderer render;
    // Use this for initialization
    void Awake () {
        render = GetComponent<Renderer>();
    }

    public void SetMaterial(Material m)
    {
        render.material = m;
    }
	
}
