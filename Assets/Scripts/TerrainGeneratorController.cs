using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorController : MonoBehaviour
{
    [Header("Templates")]
    public List<TerrainTemplateController> terrainTemplates;
    public float terrainTemplateWidth;

    [Header("Force Early Template")]
    public List<TerrainTemplateController> earlyTerrainTemplates;

    [Header("Obstacle")]
    public List<ObstacleScript> obstacles;
    public float obstacleDistance;

    [Header("Generator Area")]
    public Camera gameCamera;
    public float areaStartOffset;
    public float areaEndOffset;

    private const float debugLineHeight = 10.0f;


    private List<GameObject> spawnedTerrain;
    private List<GameObject> spawnedObstacle;

    private float lastGeneratedPositionX;
    private float lastRemovedPositionX;

    private float lastGeneratedObstacleX;
    private float lastRemovedObstacleX;

    // pool list
    private Dictionary<string, List<GameObject>> pool;
    private Dictionary<string, List<GameObject>> obstaclePool;

    // Start is called before the first frame update
    void Start()
    {
        // init pool
        pool = new Dictionary<string, List<GameObject>>();
        obstaclePool = new Dictionary<string, List<GameObject>>();

        spawnedTerrain = new List<GameObject>();
        spawnedObstacle = new List<GameObject>();

        lastGeneratedPositionX = GetHorizontalPositionStart();
        lastRemovedPositionX = lastGeneratedPositionX - terrainTemplateWidth;

        lastGeneratedObstacleX = GetHorizontalPositionStart() + 100;
        lastRemovedObstacleX = lastGeneratedObstacleX - terrainTemplateWidth;

        foreach (TerrainTemplateController terrain in earlyTerrainTemplates)
        {
            GenerateTerrain(lastGeneratedPositionX, terrain);
            lastGeneratedPositionX += terrainTemplateWidth;
        }

        while (lastGeneratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth;
        }

    }

    // Update is called once per frame
    void Update()
    {
        while (lastGeneratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth;
        }

        if (Random.value < 0.5f) 
        {
            while (lastGeneratedObstacleX < GetHorizontalPositionEnd())
            {
                GenerateObstacle(lastGeneratedObstacleX);
                lastGeneratedObstacleX += obstacleDistance;
            }
        }

        while (lastRemovedPositionX + terrainTemplateWidth < GetHorizontalPositionStart())
        {
            lastRemovedPositionX += terrainTemplateWidth;
            RemoveTerrain(lastRemovedPositionX);
        }

        while (lastRemovedObstacleX + obstacleDistance < GetHorizontalPositionStart())
        {
            lastRemovedObstacleX += obstacleDistance;
            RemoveObstacle(lastRemovedObstacleX);
        }
    }

    private float GetHorizontalPositionStart()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(0f, 0f)).x + areaStartOffset;
    }

    private float GetHorizontalPositionEnd()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(1f, 0f)).x + areaEndOffset;
    }

    private void GenerateTerrain(float posX, TerrainTemplateController forceterrain = null)
    {
        GameObject newTerrain;
        if (forceterrain is null)
        {
            newTerrain = GenerateFromPool(terrainTemplates[Random.Range(0, terrainTemplates.Count)].gameObject, transform, pool);
        }
        else
        {
            newTerrain = GenerateFromPool(forceterrain.gameObject, transform, pool);
        }

        newTerrain.transform.position = new Vector2(posX, -4.5f);

        spawnedTerrain.Add(newTerrain);
    }

    private void GenerateObstacle(float posX)
    {
        GameObject newObstacle = GenerateFromPool(obstacles[Random.Range(0, obstacles.Count)].gameObject, transform, obstaclePool);

        float randomX = Random.Range(-3.0f, 4.0f);
        newObstacle.transform.position = new Vector2(posX, randomX);

        spawnedObstacle.Add(newObstacle);
    }

    private void RemoveTerrain(float posX)
    {
        GameObject terrainToRemove = null;

        // find terrain at posX
        foreach (GameObject item in spawnedTerrain)
        {
            if (item.transform.position.x <= posX)
            {
                terrainToRemove = item;
                break;
            }
        }

        // after found;
        if (terrainToRemove != null)
        {
            spawnedTerrain.Remove(terrainToRemove);
            ReturnToPool(terrainToRemove, pool);
        }
    }

    private void RemoveObstacle(float posX)
    {
        GameObject obstacleToRemove = null;

        // find obstacle at posX
        foreach (GameObject item in spawnedObstacle)
        {
            if (item.transform.position.x <= posX)
            {
                obstacleToRemove = item;
                break;
            }
        }

        // after found;
        if (obstacleToRemove != null)
        {
            spawnedObstacle.Remove(obstacleToRemove);
            ReturnToPool(obstacleToRemove, obstaclePool);
        }
    }

    // pool function
    private GameObject GenerateFromPool(GameObject item, Transform parent, Dictionary<string, List<GameObject>> pool)
    {
        if (pool.ContainsKey(item.name))
        {
            // if item available in pool
            if (pool[item.name].Count > 0)
            {
                GameObject newItemFromPool = pool[item.name][0];
                pool[item.name].Remove(newItemFromPool);
                newItemFromPool.SetActive(true);
                return newItemFromPool;
            }
        }
        else
        {
            // if item list not defined, create new one
            pool.Add(item.name, new List<GameObject>());
        }


        // create new one if no item available in pool
        GameObject newItem = Instantiate(item, parent);
        newItem.name = item.name;
        return newItem;
    }

    private void ReturnToPool(GameObject item, Dictionary<string, List<GameObject>> pool)
    {
        if (!pool.ContainsKey(item.name))
        {
            Debug.LogError("INVALID POOL ITEM!!");
        }


        pool[item.name].Add(item);
        item.SetActive(false);
    }

    // debug
    private void OnDrawGizmos()
    {
        Vector3 areaStartPosition = transform.position;
        Vector3 areaEndPosition = transform.position;

        areaStartPosition.x = GetHorizontalPositionStart();
        areaEndPosition.x = GetHorizontalPositionEnd();

        Debug.DrawLine(areaStartPosition + Vector3.up * debugLineHeight / 2, areaStartPosition + Vector3.down * debugLineHeight / 2, Color.red);
        Debug.DrawLine(areaEndPosition + Vector3.up * debugLineHeight / 2, areaEndPosition + Vector3.down * debugLineHeight / 2, Color.red);
    }
}
