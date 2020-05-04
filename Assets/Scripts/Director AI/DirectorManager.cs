using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    int results;
    private Parameters currentState;


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
        currentState = new Parameters();
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
        currentState.room = room;
    }

    public async Task NextLevel()
    {
        finished = false;
        Measurements m = StatisticsManager.StatisticsInstance.Measurements;


        //currentState.roomSize = ? ;

        //LOGIC for JACCO en MAURITS




        
        await LevelManager.LevelInstance.CreateNewLevel(currentState);
        finished = true;
    }
}
