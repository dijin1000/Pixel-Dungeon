using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Signleton Pattern
    /// </summary>
    private static PlayerController playerInstance;
    public static PlayerController PlayerInstance
    {
        get
        {
            if (playerInstance == null)
                Debug.LogError("There is no " + PlayerInstance.GetType() + " set.");
            return playerInstance;
        }
        private set
        {
            if (playerInstance != null)
                Debug.LogError("Two instances of the " + PlayerInstance.GetType() + " are sethere is no DirectorAI set.");
            playerInstance = value;
        }
    }


    private PlayerData data;

    private Action<float> OnHealthChange;

    PlayerInput pInput;
    private void Awake()
    {
        data = new PlayerData();
        data.Health = 100;
        pInput = GetComponent<PlayerInput>();
        PlayerInstance = this;
    }

    public void SubscribeHealthChange(Action<float> actionToRegister )
    {
        OnHealthChange += actionToRegister;
    }

    public float GetHealth()
    {
        return data.Health;
    }

    public void GetDamage(float dmg)
    {
        if (!data.Death)
        { 
            data.Health -= dmg;
            OnHealthChange(data.Health);
            if (data.Health < 0)
            {
                data.Death = true;
                pInput.enabled = false;
            }
        }
    }

    public void PickUpItem(InventoryItem item)
    {
        data.Inventory.Add(item);
        //Do stuff with the item
        //IF ITEM == SWORD 
        //{
            //Weapon = item;
        //}
    }

    public bool HasItem(InventoryItem item, bool consumed)
    {
        if (data.Inventory.Any(predicate => predicate == item))
        {
            if (consumed)
                data.Inventory.Remove(item);
            return true;
        }
        else
        {
            return false;
        }
    }

}

[Serializable]
public struct PlayerData
{
    public PlayerData(float health = 100f, bool death = false, List<InventoryItem> inventory = null, InventoryItem weapon = null)
    {
        Health = health;
        Death = death;
        Inventory = inventory ?? new List<InventoryItem>();
        Weapon = weapon ?? new InventoryItem();
    }

    public float Health;
    public bool Death;

    public InventoryItem Weapon;

    public List<InventoryItem> Inventory;
}