using UnityEngine;
using System.Collections.Generic;

public class MonsterMovement : MonoBehaviour
{
    [SerializeField]
    private float minSpeed = 2f;
    [SerializeField]
    private float maxSpeed = 4f;
    private float speed;

    private Rigidbody2D rb;
    private Vector2 movement = Vector2.zero;
    public Vector3 targetTransform;


    void Awake()
    {
        speed = UnityEngine.Random.Range(minSpeed, maxSpeed) * GlobalManager.GlobalInstance.MonsterSpeedPercentage / 100 + GlobalManager.GlobalInstance.MonsterSpeedFlat;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        movement = (targetTransform - transform.position).normalized;
    }

    void FixedUpdate()
    {
        float testing = Mathf.Ceil(movement.x);
        if (testing != 0)
        {
            int equal = Mathf.Sign(transform.rotation.y) == Mathf.Sign(testing) ? 0 : 1;
            transform.rotation = Quaternion.Euler(0, Mathf.Sign(testing) * 90 - 90, 0);
        }
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

}
