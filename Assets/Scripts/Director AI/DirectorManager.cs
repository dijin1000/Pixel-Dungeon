using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    void Awake()
    {
        DirectorInstance = this;
    }

    int results;
    public bool RetrieveInformation()
    {
        var v = StatisticsManager.StatisticsInstance.Retrieve();
        if(v.Item1)
            results = v.Item2;
        return v.Item1;
    }

    internal void CreateNewLevel(Vector2Int t)
    {
        // do something with results

        Parameters p = new Parameters();
        p.firstLevel = true;
        p.startTile = t;

        LevelManager.LevelInstance.CreateNewLevel(p);
    }
}
