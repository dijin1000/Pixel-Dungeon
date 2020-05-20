using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    public bool IsHit
    {
        get
        {
            return t % 12 == 0 || t % 11 == 0;
        }
    }
    private int t = 0;
    // Start is called before the first frame update
    void Update()
    {
        t++;
    }
}
