using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public void StartOrLoadGame()
    {
        //We have a new game starting

        //

        TransitionToScene(typeOfScene.SlotLoad);
    }


    public void Settings()
    {

    }

    public void ExitGame()
    {
        //Someone exit the game prematurly
        TransitionToScene(typeOfScene.ExitGame);
    }



    public void BackToMainMenu()
    {
        //Some one stopped with our game
        TransitionToScene(typeOfScene.MainMenu);
    }

    public void TransitionToScene(typeOfScene param)
    {
        if (!isTransitioning)
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
                    StartCoroutine(NextLevel());
                    break;
            }
        }
    }

    private IEnumerator OnExit()
    {
        Task slide = Task.Run(() => UIManager.UiInstance.SlideClose());
        while (slide.Status == TaskStatus.Running)
            yield return null;
        Application.Quit();
        
    }

    private IEnumerator OnBackToMainMenu()
    {
        Task slide = Task.Run(() => UIManager.UiInstance.SlideClose());
        while (slide.Status == TaskStatus.Running)
            yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("StartScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Task slideOpen = Task.Run(() => UIManager.UiInstance.SlideOpen());
        while (slideOpen.Status == TaskStatus.Running)
            yield return null;

        isTransitioning = false;
    }

    private IEnumerator OnGameStart(int slotloading = -1)
    {
        Task slide = Task.Run(() => UIManager.UiInstance.SlideClose());
        while (slide.Status == TaskStatus.Running)
            yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("EmptyScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (slotloading != -1)  //SAVED GAME
        {
            DirectorManager.DirectorInstance.Load(slotloading);
            Task.Run(async () => await DirectorManager.DirectorInstance.NextLevel());
            while (DirectorManager.DirectorInstance.Finished)
            {
                yield return null;
            }
        }
        else //NEWGAME
        {
            DirectorManager.DirectorInstance.NewSlot();
            Task.Run(async () => await DirectorManager.DirectorInstance.NextLevel());
            while (DirectorManager.DirectorInstance.Finished)
            {
                yield return null;
            }
        }

        while(!LevelManager.LevelInstance.updated )
        {
            yield return null;
        }

        //UIManager.UiInstance.Switch(false);

        Task slideOpen = Task.Run(() => UIManager.UiInstance.SlideOpen());
        while (slideOpen.Status == TaskStatus.Running)
            yield return null;

        isTransitioning = false;
    }

    private IEnumerator NextLevel()
    {
        Task slide = Task.Run(() => UIManager.UiInstance.SlideClose());
        while (slide.Status == TaskStatus.Running)
            yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("EmptyScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Task.Run(async () => await DirectorManager.DirectorInstance.NextLevel());
        while (DirectorManager.DirectorInstance.Finished)
        {
            yield return null;
        }

        Task slideOpen = Task.Run(() => UIManager.UiInstance.SlideOpen());
        while (slideOpen.Status == TaskStatus.Running)
            yield return null;

        isTransitioning = false;
    }

}
