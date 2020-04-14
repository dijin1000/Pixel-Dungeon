using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IUnit : MonoBehaviour
{
    public abstract float Get_Health();
    public abstract bool Get_Death();
    protected abstract void Set_Health(float newHealth);
    protected abstract void Set_Death(bool dead);

    public void GetDamage(float dmg)
    {
        if (!Get_Death())
        {
            Set_Health(Get_Health() - dmg);
            if (Get_Health() < 0)
            {
                Set_Death(true);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        GameObject scriptHolder = col.transform.parent.gameObject;
        if (scriptHolder.tag == "Weapon")
        {
            Weapon t = scriptHolder.GetComponent<Weapon>();
            if (t != null)
                GetDamage(t.Get_Dmg);
        }
    }
}
