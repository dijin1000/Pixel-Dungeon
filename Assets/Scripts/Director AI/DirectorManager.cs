using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MonsterTypes
{
    Soft = 1,
    Medium = 2,
    Hard = 3
}

public class DirectorManager : MonoBehaviour
{

    /// <summary>
    /// Signleton Pattern
    /// </summary>
    private static DirectorManager directorInstance;
    public static DirectorManager DirectorInstance
    {
        get
        {
            if (directorInstance == null)
                Debug.LogError("There is no " + DirectorInstance.GetType() + " set.");
            return directorInstance;
        }
        private set
        {
            if (directorInstance != null)
                Debug.LogError("Two instances of the " + DirectorInstance.GetType() + " are sethere is no DirectorAI set.");
            directorInstance = value;
        }
    }
    int results;
    private Parameters currentState;

    void Awake()
    {
        DirectorInstance = this;
    }
    
    public void Load(int slotloading)
    {
        throw new NotImplementedException();
    }
    internal void NewSlot()
    {
        currentState = new Parameters();
        currentState.room = 0;
        currentState.door = -1;
        StatisticsManager.StatisticsInstance.Measurements = new Measurements();
    }
    public void SaveSlot()
    {
        throw new NotImplementedException();
    }

    public void UpdateState(int room,int door)
    {
        currentState.door = door;
        currentState.room = room;
    }

    public IEnumerator NextLevel()
    {
        Measurements m = StatisticsManager.StatisticsInstance.Measurements;

        //LOGIC for JACCO en MAURITS
        currentState.roomSize = 50;
        currentState.spikeRate = 0.05;
        currentState.narrowRoom = false;
        currentState.deadEnd = true;
        currentState.cycles = true;
        currentState.Monsters = new List<Tuple<int, int>>();
        currentState.Monsters.Add(new Tuple<int,int>((int)MonsterTypes.Soft, 2));

        yield return LevelManager.LevelInstance.CreateNewLevel(currentState);
    }
}
