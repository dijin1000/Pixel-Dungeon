using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ControlType
{
    Enter,
    Attack,
    Movement,
}

public class PlayerInput : MonoBehaviour
{
    public float moveSpeed = 5f;
    private InputMaster controls;
    private Rigidbody2D rb;
    private Vector2 movement;
   
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new InputMaster();
        controls.Player.Movement.performed += ctx => movement = ctx.ReadValue<Vector2>();
        //controls.Player.PickUp.performed +=;
        //controls.Player.Attack.performed +=;
    }

    public void Subscribe(Action<InputAction.CallbackContext> action, ControlType type)
    {
        switch (type)
        {
            case ControlType.Enter:
                controls.Player.Enter.performed += action;
                break;
            case ControlType.Attack:
                controls.Player.Attack.performed += action;
                break;
            case ControlType.Movement:
                controls.Player.Movement.performed += action;
                break;
        }
    }

    public void UnSubscribe(Action<InputAction.CallbackContext> action, ControlType type)
    {
        switch (type)
        {
            case ControlType.Enter:
                controls.Player.Enter.performed -= action;
                break;
            case ControlType.Attack:
                controls.Player.Attack.performed -= action;
                break;
        }
    }


    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

}
