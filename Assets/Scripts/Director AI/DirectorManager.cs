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
        buildtype = 2; // UnityEngine.Random.Range(1, 4);
    }

    public float time = 0;
    private float MaxTime = 120;
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

    private int buildtype;
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
        int modifier = 0;

        switch (buildtype)
        {
            case 1:
                modifier = -1;
                break;
            case 2:
                modifier = 1;
                break;
            case 3:
                modifier = 2;
                break;
        }

        if (FirstLevel)
        {
            FirstLevel = false;
        }
        else
        {
            Measurements m = StatisticsManager.StatisticsInstance.Measurements;


            // Monsters
            int monstercount = m.newMonsters;

            // random increase of monsters
            if (modifier > 0)
            {
                if (UnityEngine.Random.Range(0, modifier + 1) > 0)
                {
                    monstercount += modifier;
                }
            }


            if (m.monsterhit > 0)
            {
                monstercount += (modifier - 2);
            }
            if (modifier == 1 && m.lethality > 0.4)
                monstercount += modifier;
            else if (modifier == 2 && m.lethality > 0.25)
                monstercount += modifier - 2;
            if (monstercount <= 0)
            {
                monstercount = UnityEngine.Random.Range(0, 3) + modifier;
            }


            currentState.Monsters = new List<Tuple<int, int>>();

            if (monstercount <= 8)
            {
                currentState.Monsters.Add(new Tuple<int, int>((int)MonsterTypes.Soft, monstercount));
            }
            else if (monstercount <= 15)
            {
                currentState.Monsters.Add(new Tuple<int, int>((int)MonsterTypes.Soft, monstercount / 4));
                currentState.Monsters.Add(new Tuple<int, int>((int)MonsterTypes.Medium, monstercount / 4));
            }
            else if (monstercount <= 20)
            {
                currentState.Monsters.Add(new Tuple<int, int>((int)MonsterTypes.Medium, monstercount / 4));
            }
            else
            {
                currentState.Monsters.Add(new Tuple<int, int>((int)MonsterTypes.Medium, 6));
            }


            currentState.Items = new List<Tuple<TypeItem, int>>();

            // Potions
            if (m.monsterDmg > 1 || m.trappedRoom > 0)
            {
                float potion_chance = 2.0f - modifier;
                if (UnityEngine.Random.Range(0, 2) * potion_chance >= 1)
                {
                    currentState.Items.Add(new Tuple<TypeItem, int>(TypeItem.Potion, 1));
                }
            }

            // weapon
            if (UnityEngine.Random.Range(0, 10) >= 1)
            {
                currentState.Items.Add(new Tuple<TypeItem, int>(TypeItem.Weapon, 1));
            }


            // Money
            currentState.Items.Add(new Tuple<TypeItem, int>(TypeItem.Money, UnityEngine.Random.Range(0, 3)));

            //Spikes
            if (m.trappedRoom == 0)
            {
                currentState.spikeRate += 0.05 * modifier;
            }
            else
            {
                currentState.spikeRate += 0.05 * modifier - 0.1;
            }


            GlobalManager.GlobalInstance.MonsterHealthFlat = 0;
            GlobalManager.GlobalInstance.MonsterHealhtPercentage = 50f;

            GlobalManager.GlobalInstance.MonsterDamageFlat = 0;
            GlobalManager.GlobalInstance.MonsterDamagePercentage = 50f;

            GlobalManager.GlobalInstance.MonsterSpeedFlat = 0;
            GlobalManager.GlobalInstance.MonsterSpeedPercentage = 50f;

            //GlobalManager.GlobalInstance.ItemStrengthPercentage = 100f;

        }


        if (time > MaxTime)
            currentState.lastDoor = true;
        yield return LevelManager.LevelInstance.CreateNewLevel(currentState);
    }

    
   
}
