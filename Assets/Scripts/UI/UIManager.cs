using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections;
using System.Threading.Tasks;

public enum UIState
{
    InMainMenu,
    InGame,
    InGameMenu,
    InScoreboard,
    PopUp,
    Unloaded
}


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

    private GameObject[] panels;    // 0 = Main Menu
                                    // 1 = GameOverlay
                                    // 2 = Game Menu
                                    // 3 = Scoreboard
                                    // 4 = Popup

    private UIState currentState = UIState.Unloaded;

    void Awake()
    {
        UiInstance = this;

        GameObject UiElements = GetComponentInChildren<Canvas>().gameObject;


        panels = UiElements.GetComponentsInChildren<Transform>(true)
            .Where(predicate => predicate.parent == transform.GetChild(0))
            .Select(predicate => predicate.gameObject).ToArray();

        healthBar = panels[1].GetComponentInChildren<Slider>();
        scoreboard = panels[1].GetComponentInChildren<TextMeshProUGUI>();
        playerGFX = transform.GetComponentsInChildren<Image>(true).FirstOrDefault(predicate => predicate.name == "Player_GFX");

        ChangeStateTo(UIState.InMainMenu);

    }
    void Start()
    { 

    }
    void Update()
    {

    }

    public void ChangeStateTo(UIState newState)
    {
        if (currentState != newState)
        {

            switch (newState)
            {
                case UIState.InMainMenu:
                    DeActivateAll();
                    ActivateMainMenu();
                    break;
                case UIState.InGame:
                    if(currentState == UIState.InMainMenu || currentState == UIState.Unloaded)
                    {
                        DeActivateAll();
                    }
                    else
                    {
                        DeActivate(currentState);
                    }
                    ActivateGameOverlay();
                    break;
                case UIState.InGameMenu:
                    if (currentState != UIState.InGame)
                        throw new Exception("Unexpected behavior.");
                    ActivateGameMenu();
                    break;
                case UIState.InScoreboard:
                    if (currentState != UIState.InGame)
                        throw new Exception("Unexpected behavior.");
                    ActivateScoreboard();
                    break;
            }
        }
    }

    #region Activate Panels
    private void ActivateScoreboard()
    {
        //Disable all inputs
        //Freeze Game
        panels[3].SetActive(true);
    }
    private void ActivateGameMenu()
    {
        //Disable all inputs
        //Freeze Game

        panels[2].SetActive(true);
    }
    private void ActivateGameOverlay()
    {
        panels[1].SetActive(true);

        PlayerController.PlayerInstance.SubscribeHealthChange(
        (float newHealth) =>
        {
            healthBar.value = newHealth * 0.95f;
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
    private void ActivateMainMenu()
    {
        panels[0].SetActive(true);
    }
    #endregion

    #region Deactivate Panels
    private void DeActivateAll()
    {
        foreach (UIState state in Enum.GetValues(typeof(UIState)))
        {
            DeActivate(state);
        }
    }
    private void DeActivate(UIState state)
    {
        switch(state)
        {
            case UIState.InGame:
                panels[1].SetActive(false);
                break;
            case UIState.InGameMenu:
                panels[2].SetActive(false);
                break;
            case UIState.InMainMenu:
                panels[0].SetActive(false);
                break;
            case UIState.InScoreboard:
                panels[3].SetActive(false);
                break;
            case UIState.PopUp:
                panels[4].SetActive(false);
                break;
            case UIState.Unloaded:
                break;
        }
    }
    #endregion

    public void LaunchGame(Func<int, IEnumerator> onGameStart)
    {
        StartCoroutine(onGameStart(-1));
    }

    public void SlideClose()
    {
        return;
    }
    public void SlideOpen()
    {
        return;
    }

    public void AreYouSure(string displayText, Func<IEnumerator> actionOnRelease)
    {
        Action Accord =
            () =>
            {
                StartCoroutine(actionOnRelease());
            };
    }
}
