using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class LevelExit : MonoBehaviour
{
    public Dictionary<Vector2Int,int> doors = new Dictionary<Vector2Int, int>(); //which positions is for which door

    private Tilemap map;
    private TilemapCollider2D collider2D;
    private bool isProcessing = false;
    public Dictionary<Vector2Int,bool> isLastDoor = new Dictionary<Vector2Int, bool>();
    
    public void Awake()
    {
        map = GetComponent<Tilemap>();
        collider2D = GetComponent<TilemapCollider2D>();
    }
    void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            Exit(col);
        }
    }

    public void Exit(Collider2D col)
    {
        if (!isProcessing)
        {
            isProcessing = true;
            if (col.tag == "Player") 
            {
                Vector3Int collisionPoint = map.WorldToCell(collider2D.ClosestPoint(col.transform.position));
                TileBase hasTile = map.GetTile(collisionPoint);
                if (hasTile != null)
                {
                    if (!StatisticsManager.StatisticsInstance.SendEvent(messageType.levelComplete))
                        Debug.LogError("Message didnt send");

                    if (!isLastDoor[(Vector2Int)collisionPoint])
                    {
                        int door = doors[(Vector2Int)collisionPoint];
                        DirectorManager.DirectorInstance.UpdateState(door);

                        SceneTransistionManager.SceneInstance.TransitionToScene(typeOfScene.Game);
                    }
                    else
                    {
                        UIManager.UiInstance.ChangeStateTo(UIState.InScoreboard);
                    }
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
