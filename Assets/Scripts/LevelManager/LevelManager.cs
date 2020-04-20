using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
public struct Parameters
{
    public State State;
}

public class LevelManager : MonoBehaviour
{
    private LevelConfig config;
    private GameObject player;
    public List<GameObject> prefab = new List<GameObject>();

    Dictionary<Tuple<int,int>, List<Vector2Int>> mapping = new Dictionary<Tuple<int,int>, List<Vector2Int>>();

    public TileBase Wall;
    public TileBase ground;

    public Tilemap Wallsmap;
    public Tilemap GroundsMap;

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
        //PlaceSimpleLevel();
    }

    internal int Convert(Vector2Int vector2)
    {
        Tuple<int,int> key = mapping.FirstOrDefault(predicate => predicate.Value.Any(item => item == vector2)).Key;
        //Index is room index the general room what we want to spawn
        //Exit is exit is the exit number.

        int index = key.Item1;
        int exit = key.Item2;
    }

    private void PlaceWallPrime(Vector2Int pos, bool[] neigbours)
    {
        PlaceWall(pos);
        for (int i = 0; i < neigbours.Length; i++)
        {
            if (neigbours[i])
            {
                switch (i)
                {
                    case 0: //NW
                        PlaceWall(pos + Vector2Int.left + Vector2Int.up);
                        break;
                    case 1: //N
                        PlaceWall(pos + Vector2Int.up);
                        break;
                    case 2: //NE
                        PlaceWall(pos + Vector2Int.right + Vector2Int.up);
                        break;
                    case 3: //E
                        PlaceWall(pos + Vector2Int.right);
                        break;
                    case 4: //ZE
                        PlaceWall(pos + Vector2Int.right + Vector2Int.down);
                        break;
                    case 5: //Z
                        PlaceWall(pos + Vector2Int.down);
                        break;
                    case 6: //ZW
                        PlaceWall(pos + Vector2Int.left + Vector2Int.down);
                        break;
                    case 7: // W
                        PlaceWall(pos + Vector2Int.left);
                        break;
                }
            }
        }
    }
    private void PlaceWall(Vector2Int pos)
    {
        Wallsmap.SetTile(new Vector3Int(pos.x, pos.y, 0), Wall);
    }
    private void PlaceGround(Vector2Int pos)
    {
        GroundsMap.SetTile(new Vector3Int(pos.x, pos.y, 0), ground);
    }


    private async Task BuildExit(Vector2 start, Vector2 size, int index, int exit)
    {
        Tuple<int,int> key = new Tuple<int, int>(index, exit);

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

    private async Task BuildWalls(List<Vector2Int> walls, int[,] array)
    {
        foreach (Vector2Int pos in walls)
        {
            bool[] neigbours = new bool[8];
            neigbours[0] = array[pos.x - 1, pos.y + 1] == 1;
            neigbours[1] = array[pos.x, pos.y + 1] == 1;
            neigbours[2] = array[pos.x + 1, pos.y + 1] == 1;
            neigbours[3] = array[pos.x + 1, pos.y] == 1;
            neigbours[4] = array[pos.x + 1, pos.y - 1] == 1;
            neigbours[5] = array[pos.x, pos.y - 1] == 1;
            neigbours[6] = array[pos.x - 1, pos.y - 1] == 1;
            neigbours[7] = array[pos.x - 1, pos.y] == 1;

            PlaceWallPrime(pos, neigbours);
        }
    }
    private async Task BuildGround(List<Vector2Int> grounds)
    {
        foreach (Vector2Int ground in grounds)
        {
            PlaceGround(ground);
        }
    }

    private async Task SpawnPlayer(Vector2 pos)
    {
        if (player == null)
            player = Instantiate(prefab[UnityEngine.Random.Range(0, prefab.Count)], pos, Quaternion.identity);
        else
        {
            player = Instantiate(player, pos, Quaternion.identity);
        }
    }


    private async Task PlaceSimpleLevel()
    {
        int n = 10;
        int m = 10;
        int[,] thing = new int[n, m];

        List<Vector2Int> walls = new List<Vector2Int>();
        List<Vector2Int> grounds = new List<Vector2Int>();

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (i < 2 || j < 2 || j > m - 3 || i > n - 3)
                    thing[i, j] = 1;
                if ((i != 0 && j != 0 && i != n - 1 && j != m - 1) && (i == 1 || j == 1 || j == m - 2 || i == n - 2))
                    walls.Add(new Vector2Int(i, j));
                if ((i != 0 && j != 0 && i != n - 1 && j != m - 1))
                    grounds.Add(new Vector2Int(i, j));
            }
        }

        Vector2Int player = new Vector2Int(UnityEngine.Random.Range(3, n - 4), UnityEngine.Random.Range(3, m - 4));

        var groundTask = BuildGround(grounds);
        var wallsTask = BuildWalls(walls, thing);
        var playerTask = SpawnPlayer(player);
        await Task.WhenAll(new Task[] { groundTask, wallsTask, playerTask });
    }
    public async Task CreateNewLevel(Parameters param)
    {
        await PlaceSimpleLevel();
        return;
    }
    
}