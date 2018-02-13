using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    float hMin;
    public void ResetSize(float hMin)
    {
        this.hMin = hMin;
    }

    public void ResetUV (Vector2 uvScaleFactor, Vector2 uvOffsetFactor) {
        var mesh = GetComponent<MeshFilter>().mesh;
        var uvs =mesh.uv;
        for (var i = 0; i < uvs.Length; i++)
        {
            var uv = uvs[i];//這已經是複本了!!
            uvs[i].Set(uv.x* uvScaleFactor.x+ uvOffsetFactor.x, uv.y* uvScaleFactor.y+ uvOffsetFactor.y);
        }
        mesh.uv = uvs;
    }

    public void SetMaterial(Material m)
    {
        var render = GetComponent<Renderer>();
        render.material = m;
    }

    Vector3 oldScale;
    public void SwithcScale(Vector3 scale)
    {
        oldScale = transform.localScale;
        transform.localScale = scale;
    }

    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position + hMin * Vector3.up);
        Debug.DrawLine(transform.position, transform.position + hMin * Vector3.left);
    }
}
