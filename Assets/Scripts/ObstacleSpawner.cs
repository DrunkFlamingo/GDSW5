using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    // a transform which spawns obstacle objects on a timer or when the player enters an ObstacleTrigger.

    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] GameObject alternateObstaclePrefab;

    [SerializeField] bool spawnOverTime = true; // if true, the obstacle will spawn over time. if false, the obstacle will spawn only when the player enters an ObstacleTrigger.
    [SerializeField] float spawnRate = 5f; // how many seconds between each obstacle spawn.
    [SerializeField] float alternateObstacleSpawnChance = 20f; // the chance that an alternate obstacle will spawn instead of the normal obstacle.
    [SerializeField] Vector2 obstacleMoveDirection = new Vector2(-1, 0); // the direction the obstacle will move in.
 
    float lastSpawnTime = 0f;

    public void SpawnObstacle() {
        GameObject obstacleToSpawn = obstaclePrefab;
        if (alternateObstaclePrefab != null && Random.Range(0, 100) < alternateObstacleSpawnChance) {
            obstacleToSpawn = alternateObstaclePrefab;
        }
        GameObject obstacle = Instantiate(obstacleToSpawn, transform.position, Quaternion.identity);
        obstacle.GetComponent<Obstacle>().StartMoving(obstacleMoveDirection);
    }


    // Update is called once per frame
    void Update()
    {
        if (Game.Instance.gameIsOver) {
            lastSpawnTime = lastSpawnTime + Time.deltaTime;
            return;
        }
        Debug.DrawLine(transform.position, transform.position + new Vector3(obstacleMoveDirection.x*25, obstacleMoveDirection.y*25, 0), Color.green);
        if (spawnOverTime) {
            if (Time.time - lastSpawnTime > spawnRate) {
                SpawnObstacle();
                lastSpawnTime = Time.time;
            }
        }
    }
}
