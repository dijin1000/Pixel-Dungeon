using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IUnit : MonoBehaviour
{
    public bool test = true;

    public enum Unit
    {
        Player,
        Monster,
        Default
    }
    private Unit unit;
    protected Weapon w;

    public abstract float Get_Health();
    public abstract bool Get_Death();
    protected abstract void Set_Health(float newHealth);
    protected abstract void Set_Death(bool dead);

    public void GetDamage(float dmg)
    {
        if (!Get_Death())
        {
            if(!test)
                StatisticsManager.StatisticsInstance.SetDamage(unit,dmg);
            Set_Health(Get_Health() - dmg);
            if (Get_Health() < 0)
            {              
                Death();
            }
        }
    }

    protected virtual void Awake()
    {
        unit = (tag == "Player") ? Unit.Player : ((tag == "Monster") ? Unit.Monster : Unit.Default);
        w = GetComponentInChildren<Weapon>();
    }

    public virtual void Death()
    {
        Set_Death(true);
        if (!test)
            StatisticsManager.StatisticsInstance.SetDeath(unit);

        Destroy(gameObject);
    }

    public virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null && col.transform.parent != null)
        {
            GameObject scriptHolder = col.transform.parent.gameObject;
            if (scriptHolder != null && scriptHolder.tag == "Weapon")
            {
                Weapon t = scriptHolder.GetComponent<Weapon>();
                if (t != null)
                    GetDamage(t.Get_Dmg);
            }
        }
    }
}
