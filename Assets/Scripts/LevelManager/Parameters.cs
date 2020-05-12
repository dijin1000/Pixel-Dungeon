using System;
using System.Collections.Generic;

[Serializable]
public struct Parameters
{
    public int room;
    public int door;
    public int roomSize;
    public double spikeRate;
    public bool narrowRoom;
    public bool deadEnd;
    public bool cycles;
    public bool lastDoor;
    
    public List<Tuple<int, int>> Monsters;
    public Parameters(int _room = 0, int _door = 0, int _roomsize = 20, double _spikeRate = 0.05f, bool _narrowRoom = false, bool _deadEnd = false, bool _cycles = true, bool _lastDoor = false)
    {  
        room = _room;
        door = _door;
        roomSize = _roomsize;
        spikeRate = _spikeRate;
        narrowRoom = _narrowRoom;
        deadEnd = _deadEnd;
        cycles = _cycles;
        lastDoor = _lastDoor;
        Monsters = new List<Tuple<int, int>>();
    }
}