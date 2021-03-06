using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

class MapGenerator
{
    
    struct Door
    {
        public int doorNumber;
        // We store the coordinates of both of the tiles a door consists of
        public Vector2Int left;  // Stores the left tile of the door, when you look at the door from inside the room
        public Vector2Int right; // Stores the right tile of the door, when you look at the door from inside the room

        // Return a unit vector pointing in the direction the player will walk when it walk out of the room through this
        // door
        public Vector2Int outwards()
        {
            if (left + RIGHT == right)
                return UP;
            if (left + UP == right)
                return LEFT;
            if (left + LEFT == right)
                return DOWN;
            return RIGHT;
        }

        // Return the room number of the room this door belongs to
        public int roomNumber(int[,] map)
        {
            foreach (Vector2Int unit in perpUnits)
            {
                Vector2Int point = left + unit;
                if (map[point.x, point.y] > 0) // tiles belonging to the floor of a room have positive value
                {
                    return map[point.x, point.y] / 2; // return the room number
                }
            }
            // We should never get here...
            return 0; // This is an invalid room number: room numbers should be positive
        }
    }

    struct Rect
    {
        public Vector2Int topLeft;
        public int width;
        public int height;
    }
    
    struct Component
    {
        public int color;
        public Vector2Int point; // any point belonging to the component
        public int area;
    }    
    private static Vector2Int LEFT = Vector2Int.left;
    private static Vector2Int RIGHT = Vector2Int.right;
    private static Vector2Int UP = Vector2Int.down; // we use positive y direction is down
    private static Vector2Int DOWN = Vector2Int.up; // so these are not typos
    private static Vector2Int[] perpUnits = {LEFT, RIGHT, UP, DOWN};




    int roomCounter; // total number of rooms currently in the game; every room has an associated room number,
                    // so during the game these room numbers are {1, 2, 3, ..., roomCounter}

    int doorCounter; // Counts the total number of unique doors in the game. Uniqueness here means: if door A in
                    // room 1 is connected to door B in room 2, then door A and B are the same door. Hence they
                    // are also given the same door number

    const int mapWidth = 1500, mapHeight = 1500;
    int[, ] map;  // For 0 <= x < mapWidth and 0 <= y < mapHeight we will have:
                  // map[x,y] == 0    means tile at (x,y) is unwalkable wall
                  // map[x,y] > 0     means tile at (x,y) is walkable ground;
                  //                      the value map[x,y]/2 denotes the room number this tile belongs to
                  //                      the value map[x,y]%2 is 0 for empty ground and is 1 for spiked ground
                  // map[x,y] < 0     means tile at (x,y) is a door;
                  //                      the absolute value of map[x,y] denotes the door number this tile belongs to

    Dictionary<int, Vector2Int> room_location; // keys are room numbers of already existing rooms
                                               // room_location[r] gives the coordinates of some tile belonging to room
                                               // number r
                                               // So if room_location[r] == (x,y), then we know that map[x,y]/2 == r

    Dictionary<int, Tuple<int, int>> connected_doors; // keys are door numbers of doors that are connected to two rooms
                                                    // connected_doors[d] gives the two room numbers that this door connects
                                                    // warning: the order of the room numbers is undefined
                                                    
    // constants to denote types of room space. Should be bigger than roomCounter will ever be
    private const int FREE = 9999999;
    private const int FULL = 9999998;
    private const int BLOCKED = 9999997;
    private const int FULL_BLOCKED = 9999996;
    private const int SPIKES = 9999995;
    private const int GROUND = 9999994;
    private const int SPIKED_GROUND = 9999993;

    private const double MIN_DRAW_RANDOMNESS = 0.4;
    private const double MAX_DRAW_RANDOMNESS = 0.6;
    private const int MIN_ROOM_SIZE = 15;
    private const int MAX_ROOM_SIZE = 40;
    private const int MIN_ROOM_AREA = 40;
    private const int TRIES_PER_OFFSET = 3;
    private const double MIN_SPIKE_RATE = 0.0;
    private const double MAX_SPIKE_RATE = 0.9;
    private const int SPIKE_STAMP_SIZE = 2;
    private const int MIN_EXTRA_DOORS = 2; // The minimal number of unconnected doors on the map
    private const int NORMAL_AVG_EXTRA_DOORS = 4;
    private const int CYCLES_AVG_EXTRA_DOORS = 6;
    private const double NORMAL_CONNECT_DISTANCE_OVER_ROOM_SIZE_RATE = 0.4;
    private const double CYCLES_CONNECT_DISTANCE_OVER_ROOM_SIZE_RATE = 0.6;

    private System.Random rand = new System.Random();
    

    private int maxRoomWidth = 25;
    private int maxRoomHeight = 25;
    private int maxConnectDistance = 12; // maximal distance on the map for two doors to be connected
    private double drawRandomness = 0.5; // between 0 and 1
    private int avgExtraDoors = 4; // The average number of unconnected doors on the map
    private double spikeRate = 0.2;
    private bool narrowRooms = false;
    private bool extraDeadEnds = false;
    private bool placeNewDoorsTowardsRooms = false;

    private const bool verbose = true;
       
    
    public int[,] GetMap
    {
        get
        {
            return map;
        }
    }

    public Dictionary<int, Tuple<int,int>> Connected_Doors
    {
        get
        {
            return connected_doors;
        }
    }

    public MapGenerator()
    {
        int seed = rand.Next();
        rand = new System.Random(seed);
        
        roomCounter = 0;
        doorCounter = 0;
        map = new int[mapWidth, mapHeight];
        room_location = new Dictionary<int, Vector2Int>();
        connected_doors = new Dictionary<int, Tuple<int, int>>();
        for (int x = 0; x < mapWidth; x++)
        for (int y = 0; y < mapHeight; y++)
            map[x, y] = 0;
    }

    public int GenerateLevel(Parameters par)
    {
        int doorNumber = par.door; // the number of the door the player just walked through
        int old_roomNumber = par.room; // the number of the room the player was in before walking through the door
        if (verbose)
            Console.WriteLine("GenerateLevel({0}, {1})", doorNumber, old_roomNumber);

        set_parameters(par);

        if (roomCounter <= 0) // map is empty, no rooms exists yet
            return createFirstRoom();

        if (doorNumber <= 0 || old_roomNumber <= 0) // invalid. Probably want the first room
            return 1;

        if (connected_doors.ContainsKey(doorNumber)) // if this door already connects two rooms
        {
            // just look up which already existing room the player will walk into:
            Tuple<int, int> rooms = connected_doors[doorNumber];
            int new_roomNumber = rooms.Item1 == old_roomNumber ? rooms.Item2 : rooms.Item1;
            return new_roomNumber;
        }

        // We need to generate a new room
        
        roomCounter++; // We already increase our roomCounter
        // So the room number of the room that we are going to build below will be roomCounter.
        
        // Compute the door that the player just walked through:
        Door exitDoor = search_door(old_roomNumber, doorNumber);
        
        
        int[,] scratchpad = new int[maxRoomWidth, maxRoomHeight];
        Rect scratchpadRect = position_and_draw_room(scratchpad, exitDoor);
        // Most of our scratchpad is FREE. Tiles that are within distance 1 from existing rooms on our map are marked FULL
        
        
        Rect searchRect = compute_search_rect(scratchpadRect, scratchpad);
        List<Door> connectableDoors = search_connectable_doors(searchRect);
        if (!connectableDoors.Contains(exitDoor))
            connectableDoors.Insert(0, exitDoor);
        foreach (Door door in connectableDoors)
        {
            draw_and_connect_door(scratchpad, scratchpadRect, door);
        }
        // scratchpad state: doors are marked -doorNumber, tiles around & behind doors are marked (FULL_)BLOCKED, other open tiles
        // are marked FREE, the rest is FULL
        
        build_new_doors(scratchpad, scratchpadRect, connectableDoors);
        // scratchpad state: doors are marked -doorNumber, tiles around & behind doors are marked (FULL_)BLOCKED, other open tiles
        // are marked FREE, the rest is FULL
        
        add_spikes(scratchpad);
        // scratchpad state: doors are marked -doorNumber, tiles around & behind doors are marked (FULL_)BLOCKED, open tiles with
        // spikes are marked SPIKES, other open tiles are marked FREE, the rest is FULL
        
        // Remove blocked areas around doors, set them to FREE/FULL
        unblock(scratchpad);
        // scratchpad state: doors are marked -doorNumber,  open tiles with spikes are marked SPIKES, other open tiles
        // are marked FREE, the rest is FUL

        // Register the location of our new room:
        Vector2Int pointInRoom = point_in_room(scratchpad);
        Vector2Int pointInRoomOnMap = pointInRoom + scratchpadRect.topLeft;
        room_location[roomCounter] = pointInRoomOnMap;
        
        remove_size_1_bulges(scratchpad, pointInRoom);
        // scratchpad state: doors are marked -doorNumber,  open tiles with spikes are marked SPIKED_GROUND, other open
        // tiles are marked GROUND, the rest is FULL
        
        draw_scratchpad_on_map(scratchpadRect, scratchpad);
        
        return roomCounter;
    }

    public Dictionary<Vector2Int, int> GetRoom(int roomNumber)
    {
        Dictionary<Vector2Int, int> points = new Dictionary<Vector2Int, int>();
        collect_room_points(points, room_location[roomNumber]);
        return points;
    }

    public List<Vector2Int> PointsInRoom(int roomNumber, int numPoints)
        // return a random list of numPoints different points inside room number roomNumber
    {
        List<Vector2Int> points = new List<Vector2Int>();
        Vector2Int inside = room_location[roomNumber];
        while (points.Count < numPoints)
        {
            int x = rand.Next(inside.x - maxRoomWidth, inside.x + maxRoomWidth);
            int y = rand.Next(inside.y - maxRoomHeight, inside.y + maxRoomHeight);
            Vector2Int newPoint = new Vector2Int(x, y);
            if (map[x, y] / 2 == roomNumber && !points.Contains(newPoint))
                points.Add(newPoint);
        }

        return points;
    }

    /************************************
     *** GenerateLevel() subfunctions ***
     ************************************/
    
    private void set_parameters(Parameters par)
    {
        drawRandomness = random_between(MIN_DRAW_RANDOMNESS, MAX_DRAW_RANDOMNESS);
        int roomSize = Math.Max(MIN_ROOM_SIZE, Math.Min(par.roomSize, MAX_ROOM_SIZE));
        maxRoomWidth = roomSize;
        maxRoomHeight = roomSize;
        spikeRate = Math.Max(MIN_SPIKE_RATE, Math.Min(par.spikeRate, MAX_SPIKE_RATE));
        narrowRooms = par.narrowRoom;
        extraDeadEnds = par.deadEnd;

        if (par.cycles)
        {
            maxConnectDistance = Convert.ToInt32(roomSize * CYCLES_CONNECT_DISTANCE_OVER_ROOM_SIZE_RATE);
            placeNewDoorsTowardsRooms = true;
            avgExtraDoors = CYCLES_AVG_EXTRA_DOORS;
        }
        else
        {
            maxConnectDistance = Convert.ToInt32(roomSize * NORMAL_CONNECT_DISTANCE_OVER_ROOM_SIZE_RATE);
            placeNewDoorsTowardsRooms = false;
            avgExtraDoors = NORMAL_AVG_EXTRA_DOORS;
        }
    }

    private int createFirstRoom()
    {
        roomCounter++;
        Rect rect = new Rect();
        rect.topLeft = new Vector2Int(mapWidth / 2, mapHeight / 2);
        rect.width = maxRoomWidth;
        rect.height = maxRoomHeight;
        
        int[,] scratchpad = new int[maxRoomWidth, maxRoomHeight];
        initialize_scratchpad(rect, scratchpad);
        draw_room(scratchpad);
        
        Vector2Int pointInRoom = point_in_room(scratchpad);
        Vector2Int pointInRoomOnMap = pointInRoom + rect.topLeft;
        room_location[roomCounter] = pointInRoomOnMap;

        draw_new_door(scratchpad, LEFT);
        
        add_spikes(scratchpad);
        
        unblock(scratchpad);

        remove_size_1_bulges(scratchpad, pointInRoom);
        
        draw_scratchpad_on_map(rect, scratchpad);
        
        return roomCounter;
    }

    private Door search_door(int old_roomNumber, int doorNumber)
    {
        Vector2Int start = room_location[old_roomNumber];
        bool doorFound = false;
        Vector2Int doorPos = new Vector2Int();
        for (int i = 1; !doorFound; i++)
        {
            for (int j = -i; j < i; j++)
            {
                if (map[start.x + j, start.y - i] == -doorNumber)
                {
                    doorFound = true;
                    doorPos = new Vector2Int(start.x + j, start.y - i);
                    break;
                }

                if (map[start.x + i, start.y + j] == -doorNumber)
                {
                    doorFound = true;
                    doorPos = new Vector2Int(start.x + i, start.y + j);
                    break;
                }

                if (map[start.x - j, start.y + i] == -doorNumber)
                {
                    doorFound = true;
                    doorPos = new Vector2Int(start.x - j, start.y + i);
                    break;
                }

                if (map[start.x - i, start.y - j] == -doorNumber)
                {
                    doorFound = true;
                    doorPos = new Vector2Int(start.x - i, start.y - j);
                    break;
                }
            }
        }

        return get_door(doorPos);
    }

    private Rect position_and_draw_room(int[,] scratchpad, Door exitDoor)
    {
        int area = -1;
        Rect scratchpadRect = new Rect();
        int offset = 0;
        while (area < MIN_ROOM_AREA)
        {
            scratchpadRect = compute_scratchpad_rect(exitDoor, offset);
            for (int i = 0; i < TRIES_PER_OFFSET && area < MIN_ROOM_AREA; i++)
            {
                initialize_scratchpad(scratchpadRect, scratchpad);
                area = draw_room(scratchpad);
            }
            offset += maxRoomHeight / 2;
        }

        return scratchpadRect;
    }

    private Rect compute_room_rect(Rect scratchpadRect, int[,] scratchpad)
    {
        int min_x = 999999, max_x = -1, min_y = 999999, max_y = -1;
        for (int x = 0; x < maxRoomWidth; x++)
        {
            for (int y = 0; y < maxRoomHeight; y++)
            {
                if (scratchpad[x, y] == FREE)
                {
                    min_x = Math.Min(x, min_x);
                    max_x = Math.Max(x, max_x);
                    min_y = Math.Min(y, min_y);
                    max_y = Math.Max(y, max_y);
                }
            }
        }
        Rect roomRect = new Rect();
        roomRect.width = max_x - min_x + 1;
        roomRect.height = max_y - min_y + 1;
        roomRect.topLeft = new Vector2Int();
        roomRect.topLeft.x = scratchpadRect.topLeft.x + min_x;
        roomRect.topLeft.y = scratchpadRect.topLeft.y + min_y;
        return roomRect;
    }

    private Rect compute_search_rect(Rect scratchpadRect, int[,] scratchpad)
    {
        Rect roomRect = compute_room_rect(scratchpadRect, scratchpad);
        Rect searchRect = new Rect();
        searchRect.width = roomRect.width + 2 * maxConnectDistance;
        searchRect.height = roomRect.height + 2 * maxConnectDistance;
        searchRect.topLeft = roomRect.topLeft - new Vector2Int(maxConnectDistance, maxConnectDistance);
        return searchRect;
    }

    private List<Door> search_connectable_doors(Rect searchRect)
    {
        List<Door> list = new List<Door>();
        for (int x = searchRect.topLeft.x; x < searchRect.topLeft.x + searchRect.width; x++)
        {
            for (int y = searchRect.topLeft.y; y < searchRect.topLeft.y + searchRect.height; y++)
            {
                if (map[x, y] < 0)
                {
                    Door door = get_door(new Vector2Int(x, y));
                    if (!connected_doors.ContainsKey(door.doorNumber)) // if this door is still unconnected
                    {
                        if (good_orientation(searchRect, door, x, y))
                        {
                            bool foundDoorAlready = false;
                            foreach (Door foundDoor in list)
                            {
                                if (foundDoor.doorNumber == door.doorNumber)
                                {
                                    foundDoorAlready = true;
                                    break;
                                }
                            }
                            if (!foundDoorAlready)
                                list.Add(door);
                            
                        }
                    }
                }
            }
        }
        return list;
    }
    
    private void draw_and_connect_door(int[,] scratchpad, Rect scratchpadRect, Door door)
    {
        Vector2Int start = clip(scratchpadRect, door);
        bool[,] queued = new bool[maxRoomWidth, maxRoomHeight]; // remember which coordinates we queued already
        // At the start, we did not queue anything yet
        for (int x = 0; x < maxRoomWidth; x++)
            for (int y = 0; y < maxRoomHeight; y++)
                queued[x, y] = false;
         
        // The following priority queue dequeues smallest elements first.
        PriorityQueue<double, Vector2Int> pq = new PriorityQueue<double, Vector2Int>();
        pq.Enqueue(0, start);
        queued[start.x, start.y] = true;
        while (!pq.IsEmpty)
        {
            Vector2Int candidate = pq.Dequeue();
            if (place_connected_door(scratchpad, door, candidate))
            {
                return;
            }
                     
            queued[candidate.x, candidate.y] = true;
         
            List<Vector2Int> neighbors = getNeighbors(candidate);
            foreach (Vector2Int neighbor in neighbors)
            {
                if (!queued[neighbor.x, neighbor.y])
                {
                    double priority = Vector2Int.Distance(start, neighbor);
                    pq.Enqueue(priority, neighbor);
                    queued[neighbor.x, neighbor.y] = true;
                }
            }
        }
        
        // We should not get here
        // If we get here, we did not find any spot to place the door. The door will stay unconnected...
        return;
    }

    private void build_new_doors(int[,] scratchpad, Rect scratchpadRect, List<Door> doorsInRoomAlready)
    {
        int num_new_doors = number_of_new_doors(doorsInRoomAlready.Count);
        List<Vector2Int> dirs = door_placement_direction_list(scratchpad, scratchpadRect, doorsInRoomAlready);
        
        int doors_build = 0;
        int dir_idx = 0;
        while (doors_build < num_new_doors)
        {
            draw_new_door(scratchpad, dirs[dir_idx]);
            doors_build++;
            dir_idx = (dir_idx + 1) % dirs.Count;
        }
    }

    private void add_spikes(int[,] scratchpad)
    {
        int area = count_FREEs(scratchpad);
        int spike_area_goal = Convert.ToInt32(Math.Floor(spikeRate * area));
        int spike_area = 0;
        while (spike_area < spike_area_goal)
        {
            spike_area += spike_stamp(scratchpad);
        }
    }
    
    private void unblock(int[,] scratchpad)
    {
        for (int x = 0; x < maxRoomWidth; x++)
        {
            for (int y = 0; y < maxRoomHeight; y++)
            {
                if (scratchpad[x, y] == BLOCKED)
                    scratchpad[x, y] = FREE;
                if (scratchpad[x, y] == FULL_BLOCKED)
                    scratchpad[x, y] = FULL;
            }
        }
    }

    private Vector2Int point_in_room(int[,] scratchpad)
    {
        Vector2Int middle = new Vector2Int(maxRoomWidth / 2, maxRoomHeight / 2);
        Vector2Int middlestPointInside = new Vector2Int(maxRoomWidth / 2, maxRoomWidth / 2);
        double minDist = 999999;
        for (int x = 0; x < maxRoomWidth; x++)
        {
            for (int y = 0; y < maxRoomHeight; y++)
            {
                Vector2Int current = new Vector2Int(x, y);
                if (scratchpad[x, y] == FREE && Vector2Int.Distance(current, middle) < minDist)
                {
                    middlestPointInside = current;
                    minDist = Vector2Int.Distance(current, middle);
                }
            }
        }

        return middlestPointInside;
    }
    
    private void remove_size_1_bulges(int[,] scratchpad, Vector2Int point)
    {
        // We never call this function with point being a door.
        
        List<Vector2Int> neighbors = getNeighbors(point);
        if (neighbors.Count < 4) // if point lies on the edge of the scratchpad
            return; // note that in this case, point is FULL

        if (scratchpad[point.x, point.y] == FULL)
        {
            int count = 0; // count the number of non-open (FULL or door) neighbors
            foreach (Vector2Int neighbor in neighbors)
            {
                if (scratchpad[neighbor.x, neighbor.y] == FULL || scratchpad[neighbor.x, neighbor.y] < 0)
                    count++;
            }

            if (count >= 2) // if enough non-open neighbors
                return;
            
            // count < 2, so point is a bulge of size 1. Remove it
            scratchpad[point.x, point.y] = FREE;
        }
        
        // if we reach this point, point is FREE or SPIKES.
        // set to GROUND or SPIKED_GROUND
        scratchpad[point.x, point.y] = scratchpad[point.x, point.y] == FREE ? GROUND : SPIKED_GROUND;
        
        // recursively call remove_size_1_bulges for all its yet unvisited non-door neighbors
        foreach (Vector2Int neighbor in neighbors)
        {
            int neighborVal = scratchpad[neighbor.x, neighbor.y];
            if (neighborVal == FREE || neighborVal == SPIKES || neighborVal == FULL)
                remove_size_1_bulges(scratchpad, neighbor);
        }
    }
    
    private void draw_scratchpad_on_map(Rect scratchpadRect, int[,] scratchpad)
    {
        for (int x = 0; x < maxRoomWidth; x++)
        {
            for (int y = 0; y < maxRoomHeight; y++)
            {
                if (scratchpad[x, y] == GROUND)
                {
                    map[x + scratchpadRect.topLeft.x, y + scratchpadRect.topLeft.y] = 2 * roomCounter;
                }
                else if (scratchpad[x, y] == SPIKED_GROUND)
                {
                    map[x + scratchpadRect.topLeft.x, y + scratchpadRect.topLeft.y] = 2 * roomCounter + 1;
                }
                else if (scratchpad[x, y] < 0)
                {
                    map[x + scratchpadRect.topLeft.x, y + scratchpadRect.topLeft.y] = scratchpad[x, y];
                }
            }
        }
    }



    
    
    
    
    
    /*************************
     *** Utility functions ***
     *************************/
    
    private Door get_door(Vector2Int doorPos)
    {
        Door door = new Door();
        int doorNumber = -map[doorPos.x, doorPos.y];
        if (doorNumber <= 0)
            throw new Exception("Calling get_door() with coordinates that do not correspond to a door!");
        door.doorNumber = doorNumber;
        door.left = doorPos;
        door.right = doorPos;
        if (map[doorPos.x - 1, doorPos.y] > 0)
        {
            if (map[doorPos.x, doorPos.y - 1] == -doorNumber)
                door.left = new Vector2Int(doorPos.x, doorPos.y - 1);
            else
                door.right = new Vector2Int(doorPos.x, doorPos.y + 1);
        }
        else if (map[doorPos.x + 1, doorPos.y] > 0)
        {
            if (map[doorPos.x, doorPos.y - 1] == -doorNumber)
                door.right = new Vector2Int(doorPos.x, doorPos.y - 1);
            else
                door.left = new Vector2Int(doorPos.x, doorPos.y + 1);
        }
        else if (map[doorPos.x, doorPos.y - 1] > 0)
        {
            if (map[doorPos.x - 1, doorPos.y] == -doorNumber)
                door.right = new Vector2Int(doorPos.x - 1, doorPos.y);
            else
                door.left = new Vector2Int(doorPos.x + 1, doorPos.y);
        }
        else
        {
            if (map[doorPos.x - 1, doorPos.y] == -doorNumber)
                door.left = new Vector2Int(doorPos.x - 1, doorPos.y);
            else
                door.right = new Vector2Int(doorPos.x + 1, doorPos.y);
        }
        return door;
    }

    public List<Vector2Int> allNeighbors(Vector2Int p)
    {
        // return all neighbors, including the diagonal ones
        List<Vector2Int> list = new List<Vector2Int>();
        list.Add(p + LEFT + UP);
        list.Add(p + LEFT);
        list.Add(p + LEFT + DOWN);
        list.Add(p + UP);
        list.Add(p + DOWN);
        list.Add(p + RIGHT + UP);
        list.Add(p + RIGHT);
        list.Add(p + RIGHT + DOWN);
        return list;
    }

    private int count_FREEs(int[,] scratchpad)
    {
        int count = 0;
        for (int x = 0; x < maxRoomWidth; x++)
        {
            for (int y = 0; y < maxRoomHeight; y++)
            {
                if (scratchpad[x, y] == FREE)
                    count++;
            }
        }

        return count;
    }

    private int random_from_distribution(int[] candidates, double[] chances)
    {
        // sum of the chances should be 1
        double p = rand.NextDouble();
        for (int i = 0; i < Math.Min(candidates.Length, chances.Length); i++)
        {
            if (p < chances[i])
                return candidates[i];
            p -= chances[i];
        }
        // we should never reach this point
        return candidates[0];
    }

    private double random_between(double min, double max)
    {
        return min + (max - min) * rand.NextDouble();
    }

    private void collect_room_points(Dictionary<Vector2Int, int> dict, Vector2Int point)
    {
        if (dict.ContainsKey(point))
            return;
        dict[point] = map[point.x, point.y];
        if (map[point.x, point.y] / 2 > 0) // if point is inside the room
        {
            // call collect_room_points on all its neighbors, including the diagonal ones
            foreach (Vector2Int neighbor in allNeighbors(point))
            {
                collect_room_points(dict, neighbor);
            }
        }
    }
    
    
    
    
    
    
    /*********************************************
     *** position_and_draw_room() subfunctions ***
     *********************************************/

    private Rect compute_scratchpad_rect(Door exitDoor, int offset)
    {
        Vector2Int outwards = exitDoor.outwards();
        Rect rect = new Rect();
        rect.width = maxRoomWidth;
        rect.height = maxRoomHeight;
        if (outwards.y == 0) // outwards is left or right
        {
            rect.topLeft.y = exitDoor.left.y - maxRoomHeight/2;
            if (outwards.x == 1) // right
                rect.topLeft.x = exitDoor.left.x + 1;
            else // left
                rect.topLeft.x = exitDoor.left.x - 1 - rect.width;
        }
        else // outwards is up or down
        {
            rect.topLeft.x = exitDoor.left.x - maxRoomWidth/2;
            if (outwards.y == 1) // down
                rect.topLeft.y = exitDoor.left.y + 1;
            else // up
                rect.topLeft.y = exitDoor.left.y - 1 - rect.height;
        }

        Vector2Int offsetVector = new Vector2Int(offset * outwards.x, offset * outwards.y);
        rect.topLeft += offsetVector;
        return rect;
    }
    
    private void initialize_scratchpad(Rect scratchpadRect, int[,] scratchpad)
    {
        int spWidth = scratchpadRect.width;
        int spHeight = scratchpadRect.height;
        for (int x = 0; x < spWidth; x++)
        {
            for (int y = 0; y < spHeight; y++)
            {
                scratchpad[x, y] = FREE;
            }
        }
        
        for (int x = -1; x <= spWidth; x++)
        {
            for (int y = -1; y <= spHeight; y++)
            {
                int xMap = x + scratchpadRect.topLeft.x;
                int yMap = y + scratchpadRect.topLeft.y;
                if (map[xMap, yMap] != 0)
                {
                    for (int i = -2; i <= 2; i++)
                    {
                        for (int j = -2; j <= 2; j++)
                        {
                            if (0 <= x + i && x + i < spWidth && 0 <= y + j && y + j < spHeight)
                                scratchpad[x + i, y + j] = FULL;
                        }
                    }
                }
            }
        }
    }
    
    private int draw_room(int[,] scratchpad)
        // Carves out a room, whose tiles are kept FREE. The rest is set to FULL. Returns area of the room
    {
        // Set probability distribution of the shapes of rooms. Sum of all chances should be 1!
        double chance_square = narrowRooms ? 0.0 : 0.3;
        double chance_rect = narrowRooms ? 0.5 : 0.2;
        double chance_diamond = 0.1;
        double chance_C = 0.2;
        double chance_X = 0.2;

        double p = rand.NextDouble();

        if (p < chance_square)
            return draw_square_room(scratchpad);
        p -= chance_square;

        if (p < chance_rect)
            return draw_rect_room(scratchpad);
        p -= chance_rect;
        
        if (p < chance_diamond)
            return draw_diamond_room(scratchpad);
        p -= chance_diamond;

        if (p < chance_C)
            return draw_C_room(scratchpad);
        p -= chance_C;

        if (p < chance_X)
            return draw_X_room(scratchpad);
        p -= chance_X;
        
        // We should never reach this point, but anyway we set a default:
        return draw_rect_room(scratchpad);
    }

    private void keep_only_one_component(int [,] scratchpad, int colorToKeep)
    {
        for (int x = 0; x < maxRoomWidth; x++)
        {
            for (int y = 0; y < maxRoomHeight; y++)
            {
                if (scratchpad[x, y] == colorToKeep)
                    scratchpad[x, y] = FREE;
                else
                    scratchpad[x, y] = FULL;
            }
        }
    }

    private Component color_components(int[,] scratchpad)
    {
        Component biggestComponent = new Component();
        biggestComponent.area = 0;
        int color = 1;
        for (int x = 0; x < maxRoomWidth; x++)
        {
            for (int y = 0; y < maxRoomHeight; y++)
            {
                if (scratchpad[x, y] == FREE)
                {
                    int area = flood(scratchpad, new Vector2Int(x, y), color);
                    if (area > biggestComponent.area)
                    {
                        biggestComponent.color = color;
                        biggestComponent.area = area;
                        biggestComponent.point = new Vector2Int(x, y);
                    }
                    color++;
                }
            }
        }
        return biggestComponent;
    }

    private int flood(int[,] scratchpad, Vector2Int pos, int color)
    {
        scratchpad[pos.x, pos.y] = color;
        int count = 1;
        List<Vector2Int> neighbors = getNeighbors(pos);
        foreach (Vector2Int neighbor in neighbors)
        {
            if (scratchpad[neighbor.x, neighbor.y] == FREE)
                count += flood(scratchpad, neighbor, color);
        }
        return count;
    }

    private void floodFill(int[,] scratchpad)
    {
        for (int x = 0; x < maxRoomWidth; x++)
            for (int y = 0; y < maxRoomHeight; y++)
                if (scratchpad[x, y] == FREE && (x == 0 || x == maxRoomWidth-1 || y == 0 || y == maxRoomHeight-1))
                    flood(scratchpad, new Vector2Int(x, y), FULL);
    }

    private List<Vector2Int> getNeighbors(Vector2Int p)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        if (p.x > 0)
            neighbors.Add(p + LEFT);
        if (p.x < maxRoomWidth-1)
            neighbors.Add(p + RIGHT);
        if (p.y > 0)
            neighbors.Add(p + UP);
        if (p.y < maxRoomHeight-1)
            neighbors.Add(p + DOWN);
        return neighbors;
    }

    private Vector2Int chooseNext(Vector2Int build, Vector2Int target)
    {
        List<Vector2Int> neighbors = getNeighbors(build);
        int min_idx = 0;
        double min_dist = Vector2Int.Distance(target, neighbors[0]);
        for (int i = 1; i < neighbors.Count; i++)
        {
            double new_dist = Vector2Int.Distance(target, neighbors[i]);
            if (new_dist < min_dist)
            {
                min_dist = new_dist;
                min_idx = i;
            }
        }

        if (rand.NextDouble() < 1 - drawRandomness)
            return neighbors[min_idx];
        int idx = rand.Next(0, neighbors.Count);
        return neighbors[idx];
    }

    private int dig_out_room(int[,] scratchpad, List<Vector2Int> targets)
    {
        Vector2Int build = targets.LastOrDefault();
        foreach (Vector2Int target in targets)
        {
            while (build != target)
            {
                scratchpad[build.x, build.y] = FULL;
                build = chooseNext(build, target);
            }
        }
        
        floodFill(scratchpad);
        Component biggestComponent = color_components(scratchpad);
        keep_only_one_component(scratchpad, biggestComponent.color);
        return biggestComponent.area;
    }

    private int draw_square_room(int[,] scratchpad)
    {
        int small0 = rand.Next(maxRoomWidth / 10, 3 * maxRoomWidth / 10);
        int small1 = rand.Next(maxRoomWidth / 10, 3 * maxRoomWidth / 10);
        int small2 = rand.Next(maxRoomWidth / 10, 3 * maxRoomWidth / 10);
        int small3 = rand.Next(maxRoomWidth / 10, 3 * maxRoomWidth / 10);
        int big0 = rand.Next(7 * maxRoomWidth / 10, 9 * maxRoomWidth / 10);
        int big1 = rand.Next(7 * maxRoomWidth / 10, 9 * maxRoomWidth / 10);
        int big2 = rand.Next(7 * maxRoomWidth / 10, 9 * maxRoomWidth / 10);
        int big3 = rand.Next(7 * maxRoomWidth / 10, 9 * maxRoomWidth / 10);
            
        List<Vector2Int> targets = new List<Vector2Int>();
        targets.Add(new Vector2Int(small0, small1));
        targets.Add(new Vector2Int(small2, big0));
        targets.Add(new Vector2Int(big1, big2));
        targets.Add(new Vector2Int(big3, small3));
        
        return dig_out_room(scratchpad, targets);
    }

    private int draw_rect_room(int[,] scratchpad)
    {
        int x_small0 = rand.Next(0, 2 * maxRoomWidth / 10);
        int x_small1 = rand.Next(0, 2 * maxRoomWidth / 10);
        int y_small0 = rand.Next(2 * maxRoomHeight / 10, 4 * maxRoomHeight / 10);
        int y_small1 = rand.Next(2 * maxRoomHeight / 10, 4 * maxRoomHeight / 10);
        if (narrowRooms)
        {
            y_small0 = rand.Next(7 * maxRoomHeight / 20, 9 * maxRoomHeight / 20);
            y_small1 = rand.Next(7 * maxRoomHeight / 20, 9 * maxRoomHeight / 20);
        }
        int x_big0 = rand.Next(8 * maxRoomWidth / 10, maxRoomWidth);
        int x_big1 = rand.Next(8 * maxRoomWidth / 10, maxRoomWidth);
        int y_big0 = rand.Next(6 * maxRoomHeight / 10, 8 * maxRoomHeight / 10);
        int y_big1 = rand.Next(6 * maxRoomHeight / 10, 8 * maxRoomHeight / 10);
        if (narrowRooms)
        {
            y_big0 = rand.Next(11 * maxRoomHeight / 20, 13 * maxRoomHeight / 20);
            y_big1 = rand.Next(11 * maxRoomHeight / 20, 13 * maxRoomHeight / 20);
        }
            
        List<Vector2Int> targets = new List<Vector2Int>();
        targets.Add(new Vector2Int(x_small0, y_small0));
        targets.Add(new Vector2Int(x_small1, y_big0));
        targets.Add(new Vector2Int(x_big0, y_big1));
        targets.Add(new Vector2Int(x_big1, y_small1));
        
        return dig_out_room(scratchpad, targets);
    }

    private int draw_diamond_room(int[,] scratchpad)
    {
        int x_small = rand.Next(maxRoomWidth / 10, 2 * maxRoomWidth / 10);
        if (narrowRooms)
            x_small += 2 * maxRoomWidth / 10;
        int y_small = rand.Next(0, maxRoomWidth / 10);
        int x_medium0 = rand.Next(4 * maxRoomWidth / 10, 6 * maxRoomWidth / 10);
        int x_medium1 = rand.Next(4 * maxRoomWidth / 10, 6 * maxRoomWidth / 10);
        int y_medium0 = rand.Next(4 * maxRoomWidth / 10, 6 * maxRoomWidth / 10);
        int y_medium1 = rand.Next(4 * maxRoomWidth / 10, 6 * maxRoomWidth / 10);
        int x_big = rand.Next(8 * maxRoomWidth / 10, 9 * maxRoomWidth / 10);
        if (narrowRooms)
            x_big -= 2 * maxRoomWidth / 10;
        int y_big = rand.Next(9 * maxRoomWidth / 10, maxRoomWidth);
            
        List<Vector2Int> targets = new List<Vector2Int>();
        targets.Add(new Vector2Int(x_small, y_medium0));
        targets.Add(new Vector2Int(x_medium0, y_big));
        targets.Add(new Vector2Int(x_big, y_medium1));
        targets.Add(new Vector2Int(x_medium1, y_small));
        
        return dig_out_room(scratchpad, targets);
    }

    private int draw_C_room(int[,] scratchpad)
    {       
        int x_smallest = rand.Next(0, maxRoomWidth / 20);
        int x_small0 = rand.Next(maxRoomWidth / 10, 2 * maxRoomWidth / 10);
        int x_small1 = rand.Next(maxRoomWidth / 10, 2 * maxRoomWidth / 10);
        int x_medium0 = rand.Next(4 * maxRoomWidth / 10, 6 * maxRoomWidth / 10);
        int x_medium1 = rand.Next(4 * maxRoomWidth / 10, 6 * maxRoomWidth / 10);
        if (narrowRooms)
        {
            x_medium0 = rand.Next(7 * maxRoomWidth / 20, 9 * maxRoomWidth / 20);
            x_medium1 = rand.Next(7 * maxRoomWidth / 20, 9 * maxRoomWidth / 20);
        }
        int x_high0 = rand.Next(9 * maxRoomWidth / 10, maxRoomWidth);
        int x_high1 = rand.Next(9 * maxRoomWidth / 10, maxRoomWidth);
        int x_high2 = rand.Next(9 * maxRoomWidth / 10, maxRoomWidth);
        int x_high3 = rand.Next(9 * maxRoomWidth / 10, maxRoomWidth);
        
        int y_smallest = rand.Next(0, maxRoomHeight / 20);
        int y_small = rand.Next(maxRoomHeight / 10, 3 * maxRoomHeight / 10);
        int y_medium_small = rand.Next(3 * maxRoomHeight / 10, 5 * maxRoomHeight / 10);
        int y_inner_small = rand.Next(5 * maxRoomHeight / 20, 4 * maxRoomHeight / 10);
        int y_medium = rand.Next(3 * maxRoomHeight / 10, 7 * maxRoomHeight / 10);
        int y_inner_big = rand.Next(6 * maxRoomHeight / 10, 15 * maxRoomHeight / 20);
        int y_medium_big = rand.Next(5 * maxRoomHeight / 10, 7 * maxRoomHeight / 10);
        int y_big = rand.Next(7 * maxRoomHeight / 10, 9 * maxRoomHeight / 10);
        int y_biggest = rand.Next(19 * maxRoomHeight / 20, maxRoomHeight);

        List<Vector2Int> targets = new List<Vector2Int>();
        targets.Add(new Vector2Int(x_small0, y_smallest));
        targets.Add(new Vector2Int(x_smallest, y_medium));
        targets.Add(new Vector2Int(x_small1, y_biggest));
        targets.Add(new Vector2Int(x_high0, y_big));
        targets.Add(new Vector2Int(x_high1, y_medium_big));
        targets.Add(new Vector2Int(x_medium0, y_inner_big));
        targets.Add(new Vector2Int(x_medium1, y_inner_small));
        targets.Add(new Vector2Int(x_high2, y_medium_small));
        targets.Add(new Vector2Int(x_high3, y_small));
        
        return dig_out_room(scratchpad, targets);
    }

    private int draw_X_room(int[,] scratchpad)
    {
        int x_small0 = rand.Next(0, 2 * maxRoomWidth / 10);
        int x_small1 = rand.Next(0, 2 * maxRoomWidth / 10);
        int x_mid_left = rand.Next(7 * maxRoomWidth / 20, 8 * maxRoomWidth / 20);
        int x_mid_right = rand.Next(12 * maxRoomWidth / 20, 13 * maxRoomWidth / 20);
        int x_big0 = rand.Next(8 * maxRoomWidth / 10, maxRoomWidth);
        int x_big1 = rand.Next(8 * maxRoomWidth / 10, maxRoomWidth);
        
        int y_small0 = rand.Next(0, maxRoomHeight/4);
        int y_small1 = rand.Next(0, maxRoomHeight/4);
        int y_medium0 = rand.Next(4 * maxRoomHeight / 10, 6 * maxRoomHeight / 10);
        int y_medium1 = rand.Next(4 * maxRoomHeight / 10, 6 * maxRoomHeight / 10);
        int y_big0 = rand.Next(3 * maxRoomHeight/4, maxRoomHeight);
        int y_big1 = rand.Next(3 * maxRoomHeight/4, maxRoomHeight);

        int x_extra0 = rand.Next(9 * maxRoomWidth / 20, 11 * maxRoomWidth / 20);
        int x_extra1 = rand.Next(9 * maxRoomWidth / 20, 11 * maxRoomWidth / 20);
        int y_extra_small = rand.Next(1 * maxRoomWidth / 10, 2 * maxRoomWidth / 10);
        int y_extra_big = rand.Next(8 * maxRoomWidth / 10, 9 * maxRoomWidth / 10);
            
        List<Vector2Int> targets = new List<Vector2Int>();
        targets.Add(new Vector2Int(x_small0, y_small0));
        targets.Add(new Vector2Int(x_mid_left, y_medium0));
        targets.Add(new Vector2Int(x_small1, y_big0));
        if (narrowRooms)
            targets.Add(new Vector2Int(x_extra0, y_extra_big));
        targets.Add(new Vector2Int(x_big0, y_big1));
        targets.Add(new Vector2Int(x_mid_right, y_medium1));
        targets.Add(new Vector2Int(x_big1, y_small1));
        if (narrowRooms)
            targets.Add(new Vector2Int(x_extra1, y_extra_small));
        
        return dig_out_room(scratchpad, targets);
    }
    
    
    
    
    
    
    
    
    /*******************************
     *** door building functions ***
     *******************************/
    
    private Vector2Int clip(Rect scratchpadRect, Door door)
    {
        Vector2Int doorPos = door.left - scratchpadRect.topLeft;
        Vector2Int clippedDoorPos = new Vector2Int();
        if (door.outwards() == LEFT || door.outwards() == RIGHT)
        {
            clippedDoorPos.x = door.outwards() == LEFT ? maxRoomWidth - 1 : 0;
            if (doorPos.y < 0)
                clippedDoorPos.y = 0;
            else if (doorPos.y > maxRoomHeight - 1)
                clippedDoorPos.y = maxRoomHeight - 1;
            else
                clippedDoorPos.y = doorPos.y;
        }
        else
        {
            clippedDoorPos.y = door.outwards() == UP ? maxRoomHeight - 1 : 0;
            if (doorPos.x < 0)
                clippedDoorPos.x = 0;
            else if (doorPos.x > maxRoomWidth - 1)
                clippedDoorPos.x = maxRoomWidth - 1;
            else
                clippedDoorPos.x = doorPos.x;
        }

        return clippedDoorPos;
    }

    private bool place_connected_door(int[,] scratchpad, Door door, Vector2Int doorPos)
    {
        // Return true upon successfully placing the door at doorPos. Return false else.
        // Upon success, this also:
        //     - draws the door on the scratchpad as two tiles with value -door.doorNumber
        //     - marks the FREE space around the new door as BLOCKED on scratchpad
        //     - generates FULL padding around and behind the new door on scratchpad
        //     - registers the door as connected in connected_doors
        // door is the door we should connect our new door to
        // doorPos is the potential position of the new door

        if (scratchpad[doorPos.x, doorPos.y] != FULL)
            return false;
        
        Vector2Int freeSpot = doorPos + door.outwards();
        if (scratchpad[freeSpot.x, freeSpot.y] != FREE)
            return false;
        
        if (door.outwards() == LEFT || door.outwards() == RIGHT)
        {
            Vector2Int upDoorPos, downDoorPos;
            if (scratchpad[freeSpot.x, freeSpot.y - 1] == FREE)
            {
                upDoorPos = doorPos + UP;
                downDoorPos = doorPos;
                
                Vector2Int corner = doorPos + UP + UP;
                if (scratchpad[corner.x, corner.y] == FREE || scratchpad[corner.x, corner.y] == BLOCKED)
                {
                    Vector2Int P = corner + UP + door.outwards();
                    if (scratchpad[P.x, P.y] == FULL || scratchpad[P.x, P.y] == FULL_BLOCKED)
                        return false;
                }
            }
            else if (scratchpad[freeSpot.x, freeSpot.y + 1] == FREE)
            {
                upDoorPos = doorPos;
                downDoorPos = doorPos + DOWN;
                
                Vector2Int corner = doorPos + DOWN + DOWN;
                if (scratchpad[corner.x, corner.y] == FREE || scratchpad[corner.x, corner.y] == BLOCKED)
                {
                    Vector2Int P = corner + DOWN + door.outwards();
                    if (scratchpad[P.x, P.y] == FULL || scratchpad[P.x, P.y] == FULL_BLOCKED)
                        return false;
                }
            }
            else
            {
                return false;
            }
            
            int check_x_min = door.outwards() == LEFT ? upDoorPos.x + 1 : 0;
            int check_x_max = door.outwards() == LEFT ? maxRoomWidth - 1 : upDoorPos.x - 1;
            for (int check_x = check_x_min; check_x <= check_x_max; check_x++)
            {
                if (scratchpad[check_x, upDoorPos.y] == FULL_BLOCKED || scratchpad[check_x, upDoorPos.y] < 0
                                                                     || scratchpad[check_x, downDoorPos.y] ==
                                                                     FULL_BLOCKED ||
                                                                     scratchpad[check_x, downDoorPos.y] < 0)
                    return false;
            }
            

            scratchpad[upDoorPos.x, upDoorPos.y - 1] = FULL_BLOCKED;
            scratchpad[upDoorPos.x, upDoorPos.y] = -door.doorNumber;
            scratchpad[downDoorPos.x, downDoorPos.y] = -door.doorNumber;
            scratchpad[downDoorPos.x, downDoorPos.y + 1] = FULL_BLOCKED;

            int block_x = upDoorPos.x + door.outwards().x;
            for (int block_y = Math.Max(upDoorPos.y - 2, 0); block_y <= Math.Min(downDoorPos.y + 2, maxRoomHeight-1); block_y++)
                if (scratchpad[block_x, block_y] == FREE)
                    scratchpad[block_x, block_y] = BLOCKED;
            block_x += door.outwards().x;
            for (int block_y = Math.Max(upDoorPos.y - 2, 0); block_y <= Math.Min(downDoorPos.y + 2, maxRoomHeight-1); block_y++)
                if (scratchpad[block_x, block_y] == FREE)
                    scratchpad[block_x, block_y] = BLOCKED;

            int fill_x_min = door.outwards() == LEFT ? upDoorPos.x + 1 : 0;
            int fill_x_max = door.outwards() == LEFT ? maxRoomWidth - 1 : upDoorPos.x - 1;
            for (int fill_x = fill_x_min; fill_x <= fill_x_max; fill_x++)
            for (int fill_y = upDoorPos.y - 1; fill_y <= downDoorPos.y + 1; fill_y++)
                scratchpad[fill_x, fill_y] = FULL_BLOCKED;
        }
        else // outwards is UP or DOWN
        {
            Vector2Int leftDoorPos, rightDoorPos;
            if (scratchpad[freeSpot.x - 1, freeSpot.y] == FREE)
            {
                leftDoorPos = doorPos + LEFT;
                rightDoorPos = doorPos;
                
                Vector2Int corner = doorPos + LEFT + LEFT;
                if (scratchpad[corner.x, corner.y] == FREE || scratchpad[corner.x, corner.y] == BLOCKED)
                {
                    Vector2Int P = corner + LEFT + door.outwards();
                    if (scratchpad[P.x, P.y] == FULL || scratchpad[P.x, P.y] == FULL_BLOCKED)
                        return false;
                }
            }
            else if (scratchpad[freeSpot.x + 1, freeSpot.y] == FREE)
            {
                leftDoorPos = doorPos;
                rightDoorPos = doorPos + RIGHT;
                
                Vector2Int corner = doorPos + RIGHT + RIGHT;
                if (scratchpad[corner.x, corner.y] == FREE || scratchpad[corner.x, corner.y] == BLOCKED)
                {
                    Vector2Int P = corner + RIGHT + door.outwards();
                    if (scratchpad[P.x, P.y] == FULL || scratchpad[P.x, P.y] == FULL_BLOCKED)
                        return false;
                }
            }
            else
            {
                return false;
            }
            
            
            int check_y_min = door.outwards() == UP ? doorPos.y + 1 : 0;
            int check_y_max = door.outwards() == UP ? maxRoomHeight - 1 : doorPos.y - 1;
            for (int check_y = check_y_min; check_y <= check_y_max; check_y++)
            {
                if (scratchpad[leftDoorPos.x, check_y] == FULL_BLOCKED || scratchpad[leftDoorPos.x, check_y] < 0
                                                                       || scratchpad[rightDoorPos.x, check_y] ==
                                                                       FULL_BLOCKED ||
                                                                       scratchpad[rightDoorPos.x, check_y] < 0)
                    return false;
            }
            
            

            scratchpad[leftDoorPos.x - 1, leftDoorPos.y] = FULL_BLOCKED;
            scratchpad[leftDoorPos.x, leftDoorPos.y] = -door.doorNumber;
            scratchpad[rightDoorPos.x, rightDoorPos.y] = -door.doorNumber;
            scratchpad[rightDoorPos.x + 1, rightDoorPos.y] = FULL_BLOCKED;

            int block_y = doorPos.y + door.outwards().y;
            for (int block_x = Math.Max(leftDoorPos.x - 2, 0); block_x <= Math.Min(rightDoorPos.x + 2, maxRoomWidth-1); block_x++)
                if (scratchpad[block_x, block_y] == FREE)
                    scratchpad[block_x, block_y] = BLOCKED;
            block_y += door.outwards().y;
            for (int block_x = Math.Max(leftDoorPos.x - 2, 0); block_x <= Math.Min(rightDoorPos.x + 2, maxRoomWidth-1); block_x++)
                if (scratchpad[block_x, block_y] == FREE)
                    scratchpad[block_x, block_y] = BLOCKED;

            int fill_y_min = door.outwards() == UP ? doorPos.y + 1 : 0;
            int fill_y_max = door.outwards() == UP ? maxRoomHeight - 1 : doorPos.y - 1;
            for (int fill_x = leftDoorPos.x - 1; fill_x <= rightDoorPos.x + 1; fill_x++)
                for (int fill_y = fill_y_min; fill_y <= fill_y_max; fill_y++)
                    scratchpad[fill_x, fill_y] = FULL_BLOCKED;
        }
        
        // Register the door as connected.
        // It connects the room we are currently building (with room number roomCounter) and the room that door belongs
        // to.
        connected_doors.Add(door.doorNumber, new Tuple<int, int>(roomCounter, door.roomNumber(map)));
        return true;
    }

    private bool good_orientation(Rect searchRect, Door door, int x, int y)
    {
        if (x < searchRect.topLeft.x + maxConnectDistance && door.outwards() != RIGHT)
            return false;
        if (x > searchRect.topLeft.x + searchRect.width - maxConnectDistance && door.outwards() != LEFT)
            return false;
        if (y < searchRect.topLeft.y + maxConnectDistance && door.outwards() != DOWN)
            return false;
        if (y > searchRect.topLeft.y + searchRect.height - maxConnectDistance && door.outwards() != UP)
            return false;
        return true;
    }

    private int num_new_doors_dead_ends(int currentExtraDoors, int doorsInRoomAlready)
    {
        if (currentExtraDoors >= MIN_EXTRA_DOORS)
            return random_from_distribution(new int[] {0, 1}, new double[] {0.8, 0.2});
        if (doorsInRoomAlready == 1)
            return random_from_distribution(new int[] {3, 2}, new double[] {0.8, 0.2});
        return 2;
    }

    private int num_new_doors_normal(int currentExtraDoors, int doorsInRoomAlready)
    {
        int perfect = avgExtraDoors - currentExtraDoors;
        if (perfect < 0)
            return 0;
        if (perfect == 0)
        {
            if (doorsInRoomAlready <= 2)
                return random_from_distribution(new int[] {0, 1, 2}, new double[] {0.75, 0.2, 0.05});
            return random_from_distribution(new int[] {0, 1}, new double[] {0.8, 0.2});
        }

        if (perfect == 1)
        {
            if (doorsInRoomAlready <= 1)
                return random_from_distribution(new int[] {0, 1, 2, 3}, new double[] {0.3, 0.4, 0.2, 0.1});
            return random_from_distribution(new int[] {0, 1, 2, 3}, new double[] {0.35, 0.45, 0.15, 0.05});
        }
        if (perfect == 2)
        {
            if (doorsInRoomAlready <= 1)
                return random_from_distribution(new int[] {0, 1, 2, 3}, new double[] {0.1, 0.2, 0.4, 0.3});
            return random_from_distribution(new int[] {0, 1, 2, 3}, new double[] {0.1, 0.35, 0.45, 0.1});
        }
        // perfect >= 3 
        if (doorsInRoomAlready <= 1)
            return random_from_distribution(new int[] {0, 1, 2, 3, 4}, new double[] {0.1, 0.2, 0.25, 0.35, 0.1});
        return random_from_distribution(new int[] {0, 1, 2, 3}, new double[] {0.1, 0.2, 0.6, 0.1});
    }
    
    private int number_of_new_doors(int doorInRoomAlready)
        // note that roomCounter has already been increased while building the room. The doorCounter is not yet.
    {
        int currentExtraDoors = doorCounter - connected_doors.Count;
        int minimal = Math.Max(MIN_EXTRA_DOORS - currentExtraDoors, 0);

        int number;
        if (extraDeadEnds)
            number = num_new_doors_dead_ends(currentExtraDoors, doorInRoomAlready);
        else
            number = num_new_doors_normal(currentExtraDoors, doorInRoomAlready);

        if (number < minimal)
            number = minimal;
        
        return number;
    }

    private void permute_randomly(List<Vector2Int> dirs)
    {
        int n = dirs.Count();
        while (n > 1)
        {
            n--;
            int i = rand.Next(n + 1);
            Vector2Int temp = dirs[i];
            dirs[i] = dirs[n];
            dirs[n] = temp;
        }
    }
    
    private void draw_new_door(int[,] scratchpad, Vector2Int inwards)
    {
        Vector2Int start = new Vector2Int(maxRoomWidth/2, maxRoomHeight/2);
        if (inwards == LEFT)
            start.x = maxRoomWidth - 1;
        else if (inwards == RIGHT)
            start.x = 0;
        else if (inwards == UP)
            start.y = maxRoomHeight - 1;
        else // inwards == DOWN
            start.y = 0;

        Vector2Int translate = new Vector2Int();
        translate.x = -(maxRoomWidth / 2 + maxRoomHeight / 2) * inwards.x;
        translate.y = -(maxRoomWidth / 2 + maxRoomHeight / 2) * inwards.y;
        Vector2Int distPoint = start + translate;
        bool[,] queued = new bool[maxRoomWidth, maxRoomHeight]; // remember which coordinates we queued already
        // At the start, we did not queue anything yet
        for (int x = 0; x < maxRoomWidth; x++)
            for (int y = 0; y < maxRoomHeight; y++)
                queued[x, y] = false;
         
        // The following priority queue dequeues smallest elements first.
        PriorityQueue<double, Vector2Int> pq = new PriorityQueue<double, Vector2Int>();
        pq.Enqueue(0, start);
        queued[start.x, start.y] = true;
        while (!pq.IsEmpty)
        {
            Vector2Int candidate = pq.Dequeue();
            if (place_new_door(scratchpad, inwards, candidate))
            {
                return;
            }
                     
            queued[candidate.x, candidate.y] = true;
         
            List<Vector2Int> neighbors = getNeighbors(candidate);
            foreach (Vector2Int neighbor in neighbors)
            {
                if (!queued[neighbor.x, neighbor.y])
                {
                    double priority = Vector2Int.Distance(distPoint, neighbor);
                    pq.Enqueue(priority, neighbor);
                    queued[neighbor.x, neighbor.y] = true;
                }
            }
        }
        
        // We should not get here
        // If we get here, we did not find any spot to place the door. The door will stay unconnected...
        return;
    }

    // Return true upon successfully placing the new door at doorPos. Return false else.
    // Upon success, this also:
    //     - increases doorCounter by 1
    //     - draws the door on the scratchpad as two tiles with value minus the new doorCounter valued
    //     - marks the FREE space around the new door as BLOCKED on scratchpad
    //     - generates FULL padding around and behind the new door on scratchpad
    // inwards is a unit vector pointing inwards (from new door to inside the room)
    // doorPos is the potential position of the new door
    private bool place_new_door(int[,] scratchpad, Vector2Int inwards, Vector2Int doorPos)
    {
        if (scratchpad[doorPos.x, doorPos.y] != FULL)
            return false;
        
        Vector2Int freeSpot = doorPos + inwards;
        if (scratchpad[freeSpot.x, freeSpot.y] != FREE)
            return false;
        
        if (inwards == LEFT || inwards == RIGHT)
        {
            Vector2Int upDoorPos, downDoorPos;
            if (scratchpad[freeSpot.x, freeSpot.y - 1] == FREE)
            {
                upDoorPos = doorPos + UP;
                downDoorPos = doorPos;
                
                Vector2Int corner = doorPos + UP + UP;
                if (scratchpad[corner.x, corner.y] == FREE || scratchpad[corner.x, corner.y] == BLOCKED)
                {
                    Vector2Int P = corner + UP + inwards;
                    if (scratchpad[P.x, P.y] == FULL || scratchpad[P.x, P.y] == FULL_BLOCKED)
                        return false;
                }
            }
            else if (scratchpad[freeSpot.x, freeSpot.y + 1] == FREE)
            {
                upDoorPos = doorPos;
                downDoorPos = doorPos + DOWN;
                
                Vector2Int corner = doorPos + UP + UP;
                if (scratchpad[corner.x, corner.y] == FREE || scratchpad[corner.x, corner.y] == BLOCKED)
                {
                    Vector2Int P = corner + DOWN + inwards;
                    if (scratchpad[P.x, P.y] == FULL || scratchpad[P.x, P.y] == FULL_BLOCKED)
                        return false;
                }
            }
            else
            {
                return false;
            }
            
            int check_x_min = inwards == LEFT ? upDoorPos.x + 1 : 0;
            int check_x_max = inwards == LEFT ? maxRoomWidth - 1 : upDoorPos.x - 1;
            for (int check_x = check_x_min; check_x <= check_x_max; check_x++)
            {
                if (scratchpad[check_x, upDoorPos.y] == FULL_BLOCKED || scratchpad[check_x, upDoorPos.y] < 0
                                                                     || scratchpad[check_x, downDoorPos.y] ==
                                                                     FULL_BLOCKED ||
                                                                     scratchpad[check_x, downDoorPos.y] < 0)
                    return false;
            }

            // Found a spot to place the new door!
            doorCounter++;
            scratchpad[upDoorPos.x, upDoorPos.y - 1] = FULL_BLOCKED;
            scratchpad[upDoorPos.x, upDoorPos.y] = -doorCounter;
            scratchpad[downDoorPos.x, downDoorPos.y] = -doorCounter;
            scratchpad[downDoorPos.x, downDoorPos.y + 1] = FULL_BLOCKED;

            int block_x = upDoorPos.x + inwards.x;
            for (int block_y = Math.Max(upDoorPos.y - 2, 0); block_y <= Math.Min(downDoorPos.y + 2, maxRoomHeight-1); block_y++)
                if (scratchpad[block_x, block_y] == FREE)
                    scratchpad[block_x, block_y] = BLOCKED;
            block_x += inwards.x;
            for (int block_y = Math.Max(upDoorPos.y - 2, 0); block_y <= Math.Min(downDoorPos.y + 2, maxRoomHeight-1); block_y++)
                if (scratchpad[block_x, block_y] == FREE)
                    scratchpad[block_x, block_y] = BLOCKED;

            int fill_x_min = inwards == LEFT ? upDoorPos.x + 1 : 0;
            int fill_x_max = inwards == LEFT ? maxRoomWidth - 1 : upDoorPos.x - 1;
            for (int fill_x = fill_x_min; fill_x <= fill_x_max; fill_x++)
            for (int fill_y = upDoorPos.y - 1; fill_y <= downDoorPos.y + 1; fill_y++)
                scratchpad[fill_x, fill_y] = FULL_BLOCKED;
        }
        else // outwards is UP or DOWN
        {
            Vector2Int leftDoorPos, rightDoorPos;
            if (scratchpad[freeSpot.x - 1, freeSpot.y] == FREE)
            {
                leftDoorPos = doorPos + LEFT;
                rightDoorPos = doorPos;
                
                Vector2Int corner = doorPos + LEFT + LEFT;
                if (scratchpad[corner.x, corner.y] == FREE || scratchpad[corner.x, corner.y] == BLOCKED)
                {
                    Vector2Int P = corner + LEFT + inwards;
                    if (scratchpad[P.x, P.y] == FULL || scratchpad[P.x, P.y] == FULL_BLOCKED)
                        return false;
                }
            }
            else if (scratchpad[freeSpot.x + 1, freeSpot.y] == FREE)
            {
                leftDoorPos = doorPos;
                rightDoorPos = doorPos + RIGHT;
                
                Vector2Int corner = doorPos + RIGHT + RIGHT;
                if (scratchpad[corner.x, corner.y] == FREE || scratchpad[corner.x, corner.y] == BLOCKED)
                {
                    Vector2Int P = corner + RIGHT + inwards;
                    if (scratchpad[P.x, P.y] == FULL || scratchpad[P.x, P.y] == FULL_BLOCKED)
                        return false;
                }
            }
            else
            {
                return false;
            }

            int check_y_min = inwards == UP ? doorPos.y + 1 : 0;
            int check_y_max = inwards == UP ? maxRoomHeight - 1 : doorPos.y - 1;
            for (int check_y = check_y_min; check_y <= check_y_max; check_y++)
            {
                if (scratchpad[leftDoorPos.x, check_y] == FULL_BLOCKED || scratchpad[leftDoorPos.x, check_y] < 0
                                                                       || scratchpad[rightDoorPos.x, check_y] ==
                                                                       FULL_BLOCKED ||
                                                                       scratchpad[rightDoorPos.x, check_y] < 0)
                    return false;
            }
            
            
            // Found a spot to place the new door!
            doorCounter++;
            scratchpad[leftDoorPos.x - 1, leftDoorPos.y] = FULL_BLOCKED;
            scratchpad[leftDoorPos.x, leftDoorPos.y] = -doorCounter;
            scratchpad[rightDoorPos.x, rightDoorPos.y] = -doorCounter;
            scratchpad[rightDoorPos.x + 1, rightDoorPos.y] = FULL_BLOCKED;

            int block_y = doorPos.y + inwards.y;
            for (int block_x = Math.Max(leftDoorPos.x - 2, 0); block_x <= Math.Min(rightDoorPos.x + 2, maxRoomWidth-1); block_x++)
                if (scratchpad[block_x, block_y] == FREE)
                    scratchpad[block_x, block_y] = BLOCKED;
            block_y += inwards.y;
            for (int block_x = Math.Max(leftDoorPos.x - 2, 0); block_x <= Math.Min(rightDoorPos.x + 2, maxRoomWidth-1); block_x++)
                if (scratchpad[block_x, block_y] == FREE)
                    scratchpad[block_x, block_y] = BLOCKED;

            int fill_y_min = inwards == UP ? doorPos.y + 1 : 0;
            int fill_y_max = inwards == UP ? maxRoomHeight - 1 : doorPos.y - 1;
            for (int fill_x = leftDoorPos.x - 1; fill_x <= rightDoorPos.x + 1; fill_x++)
            for (int fill_y = fill_y_min; fill_y <= fill_y_max; fill_y++)
                scratchpad[fill_x, fill_y] = FULL_BLOCKED;
        }
        
        return true;
    }
    
    private List<Vector2Int> door_placement_direction_list_random(int[,] scratchpad, List<Door> doorsInRoomAlready)
    {
        List<Vector2Int> dirs = new List<Vector2Int>()
        {
            LEFT, RIGHT, UP, DOWN
        };
        
        foreach (Door door in doorsInRoomAlready)
        {
            dirs.Remove(door.outwards());
        }
        permute_randomly(dirs);
        return dirs;
    }
    
    private List<Vector2Int> door_placement_direction_list_towards_rooms(int[,] scratchpad, Rect scratchpadRect, List<Door> doorsInRoomAlready)
    {
        List<Vector2Int> dirsAway = new List<Vector2Int>(perpUnits);
        
        foreach (Door door in doorsInRoomAlready)
        {
            dirsAway.Remove(door.outwards());
        }

        List<Vector2Int> dirsTowardsRooms = new List<Vector2Int>();
        foreach (KeyValuePair<int, Vector2Int> roomLoc in room_location)
        {
            if (dirsAway.Contains(RIGHT) && roomLoc.Value.x < scratchpadRect.topLeft.x)
            {
                dirsAway.Remove(RIGHT);
                dirsTowardsRooms.Add(RIGHT);
            }
            if (dirsAway.Contains(LEFT) && scratchpadRect.topLeft.x + scratchpadRect.width < roomLoc.Value.x)
            {
                dirsAway.Remove(LEFT);
                dirsTowardsRooms.Add(LEFT);
            }
            if (dirsAway.Contains(DOWN) && roomLoc.Value.y < scratchpadRect.topLeft.y)
            {
                dirsAway.Remove(DOWN);
                dirsTowardsRooms.Add(DOWN);
            }
            if (dirsAway.Contains(UP) && scratchpadRect.topLeft.y + scratchpadRect.height < roomLoc.Value.y)
            {
                dirsAway.Remove(UP);
                dirsTowardsRooms.Add(UP);
            }
        }

        permute_randomly(dirsTowardsRooms);
        permute_randomly(dirsAway);

        List<Vector2Int> dirs = new List<Vector2Int>();
        for (int i = 0; i < dirsTowardsRooms.Count; i++)
            dirs.Add(dirsTowardsRooms[i]);
        for (int i = 0; i < dirsAway.Count; i++)
            dirs.Add(dirsAway[i]);

        return dirs;
    }

    private List<Vector2Int> door_placement_direction_list(int[,] scratchpad, Rect scratchpadRect, List<Door> doorsInRoomAlready)
    {
        if (placeNewDoorsTowardsRooms)
            return door_placement_direction_list_towards_rooms(scratchpad, scratchpadRect, doorsInRoomAlready);
        return door_placement_direction_list_random(scratchpad, doorsInRoomAlready);
    }

    
    
    
    
    
    
    
    /*********************************
     *** add_spikes() subfunctions ***
     *********************************/

    private int spike_stamp(int[,] scratchpad)
        // Returns the number of spikes added to scratchpad
    {
        int left = rand.Next(0, maxRoomWidth - (SPIKE_STAMP_SIZE - 1));
        int top = rand.Next(0, maxRoomHeight - (SPIKE_STAMP_SIZE - 1));
        int count = 0;
        for (int x = left; x < left + SPIKE_STAMP_SIZE; x++)
        {
            for (int y = top; y < top + SPIKE_STAMP_SIZE; y++)
            {
                if (scratchpad[x, y] == FREE)
                {
                    scratchpad[x, y] = SPIKES;
                    count++;
                }
            }
        }

        return count;
    }

    
} // end of class MapGenerator
