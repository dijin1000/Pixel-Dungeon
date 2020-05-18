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
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

}
