using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleTypes {Car, Taxi, Bike, Any}

public struct ObstacleRewindSnapshot {
    public Vector3 position;
    public Vector2 moveDirection;
    public bool hasBegunMoving;
    public bool isTurning;
    public float currentSpeed;
    public bool isStoppedAtIntersection;

    public Vector2 velocity;

    public ObstacleRewindSnapshot(Vector3 position, Vector2 moveDirection, bool hasBegunMoving, bool isTurning, float currentSpeed, Vector2 velocity, bool isStoppedAtIntersection) {
        this.position = position;
        this.moveDirection = moveDirection;
        this.hasBegunMoving = hasBegunMoving;
        this.isTurning = isTurning;
        this.currentSpeed = currentSpeed;
        this.velocity = velocity;
        this.isStoppedAtIntersection = isStoppedAtIntersection;
    }
}

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
    public bool isMoving = false;
    public bool isStoppedAtIntersection = false;
    private bool isTurning = false;
    public float currentSpeed = 0f;

    private float creationTime = 0f;

    public List<ObstacleRewindSnapshot> rewindSnapshots = new List<ObstacleRewindSnapshot>();
    private float rewindSnapshotTimer = 0f;

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

    public IEnumerator Rewind() {
        int numSnapshots = (int) Mathf.Min( rewindSnapshots.Count,  Game.Instance.rewindTime /  Game.Instance.snapShotInterval) ;
        for (int i = rewindSnapshots.Count - 1 ; i >= rewindSnapshots.Count - numSnapshots; i--) {
            ObstacleRewindSnapshot snapshot = rewindSnapshots[i];
            //Debug.Log("Rewinding "+gameObject.name+" to snapshot: " + i);
            //Debug.Log("Rewinding to position: " + snapshot.position + " with turning: " + snapshot.moveDirection.x + ", " + snapshot.moveDirection.y);
            this.gameObject.transform.position = snapshot.position;
            SetRotation(snapshot.moveDirection);
            moveDirection = snapshot.moveDirection;
            isMoving = snapshot.hasBegunMoving;
            isTurning = snapshot.isTurning;
            currentSpeed = snapshot.currentSpeed;
            isStoppedAtIntersection = snapshot.isStoppedAtIntersection;
            yield return new WaitForSeconds(Game.Instance.snapShotInterval);
        }
        for (int i = 0; i < numSnapshots; i++) {
            rewindSnapshots.RemoveAt(rewindSnapshots.Count - 1);
        }

    }


    void Update()
    {
        if (Game.Instance.gameIsOver) {
            return;
        }
        if (isMoving) {
            rigidBody.velocity = moveDirection * currentSpeed;
        }

        if (rewindSnapshotTimer <= 0) {
            int numSnapshots = (int)(Game.Instance.rewindTime / Game.Instance.snapShotInterval) *3;
            if (rewindSnapshots.Count >= numSnapshots) {
                rewindSnapshots.RemoveAt(0);
            }

            rewindSnapshots.Add(new ObstacleRewindSnapshot(transform.position, moveDirection, isMoving, isTurning, currentSpeed, rigidBody.velocity, isStoppedAtIntersection));
            rewindSnapshotTimer = Game.Instance.snapShotInterval;
        } else {
            rewindSnapshotTimer -= Time.deltaTime;
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
        isMoving = true;
        currentSpeed = speed;

        rigidBody.velocity = moveDirection * currentSpeed;
    }

    public void StopMoving() {
        isMoving = false;
        isStoppedAtIntersection = true;
        rigidBody.velocity = Vector2.zero;
    }

    public void ResumeMoving() {
        isMoving = true;
        isStoppedAtIntersection = false;
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
