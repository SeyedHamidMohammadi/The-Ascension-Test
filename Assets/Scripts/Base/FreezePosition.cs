using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezePosition : MonoBehaviour
{
    public bool x;
    public bool y;
    public bool z;

    void LateUpdate()
    {
        if (!x && !y && !z) return;
        
        Vector3 pos = transform.localPosition;
        
        if (x)
            pos.x = 0;
        
        if (y)
            pos.y = 0;
        
        if (z)
            pos.z = 0;

        transform.localPosition = pos;
    }
}
