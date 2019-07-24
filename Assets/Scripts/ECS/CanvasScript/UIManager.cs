using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public CanvasGroupToggle Menu, InGameHud, Popup;

    public Text HighestScoreText1, HighestScoreText2;
    public Text CurrentScore;
    public Text Launcher1Ammo1, Launcher1Ammo2;
    public Text Launcher2Ammo1, Launcher2Ammo2;
    public Text Launcher3Ammo1, Launcher3Ammo2;

    public Text DisplayPopup;

    public void Awake()
    {
        Instance = this;
        Menu.Show();
        InGameHud.Hide();
    }

    public void StartGame()
    {
        Menu.Hide();
        InGameHud.Show();
    }

    public void BackToMenu()
    {
        Menu.Show();
        InGameHud.Hide();
    }

    public void UpdateScore(int score)
    {
        CurrentScore.text = score.ToString();
    }

    public void UpdateHighest(int score)
    {
        HighestScoreText1.text = score.ToString();
        HighestScoreText2.text = score.ToString();
    }

    public void UpdateLauncher1(int ammo1, int ammo2)
    {
        Launcher1Ammo1.text = ammo1.ToString();
        Launcher1Ammo2.text = ammo2.ToString();
    }

    public void UpdateLauncher2(int ammo1, int ammo2)
    {
        Launcher2Ammo1.text = ammo1.ToString();
        Launcher2Ammo2.text = ammo2.ToString();
    }
    public void UpdateLauncher3(int ammo1, int ammo2)
    {
        Launcher3Ammo1.text = ammo1.ToString();
        Launcher3Ammo2.text = ammo2.ToString();
    }

    public void ShowDisplayPopup(string text)
    {
        Popup.Show();
        StopCoroutine("ShowDisplayPopupCoroutine");
        DisplayPopup.text = text;
        DisplayPopup.color = Color.white;
        StartCoroutine("ShowDisplayPopupCoroutine");
    }

    private IEnumerator ShowDisplayPopupCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        while(DisplayPopup.color.a > 0)
        {
            DisplayPopup.color = new Color(1f, 1f, 1f, DisplayPopup.color.a - Time.deltaTime);
            yield return null;
        }

        Popup.Hide();
    }
}
