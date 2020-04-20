using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections;

public class UIManager : MonoBehaviour
{
    /// <summary>
    /// Signleton Pattern
    /// </summary>
    private static UIManager uiInstance;
    public static UIManager UiInstance
    {
        get
        {
            if (uiInstance == null)
                Debug.LogError("There is no " + UiInstance.GetType() + " set.");
            return uiInstance;
        }
        private set
        {
            if (uiInstance != null)
                Debug.LogError("Two instances of the " + UiInstance.GetType() + " are set.");
            uiInstance = value;
        }
    }

    private Slider healthBar;
    private TextMeshProUGUI scoreboard;
    private Image playerGFX;

    private GameObject[] panels;

    void Awake()
    {
        UiInstance = this;


        panels = (transform.GetChild(0)).GetComponentsInChildren<Transform>(true)
            .Where(predicate => predicate.parent == transform.GetChild(0))
            .Select(predicate => predicate.gameObject).ToArray();

        healthBar = panels[1].GetComponentInChildren<Slider>();
        scoreboard = panels[1].GetComponentInChildren<TextMeshProUGUI>();
        playerGFX = transform.GetComponentsInChildren<Image>(true).FirstOrDefault(predicate => predicate.name == "Player_GFX");
    }

    //Can be upgrade to enum
    public bool CurrentState = true;
    public void SwithToInGame() {
        //DISABLE OTHE PANES
        panels[1].SetActive(true);
        panels[0].SetActive(false);
        PlayerController.PlayerInstance.SubscribeHealthChange(
            (float newHealth) => 
            {
                healthBar.value = newHealth*0.95f;
            }
        );

        PlayerController.PlayerInstance.SubscribeGraphicxsChange(
            (Sprite newSprite) =>
            {
                playerGFX.sprite = newSprite;
            }
        );

        StatisticsManager.StatisticsInstance.SubscribeScoreChange(
            (float newScore) =>
            {
                scoreboard.text = ((int)newScore).ToString();
            }
        );

        healthBar.value = PlayerController.PlayerInstance.Get_Health();
        scoreboard.text = ((int)StatisticsManager.StatisticsInstance.Score).ToString();
        playerGFX.sprite = PlayerController.PlayerInstance.Sprite_Player;
    }

    public void AreYouSure(string displayText, Func<IEnumerator> actionOnRelease)
    {
        Action Accord =
            () =>
            {
                StartCoroutine(actionOnRelease());
            };
    }

    public void SwithToMainMenu()
    {
        panels[0].SetActive(true);
        panels[1].SetActive(false);
    }

    internal void Switch(bool goingTo)
    {
        if (!CurrentState && goingTo)
        {
            SwithToMainMenu();
            CurrentState = true;
        }
        else if(CurrentState && !goingTo)
        {
            SwithToInGame();
            CurrentState = false;
        }
    }
}
