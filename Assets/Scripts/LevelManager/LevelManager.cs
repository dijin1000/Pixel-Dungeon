using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;

public class LevelManager : MonoBehaviour
{
    private struct userAttributes { }

    private struct appAttributes { }

    public string assignmentId;
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
        config = new LevelConfig();

        // Add a listener to apply settings when successfully retrieved: 
        ConfigManager.FetchCompleted += ApplyRemoteSettings;

        // Set the user’s unique ID:
        ConfigManager.SetCustomUserID("some-user-id");

        // Fetch configuration setting from the remote service: 
        ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
    }

    // Create a function to set your variables to their keyed values:
    void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.Log("No settings loaded this session; using default values.");
                break;
            case ConfigOrigin.Cached:
                Debug.Log("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                Debug.Log("New settings loaded this session; update values accordingly.");
                config.Width = ConfigManager.appConfig.GetInt("Width");
                config.Height = ConfigManager.appConfig.GetInt("Height");
                assignmentId = ConfigManager.appConfig.assignmentID;
                break;
        }
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