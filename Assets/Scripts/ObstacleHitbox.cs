using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleHitbox : MonoBehaviour
{
    private Obstacle obstacle;
    private AudioSource audioSource;
    private Rigidbody2D rigidBody;
    new private Collider2D collider;

    private bool hasAlreadyWarned = false;
    private bool hasAlreadyHit = false;

    public float turnedTime = 0f;

    void Awake()
    {
        obstacle = GetComponentInParent<Obstacle>();
        audioSource = GetComponentInParent<AudioSource>();
        collider = GetComponent<Collider2D>();
        rigidBody = GetComponentInParent<Rigidbody2D>();
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
    }

    (bool, bool) ShouldSlowDown(List<RaycastHit2D> hits) {
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.gameObject.CompareTag("Player")) {
                return (true, true);
            } else if (hit.collider.gameObject.CompareTag("Obstacle")) {
                Obstacle otherObstacle = hit.collider.gameObject.GetComponent<Obstacle>();
                if (otherObstacle.moveDirection != obstacle.moveDirection) {
                    return (true, false);
                }
            }
        }
        return (false, false);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (Time.time - turnedTime  < 0.5f) {
            Debug.Log("Time since turned: " + (Time.time - turnedTime));
            Debug.Log(Time.time);
            Debug.Log(turnedTime);
            //return;
        }
        if (other.gameObject.CompareTag("Player")) {
            // play a hit sound and destroy the player.
            if (obstacle.hitSound != null && !hasAlreadyHit) {
                GetComponentInParent<AudioSource>().PlayOneShot(obstacle.hitSound);
                hasAlreadyHit = true;
            }
            Game.Instance.StartGameOver(other.gameObject);
        }
    }

    void Update() {
        SetRotation(obstacle.moveDirection);
        if (Game.Instance.gameIsOver) {
            return;
        }
        // cast the collider in the direction we are travelling to see if we are about to hit the player.
        // if we are, play a warning sound and reduce our speed by avoidanceDeceleration until it is at 1/5;
        Vector2 moveDirection = obstacle.moveDirection;
        AudioClip warningSound = obstacle.warningSound;
        bool avoidHittingPlayer = obstacle.avoidHittingPlayer;


        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        ContactFilter2D filter = new ContactFilter2D();
        //filter.SetLayerMask(LayerMask.GetMask("Player", "Obstacles"));
        filter.SetLayerMask(LayerMask.GetMask("Player"));
        float checkDist = obstacle.speed * 0.8f;
        int numHits = collider.Cast(moveDirection, filter, hits, checkDist);
        Debug.DrawRay(transform.position, moveDirection * checkDist, Color.red);
        (bool shouldSlowDown, bool hitPlayer) = ShouldSlowDown(hits);
        if (shouldSlowDown) {
            Debug.Log("Obstacle " + this.obstacle.name + " is about to hit " + hits[0].collider.gameObject.name);
            if (hitPlayer && warningSound != null && hasAlreadyWarned == false) {
                audioSource.PlayOneShot(warningSound);
                hasAlreadyWarned = true;
            }
            float obstacleMinSpeed = obstacle.speed/5;
            if (!hitPlayer) obstacleMinSpeed = 0;
            if (avoidHittingPlayer || !hitPlayer) {
                obstacle.currentSpeed = Mathf.Max(obstacle.currentSpeed - (obstacle.avoidanceDeceleration*Time.deltaTime), obstacleMinSpeed);
            }
        } else if (obstacle.currentSpeed < obstacle.speed) {
            obstacle.currentSpeed = Mathf.Min(obstacle.currentSpeed + (obstacle.avoidanceDeceleration*2 *Time.deltaTime), obstacle.speed);
        }
    }
}
