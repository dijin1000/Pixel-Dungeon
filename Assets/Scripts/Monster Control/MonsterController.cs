using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterController : IUnit
{
    private bool death = false;
    private float health = 100f;

    [SerializeField]
    private float minmaxHealth = 80f;
    [SerializeField]
    private float maxmaxHealth = 100f;
    private float maxHealth;

    private float MaxHealth
    {
        get
        {
            return maxHealth * GlobalManager.GlobalInstance.MonsterHealhtPercentage + GlobalManager.GlobalInstance.MonsterHealthFlat;
        }
    }

    private Action<float> onHealthChange;
    private GameObject UICanvas;
    private Slider healthBar;

    protected override void Awake()
    {
        maxHealth = UnityEngine.Random.Range(minmaxHealth,maxmaxHealth);
        base.Awake();
    }

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
        Set_Health(MaxHealth);
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
        health = newHealth;
        onHealthChange?.Invoke(health/maxHealth);
    }

    public void Subscribe(Action<float> registerAction)
    {
        onHealthChange += registerAction;
    }


}
