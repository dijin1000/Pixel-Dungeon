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

        int monstercount = m.monsters;
        if (m.monsterKille == 0)
            monstercount++;
        if (m.lethality() > 0.5)
            monstercount++;
        if (m.roomTime < 10)
            monstercount++;
        if (m.lethality() < 0.5 && m.monsterhit > 0)
            monstercount--;
        currentState.Monsters = new List<Tuple<int, int>>();
        currentState.Monsters.Add(new Tuple<int, int>((int)MonsterTypes.Medium, monstercount));

        double RSmean = 30 + 10 * monstercount;
        double RSstdDev = 10;
        MathNet.Numerics.Distributions.Normal normalDist = new Normal(RSmean, RSstdDev);
        currentState.roomSize = randomGaussianValue = normalDist.Sample();

        int spikeChange = 0;
        if (m.trapped == 0)
            spikeChange += 0.05;
        else
            spikeChange -= m.trapped / 100;
        if (spikeChange < -0.10)
            spikeChange = -0.10;

        double Smean = 0.15;
        double SstdDev = 0.05;
        MathNet.Numerics.Distributions.Normal normalDist = new Normal(Smean, SstdDev);
        currentState.spikeRate = randomGaussianValue = normalDist.Sample();

        if(m.monsterhit == 0)
            currentState.narrowRoom = true;
        if (m.monsterhit > 1)
            currentState.narrowRoom = false;

        if (m.monsterhit > 1)
        {
            currentState.cycles = true;
            currentState.deadEnd = false;
        }
        else
        {
            currentState.cycles = false;
            currentState.deadEnd = true;
        }

        yield return LevelManager.LevelInstance.CreateNewLevel(currentState);
    }
}
