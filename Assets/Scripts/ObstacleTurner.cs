using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleTurner : MonoBehaviour
{
    // a trigger box which tells an obstacle object to turn.
    [SerializeField] private List<ObstacleTypes> obstacleTypeFilter;
    [SerializeField] private Vector2 obstacleTurnDirection = new Vector2(0, 1); // the direction the obstacle will turn in.

    // Start is called before the first frame update

    void TurnIfTypeMatches(Obstacle obstacle) {
        Debug.Log("TurnIfTypeMatches " + obstacle.name);
        foreach (ObstacleTypes type in obstacleTypeFilter) {
            if (obstacle.MatchObstacleType(type)) {
                obstacle.Turn(obstacleTurnDirection);
                return;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Obstacle") {
            TurnIfTypeMatches(other.gameObject.GetComponent<Obstacle>());
        }
    }
}
