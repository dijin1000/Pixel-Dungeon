using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.Distributions;

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
        if (m.monsterKilled == 0)
            monstercount++;
        if (m.lethality > 0.5)
            monstercount++;
        if (m.roomTime[currentState.room] < 10)
            monstercount++;
        if (m.lethality < 0.5 && m.monsterhit > 0)
            monstercount--;

        currentState.Monsters = new List<Tuple<int, int>>();
        currentState.Monsters.Add(new Tuple<int, int>((int)MonsterTypes.Medium, monstercount));

        float RSmean = 30 + 10 * monstercount;
        float RSstdDev = 10;
        Normal normalDist = new Normal(RSmean, RSstdDev);
        currentState.roomSize = normalDist.Sample();

        float spikeChange = 0;
        if (m.trapped == 0)
            spikeChange += 0.05f;
        else
            spikeChange -= m.trapped / 100;
        if (spikeChange < -0.10f)
            spikeChange = -0.10f;

        float Smean = 0.15f;
        float SstdDev = 0.05f;
        Normal normalDist = new Normal(Smean, SstdDev);
        currentState.spikeRate = normalDist.Sample();

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

        GlobalManager.GlobalInstance.MonsterHealthFlat = 0;
        GlobalManager.GlobalInstance.MonsterHealhtPercentage = 100f;

        GlobalManager.GlobalInstance.MonsterDamageFlat = 0;
        GlobalManager.GlobalInstance.MonsterDamagePercentage = 100f;

        GlobalManager.GlobalInstance.MonsterSpeedFlat = 0;
        GlobalManager.GlobalInstance.MonsterSpeedPercentage = 100f;

        yield return LevelManager.LevelInstance.CreateNewLevel(currentState);
    }
}
