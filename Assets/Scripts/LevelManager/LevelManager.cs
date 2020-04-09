using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LevelManager : MonoBehaviour
{
    private LevelConfig config;

    /// <summary>
    /// Signleton Pattern
    /// </summary>
    private static LevelManager levelInstance;
    public static LevelManager LevelInstance
    {
        get
        {
            if (levelInstance == null)
                Debug.LogError("There is no " + LevelInstance.GetType() + " set.");
            return levelInstance;
        }
        private set
        {
            if (levelInstance != null)
                Debug.LogError("Two instances of the " + LevelInstance.GetType() + " are set.");
            levelInstance = value;
        }
    }

    void Awake()
    {
        LevelInstance = this;
    }
    void Start() { 
        config = new LevelConfig();
        config.Width = ConfigurationManager.ConfigInstance.getConfig<int>("Width");
        config.Height = ConfigurationManager.ConfigInstance.getConfig<int>("Height");
    }

    public void Level(Parameters p )
    {
        if (!StatisticsManager.StatisticsInstance.SendEvent(messageType.death))
            Debug.LogError("Message didnt send");
        //If there is a need for a new level with the intensity of some sort captured in p
        if (true)
        {
            CreateLevel(p);
        }
        else
        {
            //Need some sort of back and forth method linked list of levels
        }
    }

    private void CreateLevel(Parameters param)
    {
        // Creating the first entry level
        if(param.firstLevel)
        {
            Debug.Log("Create new Level with H X W: 10 X 10");
        }
        else
        {
            Debug.Log("Create new Level with H X W: " + config.Height + " X " + config.Width);
        }
    }
}
public struct Parameters
{
    public bool firstLevel;
}