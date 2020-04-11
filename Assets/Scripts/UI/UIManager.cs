using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public GameObject Canvas;

    private Slider healthBar;
    private TextMeshProUGUI scoreboard;

    void Awake()
    {
        UiInstance = this;
        healthBar = Canvas.GetComponentInChildren<Slider>();
        scoreboard = Canvas.GetComponentInChildren<TextMeshProUGUI>();

    }
    private void Start()
    {
        PlayerController.PlayerInstance.SubscribeHealthChange(
            (float newHealth) => 
            {
                healthBar.value = newHealth/100f*0.95f;
            }
        );
        StatisticsManager.StatisticsInstance.SubscribeScoreChange(
            (float newScore) =>
            {
                scoreboard.text = ((int)newScore).ToString();
            }
        );
    }


}
