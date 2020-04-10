using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    public float accelerationTime = 2f;
    public float moveSpeed = 5f;
    private float timeLeft;

    private Rigidbody2D rb;
    private Vector2 movement;
   
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        if(timeLeft <= 0)
        {
            movement = new Vector2(Random.Range(-0.7f, 0.7f), Random.Range(-0.7f, 0.7f));
            timeLeft += accelerationTime;
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(movement * moveSpeed);
    }


}
