using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SimpleMovement : MonoBehaviour
{
    public float maxSpeed = 5.0f;
    public float acceleration = 3f;
    public float deceleration = 5f;
    public float jumpForce = 8f;
    public float minimumGroundNormalY = 0.5f;
    public float currentSpeed = 0f;

    private Rigidbody body;
    private readonly HashSet<Collider> groundColliders = new();
    private float direction;
    private float blockedDirection;
    private bool jumpRequested;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        direction = 0f;

        if (Keyboard.current != null && Keyboard.current.dKey.isPressed)
        {
            direction += 1f;
        }

        if (Keyboard.current != null && Keyboard.current.aKey.isPressed)
        {
            direction -= 1f;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        if (direction == 0f || Mathf.Sign(direction) != blockedDirection)
        {
            blockedDirection = 0f;
        }

        float targetSpeed = direction * maxSpeed;
        float speedChange = direction == 0f ? deceleration : acceleration;
        if (blockedDirection != 0f)
        {
            currentSpeed = 0f;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChange * Time.fixedDeltaTime);
        }

        Vector3 velocity = body.linearVelocity;
        velocity.x = currentSpeed;
        body.linearVelocity = velocity;

        if (jumpRequested)
        {
            jumpRequested = false;

            if (groundColliders.Count > 0)
            {
                body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                groundColliders.Clear();
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        bool touchingGround = false;

        for (int contactIndex = 0; contactIndex < collision.contactCount; contactIndex++)
        {
            Vector3 normal = collision.GetContact(contactIndex).normal;

            if (normal.y >= minimumGroundNormalY)
            {
                touchingGround = true;
            }

            if (Mathf.Abs(normal.x) > 0.5f && Vector3.Dot(body.linearVelocity, normal) < 0f)
            {
                Vector3 velocity = body.linearVelocity;
                velocity -= normal * Vector3.Dot(velocity, normal);
                body.linearVelocity = velocity;
                currentSpeed = velocity.x;
                blockedDirection = Mathf.Sign(direction);
            }
        }

        if (touchingGround)
        {
            groundColliders.Add(collision.collider);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        groundColliders.Remove(collision.collider);
    }
}
