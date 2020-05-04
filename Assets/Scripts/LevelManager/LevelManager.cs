using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
public struct Parameters
{
    public int room;
    public int door;
    public int roomSize;
    public double spikeRate;
    public bool narrowRoom;
    public bool deadEnd;
    public bool cycles;
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
    private void BuildGround(Vector2Int pos)
    {
        GroundsMap.SetTile(new Vector3Int(pos.x, pos.y, 0), ground);
    }

    private void BuildExit(int doorNumber, int roomNumber,Vector2Int pos)
    {

        //Needed To Visualize if()
        GroundsMap.SetTile(new Vector3Int(pos.x, pos.y, 0), exit);

    }
    private void BuildWall(Vector2Int pos)
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

    private void BuildSpike(Vector2Int key)
    {
        throw new NotImplementedException();
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
        return map.GenerateLevel(param);
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
    {
        updated = true;
        Wallsmap.ClearAllTiles();
        GroundsMap.ClearAllTiles();

        bool playerSet = false;
        Dictionary<Vector2Int, int> points = map.GetRoom(currentRoom);

        foreach(KeyValuePair<Vector2Int,int> point in points)
        {
            int currentLocation = point.Value;

            if (currentLocation == 0)
            {
                BuildWall(point.Key);
            }
            else if (currentLocation > 0)
            {
                if( currentLocation % 2 == 0)
                {
                    BuildSpike(point.Key);
                }
                else
                {
                    BuildGround(point.Key);
                }

            }
            else if ( currentLocation < 0 )
            {
                Tuple<int, int> rooms = map.connected_doors[currentLocation];

                int room = rooms.Item1 == currentLocation ? rooms.Item2 : rooms.Item1;

                BuildExit(currentLocation,room,point.Key);
                if (Math.Abs(currentLocation)  == door && playerSet == false)
                {
                    SpawnPlayer(point.Key);
                    playerSet = true;
                }
            }
        }
        yield return null;
    }
}