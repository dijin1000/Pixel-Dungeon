using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public float MonsterHealthFlat = 0;
    public float MonsterHealhtPercentage = 100f;

    public float MonsterDamageFlat = 0;
    public float MonsterDamagePercentage = 100f;

    public float MonsterSpeedFlat = 0;
    public float MonsterSpeedPercentage = 100f;

    public float ItemStrengthFlat = 0;
    public float ItemStrengthPercentage = 100f;

    public float PotionHealingFlat = 0;
    public float PotionHealingPercentage = 100f;

    public float PotionSpeedFlat = 0;
    public float PotionSpeedPercentage = 100f;

    public float PotionDmgFlat = 0;
    public float PotionDmgPercentage = 100f;

    public float MoneyIncreaseFlat = 0;
    public float MoneyIncreasePercentage = 100f;

    public float WeaponRarity = 0;

    public float SpikeDmg = 1f;
    /// <summary>
    /// Signleton Pattern
    /// </summary>
    private static GlobalManager globalInstance;
    public static GlobalManager GlobalInstance
    {
        get
        {
            if (globalInstance == null)
                Debug.LogError("There is no " + GlobalInstance.GetType() + " set.");
            return globalInstance;
        }
        private set
        {
            if (globalInstance != null)
                Debug.LogError("Two instances of the " + GlobalInstance.GetType() + " are set.");
            globalInstance = value;
        }
    }

    private void Awake()
    {
        GlobalInstance = this;
    }
}
