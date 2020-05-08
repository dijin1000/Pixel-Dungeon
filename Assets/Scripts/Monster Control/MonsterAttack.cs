using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    public float strength = 4f; // damage on hit
    public float attackSpeed = 0.5f; // attacks per second
    public float hitChance = 0.65f; // change the monster hits player

    private Rigidbody2D rb;
   
    void Awake()
    {
        Debug.Log("Awake!");
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Debug.Log("Fixed update");
        if(PlayerIsCloseEnough()) {
            Attack();
        }
    }

    bool PlayerIsCloseEnough()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        Rigidbody2D playerRigidBody = playerObject.GetComponent<Rigidbody2D>();

        Vector2 currentPosition = rb.position;
        Vector2 playerPosition = playerRigidBody.position;

        double distanceFromPlayer = System.Math.Sqrt(System.Math.Pow((double)(currentPosition.x - playerPosition.x), (double)2) + System.Math.Pow((double)(currentPosition.y - playerPosition.y), (double)2));

        return (float)distanceFromPlayer <= 1f;
    }

    bool TimeToAttack()
    {
        // something to keep track of the hit speed
        // hit speed of 0.5 -> attack each 2 seconds
        // hit speed of 0.1 -> attack every 10 seconds

        return true;
    }

    void Attack()
    {
        Debug.Log("Attack!");
        // if(TimeToAttack())
        // {
        //     if(Random.Range(0f, 1f) < hitChance) {
        //         Debug.Log("Hit!");
        //     }
        // }
    }
}
