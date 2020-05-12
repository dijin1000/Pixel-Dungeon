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
