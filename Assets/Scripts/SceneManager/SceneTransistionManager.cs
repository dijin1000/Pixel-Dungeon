using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public enum typeOfScene
{
    MainMenu,
    Game,
    ExitGame,
    SlotLoad
}

public class SceneTransistionManager : MonoBehaviour
{
    /// <summary>
    /// Signleton Pattern
    /// </summary>
    private static SceneTransistionManager sceneInstance;
    public static SceneTransistionManager SceneInstance
    {
        get
        {
            if (sceneInstance == null)
                Debug.LogError("There is no " + SceneInstance.GetType() + " set.");
            return sceneInstance;
        }
        private set
        {
            if (sceneInstance != null)
                Debug.LogError("Two instances of the " + SceneInstance.GetType() + " are set.");
            sceneInstance = value;
        }
    }

    private bool isTransitioning = false;

    void Awake()
    {
        SceneInstance = this;
        DontDestroyOnLoad(gameObject.transform.parent.gameObject);
        
    }

    public void ExitGame()
    {
        TransitionToScene(typeOfScene.ExitGame);
    }

    public void StartOrLoadGame()
    {
        TransitionToScene(typeOfScene.Game);
    }

    public void BackToMainMenu()
    {
        TransitionToScene(typeOfScene.MainMenu);
    }

    public void TransitionToScene(typeOfScene param)
    {
        if (isTransitioning)
        {
            switch (param)
            {
                case typeOfScene.ExitGame:
                    UIManager.UiInstance.AreYouSure("", OnExit);
                    isTransitioning = true;
                    break;
                case typeOfScene.MainMenu:
                    //IS Saved?
                    isTransitioning = true;
                    if (true)
                        UIManager.UiInstance.AreYouSure("", OnBackToMainMenu);
                    else
                        UIManager.UiInstance.AreYouSure("", OnBackToMainMenu);
                    break;
                case typeOfScene.SlotLoad:
                    isTransitioning = true;
                    UIManager.UiInstance.LaunchGame(OnGameStart);
                    break;
                case typeOfScene.Game:
                    isTransitioning = true;
                    StartCoroutine(NextLevel);
                    break;
            }
        }
    }

    private IEnumerator OnExit()
    {
        yield return new WaitUntil(UIManager.UiInstance.SlideClosed());
        Application.Quit();
        
    }

    private IEnumerator OnBackToMainMenu()
    {
        yield return new WaitUntil(UIManager.UiInstance.SlideClosed);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("StartScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitUntil(UIManager.UiInstance.SlideOpened);

        isTransitioning = false;
    }

    private IEnumerator OnGameStart(int slotloading = -1)
    {
        yield return new WaitUntil(UIManager.UiInstance.SlideClosed);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("StartScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (slotloading != -1)  //SAVED GAME
        {
            DirectorManager.DirectorInstance.Load(slotloading);
            yield return new WaitUntil(DirectorManager.DirectorInstance.NextLevel());
        }
        else //NEWGAME
        {
            DirectorManager.DirectorInstance.NewSlot();
            yield return new WaitUntil(DirectorManager.DirectorInstance.NextLevel());
        }

        yield return new WaitUntil(UIManager.UiInstance.SlideOpened);

        isTransitioning = false;
    }

    private IEnumerator NextLevel()
    {
        yield return new WaitUntil(UIManager.UiInstance.SlideClosed);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("StartScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitUntil(DirectorManager.DirectorInstance.NextLevel());

        yield return new WaitUntil(UIManager.UiInstance.SlideOpened);

        isTransitioning = false;
    }

}
