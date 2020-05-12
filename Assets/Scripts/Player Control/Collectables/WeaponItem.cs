using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : Item
{
    private enum WeaponType
    {
        Dagger = 0,
        Sword = 1
    }

    [SerializeField]
    private WeaponType Weapontype;

    [SerializeField]
    private List<AnimatorOverrideController> weaponAnimations;

    public GameObject weapon;
    public AnimatorOverrideController anim;

    public void Awake()
    {
        anim = weaponAnimations[(int)Weapontype];
        GetComponentInChildren<SpriteRenderer>().sprite = weapon.GetComponent<SpriteRenderer>().sprite;
    }

    public override void Consume(ref PlayerData data)
    {
        data.Controller = anim;
        data.Weapon = weapon;

        base.Consume(ref data);
    }
}
