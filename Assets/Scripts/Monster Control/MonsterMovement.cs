using UnityEngine;
using System.Collections.Generic;

public enum MovementTypes
{
    Sleep,
    Random,
    Horizontal,
    Vertical,
    ToPlayer,
    ReturnToInitialPosition
}

public class MonsterMovement : MonoBehaviour
{
    [SerializeField]
    private float minSpeed = 2f;
    private float maxSpeed = 4f;

    private float speed ;

    private float Speed
    {
        get
        {
            return speed * GlobalManager.GlobalInstance.MonsterSpeedPercentage + GlobalManager.GlobalInstance.MonsterSpeedFlat;
        }
    }

    public float maxRange = 4f;
    public float viewRange = 6f;
    public MovementTypes movementType = MovementTypes.Random;

    private Rigidbody2D rb;
    private float timeLeftForDirectionChange = 2f;
    private Vector2 movement = Vector2.zero;
    private Vector2 initialPosition;
    private Vector2 playerPosition;
   
    void Awake()
    {
        speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        rb = GetComponent<Rigidbody2D>();

        initialPosition = rb.position;

        // Cannot go directly to player on awake
        // need to spot the player first
        if(movementType == MovementTypes.ToPlayer || movementType == MovementTypes.ReturnToInitialPosition)
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
        GameObject playerObject = GameObject.FindWithTag("Player");
        Rigidbody2D playerRigidBody = playerObject.GetComponent<Rigidbody2D>();

        Vector2 currentPosition = rb.position;
        playerPosition = playerRigidBody.position;

        double distanceFromPlayer = System.Math.Sqrt(System.Math.Pow((double)(currentPosition.x - playerPosition.x), (double)2) + System.Math.Pow((double)(currentPosition.y - playerPosition.y), (double)2));

        if ((float)distanceFromPlayer <= viewRange)
		{
			movementType = MovementTypes.ToPlayer;
		}
        else if(movementType == MovementTypes.ToPlayer)
        {
            movementType = MovementTypes.ReturnToInitialPosition;
        }
	}

    void CheckMovementRangeBounds()
	{
        if (
            movementType == MovementTypes.Sleep
            || movementType == MovementTypes.ToPlayer
            || movementType == MovementTypes.ReturnToInitialPosition
        )
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

        if(movementType == MovementTypes.ReturnToInitialPosition)
		{
            MoveToInitialPosition();
            return;
		}

        rb.MovePosition(rb.position + movement * Speed * Time.fixedDeltaTime);
    }

    void MoveToPlayer()
	{
        if(movementType == MovementTypes.ToPlayer) {
            rb.MovePosition (Vector2.Lerp (rb.position, playerPosition, (Speed * Time.fixedDeltaTime)));
        }
	}

    void MoveToInitialPosition()
    {
        if(movementType == MovementTypes.ReturnToInitialPosition) {
            rb.MovePosition (Vector2.Lerp (rb.position, initialPosition, (Speed * Time.fixedDeltaTime)));
        }
    }
}
