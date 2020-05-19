using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Active());
    }

    public bool IsHit = false;
    private IEnumerator Active()
    {
        while (true)
        {
            yield return null;
            yield return null;
            IsHit = true;
            for (int i = 0; i < 5; i++)
                yield return null;
            IsHit = false;
        }
    }
}
