using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public ObstacleData obstacleData;
    public GameObject obstaclePrefab;

    void Start()
    {
        GenerateObstacles();
    }

    void GenerateObstacles()
    {
        for (int i = 0; i < obstacleData.obstacleGrid.Length; i++)
        {
            if (obstacleData.obstacleGrid[i])
            {
                int x = i % 10;
                int z = i / 10;
                Vector3 position = new Vector3(x * 1.1f, 1.0f, z * 1.1f);
                Instantiate(obstaclePrefab, position, Quaternion.identity);
            }
        }
    }
}
