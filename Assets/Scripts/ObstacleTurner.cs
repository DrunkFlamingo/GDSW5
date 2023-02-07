using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleTurner : MonoBehaviour
{
    // a trigger box which tells an obstacle object to turn.
    [SerializeField] private List<ObstacleTypes> obstacleTypeFilter;
    [SerializeField] private Vector2 obstacleTurnDirection = new Vector2(0, 1); // the direction the obstacle will turn in.
    [SerializeField] private int turnChance = 100; // the chance that the obstacle will turn, out of 100.

    // Start is called before the first frame update

    void TurnIfTypeMatches(Obstacle obstacle) {
        Debug.Log("TurnIfTypeMatches " + obstacle.name);
        foreach (ObstacleTypes type in obstacleTypeFilter) {
            if (obstacle.MatchObstacleType(type)) {
                if (Random.Range(0, 100) < turnChance) {
                    obstacle.Turn(obstacleTurnDirection);
                    
                }
                return;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Obstacle") {
            TurnIfTypeMatches(other.gameObject.GetComponent<Obstacle>());
        }
    }

    void Update() {
         Debug.DrawLine(transform.position, transform.position + new Vector3(obstacleTurnDirection.x*25, obstacleTurnDirection.y*25, 0), Color.green);
    }
}
