using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct IntersectionSnapshot {
    public List<Obstacle> waitingObstacles;
    public float lastCarResumeTimeDifference;

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
    private float lastCarResumeTime = 0f;
    private float lastSnapshotTime = 0f;

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



    public void Rewind() {
        IntersectionSnapshot snapshot = snapshots[0];
        waitingObstacles = snapshot.waitingObstacles;
        lastCarResumeTime = Time.time - snapshot.lastCarResumeTimeDifference - Game.Instance.rewindTime;
        snapshots.Clear();
    }

    void Update() {
        if (Game.Instance.gameIsOver) {
            lastSnapshotTime = lastSnapshotTime + Time.deltaTime;
            return;
        }
        if (waitingObstacles.Count > 0) {
            if (Time.time - lastCarResumeTime > carResumeTime) {
                lastCarResumeTime = Time.time;
                if  (waitingObstacles[0] != null) {                    
                    waitingObstacles[0].ResumeMoving();
                    waitingObstacles.RemoveAt(0);
                }
            }
        }
        /*
        if (Time.time - lastSnapshotTime > Game.Instance.snapShotInterval) {
            lastSnapshotTime = Time.time;
            snapshots.Add(new IntersectionSnapshot {
                waitingObstacles = waitingObstacles,
                lastCarResumeTimeDifference = Time.time - lastCarResumeTime
            });
            if (snapshots.Count > Game.Instance.rewindTime / Game.Instance.snapShotInterval) {
                snapshots.RemoveAt(0);
            }
        }
        */
    }
}
