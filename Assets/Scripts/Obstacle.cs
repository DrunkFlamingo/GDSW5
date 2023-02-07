using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleTypes {Car, Taxi, Bike, Any}

public class Obstacle : MonoBehaviour
{
    // handles movement and collision interactions for an obstacle object.

    [Header("Obstacle Settings")]
    [SerializeField] private ObstacleTypes obstacleType;
    [SerializeField] public float speed = 10f;
    [SerializeField] private bool canTurn = true;
    [SerializeField] public bool avoidHittingPlayer = true;
    [SerializeField] public float avoidanceDeceleration = 2f;
    [SerializeField] public AudioClip warningSound;
    [SerializeField] public AudioClip hitSound;

    private AudioSource audioSource;
    private Rigidbody2D rigidBody;
    new private Collider2D collider;
    public Vector2 moveDirection = new Vector2(-1, 0);
    private bool hasBegunMoving = false;
    private bool isTurning = false;
    public float currentSpeed = 0f;

    private float creationTime = 0f;

    void Awake()
    {
        audioSource = this.gameObject.AddComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        creationTime = Time.time;
    }


    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            // play a hit sound and destroy the player.
            if (hitSound != null) {
                audioSource.PlayOneShot(hitSound);
            }
            Game.Instance.StartGameOver(other.gameObject);
        }
    }

    (bool, bool) ShouldSlowDown(List<RaycastHit2D> hits) {
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.gameObject.CompareTag("Player")) {
                return (true, true);
            } else if (hit.collider.gameObject.CompareTag("Obstacle")) {
                Obstacle obstacle = hit.collider.gameObject.GetComponent<Obstacle>();
                if (obstacle.moveDirection != moveDirection) {
                    return (true, false);
                }
            }
        }
        return (false, false);
    }

    public void Destroy() {
        Debug.Log("Destroying " + this.name + " at " + Time.time + " after " + (Time.time - creationTime) + " seconds");       
        Destroy(this.gameObject);
    }


    void Update()
    {
        if (!avoidHittingPlayer) {
            return;
        }
        // cast the collider in the direction we are travelling to see if we are about to hit the player.
        // if we are, play a warning sound and reduce our speed by avoidanceDeceleration until it is at 1/5;
        
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        ContactFilter2D filter = new ContactFilter2D();
        //filter.SetLayerMask(LayerMask.GetMask("Player", "Obstacles"));
        filter.SetLayerMask(LayerMask.GetMask("Player"));
        float checkDist = speed * 0.5f;
        int numHits = collider.Cast(moveDirection, filter, hits, checkDist);
        Debug.DrawRay(transform.position, moveDirection * checkDist, Color.red);
        (bool shouldSlowDown, bool hitPlayer) = ShouldSlowDown(hits);
        if (shouldSlowDown) {
            Debug.Log("Obstacle " + this.name + " is about to hit " + hits[0].collider.gameObject.name);
            if (hitPlayer && warningSound != null) {
                audioSource.PlayOneShot(warningSound);
            }
            currentSpeed = Mathf.Max(currentSpeed - (avoidanceDeceleration*Time.deltaTime), speed/5);
            rigidBody.velocity = moveDirection * currentSpeed;
        } else if (currentSpeed < speed) {
            currentSpeed = Mathf.Min(currentSpeed + (avoidanceDeceleration*2 *Time.deltaTime), speed);
            rigidBody.velocity = moveDirection * currentSpeed;
        }
      
    }

    void SetRotation(Vector2 direction) {
        if (direction == Vector2.up) {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        } else if (direction == Vector2.down) {
            transform.rotation = Quaternion.Euler(0, 0, 180);
        } else if (direction == Vector2.left) {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        } else if (direction == Vector2.right) {
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        moveDirection = direction;
    }

    public void StartMoving(Vector2 direction) {
        SetRotation(direction);
        hasBegunMoving = true;
        currentSpeed = speed;

        rigidBody.velocity = moveDirection * currentSpeed;
    }

    public void StopMoving() {
        hasBegunMoving = false;
        rigidBody.velocity = Vector2.zero;
    }

    public void ResumeMoving() {
        hasBegunMoving = true;
        rigidBody.velocity = moveDirection * currentSpeed;
    }


    public void Turn(Vector2 direction) {
        if (!canTurn) {
            return;
        }
        isTurning = true;
        SetRotation(direction);
        rigidBody.velocity = moveDirection * currentSpeed;
    }

    public bool MatchObstacleType(ObstacleTypes type) {
        return obstacleType == type || type == ObstacleTypes.Any;
    }
}
