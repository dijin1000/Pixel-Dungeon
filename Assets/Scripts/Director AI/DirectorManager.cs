using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    /// <summary>
    /// This is the test setup that the director should be called when a level should be started.
    /// </summary>
    void Test()
    {
        Parameters p = new Parameters();
        p.firstLevel = true;

        int v = StatisticsManager.StatisticsInstance.Retrieve();

        LevelManager.LevelInstance.Level(p);
        
    }
}
