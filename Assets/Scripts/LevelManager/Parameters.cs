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
    
    public List<Tuple<int, int>> Monsters;
}