using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeRotation : MonoBehaviour
{
    public bool x;
    public bool y;
    public bool z;

    void LateUpdate()
    {
        if (!x && !y && !z) return;
        
        Quaternion rot = transform.localRotation;
        
        if (x)
            rot.x = 0;
        
        if (y)
            rot.y = 0;
        
        if (z)
            rot.z = 0;

        transform.localRotation = rot;
    }
}
