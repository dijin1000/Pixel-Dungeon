using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : IUnit
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
    private Animator anim;
    private Transform graphics;

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

    public void Subscribe(Action<InputAction.CallbackContext> actionToRegister, ControlType type)
    {
        p_Input.Subscribe(actionToRegister, type);
    }

    public void UnSubscribe(Action<InputAction.CallbackContext> actionToRegister, ControlType type)
    {
        p_Input.UnSubscribe(actionToRegister,type);
    }
    public void SubscribeHealthChange(Action<float> actionToRegister)
    {
        data.SubscribeHealthChange(actionToRegister);
    }
    public void SubscribeGraphicxsChange(Action<Sprite> actionToRegister)
    {
        OnSpriteChange += actionToRegister;
    }

    public override float Get_Health()
    {
        return data.Health;
    }

    public override bool Get_Death()
    {
        return data.Death;
    }

    protected override void Set_Health(float newHealth)
    {
        data.Health = newHealth;
    }

    protected override void Set_Death(bool dead)
    {
        data.Death = dead;
    }

    private void Awake()
    {
        p_Input = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        graphics = transform.Find("GFX");
        PlayerInstance = this;
        data = new PlayerData(null,100f,false,null,null);
        renderer_sprite = transform.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(predicate => predicate.name == "Player_Body"); ;
        Sprite_Player = renderer_sprite.sprite;
        SubscribeGraphicxsChange((Sprite sprite) => { renderer_sprite.sprite = sprite; });
        p_Input.Subscribe((InputAction.CallbackContext action) => { anim.SetTrigger("Attack"); }, ControlType.Attack);
        p_Input.Subscribe(
            (InputAction.CallbackContext action) => 
            { 
                float testing = Mathf.Ceil(action.ReadValue<Vector2>().x);
                Debug.Log(testing);
                anim.SetInteger("Movement",(int)testing);

                int equal = Mathf.Sign(graphics.rotation.y) == Mathf.Sign(testing) ? 0 : 1;
                Debug.Log(equal);
                graphics.Rotate(new Vector3(0,equal *180,0)); 

            }
            , ControlType.Movement);
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