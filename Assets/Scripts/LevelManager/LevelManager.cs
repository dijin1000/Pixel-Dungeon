using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class MonsterCategory
{
    public List<GameObject> monsters;
    public int difficulty;
}

public class LevelManager : MonoBehaviour
{
    private GameObject player;
    private LevelExit exitComponent;
    private MapGenerator generator;
    private bool updated = true;
    private Parameters copy;

    [SerializeField]
    private List<MonsterCategory> all_monster = new List<MonsterCategory>();
    [SerializeField]
    private List<GameObject> prefabPlayer = new List<GameObject>();

    //Different Tile bases for different tile maps
    public TileBase wall;
    public TileBase ground;
    public TileBase exit;
    public TileBase finalExit;
    public TileBase spike;

    public Tilemap Wallsmap;
    public Tilemap GroundsMap;
    public Tilemap ExitMap;
    public Tilemap SpikeMap;


    /// <summary>
    /// Signleton Pattern
    /// </summary>
    private static LevelManager levelInstance;
    private int FinalDoorMark;

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

    #region Static Placements
    private void PlaceWallAndNeigbours(Vector2Int pos)
    {
        bool[] neigbours = new bool[8];

        neigbours[0] = generator.GetMap[pos.x - 1, pos.y + 1] == 0;
        neigbours[1] = generator.GetMap[pos.x, pos.y + 1] == 0;
        neigbours[2] = generator.GetMap[pos.x + 1, pos.y + 1] == 0;
        neigbours[3] = generator.GetMap[pos.x + 1, pos.y] == 0;
        neigbours[4] = generator.GetMap[pos.x + 1, pos.y - 1] == 0;
        neigbours[5] = generator.GetMap[pos.x, pos.y - 1] == 0;
        neigbours[6] = generator.GetMap[pos.x - 1, pos.y - 1] == 0;
        neigbours[7] = generator.GetMap[pos.x - 1, pos.y] == 0;

        PlaceWallPrime(pos, neigbours);
        PlaceGround(pos);

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
        Wallsmap.SetTile(new Vector3Int(pos.x, pos.y, 0), wall);
    }
    private void PlaceGround(Vector2Int pos)
    {
        GroundsMap.SetTile(new Vector3Int(pos.x, pos.y, 0), ground);
    }
    private void PlaceExit(int doorNumber, Vector2Int pos)
    {
        bool isLastDoor = doorNumber == FinalDoorMark;
        exitComponent.isLastDoor.Add(pos,isLastDoor);
        exitComponent.doors.Add(pos, doorNumber);

        TileBase Exit = isLastDoor ? finalExit : exit;

        ExitMap.SetTile(new Vector3Int(pos.x, pos.y, 0), Exit);
        PlaceGround(pos);

        int[,] tempMap = generator.GetMap;

        if (tempMap[pos.x, pos.y - 1] > 0)
        {
            if (Mathf.Abs(tempMap[pos.x - 1, pos.y]) == Mathf.Abs(doorNumber))
            {
                ExitMap.SetTile(new Vector3Int(pos.x, pos.y + 1, 0), Exit);
                ExitMap.SetTile(new Vector3Int(pos.x, pos.y + 2, 0), Exit);
                ExitMap.SetTile(new Vector3Int(pos.x + 1, pos.y, 0), Exit);
                ExitMap.SetTile(new Vector3Int(pos.x + 1, pos.y + 1, 0), Exit);
            }
            else if (Mathf.Abs(tempMap[pos.x + 1, pos.y]) == Mathf.Abs(doorNumber))
            {
                ExitMap.SetTile(new Vector3Int(pos.x, pos.y + 1, 0), Exit);
                ExitMap.SetTile(new Vector3Int(pos.x, pos.y + 2, 0), Exit);
                ExitMap.SetTile(new Vector3Int(pos.x - 1, pos.y, 0), Exit);
                ExitMap.SetTile(new Vector3Int(pos.x - 1, pos.y + 1, 0), Exit);
            }
        }
    }

    private void RemoveWall(Vector2Int pos)
    {
        Wallsmap.SetTile(new Vector3Int(pos.x, pos.y, 0), null);
    }

    private void PlaceSpike(Vector2Int pos)
    {
        SpikeMap.SetTile(new Vector3Int(pos.x, pos.y, 0), spike);
    }
    #endregion

    #region Object Placement
    #region Item Placement
    private void SpawnItems()
    {
        List<Vector2Int> points = new List<Vector2Int>();
        foreach(Vector2Int point in points)
        {
            SpawnItem(point);
        }
    }
    private void SpawnItem(Vector2Int point)
    {
        GameObject prefab = null;
        GameObject item = Instantiate(prefab, new Vector3(point.x, point.y, 0), Quaternion.identity);
    }
    #endregion
    #region Monster Placement
    private void SpawnMonsters(int room, List<Tuple<int,int>> monsters)
    {
        if (monsters != null && monsters.Count > 0)
        {
            List<Vector2Int> points = generator.PointsInRoom(room, monsters.Select(predicate => predicate.Item2).Sum());
            for (int i = 0; i < points.Count; i++)
            {
                Vector2Int point = points[i];
                SpawnMonster(point, monsters[0].Item1);
                monsters[0] = new Tuple<int, int>(monsters[0].Item1, monsters[0].Item2 - 1);
                if (monsters[0].Item2 == 0)
                    monsters.RemoveAt(0);

            }
        }
    }
    private void SpawnMonster(Vector2Int point, int power)
    {
        List<GameObject> powerLeveledMonsters = all_monster[power].monsters;
        GameObject prefab = powerLeveledMonsters[UnityEngine.Random.Range(0, powerLeveledMonsters.Count)];
        GameObject monster = Instantiate(prefab, new Vector3(point.x, point.y, 0), Quaternion.identity);
    }
    #endregion
    private void SpawnPlayer(Vector2 pos)
    { 
        if (player == null)
            player = Instantiate(prefabPlayer[UnityEngine.Random.Range(0, prefabPlayer.Count)], pos, Quaternion.identity);
        else
        {
            player = Instantiate(player, pos, Quaternion.identity);
        }
    }
    #endregion

    private IEnumerator PlaceLevel(Parameters param)
    {
        int currentRoom = generator.GenerateLevel(param);


        int door = param.door;
        SpawnMonsters(currentRoom, param.Monsters);

        //SpawnItems();


        if (param.room == 0)
        {
            SpawnPlayer(generator.PointsInRoom(currentRoom,1).FirstOrDefault());
        }
        else
        {
            Dictionary<Vector2Int,int> room = generator.GetRoom(currentRoom);
            Vector2Int location = room.FirstOrDefault(predicate => Mathf.Abs(predicate.Value) == param.door).Key;

            Vector2Int Up = new Vector2Int(location.x, location.y + 1);
            Vector2Int Down = new Vector2Int(location.x, location.y - 1);
            Vector2Int Left = new Vector2Int(location.x - 1, location.y);
            Vector2Int Right = new Vector2Int(location.x + 1, location.y);

            if (room.ContainsKey(Up) && room[Up] > 1)
                SpawnPlayer(Up);
            else if (room.ContainsKey(Down) && room[Down] > 1)
                SpawnPlayer(Down);
            else if (room.ContainsKey(Left) && room[Left] > 1)
                SpawnPlayer(Left);
            else if (room.ContainsKey(Right) && room[Right] > 1)
                SpawnPlayer(Right);
        }
        exitComponent.doors.Clear();
        exitComponent.isLastDoor.Clear();
        param.room = currentRoom;
        copy = param;


        if(param.lastDoor)
        {
            List<int> doors = generator.GetRoom(currentRoom).Select(predicate => predicate.Value).Where(predicate => predicate < 0).Distinct().ToList();

            int randomIndex = UnityEngine.Random.Range(0, doors.Count);
            FinalDoorMark = doors[randomIndex];
        }
        yield return null;
    }

    public IEnumerator CreateNewLevel(Parameters param)
    {
        yield return PlaceLevel(param);
        updated = false;
        DirectorManager.DirectorInstance.currentState = copy;
        yield return RendererLevel(copy);
    }

    private IEnumerator RendererLevel(Parameters copy)
    {

        updated = true;
        Wallsmap.ClearAllTiles();
        GroundsMap.ClearAllTiles();
        ExitMap.ClearAllTiles();
        SpikeMap.ClearAllTiles();

        Dictionary<Vector2Int, int> points = generator.GetRoom(copy.room);

        foreach (KeyValuePair<Vector2Int, int> point in points)
        {
            int currentLocation = point.Value;

            if (currentLocation == 0)
            {
                PlaceWallAndNeigbours(point.Key);
            }
            else if (currentLocation > 0)
            {
                if (currentLocation % 2 == 1)
                {
                    PlaceSpike(point.Key);
                }
                else
                {
                    PlaceGround(point.Key);
                }

            }
            else if (currentLocation < 0)
            {
                PlaceExit(Mathf.Abs(currentLocation), point.Key);
            }
        }
        
        yield return null;
    }

    #region UnityCalls
    void Awake()
    {
        LevelInstance = this;
        generator = new MapGenerator();

        exitComponent = ExitMap.gameObject.GetComponent<LevelExit>();
    }
    public void Update()
    {
        if (updated == false)
            StartCoroutine(RendererLevel(copy));
    }
    #endregion

}