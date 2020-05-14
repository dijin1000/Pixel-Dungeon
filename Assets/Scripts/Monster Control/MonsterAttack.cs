using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    [SerializeField]
    private float minStrength = 2f;
    [SerializeField]
    private float maxStrength = 4f;
    private float strength; // damage on hit
    [SerializeField]
    private float hitsPerSecond = 2f; // attacks per second
    [SerializeField]
    private float hitChance = 0.65f; // change the monster hits player


    private Rigidbody2D rb;
   
    void Awake()
    {
        strength = UnityEngine.Random.Range(minStrength, maxStrength);
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if(PlayerIsCloseEnough()) {
            Invoke("Attack", hitsPerSecond);
        }
    }

    bool PlayerIsCloseEnough()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        Rigidbody2D playerRigidBody = playerObject.GetComponent<Rigidbody2D>();

        Vector2 currentPosition = rb.position;
        Vector2 playerPosition = playerRigidBody.position;

        double distanceFromPlayer = System.Math.Sqrt(System.Math.Pow((double)(currentPosition.x - playerPosition.x), (double)2) + System.Math.Pow((double)(currentPosition.y - playerPosition.y), (double)2));

        return (float)distanceFromPlayer <= 1.1f;
    }

    void Attack()
    {
        if(PlayerIsCloseEnough())
        {
            if(Random.Range(0f, 1f) < hitChance) {
                Debug.Log("Hit!");
            }
        }
    }
}
