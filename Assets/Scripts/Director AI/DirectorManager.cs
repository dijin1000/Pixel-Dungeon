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
        Measurements m = StatisticsManager.StatisticsInstance.Measurements;
        Parameters p = new Parameters(currentState);
        
        //LOGIC for JACCO en MAURITS




        
        await LevelManager.LevelInstance.CreateNewLevel(p);
        finished = true;
    }
}
