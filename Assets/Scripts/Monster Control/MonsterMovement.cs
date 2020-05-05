using UnityEngine;
using System.Collections.Generic;

public enum MovementTypes
{
    Sleep,
    Random,
    Horizontal,
    Vertical,
    ToPlayer
}

public class MonsterMovement : MonoBehaviour
{
    public float speed = 2f;
    public float maxRange = 4f;
    public float viewRange = 6f;
    public MovementTypes movementType = MovementTypes.Random;

    private Rigidbody2D rb;
    private float timeLeftForDirectionChange = 2f;
    private Vector2 movement = Vector2.zero;
    private Vector2 initialPosition;
   
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        initialPosition = rb.position;

        Debug.Log(initialPosition);

        // Cannot go directly to player on awake
        // need to spot the player first
        if(movementType == MovementTypes.ToPlayer)
		{
            movementType = MovementTypes.Random;
		}

        if (movementType == MovementTypes.Horizontal)
        {
            List<Vector2> possibleDirections = new List<Vector2> { Vector2.up, Vector2.down };
            movement = possibleDirections[Random.Range(0, possibleDirections.Count)];
		}

        if (movementType == MovementTypes.Vertical)
        {
            List<Vector2> possibleDirections = new List<Vector2> { Vector2.left, Vector2.right };
            movement = possibleDirections[Random.Range(0, possibleDirections.Count)];
        }

        if (movementType == MovementTypes.Random)
        {
            List<Vector2> possibleDirections = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
            movement = possibleDirections[Random.Range(0, possibleDirections.Count)];
        }
    }

    void FixedUpdate()
    {
        DetectPlayer();
        CheckMovementRangeBounds();
        DetermineDirection();
        DoMove();
    }

    void DetectPlayer()
	{
        // check if player is in visual range
        // if so set movement type to MovementTypes.ToPlayer
	}

    void CheckMovementRangeBounds()
	{
        if (movementType == MovementTypes.Sleep)
        {
            return;
        }

        if (movementType == MovementTypes.ToPlayer)
        {
            return;
        }

        Vector2 currentPosition = rb.position;

		double distanceFromOrigin = System.Math.Sqrt(System.Math.Pow((double)(currentPosition.x - initialPosition.x), (double)2) + System.Math.Pow((double)(currentPosition.y - initialPosition.y), (double)2));

		if ((float)distanceFromOrigin >= maxRange)
		{
			Debug.Log("Out of range");
			Debug.Log(distanceFromOrigin);

			timeLeftForDirectionChange = -1f;
		}
	}

    void DetermineDirection()
	{
        if(movementType == MovementTypes.Sleep)
		{
            return;
		}

        if (movementType == MovementTypes.ToPlayer)
        {
            return;
        }

        timeLeftForDirectionChange -= Time.deltaTime;

        if(timeLeftForDirectionChange < 0f)
		{
            timeLeftForDirectionChange = Random.Range(1f, 5f);

            if(movementType == MovementTypes.Horizontal)
			{
                movement = movement == Vector2.up
                    ? Vector2.down
                    : Vector2.up;
			}

            if (movementType == MovementTypes.Vertical)
            {
                movement = movement == Vector2.left
                    ? Vector2.right
                    : Vector2.left;
            }

            if (movementType == MovementTypes.Random)
            {
                Vector2 currentDirection = movement;
                List<Vector2> possibleDirections = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

                do
                {
                    movement = possibleDirections[Random.Range(0, possibleDirections.Count)];
                } while (movement == currentDirection);
            }
        }
    }

    void DoMove()
	{
        if (movementType == MovementTypes.Sleep)
        {
            return;
        }

        if(movementType == MovementTypes.ToPlayer)
		{
            MoveToPlayer();
            return;
		}

        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    void MoveToPlayer()
	{
        // determine where the player is.
        // move to that position
	}
}
