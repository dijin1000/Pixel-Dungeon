using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public struct State
{
    public int door;
    public int room;
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
    private State currentState;


    private bool finished;
    public bool Finished
    {
        get
        { 
            return finished; 
        }
    }

    void Awake()
    {
        DirectorInstance = this;
    }
    
    public bool RetrieveInformation()
    {
        var v = StatisticsManager.StatisticsInstance.Retrieve();
        if(v.Item1)
            results = v.Item2;
        return v.Item1;
    }


    public void Load(int slotloading)
    {
        throw new NotImplementedException();
    }
    internal void NewSlot()
    {
        currentState = new State();
        currentState.room = 0;
        currentState.door = -1;
    }
    public void SaveSlot()
    {
        throw new NotImplementedException();
    }

    public void UpdateState(int room,int door)
    {
        currentState.door = door;
    }

    public async Task NextLevel()
    {
        finished = false;
        Parameters p = new Parameters(currentState);
        //p.difficulty = Evaluate();
        
        await LevelManager.LevelInstance.CreateNewLevel(p);
        finished = true;
    }
}
