using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDestroyer : MonoBehaviour
{

    // a trigger box which destroys an obstacle object when the obstacle enters it.
    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Obstacle") {
            other.gameObject.GetComponent<Obstacle>().Destroy();
        }
    }
}
