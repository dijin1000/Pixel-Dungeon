using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public struct Node
{
    List<Node> connected;
    readonly int seed;

    public Node(int _seed)
    {
        connected = new List<Node>();
        seed = _seed;
    }
    public Node Get(int seedNext)
    {
        return connected.FirstOrDefault(predicate => predicate.seed == seedNext);
    }
}
public struct State
{
    public Node previousNode;
    public Node currentNode;
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

    State currentState;
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
        currentState.currentNode = new Node(UnityEngine.Random.seed);
    }
    public void SaveSlot()
    {
        throw new NotImplementedException();
    }


    public void UpdateState(int seedNext)
    {
        Node newCurrent = currentState.currentNode.Get(seedNext);
        State newState = new State();
        newState.currentNode = newCurrent;
        newState.previousNode = currentState.currentNode;
        currentState = newState;
    }

    public async Task NextLevel()
    {
        finished = false;
        Parameters p = new Parameters();
        p.State = currentState;
        //p.difficulty = Evaluate();
        
        await LevelManager.LevelInstance.CreateNewLevel(p);
        finished = true;
    }
}
