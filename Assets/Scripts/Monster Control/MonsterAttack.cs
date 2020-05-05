using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    public float strength = 4f; // damage on hit
    public float attackSpeed = 0.5f; // attacks per second
    public float hitChance = 0.65f; // change the monster hits player

    private Rigidbody2D rb;
   
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        
    }

}
