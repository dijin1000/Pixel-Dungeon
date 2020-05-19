using UnityEngine;

public enum TypeItem
{
    Weapon,
    Potion,
    Money
}

public class Item : MonoBehaviour
{
    private float DmgIncrease = 0;
    [SerializeField]
    private float DmgIncreaseMin = 1f;
    [SerializeField]
    private float DmgIncreaseMax = 4f;

    private float SpeedIncrease = 0;
    [SerializeField]
    private float SpeedIncreeseMin = 1f;
    [SerializeField]
    private float SpeecIncreaseMax = 4f;

    private float Heal = 0;
    [SerializeField]
    private float HealMin = 1f;
    [SerializeField]
    private float HealMax = 4f;

    private float Value = 0;
    [SerializeField]
    private float ValueMin = 1f;
    [SerializeField]
    private float ValueMax = 25f;

    public float GetValue
    {
        get
        {
            return Value;
        }
    }


    [SerializeField]
    private TypeItem type;
    private bool isInit = false;
    public TypeItem TypeItem
    {
        get
        {
            return type;
        }
    }

    public void Init(TypeItem item = TypeItem.Money)
    {
        isInit = true;
        type = item;
        switch(type)
        {
            case TypeItem.Money:
                Value = UnityEngine.Random.Range(ValueMin, ValueMax);
                break;
            case TypeItem.Potion:
                int random = UnityEngine.Random.Range(0, 3);
                switch (random)
                {
                    case 0:
                        Heal = UnityEngine.Random.Range(HealMin, HealMax);
                    break;
                    case 1:
                        DmgIncrease = UnityEngine.Random.Range(DmgIncreaseMin, DmgIncreaseMax);
                        break;
                    case 2:
                        SpeedIncrease = UnityEngine.Random.Range(SpeedIncreeseMin, SpeecIncreaseMax);
                        break;
                }
                break;
            case TypeItem.Weapon:
                break;
        }
    }


    public virtual void Consume(ref PlayerData data) 
    {
        if (isInit) {
            if (Heal > 0)
                data.Health += Heal * GlobalManager.GlobalInstance.ItemStrengthPercentage / 100 * GlobalManager.GlobalInstance.PotionHealingPercentage / 100 + (Heal > 0 ? 1 : 0) * (GlobalManager.GlobalInstance.ItemStrengthFlat + GlobalManager.GlobalInstance.PotionHealingFlat);
            if (DmgIncrease > 0)
                data.Dmg = Mathf.Min(data.Dmg + DmgIncrease * GlobalManager.GlobalInstance.ItemStrengthPercentage / 100 * GlobalManager.GlobalInstance.PotionDmgPercentage / 100 + (DmgIncrease > 0 ? 1 : 0) * (GlobalManager.GlobalInstance.ItemStrengthFlat + GlobalManager.GlobalInstance.PotionDmgFlat), 10);
            if (SpeedIncrease > 0)
                data.Speed = Mathf.Min(SpeedIncrease * GlobalManager.GlobalInstance.ItemStrengthPercentage / 100 * GlobalManager.GlobalInstance.PotionSpeedPercentage / 100 + (SpeedIncrease > 0 ? 1 : 0) * (GlobalManager.GlobalInstance.ItemStrengthFlat + GlobalManager.GlobalInstance.PotionSpeedFlat), 40f);

            Consume();
        }
    }

    public void Consume()
    {
        StatisticsManager.StatisticsInstance.GetItem(Value * GlobalManager.GlobalInstance.MoneyIncreasePercentage/100 + GlobalManager.GlobalInstance.MoneyIncreaseFlat);
        Destroy(gameObject);
    }
}
