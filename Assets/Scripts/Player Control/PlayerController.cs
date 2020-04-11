using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private PlayerInput p_Input;
    private Sprite sprite_Player;
    private Action<Sprite> OnSpriteChange;
    private SpriteRenderer renderer_sprite;
    public Sprite Sprite_Player
    {
        get
        {
            return sprite_Player;
        }
        set
        {
            sprite_Player = value;
            OnSpriteChange?.Invoke(sprite_Player);
        }
    }

    public void Subscribe(Action<InputAction.CallbackContext> actionToRegister)
    {
        p_Input.Subscribe(actionToRegister);
    }

    public void UnSubscribe(Action<InputAction.CallbackContext> actionToRegister)
    {
        p_Input.UnSubscribe(actionToRegister);
    }
    public void SubscribeHealthChange(Action<float> actionToRegister)
    {
        data.SubscribeHealthChange(actionToRegister);
    }
    public void SubscribeGraphicxsChange(Action<Sprite> actionToRegister)
    {
        OnSpriteChange += actionToRegister;
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
            if (data.Health < 0)
            {
                data.Death = true;
                p_Input.enabled = false;
            }
        }
    }

    private void Awake()
    {
        p_Input = GetComponent<PlayerInput>();
        PlayerInstance = this;
        data = new PlayerData(null,100f,false,null,null);
        renderer_sprite = transform.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(predicate => predicate.name == "Player_Body"); ;
        Sprite_Player = renderer_sprite.sprite;
        SubscribeGraphicxsChange((Sprite sprite) => { renderer_sprite.sprite = sprite; });
        //DontDestroyOnLoad(gameObject);
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
    public PlayerData(Action<float> onHealthChange = null, float _health = 100f, bool death = false, List<InventoryItem> inventory = null, InventoryItem weapon = null)
    {
        Death = death;
        maxHealth = 100;
        Inventory = inventory ?? new List<InventoryItem>();
        Weapon = weapon ?? new InventoryItem();
        health = _health;
        OnHealthChange = onHealthChange ?? ((float newhealth) => { });
    }

    private Action<float> OnHealthChange;
    private float health;
    private float maxHealth;
    public float Health
    {
        get
        {
            return health/maxHealth;
        }
        set
        {
            health = value;
            OnHealthChange?.Invoke(Health);
        }
    }
    public bool Death;

    public InventoryItem Weapon;

    public List<InventoryItem> Inventory;

    internal void SubscribeHealthChange(Action<float> actionToRegister)
    {
        OnHealthChange += actionToRegister;
    }
}