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
        data.Health += Heal;
        data.Dmg += DmgIncrease;
        data.Speed += SpeedIncrease;

        Consume();
    }

    public void Consume()
    {
        StatisticsManager.StatisticsInstance.GetItem(Value);
    }

}
