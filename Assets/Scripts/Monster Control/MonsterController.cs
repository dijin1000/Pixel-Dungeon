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

    [SerializeField]
    private float minStrength = 2f;
    [SerializeField]
    private float maxStrength = 4f;
    private float strength; // damage on hit

    private Action<float> onHealthChange;
    private GameObject UICanvas;
    private Slider healthBar;
    private bool isAttacking;
    private MonsterMovement movement;
    private Vector3 target;
    private Vector3 lastTarget;
    private Transform player;
    [SerializeField]
    private Weapon weapon = null;
    [SerializeField]
    private Animator animator = null;
    private Vector3 lastPosition;

    protected override void Awake()
    {
        maxHealth = UnityEngine.Random.Range(minmaxHealth,maxmaxHealth) * GlobalManager.GlobalInstance.MonsterHealhtPercentage / 100 + GlobalManager.GlobalInstance.MonsterHealthFlat;
        strength = UnityEngine.Random.Range(minStrength, maxStrength) * GlobalManager.GlobalInstance.MonsterDamagePercentage / 100 + GlobalManager.GlobalInstance.MonsterDamageFlat;
        weapon.Subscribe(
            (float dmg) =>
            {
                return strength;
            }
            );
        movement = GetComponent<MonsterMovement>();

        base.Awake();
    }
    public void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        UICanvas = GetComponentInChildren<Canvas>().gameObject;
        healthBar = UICanvas.GetComponentInChildren<Slider>();
        Subscribe(
            (float newHealth) =>
            {
                healthBar.value = newHealth;
            }
            );
        Set_Health(maxHealth);
    }
    public void Update()
    {
        if (isAttacking == false) {
            //Check how far is player
            if ((player.position - transform.position).sqrMagnitude < 10f  )
            {
                if ((player.position - transform.position).sqrMagnitude < 0.1f)
                {
                    StartCoroutine(Attack());
                    //Attack
                }
                else
                {
                    target = player.position;
                }
            }
            else if ((target - transform.position).sqrMagnitude < 0.1f || transform.position == lastPosition)
            {
                target = transform.position + new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f), 0);
            }
        }
        if(target != lastTarget)
        {
            movement.targetTransform = target;
            lastTarget = target;
        }
        
    }


    private void LateUpdate()
    {
        lastPosition = transform.position;
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        weapon.gameObject.SetActive(true);
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        weapon.gameObject.SetActive(false);
        isAttacking = false;
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
