using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public Camera UICamera;
    public tk2dTextMesh ScoreDisplay;
    public tk2dTextMesh LevelDisplay;

    public void InitializeUI()
    {
    }

    public void RefreshScore()
    {
        ScoreDisplay.text = GameManager.Instance.PlayerPerformanceStatistics.Score.ToString();
        ScoreDisplay.Commit();
    }

    public void RefreshLevel()
    {
        LevelDisplay.text = (GameManager.Instance.LevelManager.CurrentLevelNumber + 1).ToString();
        LevelDisplay.Commit();
    }

    public void ShowGameOver()
    {
    }

    public void HideGameOver()
    {
    }

    public void ShowGamePaused()
    {
    }

    public void HideGamePaused()
    {
    }

    public void ShowTitleScreen()
    {
    }

    public void HideTitleScreen()
    {
    }

    public void ShowLoadingScreen()
    {
    }

    public void HideLoadingScreen()
    {
    }

    public void ShowHelpScreen()
    {
    }

    public void HideHelpScreen()
    {
    }

    public void HideAllMasks()
    {
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
