using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public Tilemap ExitMap;

    int width;
    int height;

    LevelExit ExitComponent;
    MapGenerator map;
    public bool updated = true;
    private Parameters copy;

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
        ExitMap.SetTile(new Vector3Int(pos.x, pos.y, 0), exit);

        int[,] tempMap = map.GetMap;

        if(tempMap[pos.x - 1,pos.y]  == Mathf.Abs(doorNumber))
        {
            ExitMap.SetTile(new Vector3Int(pos.x, pos.y + 1, 0), exit);
            ExitMap.SetTile(new Vector3Int(pos.x, pos.y + 2, 0), exit);
            ExitMap.SetTile(new Vector3Int(pos.x + 1, pos.y, 0), exit);
            ExitMap.SetTile(new Vector3Int(pos.x + 1, pos.y + 1, 0), exit);
        }
        else if (tempMap[pos.x + 1, pos.y] == Mathf.Abs(doorNumber))
        {
            ExitMap.SetTile(new Vector3Int(pos.x , pos.y + 1, 0), exit);
            ExitMap.SetTile(new Vector3Int(pos.x, pos.y + 2, 0), exit);
            ExitMap.SetTile(new Vector3Int(pos.x - 1, pos.y, 0), exit);
            ExitMap.SetTile(new Vector3Int(pos.x - 1, pos.y + 1, 0), exit);
        }
        else if(tempMap[pos.x,pos.y+1] != Mathf.Abs(doorNumber))
        {
            PlaceWall(pos);
            if(tempMap[pos.x+1,pos.y+1] == 0)
            {
                PlaceWall(new Vector2Int(pos.x,pos.y+1));
            }
            else
            {
                PlaceWall(new Vector2Int(pos.x, pos.y - 1));
            }
        }

    }
    private void BuildWall(Vector2Int pos)
    {

        bool[] neigbours = new bool[8];


        neigbours[0] = map.GetMap[pos.x - 1, pos.y + 1] == 1;
        neigbours[1] = map.GetMap[pos.x, pos.y + 1] == 1;
        neigbours[2] = map.GetMap[pos.x + 1, pos.y + 1] == 1;
        neigbours[3] = map.GetMap[pos.x + 1, pos.y] == 1;
        neigbours[4] = map.GetMap[pos.x + 1, pos.y - 1] == 1;
        neigbours[5] = map.GetMap[pos.x, pos.y - 1] == 1;
        neigbours[6] = map.GetMap[pos.x - 1, pos.y - 1] == 1;
        neigbours[7] = map.GetMap[pos.x - 1, pos.y] == 1;

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

    private void PlaceItems()
    {
        List<Vector2Int> points = new List<Vector2Int>();
        foreach(Vector2Int point in points)
        {
            PlaceItem(point);
        }
    }
    private void PlaceItem(Vector2Int point)
    {
        GameObject prefab = null;
        GameObject item = Instantiate(prefab, new Vector3(point.x, point.y, 0), Quaternion.identity);


    }
    private void PlaceMonsters(int room, List<Tuple<int,int>> monsters)
    {
        
        List<Vector2Int> points = map.PointsInRoom(room,monsters.Count);
        for (int i = 0; i < points.Count; i++)
        {
            Vector2Int point = points[i];
            PlaceMonster(point, monsters[i].Item1);
            monsters[i] = new Tuple<int, int>(monsters[i].Item1, monsters[i].Item2 - 1);
            if (monsters[i].Item2 == 0)
                monsters.RemoveAt(i);

        }
    }
    private void PlaceMonster(Vector2Int point, int power)
    {
        GameObject prefab = null;// = all_monster[power];
        GameObject monster = Instantiate(prefab, new Vector3(point.x, point.y, 0), Quaternion.identity);
    }

    private async Task<int> PlaceLevel(Parameters param)
    {
        int currentRoom = map.GenerateLevel(param);

        PlaceMonsters(currentRoom, param.Monsters);

        PlaceItems();

        return currentRoom;
    }

    public async Task CreateNewLevel(Parameters param)
    {
        int x = await PlaceLevel(param);
        copy = param;
        updated = false;
        return;
    }

    private IEnumerator RendererLevel(Parameters copy)
    {
        updated = true;
        Wallsmap.ClearAllTiles();
        GroundsMap.ClearAllTiles();

        bool playerSet = false;
        Dictionary<Vector2Int, int> points = map.GetRoom(copy.room);

        foreach (KeyValuePair<Vector2Int, int> point in points)
        {
            int currentLocation = point.Value;

            if (currentLocation == 0)
            {
                BuildWall(point.Key);
            }
            else if (currentLocation > 0)
            {
                if (currentLocation % 2 == 0)
                {
                    BuildSpike(point.Key);
                }
                else
                {
                    BuildGround(point.Key);
                }

            }
            else if (currentLocation < 0)
            {
                Tuple<int, int> rooms = map.Connected_Doors[currentLocation];

                int room = rooms.Item1 == currentLocation ? rooms.Item2 : rooms.Item1;

                BuildExit(currentLocation, room, point.Key);
                if (Math.Abs(currentLocation) == copy.door && playerSet == false)
                {
                    SpawnPlayer(point.Key);
                    playerSet = true;
                }
            }
        }
        yield return null;
    }

    #region UnityCalls
    void Awake()
    {
        LevelInstance = this;
        config = new LevelConfig();
        config.Width = ConfigurationManager.ConfigInstance.getConfig<int>("Width");
        config.Height = ConfigurationManager.ConfigInstance.getConfig<int>("Height");
        map = new MapGenerator();

        ExitComponent = ExitMap.gameObject.GetComponent<LevelExit>();
    }
    public void Update()
    {
        if (updated == false)
        {
            StartCoroutine(RendererLevel(copy));
        }
    }
    #endregion

}