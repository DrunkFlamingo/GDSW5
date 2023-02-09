using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct IntersectionSnapshot {
    public List<Obstacle> waitingObstacles;
    public float carResumeTimer;

}
public class Intersection : MonoBehaviour
{
    // a trigger box which adds obstacles to a list of obstacles waiting at the intersection.
    // when a car enters, it is stopped.
    // the intersection tells one car to resume moving every X seconds.

    [SerializeField] private List<ObstacleTypes> obstacleTypeFilter;
    [SerializeField] private float carResumeTime = 3f;

    private List<Obstacle> waitingObstacles = new List<Obstacle>();
    private List<IntersectionSnapshot> snapshots = new List<IntersectionSnapshot>();
    private float carResumeTimer = 0f;
    //private float snapshotTimer = 0f;

    bool CheckTypeMatch (Obstacle obstacle) {
        foreach (ObstacleTypes type in obstacleTypeFilter) {
            if (obstacle.MatchObstacleType(type)) {
                return true;
            }
        } 
        return false;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Obstacle") {
            Obstacle obstacle = other.gameObject.GetComponent<Obstacle>();
            if (CheckTypeMatch(obstacle)) {
                waitingObstacles.Add(obstacle);
                obstacle.StopMoving();
            }
        }
    }

    public IEnumerator Rewind() {
        Debug.Log("Rewinding intersection...");
        int numSnapshots = (int) Mathf.Min( snapshots.Count, Game.Instance.rewindTime / Game.Instance.snapShotInterval);
        Debug.Log("Number of snapshots: " + numSnapshots);
        for (int i = snapshots.Count - 1 ; i >= snapshots.Count - numSnapshots; i--) {
            IntersectionSnapshot snapshot = snapshots[i];
            if (i == snapshots.Count - numSnapshots) {
                waitingObstacles = snapshot.waitingObstacles;
                carResumeTimer = snapshot.carResumeTimer;
                Debug.Log("Rewound intersection.");
                Debug.Log("Waiting obstacles: " + waitingObstacles.Count);
                Debug.Log("Car resume timer: " + carResumeTimer);
            }
            yield return new WaitForSeconds(Game.Instance.snapShotInterval);
        }
        Debug.Log("Finished rewinding.");
        for (int i = 0; i < numSnapshots; i++) {
            snapshots.RemoveAt(snapshots.Count - 1);
        }
    }

    public void UpdateIntersectionAfterRewind() {
        waitingObstacles = new List<Obstacle>();
        carResumeTimer = 0f;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Obstacles"));
        Collider2D[] results = new Collider2D[10];
        Collider2D collider = GetComponent<Collider2D>();
        int numColliders = collider.OverlapCollider(filter, results);
        foreach (Collider2D result in results) {
            if (result != null) {
                Obstacle obstacle = result.gameObject.GetComponentInParent<Obstacle>() ?? result.gameObject.GetComponent<Obstacle>();
                if (obstacle == null) {
                    Debug.Log("Obstacle is null on " + result.gameObject.name);
                } else 
                if (CheckTypeMatch(obstacle) && obstacle.isStoppedAtIntersection) {
                    waitingObstacles.Add(obstacle);
                }
            }
        }
        Debug.Log("Updated intersection after rewind.");
        Debug.Log("Waiting obstacles: " + waitingObstacles.Count);
    }


    void Update() {
        if (Game.Instance.gameIsOver) {
            return;
        }
        carResumeTimer += Time.deltaTime;

        if (waitingObstacles.Count > 0) {
            if (carResumeTimer > carResumeTime) {
                carResumeTimer = 0f;
                if  (waitingObstacles[0] != null) {                    
                    waitingObstacles[0].ResumeMoving();
                    waitingObstacles.RemoveAt(0);
                }
            }
        }
        /*
        if (snapshotTimer >= Game.Instance.snapShotInterval) {
            snapshotTimer = 0f;
            int numSnapshots = (int)(Game.Instance.rewindTime / Game.Instance.snapShotInterval) * 3;
            if (snapshots.Count >= Game.Instance.snapShotInterval) {
                snapshots.RemoveAt(0);
            }
            snapshots.Add(new IntersectionSnapshot {
                waitingObstacles = new List<Obstacle>(waitingObstacles),
                carResumeTimer = carResumeTimer
            });
        } else {
            snapshotTimer += Time.deltaTime;
        }
        */
    }
}
