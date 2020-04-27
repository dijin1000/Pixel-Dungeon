using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : IUnit
{
    private PlayerData data;
    private PlayerInput p_Input;
    private Sprite sprite_Player;
    private Action<Sprite> OnSpriteChange;
    private SpriteRenderer renderer_sprite;
    private Animator anim;
    private Transform graphics;

    #region Getters/Setters
    /// <summary>
    /// Signleton Pattern
    /// </summary>
    private static PlayerController playerInstance;
    public static PlayerController PlayerInstance
    {
        get
        {
            if (playerInstance == null)
                Debug.LogError("There is no " + playerInstance.GetType() + " set.");
            return playerInstance;
        }
        private set
        {
            if (playerInstance != null)
                Debug.LogError("Two instances of the " + playerInstance.GetType() + " are sethere is no DirectorAI set.");
            playerInstance = value;
        }
    }
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
    #endregion

    #region Subscribe Events
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
    #endregion

    #region Getter/Setters Functions
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
    #endregion

    public override void Death()
    {
        //Send Statistics

        //SceneTransitionManager.Transit();

        base.Death();
    }



    protected override void Awake()
    {
        base.Awake();
        p_Input = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        graphics = transform.Find("GFX");
        PlayerInstance = this;
        data = new PlayerData(null,null,null,100f,false);
        renderer_sprite = transform.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(predicate => predicate.name == "Player_Body"); ;
        Sprite_Player = renderer_sprite.sprite;
        SubscribeGraphicxsChange((Sprite sprite) => { renderer_sprite.sprite = sprite; });
        p_Input.Subscribe((InputAction.CallbackContext action) => { anim.SetTrigger("Attack"); }, ControlType.Attack);
        p_Input.Subscribe(
            (InputAction.CallbackContext action) => 
            { 
                float testing = Mathf.Ceil(action.ReadValue<Vector2>().x);
                anim.SetInteger("Movement",(int)testing);

                int equal = Mathf.Sign(graphics.rotation.y) == Mathf.Sign(testing) ? 0 : 1;
                graphics.Rotate(new Vector3(0,equal *180,0)); 

            }
            , ControlType.Movement);
        //DontDestroyOnLoad(gameObject);

        data.SubscribeDmgChange((float addedDmg) => { this.w.Subscribe((float input) => { return input + addedDmg; }); });
        data.SubscribeSpeedChange((float addedSpeed) => { this.p_Input.moveSpeed += addedSpeed; });
    }


    public void PickUpItem(Item item)
    {
        switch(item.TypeItem)
        {
            case TypeItem.Weapon:
                data.SubscribeDmgChange((float addedDmg) => { this.w.Subscribe((float input) => { return input + addedDmg; }); });
                goto case TypeItem.Potion;
            case TypeItem.Potion:
                item.Consume(ref data);      
                break;
            case TypeItem.Money:
                item.Consume();
                break;
        }
    }
}

[Serializable]
public struct PlayerData
{
    public PlayerData(Action<float> onHealthChange = null, Action<float> onDmgChange = null, Action<float> onSpeedChange = null, float _health = 100f, bool death = false, float _dmg = 1f, float _speed = 5f)
    {
        Death = death;
        maxHealth = _health;
        dmg = _dmg;
        health = _health;
        speed = _speed;
        OnHealthChange = onHealthChange ?? ((float newhealth) => { });
        OnDmgChange = onDmgChange ?? ((float newdmg) => { });
        OnSpeedChange = onSpeedChange ?? ((float newspeed) => { });
    }

    private Action<float> OnHealthChange;
    private Action<float> OnDmgChange;
    private Action<float> OnSpeedChange;
    private float health;
    private float maxHealth;
    private float dmg;
    private float speed;

    public float Dmg
    {
        get
        {
            return dmg;
        }
        set
        {
            float difference = value - dmg;
            dmg = value;

            OnDmgChange?.Invoke(difference);
        }
    }
    public float Speed
    {
        get
        {
            return speed;
        }
        set
        {
            float difference = value - speed;
            speed = value;

            OnSpeedChange?.Invoke(difference);
        }
    }
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


    internal void SubscribeHealthChange(Action<float> actionToRegister)
    {
        OnHealthChange += actionToRegister;
    }

    internal void SubscribeDmgChange(Action<float> actionToRegister)
    {
        OnDmgChange += actionToRegister;
    }
    internal void SubscribeSpeedChange(Action<float> actionToRegister)
    {
        OnSpeedChange += actionToRegister;
    }
}