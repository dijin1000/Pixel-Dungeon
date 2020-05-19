using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Dagger = 0,
    Sword = 1
}

[Serializable]
public class WeaponObject
{
    public WeaponType type;
    public GameObject weaponObject;
}

public class WeaponItem : Item
{
    [SerializeField]
    private List<AnimatorOverrideController> weaponAnimations = new List<AnimatorOverrideController>();

    public List<WeaponObject> objectList = new List<WeaponObject>();
    private int index;

    public void Awake()
    {
        index = UnityEngine.Random.Range(0, objectList.Count); 
        GetComponentInChildren<SpriteRenderer>().sprite = objectList[index].weaponObject.GetComponent<SpriteRenderer>().sprite;
    }

    public override void Consume(ref PlayerData data)
    {
        data.Controller = weaponAnimations[(int)objectList[index].type];
        data.Weapon = objectList[index].weaponObject;

        base.Consume(ref data);
    }
}
