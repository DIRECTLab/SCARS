using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform minStartPosition;
    public Transform maxEndPosition;
    public GameObject enemyPrefab;
    public LayerMask lidarLayer;
    public float enemyYHeight;

    private GameObject[] enemies;

    RaycastHit hit;


    public void SpawnEnemies(int iteration, int numberOfObservers)
    {
        Random.InitState(iteration);
        if (enemies == null)
        {
            enemies = new GameObject[numberOfObservers];
        }
        else
        {
            foreach (GameObject obj in enemies)
            {
                Destroy(obj);
            }

            enemies = new GameObject[numberOfObservers];
        }

        for (int i = 0; i < numberOfObservers; i++)
        {
            Vector3 location = new Vector3(Random.Range(minStartPosition.position.x, maxEndPosition.position.x), enemyYHeight, Random.Range(minStartPosition.position.z, maxEndPosition.position.z));

            while (Physics.SphereCast(location + Vector3.up * 15, 2.1f, Vector3.down, out hit, 20, lidarLayer))
            {
                location = new Vector3(Random.Range(minStartPosition.position.x, maxEndPosition.position.x), enemyYHeight, Random.Range(minStartPosition.position.z, maxEndPosition.position.z));
            }
            GameObject newItem = Instantiate(enemyPrefab);
            newItem.transform.position = location;
            enemies[i] = newItem;
        }
    }
}
