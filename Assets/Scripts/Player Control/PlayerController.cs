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
    [SerializeField]
    private GameObject weaponHolder = null;

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

        SceneTransistionManager.SceneInstance.TransitionToScene(typeOfScene.ExitGame);

        base.Death();
    }



    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
        base.Awake();
        p_Input = GetComponent<PlayerInput>();
        anim = GetComponent<Animator>();
        graphics = transform.Find("GFX");
        PlayerInstance = this;
        data = new PlayerData(null,null,null,null,null,100f,false,1,5,null,null);
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
                graphics.transform.rotation = Quaternion.Euler(0,Mathf.Sign(testing) * 90 - 90,0); 

            }
            , ControlType.Movement);

        //DontDestroyOnLoad(gameObject);

        data.SubscribeDmgChange((float addedDmg) => { this.w.Subscribe((float input) => { return input + addedDmg; }); });
        data.SubscribeSpeedChange((float addedSpeed) => { this.p_Input.moveSpeed += addedSpeed; });
        data.SubscribeControllerChange((AnimatorOverrideController controller) => { anim.runtimeAnimatorController = controller; });
        data.SubscribeWeaponChange((GameObject weapon) =>
        {
            if (weaponHolder.transform.childCount != 0)
            {
                foreach (Transform child in weaponHolder.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            GameObject.Instantiate(weapon, weaponHolder.transform);
        });
    }


    private void PickUpItem(Item item)
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

    public override void OnTriggerEnter2D(Collider2D col )
    { 
        if (col.gameObject.tag == "Item")
        {
            PickUpItem(col.gameObject.GetComponent<Item>());
        }
        base.OnTriggerEnter2D(col);
    }
}

[Serializable]
public struct PlayerData
{
    public PlayerData(Action<float> onHealthChange = null, Action<float> onDmgChange = null, Action<float> onSpeedChange = null, Action<GameObject> onWeaponChange = null, Action<AnimatorOverrideController> onControllerChange = null, float _health = 100f, bool death = false, float _dmg = 1f, float _speed = 5f, GameObject _weapon = null, AnimatorOverrideController _controller = null)
    {
        Death = death;
        maxHealth = _health;
        dmg = _dmg;
        health = _health;
        speed = _speed;
        weapon = _weapon;
        controller = _controller;
        OnHealthChange = onHealthChange ?? ((float newhealth) => { });
        OnDmgChange = onDmgChange ?? ((float newdmg) => { });
        OnSpeedChange = onSpeedChange ?? ((float newspeed) => { });
        OnWeaponChange = onWeaponChange ?? ((GameObject newweapon) => { });
        OnControllerChange = onControllerChange ?? ((AnimatorOverrideController controller) => { });

    }

    private GameObject weapon;
    private AnimatorOverrideController controller;

    private Action<GameObject> OnWeaponChange;
    private Action<AnimatorOverrideController> OnControllerChange;

    private Action<float> OnHealthChange;
    private Action<float> OnDmgChange;
    private Action<float> OnSpeedChange;
    private float health;
    public float maxHealth;
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
            return health;
        }
        set
        {
            health = Mathf.Min(value,maxHealth);
            OnHealthChange?.Invoke((health/maxHealth));
        }
    }
    public bool Death;

    public GameObject Weapon
    {
        get
        {
            return weapon;
        }
        set
        {
            weapon = value;
            OnWeaponChange?.Invoke(weapon);
        }
    }

    public AnimatorOverrideController Controller
    {
        get
        {
            return controller;
        }
        set
        {
            controller = value;
            OnControllerChange?.Invoke(controller);
        }
    }

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

    public void SubscribeWeaponChange(Action<GameObject> actionToRegister)
    {
        OnWeaponChange += actionToRegister;
    }

    public void SubscribeControllerChange(Action<AnimatorOverrideController> actionToRegister)
    {
        OnControllerChange += actionToRegister;
    }
}