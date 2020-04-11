using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

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
    void Awake()
    {
        SceneInstance = this;
        DontDestroyOnLoad(gameObject.transform.parent.gameObject);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LoadGame()
    {

        //Some Basic initialization
        Transit(new Vector2Int(2, 1));
    }

    public void Transit(Vector2Int t)
    {
        StartCoroutine(transistion(t));
    }

    IEnumerator transistion(Vector2Int t)
    {
        if(DirectorManager.DirectorInstance.RetrieveInformation())
        {
            //ANIMATOR SLIDE


            ////////////////

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SampleScene");

            //possibility to store previous scene.

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            DirectorManager.DirectorInstance.CreateNewLevel(t);
            UIManager.UiInstance.Switch(false);
            //ANIMATOR SLIDE


            ////////////////
        }
        else
        {
            Debug.LogError("Something terrible went wrong.");
        }
    }

}
