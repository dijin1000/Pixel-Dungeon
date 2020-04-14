using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private float base_dmg = 5f;
    private Func<float,float> modifiers; 

    private void Start()
    {
        modifiers += (float base_dmg) => { return base_dmg; };
    }

    public float Get_Dmg
    {
        get
        {
            return modifiers(base_dmg);
        }
    }

    public void Subscribe(Func<float,float> registeredAction)
    {
        modifiers += registeredAction;
    }
    public void OnTriggerEnter2D(Collider2D collider)
    {

    }
}
