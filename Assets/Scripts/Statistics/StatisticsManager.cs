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

    private void Awake()
    {
        StatisticsInstance = this;
    }

    public int Retrieve()
    {
        return 0;
    }

    public bool SendEvent(messageType encoding)
    {
        AnalyticsResult result = AnalyticsResult.InvalidData;
        switch(encoding)
        {
            case messageType.death:
                result = Analytics.CustomEvent("Death");
                break;
            case messageType.levelComplete:
                result = Analytics.CustomEvent("secret_found", new Dictionary<string, object>
                {
                    { "secret_id", 0 },
                    { "time_elapsed", Time.timeSinceLevelLoad }
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