using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

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

    private float score;
    private Action<float> OnScoreChange;
    public float Score
    {
        get
        {
            return score;
        }
        private set
        {
            score = value;
            OnScoreChange?.Invoke(score);
        }
    }

    public void SubscribeScoreChange(Action<float> registerAction)
    {
        OnScoreChange += registerAction;
    }

    private void Awake()
    {
        StatisticsInstance = this;
    }

    //#TODO needs to return more information such that the DirectorAI can create and request new levels
    //INT Can be any class
    public Tuple<bool,int> Retrieve()
    {
        return new Tuple<bool, int>(true,0);
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
                    { "score", 0 },
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
}
public enum messageType
{
    levelComplete,
    death,
    gameover
}