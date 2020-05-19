using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public delegate float test(ref float param);

    public float base_dmg = 1f;
    private test modifiers; 

    private void Start()
    {
        modifiers += (ref float base_dmg) => { return base_dmg; };
    }

    public float Get_Dmg
    {
        get
        {
            float x = base_dmg;
            return modifiers(ref x);
        }
        set
        {
            base_dmg = value;
        }
    }

    public void Subscribe(test registeredAction)
    {
        modifiers += registeredAction;
    }
    public void OnTriggerEnter2D(Collider2D collider)
    {

    }
}
