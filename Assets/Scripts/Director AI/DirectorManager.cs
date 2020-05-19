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
    public Parameters currentState;

    void Awake()
    {
        DirectorInstance = this;
    }

    public float time = 0;
    private float MaxTime = 300;
    private void Update()
    {
        time += Time.deltaTime;
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

    public void UpdateState(int door)
    {
        currentState.door = door;
        StatisticsManager.StatisticsInstance.FinishedRoom(currentState.room);
    }

    private int buildtype = 2;
    public int GetBuildType
    {
        get
        {
            return buildtype;
        }
    }
    private bool FirstLevel = true;

    public IEnumerator NextLevel()
    {
        switch (buildtype)
        {
            case 1:
                Easy();
                break;
            case 2:
                Scaled();
                break;
            case 3:
                Hard();
                break;
        }
        if (time > MaxTime)
            currentState.lastDoor = true;
        yield return LevelManager.LevelInstance.CreateNewLevel(currentState);
    }

    private void Easy()
    {
        if (FirstLevel)
        {
            FirstLevel = false;
        }
        else
        {
        }
    }
    private void Scaled() 
    {
        if (FirstLevel)
        {
            FirstLevel = false;
        }
        else
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
            /*
            float RSmean = 30 + 10 * monstercount;
            float RSstdDev = 10;
            NormalDistribution normalDist = new NormalDistribution(RSmean, RSstdDev);
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
            NormalDistribution normalDist = new NormalDistribution(Smean, SstdDev);
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
            */
            GlobalManager.GlobalInstance.MonsterHealthFlat = 0;
            GlobalManager.GlobalInstance.MonsterHealhtPercentage = 100f;

            GlobalManager.GlobalInstance.MonsterDamageFlat = 0;
            GlobalManager.GlobalInstance.MonsterDamagePercentage = 100f;

            GlobalManager.GlobalInstance.MonsterSpeedFlat = 0;
            GlobalManager.GlobalInstance.MonsterSpeedPercentage = 100f;
        }
    }
    private void Hard()
    {
        if (FirstLevel)
        {
            FirstLevel = false;
        }
        else
        {

        }
    }
}
