using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class LevelExit : MonoBehaviour
{
    public int room;
    public int door;

    private Tilemap map;
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

    bool x = true;
    public void Exit(Collider2D col)
    {
        if (x)
        {
            this.x = false;
            List<ContactPoint2D> points = new List<ContactPoint2D>();
            int amount = col.GetContacts(points);
            if (amount != 1)
            {
                Debug.LogError("Error");
            }

            var x = map.WorldToCell(points[0].point);
            if (!StatisticsManager.StatisticsInstance.SendEvent(messageType.levelComplete))
                Debug.LogError("Message didnt send");


            DirectorManager.DirectorInstance.UpdateState(room,door);

            SceneTransistionManager.SceneInstance.TransitionToScene(typeOfScene.Game);
            this.x = true;
        }
    }
}
