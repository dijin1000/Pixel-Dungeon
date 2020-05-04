using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TypeItem
{
    Weapon,
    Potion,
    Money
}

public class Item : MonoBehaviour
{
    [SerializeField]
    private float DmgIncrease = 0;
    [SerializeField]
    private float SpeedIncrease =0;
    [SerializeField]
    private float Heal = 0;
    [SerializeField]
    private float Value = 0;

    [SerializeField]
    private TypeItem type;
    public TypeItem TypeItem
    {
        get
        {
            return type;
        }
    }

    public virtual void Consume(ref PlayerData data) 
    {
        if(Heal > 0)
            data.Health += Heal;
        if(DmgIncrease > 0)
            data.Dmg += DmgIncrease;
        if(SpeedIncrease > 0)
            data.Speed += SpeedIncrease;

        Consume();
    }

    public void Consume()
    {
        StatisticsManager.StatisticsInstance.GetItem(Value);
        Destroy(gameObject);
    }
}
