using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RewindSnapshot {
    public Vector3 position;
    public Vector2 direction;
    public Vector2 velocity;
}
public class Rewindable : MonoBehaviour
{
    private float rewindTime = 2.45f;
    private float snapShotInterval = 0.05f;
    private List<Vector3> position;
    private List<RewindSnapshot> snapshots;
    private Quaternion rotation;
    private float lastSnapshotTime = 0f;

    private Rigidbody2D rigidBody;
    

    void Awake() {
        snapshots = new List<RewindSnapshot>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Start() {
        rewindTime = Game.Instance.rewindTime;
        snapShotInterval = Game.Instance.snapShotInterval;
    }

    RewindSnapshot TakeSnapshot(Player player) {
        return new RewindSnapshot {
            position = transform.position,
            direction = player.movementVector,
            velocity = rigidBody.velocity
        };
    }

    RewindSnapshot TakeSnapshot(Obstacle obstacle) {
        return new RewindSnapshot {
            position = transform.position,
            direction = obstacle.moveDirection * obstacle.currentSpeed,
            velocity = rigidBody.velocity
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (Game.Instance.gameIsOver) {
            lastSnapshotTime = lastSnapshotTime + Time.deltaTime;
            return;
        }
        if (Time.time - lastSnapshotTime > snapShotInterval) {
            lastSnapshotTime = Time.time;
            if (GetComponent<Player>()) {
                snapshots.Add(TakeSnapshot(GetComponent<Player>()));
            } else if (GetComponent<Obstacle>()) {
                snapshots.Add(TakeSnapshot(GetComponent<Obstacle>()));
            }
            if (snapshots.Count > rewindTime / snapShotInterval) {
                snapshots.RemoveAt(0);
            }
        }
    }
    
    IEnumerator RewindRoutine() {
        int lastSnapIndex = snapshots.Count - 1;

        for (int i = lastSnapIndex; i >= 0; i--) {
            transform.position = snapshots[i].position;
            if (GetComponent<Player>()) {
                GetComponent<Player>().SetSpriteOrientation(snapshots[i].direction);
            } else if (GetComponent<Obstacle>()) {
                GetComponent<Obstacle>().Turn(snapshots[i].direction);
            }
            if (i == lastSnapIndex) {
                rigidBody.velocity = Vector2.zero;
            } else if (i == 0 && GetComponent<Obstacle>()) {
                GetComponent<Obstacle>().StartMoving(snapshots[i].direction);
            }
            yield return new WaitForSeconds(snapShotInterval);
        }
    }

    public void Rewind() {
        StartCoroutine(RewindRoutine());
    }
}
