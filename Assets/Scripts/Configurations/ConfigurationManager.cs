using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;
using System.Linq;
using System;

public class ConfigurationManager : MonoBehaviour
{

    private struct userAttributes { }

    private struct appAttributes { }

    public string assignmentId;

    private string[] keys;

    /// <summary>
    /// Signleton Pattern
    /// </summary>
    private static ConfigurationManager configInstance;
    public static ConfigurationManager ConfigInstance
    {
        get
        {
            if (configInstance == null)
                Debug.LogError("There is no " + ConfigInstance.GetType() + " set.");
            return configInstance;
        }
        private set
        {
            if (configInstance != null)
                Debug.LogError("Two instances of the " + ConfigInstance.GetType() + " are set.");
            configInstance = value;
        }
    }

    void Awake()
    {
        ConfigInstance = this;

        ConfigManager.FetchCompleted += ApplyRemoteSettings;
        ConfigManager.SetCustomUserID("some-user-id");
        ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
    }

    public T getConfig<T>(string key) where T : IConvertible
    {
        keys = ConfigManager.appConfig.GetKeys();
        if (keys == null && !keys.Any(predicate => predicate == key))
            return default;
        if(typeof(T) == typeof(int))
            return (T)(object)ConfigManager.appConfig.GetInt(key);
        else if(typeof(T) == typeof(float))
            return (T)(object)ConfigManager.appConfig.GetFloat(key);
        else if (typeof(T) == typeof(bool))
            return (T)(object)ConfigManager.appConfig.GetBool(key);
        else if (typeof(T) == typeof(long))
            return (T)(object)ConfigManager.appConfig.GetLong(key);
        else if (typeof(T) == typeof(string))
            return (T)(object)ConfigManager.appConfig.GetString(key);
        else
            return default;
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
                assignmentId = ConfigManager.appConfig.assignmentID;
                break;
        }
    }
}
