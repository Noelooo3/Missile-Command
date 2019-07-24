using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float CurrentGameLevel = 0;
    public int CurrentScore = 0;
    public int HighestScore = 0; 
    private bool GameOn = false;
    
    private int CurrentSpeedLevel = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("HighestScore"))
        {
            PlayerPrefs.SetInt("HighestScore", 0);
        }

        HighestScore = PlayerPrefs.GetInt("HighestScore");
        UIManager.Instance.UpdateHighest(HighestScore);
    }

    public void StartNewGame()
    {
        UIManager.Instance.StartGame();
        GameOn = true;
        CurrentScore = 0;
        CurrentGameLevel = 0;

        UIManager.Instance.ShowDisplayPopup($"Level {CurrentGameLevel + 1}");
        UIManager.Instance.UpdateScore(0);

        BuildingManager.Instance.SpawnBuilding(true);
        LaunchManager.Instance.StartGame(10 + (int)CurrentGameLevel * 2, 1 + (int)(0.5f * CurrentGameLevel));
        EnemyManager.Instance.StartGame(10 + (int)CurrentGameLevel * 5, Mathf.Min(0, 1 - CurrentGameLevel), Mathf.Min(5, 10 - CurrentGameLevel), 0 + (int)(CurrentGameLevel * 0.75f), Mathf.Min(2, 4 - CurrentGameLevel), Mathf.Min(7, 10 - CurrentGameLevel));
    }

    private void ContinueGame()
    {
        GameOn = true;
        CurrentGameLevel++;

        UIManager.Instance.ShowDisplayPopup($"Level {CurrentGameLevel + 1}");

        BuildingManager.Instance.SpawnBuilding(false);
        LaunchManager.Instance.StartGame(10 + (int)CurrentGameLevel * 2, 1 + (int)(0.5f * CurrentGameLevel));
        EnemyManager.Instance.StartGame(10 + (int)CurrentGameLevel * 5, Mathf.Min(0, 1 - CurrentGameLevel), Mathf.Min(5, 10 - CurrentGameLevel), 0 + (int)(CurrentGameLevel * 0.75f), Mathf.Min(2, 4 - CurrentGameLevel), Mathf.Min(7, 10 - CurrentGameLevel));
    }

    public IEnumerator LevelUp()
    {
        yield return new WaitForSeconds(2f);

        LaunchManager.Instance.StopGame();
        EnemyManager.Instance.StopGame();

        yield return StartCoroutine(BuildingManager.Instance.RemoveBuildings());
        yield return StartCoroutine(LaunchManager.Instance.RemoveLaunchers());

        yield return new WaitForSeconds(1f);

        if (CurrentScore > HighestScore)
        {
            HighestScore = CurrentScore;
            UIManager.Instance.UpdateHighest(HighestScore);
            PlayerPrefs.SetInt("HighestScore", HighestScore);
        }

        yield return new WaitForSeconds(1f);

        ContinueGame();
    }

    public IEnumerator GameOver()
    {
        yield return new WaitForSeconds(2f);

        LaunchManager.Instance.StopGame();
        EnemyManager.Instance.StopGame();

        yield return StartCoroutine(BuildingManager.Instance.RemoveBuildings());
        yield return StartCoroutine(LaunchManager.Instance.RemoveLaunchers());

        yield return new WaitForSeconds(1f);

        if (CurrentScore > HighestScore)
        {
            HighestScore = CurrentScore;
            UIManager.Instance.UpdateHighest(HighestScore);
            PlayerPrefs.SetInt("HighestScore", HighestScore);
        }

        UIManager.Instance.ShowDisplayPopup("Game Over");

        yield return new WaitForSeconds(1f);

        UIManager.Instance.BackToMenu();
    }

    public void GameOverWithNoBuilding()
    {
        if (GameOn)
        {
            GameOn = false;
            StartCoroutine(GameOver());
        }
    }

    public void GameFinishedWithNoEnemy()
    {
        if (GameOn)
        {
            GameOn = false;
            StartCoroutine(LevelUp());
        }
    }

    public void OnPauseClicked()
    {
        Time.timeScale = (Time.timeScale == 1f) ? 0f : 1f;
    }

    public void AddScore(int score, bool display)
    {
        CurrentScore += score;
        UIManager.Instance.UpdateScore(CurrentScore);
        if (display)
        {
            UIManager.Instance.ShowDisplayPopup($"+{score}");
        }
    }

    #region Set slow motion for great shot
    public void DoSlowMotion(int level)
    {
        if(level >= 3)
        {
            if (level < CurrentSpeedLevel) return;
            if(_SlowMotionCoroutine != null)
            {
                StopCoroutine(_SlowMotionCoroutine);
            }

            _SlowMotionCoroutine = SlowMotionCoroutine(level);
            StartCoroutine(_SlowMotionCoroutine);
        }
    }


    private IEnumerator _SlowMotionCoroutine;
    private IEnumerator SlowMotionCoroutine(int level)
    {
        CurrentSpeedLevel = level;
        float timeScale = Mathf.Max(0.4f, (1f - (float)level / 10f));

        while(Time.timeScale > timeScale)
        {
            Time.timeScale -= Time.deltaTime;
            yield return null;
        }
        Time.timeScale = timeScale;

        yield return new WaitForSeconds(2f);

        while (Time.timeScale < 1)
        {
            Time.timeScale += Time.deltaTime;
            yield return null;
        }

        Time.timeScale = 1;
        CurrentSpeedLevel = 0;
    }
    #endregion
}

