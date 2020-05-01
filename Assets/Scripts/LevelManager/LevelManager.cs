using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
public struct Parameters
{
    public int room;
    public int door;

    public Parameters(State currentState) : this()
    {
        door = currentState.door;
        room = currentState.room;
    }
}

public class LevelManager : MonoBehaviour
{
    private LevelConfig config;
    private GameObject player;
    public List<GameObject> prefab = new List<GameObject>();

    public TileBase Wall;
    public TileBase ground;
    public TileBase exit;

    public Tilemap Wallsmap;
    public Tilemap GroundsMap;


    int width;
    int height;

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

    MapGenerator map;

    void Awake()
    {
        LevelInstance = this; 
        config = new LevelConfig();
        config.Width = ConfigurationManager.ConfigInstance.getConfig<int>("Width");
        config.Height = ConfigurationManager.ConfigInstance.getConfig<int>("Height");
        map = new MapGenerator();
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



    private void BuildExit(int doorNumber, int roomNumber,Vector2Int pos)
    {

        //Needed To Visualize if()
        GroundsMap.SetTile(new Vector3Int(pos.x, pos.y, 0), exit);

    }
    /*
    private void BuildWalls(List<Vector2Int> walls)
    {
        foreach (Vector2Int pos in walls)
        {
            bool[] neigbours = new bool[8];


            neigbours[0] = map.map[pos.x - 1, pos.y + 1] == 1;
            neigbours[1] = map.map[pos.x, pos.y + 1] == 1;
            neigbours[2] = map.map[pos.x + 1, pos.y + 1] == 1;
            neigbours[3] = map.map[pos.x + 1, pos.y] == 1;
            neigbours[4] = map.map[pos.x + 1, pos.y - 1] == 1;
            neigbours[5] = map.map[pos.x, pos.y - 1] == 1;
            neigbours[6] = map.map[pos.x - 1, pos.y - 1] == 1;
            neigbours[7] = map.map[pos.x - 1, pos.y] == 1;

            PlaceWallPrime(pos, neigbours);
        }
    }
    */
    private void BuildGround(List<Vector2Int> grounds)
    {
        foreach (Vector2Int ground in grounds)
        {
            PlaceGround(ground);
        }
    }

    private void SpawnPlayer(Vector2 pos)
    {
        if (player == null)
            player = Instantiate(prefab[UnityEngine.Random.Range(0, prefab.Count)], pos, Quaternion.identity);
        else
        {
            player = Instantiate(player, pos, Quaternion.identity);
        }
    }


    private async Task<int> PlaceLevel(Parameters param)
    {
        //Mathe's Code
        int doorNumber = param.door;
        int roomNumber = param.room;
        if (true)
            return 0;// map.createFirstRoom();
        else
            return 0;// map.GenerateLevel(doorNumber, roomNumber);
    }



    public async Task CreateNewLevel(Parameters param)
    {
        int x = await PlaceLevel(param);
        t = new State();
        t.door = param.door;
        t.room = x;
        updated = false;
        return;
    }

    State t;
    public bool updated = true;
    public void Update()
    {
        if (updated == false)
        {
            StartCoroutine(RendererLevel(t.door, t.room));
        }
    }


    private IEnumerator RendererLevel(int door, int currentRoom)
    {/*
        updated = true;
        Wallsmap.ClearAllTiles();
        GroundsMap.ClearAllTiles();

        List<Vector2Int> walls = new List<Vector2Int>();
        List<Vector2Int> grounds = new List<Vector2Int>();
        Dictionary<int, List<Vector2Int>> doors = new Dictionary<int, List<Vector2Int>>();
        Vector2Int player = new Vector2Int(2,2);
        bool playerSet = false;
        Debug.Log(currentRoom);
        int x = map.room_location[currentRoom].x;
        int y = map.room_location[currentRoom].y;
        List<Task> tasks = new List<Task>();
        while (y < 10)
        {
            if(map.map[x, y] < 0)
            {
                BuildExit(map.map[x,y],currentRoom,new Vector2Int(x,y));
            }


            if (map.map[x, y] == 1 && x != 0 && y != 0 && y != height - 1 && x != width - 1)
            {
                walls.Add(new Vector2Int(x, y));
            }

            if (map.map[x,y] > 0)
            {
                grounds.Add(new Vector2Int(x, y));
            }

            if(map.map[x,y] == door && !playerSet)
            {
                player = new Vector2Int(x, y);
                playerSet = true;
 
            }
            
            if (x == 9)
            {
                y++;
                x = 0;
            }
            x++;
        }

        BuildGround(grounds);
        BuildWalls(walls);
        SpawnPlayer(player);
        */
        yield return null;
    }
}