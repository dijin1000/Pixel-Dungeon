using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Measurements
{
    public int hitted;                      //1.    aantal hits raak
    public float score;                     //2.    aantal punten die iemand pakt
    public int items;                       //3.    aantal items dat iemand pakt
    public int monsterKilled;               //4.    aantal monsters vermoord
    public float damage;                    //5.    hoeveelheid damage gedaan
    public int rooms;                       //6.    hoeveel kamers zijn er al gespeeld 
    public Dictionary<int,float> roomTime;  //7.    tijd per kamer(init tot complete)
    public int monsterhit;                  //8.    aantal keer geraakt door monster
    public float monsterDmg;                //9.    hoeveelheid damage gekregen
    public int deaths;                      //10.   totaal aantal keer dood gegaan
    public int trapped;                     //11.   aantal keer in een valstrik gevallen
    public float steps;                     //12.   aantal stappen dat iemand neemt
    public int attacks;                     //13.   aantal aanvallen gebruikt
    public Dictionary<int, int> runRoom;    //15.   hoeveelste keer wordt de kamer gespeeld
    public float noMove;                    //16.   tijd stilgestaan(geen move actie gedaan)

    public int monsters;        //

    public float accuracy       //accuracy = aantal hits raak/ totaal aantal hits
    {
        get
        {
            return (float)hitted / (float)attacks;
        }
    }
    public float lethality      //percentage monsters vermoord = aantal vermoord / aantal monsters 
    {
        get
        {
            return (float)monsterKilled / (float)monsters;
        }
    }
    public float averagemonster   //14.aantal monsters per kamer
    {
        get
        {
            return (float)monsters / (float)rooms;
        }
    }
    public Measurements(int _hitted = 0, float _score = 0, int _items = 0, int _monsterKilled = 0, float _damage = 0, int _rooms = 0, int _monsterhit = 0, float _monsterDmg = 0, int _deaths = 0, int _trapped = 0, float _steps = 0, int _attacks = 0, float _noMove = 0, int _monsters = 0)
    {
        hitted = _hitted;
        score = _score;
        items = _items;
        monsterKilled = _monsterKilled;
        damage = _damage;
        rooms = _rooms;
        monsterhit = _monsterhit;
        monsterDmg = _monsterDmg;
        deaths = _deaths;
        trapped = _trapped;
        steps = _steps;
        attacks = _attacks;
        noMove = _noMove;
        monsters = _monsters;

        runRoom = new Dictionary<int, int>();
        
        roomTime = new Dictionary<int, float>();
    }

    public void VisitRoom(int roomnumber)
    {
        rooms++;
        if (runRoom.ContainsKey(roomnumber))
            runRoom[roomnumber]++;
        else {
            roomTime.Add(roomnumber, 0f);
            runRoom.Add(roomnumber, 1);
        }
    }
    public void DoDmg(float dmg)
    {
        hitted++;
        damage += dmg;  
    }
    public void GetDmg(float dmg)
    {
        monsterhit++;
        monsterDmg += dmg;
    }
}

public class StatisticsManager : MonoBehaviour
{
    /// <summary>
    /// Signleton Pattern
    /// </summary>
    private static StatisticsManager statisticsInstance;
    public static StatisticsManager StatisticsInstance
    {
        get
        {
            if (statisticsInstance == null)
                Debug.LogError("There is no " + StatisticsInstance.GetType() + " set.");
            return statisticsInstance;
        }
        private set
        {
            if (statisticsInstance != null)
                Debug.LogError("Two instances of the " + StatisticsInstance.GetType() + " are sethere is no DirectorAI set.");
            statisticsInstance = value;
        }
    }

    private Measurements measurements;
    private float Timer = 0;
    public bool Run = false;
    private Action<float> OnScoreChange;
    public float Score
    {
        get
        {
            return measurements.score;
        }
        private set
        {
            measurements.score = value;
            OnScoreChange?.Invoke(value);
        }
    }
    public Measurements Measurements
    {
        get
        {
            return measurements;
        }
    }
    public void SubscribeScoreChange(Action<float> registerAction)
    {
        OnScoreChange += registerAction;
    }


    public void SetDamage(IUnit.Unit unit, float dmg)
    {
        if (unit == IUnit.Unit.Monster)
            measurements.DoDmg(dmg);
        else if (unit == IUnit.Unit.Player)
            measurements.GetDmg(dmg);
    }
    public void SetDeath(IUnit.Unit unit)
    {
        if (unit == IUnit.Unit.Monster)
        {
            measurements.monsterKilled++;
        }
        else if (unit == IUnit.Unit.Player)
        {
            measurements.deaths++;
        }
    } 
    public void FinishedRoom(int currentroom, int nextroom, int monsterroom)
    {
        measurements.monsters += monsterroom;
        measurements.roomTime[currentroom] = Timer;
        Timer = 0;
        measurements.VisitRoom(nextroom);
    }
    public void GetItem(float value = 0)
    {
        measurements.items++;
        Score += value;
    }
 

    /// <summary>
    /// This sends and message to the analytics dashboard.
    /// </summary>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public bool SendEvent(messageType encoding)
    {
        AnalyticsResult result = AnalyticsResult.InvalidData;
        switch(encoding)
        {
            case messageType.death:
                result = Analytics.CustomEvent("Death", new Dictionary<string, object>
                {
                    { "level_id", 0 },
                    { "score", Score },
                    { "difficulty", 0}
                });
                break;
            case messageType.levelComplete:
                result = Analytics.CustomEvent("Level Completed", new Dictionary<string, object>
                {
                    { "level_id", 0 },
                    { "time_elapsed", Time.timeSinceLevelLoad },
                    { "difficulty", 0}
                });
                break;
            default:
                break;
        }
        return result == AnalyticsResult.Ok;
    }


    private void Awake()
    {
        StatisticsInstance = this;
    }
    public void Update()
    {
        Timer += Time.deltaTime;
        if (Run)
            measurements.steps += Time.deltaTime;
        else
            measurements.noMove += Time.deltaTime;
    }
}
public enum messageType
{
    levelComplete,
    death,
    gameover
}


// TODO
// IMLEMENT THE ITEMS AND LINK THE GETSCORE
// IMPLEMENT FINISHED ROOM
//  TRAPPED
// NOMOVE
// STEPS