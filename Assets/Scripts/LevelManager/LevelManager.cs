using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    private LevelConfig config;
    private GameObject player;
    public List<GameObject> prefab = new List<GameObject>();
    /// <summary>
    /// Signleton Pattern
    /// </summary>
    private static LevelManager levelInstance;
    public static LevelManager LevelInstance
    {
        get
        {
            if (levelInstance == null)
                Debug.LogError("There is no " + LevelInstance.GetType() + " set.");
            return levelInstance;
        }
        private set
        {
            if (levelInstance != null)
                Debug.LogError("Two instances of the " + LevelInstance.GetType() + " are set.");
            levelInstance = value;
        }
    }

    void Awake()
    {
        LevelInstance = this; 
        config = new LevelConfig();
        config.Width = ConfigurationManager.ConfigInstance.getConfig<int>("Width");
        config.Height = ConfigurationManager.ConfigInstance.getConfig<int>("Height");
    }

    Dictionary<Vector2Int, List<Vector2Int>> mapping = new Dictionary<Vector2Int, List<Vector2Int>>();
    private void BuildExit(Vector2 start, Vector2 size, int index, int exit)
    {
        Vector2Int key = new Vector2Int(index, exit);

        //Index is room index the general room what we want to spawn
        //Exit is exit is the exit number.

        List<Vector2Int> exitTiles = new List<Vector2Int>();
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Vector2Int pos = new Vector2Int((int)start.x + i, (int)start.y + j);
                exitTiles.Add(pos);
            }
        }
        mapping.Add(key, exitTiles);
    }

    public void CreateNewLevel(Parameters param)
    {
        Vector2Int startTile = param.startTile;
        int seed;
        int exitNumber;
        if (startTile != null)
        {
            Vector2Int key = mapping.Where(predicate => predicate.Value.Any(p => p == startTile)).FirstOrDefault().Key;

            seed = key.x;
            exitNumber = key.y;
        }
        else
        {
            seed = 1;
            exitNumber = 0;
        }


        // Creating the first entry level
        if(param.firstLevel)
        {
            Debug.Log("Create new Level with H X W: 10 X 10");
        }
        else
        {
            Debug.Log("Create new Level with H X W: " + config.Height + " X " + config.Width);
        }
        SpawnPlayer(new Vector2(2,1));
    }

    private void SpawnPlayer(Vector2 pos)
    {
        if (player == null)
            player = Instantiate(prefab[UnityEngine.Random.Range(0,prefab.Count)],pos, Quaternion.identity);
        else
        {
            player = Instantiate(player, pos, Quaternion.identity);
        }
    }
}
public struct Parameters
{
    public bool firstLevel;
    public Vector2Int startTile;
}