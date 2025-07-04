using UnityEngine;

public class PuzzleSpawner : MonoBehaviour
{
    public GameObject[] puzzlePrefabs; // Array of puzzle prefabs to spawn

    void Start()
    {
        SpawnPuzzleGrid();
    }

    void SpawnPuzzleGrid()
    {
        if (puzzlePrefabs.Length != 18)
        {
            Debug.LogError("puzzlePrefabs array must contain exactly 18 prefabs.");
            return;
        }

        // Generate grid positions (6 columns x 3 rows)
        Vector3[] gridPositions = new Vector3[18];
        int index = 0;
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                float x = 4 - col; // 4 to -1
                float z = 1 - row; // 1 to -1
                gridPositions[index++] = new Vector3(x, transform.position.y, z);
            }
        }

        // Shuffle the puzzlePrefabs array
        GameObject[] shuffledPrefabs = (GameObject[])puzzlePrefabs.Clone();
        for (int i = 0; i < shuffledPrefabs.Length; i++)
        {
            int rand = Random.Range(i, shuffledPrefabs.Length);
            GameObject temp = shuffledPrefabs[i];
            shuffledPrefabs[i] = shuffledPrefabs[rand];
            shuffledPrefabs[rand] = temp;
        }

        // Spawn each prefab at a unique grid position
        for (int i = 0; i < 18; i++)
        {
            Instantiate(shuffledPrefabs[i], gridPositions[i], Quaternion.identity);
        }
    }
}
