using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterController : IUnit
{
    private bool death = false;
    private float health = 100f;

    private Action<float> onHealthChange;
    private GameObject UICanvas;
    private Slider healthBar;

    public void Start()
    {
        UICanvas = GetComponentInChildren<Canvas>().gameObject;
        healthBar = UICanvas.GetComponentInChildren<Slider>();
        Subscribe(
            (float newHealth) =>
            {
                healthBar.value = newHealth;
            }
            );
        Set_Health(100);
    }

    public override bool Get_Death()
    {
        return death;
    }

    public override float Get_Health()
    {
        return health;
    }

    protected override void Set_Death(bool dead)
    {
        death = dead;
    }

    protected override void Set_Health(float newHealth)
    {
        Debug.Log("Setting Health");
        health = newHealth;
        onHealthChange?.Invoke(health);
    }

    public void Subscribe(Action<float> registerAction)
    {
        onHealthChange += registerAction;
    }
}
