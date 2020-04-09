using UnityEngine;

public class PlayerMovement : MonoBehaviour
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
