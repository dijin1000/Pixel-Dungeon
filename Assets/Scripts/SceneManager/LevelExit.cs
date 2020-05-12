using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class LevelExit : MonoBehaviour
{
    public Dictionary<Vector2Int,int> doors = new Dictionary<Vector2Int, int>(); //which positions is for which door

    private Tilemap map;
    private bool isProcessing = false;
    public Dictionary<Vector2Int,bool> isLastDoor = new Dictionary<Vector2Int, bool>();
    
    public void Awake()
    {
        map = GetComponent<Tilemap>();
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            storedAction = (InputAction.CallbackContext parameter) => { Exit(col); };
            PlayerController.PlayerInstance.Subscribe(storedAction, ControlType.Enter);
        }
    }

    Action<InputAction.CallbackContext> storedAction;

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            PlayerController.PlayerInstance.UnSubscribe(storedAction, ControlType.Enter);
        }
    }


    public void Exit(Collider2D col)
    {
        if (!isProcessing)
        {
            isProcessing = true;
            if (col.tag == "Player") 
            {
                var x = map.WorldToCell(col.transform.position);
                if (!StatisticsManager.StatisticsInstance.SendEvent(messageType.levelComplete))
                    Debug.LogError("Message didnt send");

                if (!isLastDoor[new Vector2Int(x.x, x.y)])
                {
                    int door = doors[new Vector2Int(x.x, x.y)];
                    DirectorManager.DirectorInstance.UpdateState(door);

                    SceneTransistionManager.SceneInstance.TransitionToScene(typeOfScene.Game);
                }
                else
                {
                    UIManager.UiInstance.ChangeStateTo(UIState.InScoreboard);
                } 
            }
            isProcessing = false;
        }
        else
        {
            UIManager.UiInstance.ChangeStateTo(UIState.InScoreboard);
        }
    }
}
