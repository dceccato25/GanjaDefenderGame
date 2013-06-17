using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GanjaManager GanjaManager;
    public LevelManager LevelManager;
    public UIManager UIManager;

    public PlayerPerformanceStatistics PlayerPerformanceStatistics = new PlayerPerformanceStatistics();

    public int LevelStartOverride = 0;

    private bool isGameOver;

    public void AddPoints(int points)
    {
        PlayerPerformanceStatistics.Score += points;
        UIManager.RefreshScore();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            //DontDestroyOnLoad(gameObject);
            Instance = this;
            HandleGameReset();
        }
        else
        {
            Debug.LogWarning("There should only be one of these!");
        }

        UIManager.InitializeUI();
        UIManager.ShowLoadingScreen();
        UIManager.RefreshScore();
    }

    // Use this for initialization
    private void Start()
    {
        UIManager.ShowLoadingScreen();
        UIManager.RefreshScore();
    }

    private bool isSlowMotion = false;
    private bool isPaused = false;
    // Update is called once per frame
    private void Update()
    {
        if (GanjaManager.GanjaPlants.All(p => !p.IsAlive))
        {
            HandleGameOver();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (!isSlowMotion)
            {
                Time.timeScale = 0.1f;
                isSlowMotion = true;
            }
            else
            {
                Time.timeScale = 1.0f;
                isSlowMotion = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isPaused)
            {
                isPaused = true;
                Time.timeScale = 0f;
            }
            else
            {
                isPaused = false;
                Time.timeScale = isSlowMotion ? 0.2f : 1.0f;
            }
        }
    }

    private void HandleGameOver()
    {
        isGameOver = true;
        Time.timeScale = 0.0f;
        UIManager.ShowGameOver();
    }

    private void HandleGameReset()
    {
        GanjaManager.ResetPlants();

        foreach (ArmyGuyController guy in gameObject.GetComponents<ArmyGuyController>())
        {
            Destroy(guy.gameObject);
        }

        Time.timeScale = 1.0f;
        isGameOver = false;
        LevelManager.SelectStage(0);
        LevelManager.SelectLevel(LevelStartOverride);
        //LevelManager.ResetCurrentLevel();
        LevelManager.StartCurrentLevel();
        UIManager.HideAllMasks();
    }

    private void OnGUI()
    {
        //if (isGameOver)
        //{
        //    var menuRect = new Rect(
        //        (Screen.width - 150)*0.5f,
        //        (Screen.height - 150)*0.5f,
        //        150,
        //        150
        //        );

        //    GUILayout.BeginArea(menuRect);
        //    GUILayout.FlexibleSpace();

        //    GUI.TextArea(new Rect(0, 0, 150, 25), "Game over, man.");

        //    if (MenuButton("Try Again!"))
        //    {
        //        HandleGameReset();
        //    }

        //    GUILayout.FlexibleSpace();
        //    GUILayout.EndArea();
        //    return;
        //}

        //GUI.TextField(new Rect(15, 15, 100, 25), "Level: " + (LevelManager.CurrentLevelNumber + 1));

        //if (LevelManager.LevelTimeElapsed < 4)
        //{
        //    var menuRect = new Rect(
        //        (Screen.width - 250)*0.5f,
        //        (Screen.height - 150)*0.5f,
        //        250,
        //        150
        //        );

        //    GUILayout.BeginArea(menuRect);
        //    GUILayout.FlexibleSpace();

        //    GUI.TextArea(new Rect(0, 0, 250, 25),
        //                 "Starting level " + (LevelManager.CurrentLevelNumber + 1) + ". Get ready!");

        //    GUILayout.FlexibleSpace();
        //    GUILayout.EndArea();
        //}
    }

    private bool MenuButton(string text)
    {
        bool wasPressed = false;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        Rect rect = GUILayoutUtility.GetRect(150, 25, GUILayout.Width(150), GUILayout.Height(25));

        bool ishit = GUI.Button(rect, text);

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        return ishit;
    }
}

public struct PlayerPerformanceStatistics
{
    public int Score;
}