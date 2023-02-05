using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Serializable]
    public class LevelData
    {
        public string sceneName;
        public string levelName;
    }
    public const string UNLOCKED_SUFIX = "_Unlocked";

    [Header("Container")]
    public GameObject mainContainer;
    public GameObject levelsContainer;
    public GameObject creditsContainer;

    [Header("Buttons")]
    public Button playButton;
    public Button creditsButton;
    public Button exitButton;
    public Button levelBackButton;
    public Button creditsBackButton;
    public Button clearProgressButton;
    public TextButton levelButtonTemplate;

    [Header("Levels")]
    public LevelData[] levelList = new LevelData[0];
    public bool unlockAllLevels = false;
    private List<TextButton> levelButtons = new List<TextButton>();

    private void Awake()
    {
        ShowMainScreen();

        playButton.onClick.AddListener(ShowLevelSelection);
        creditsButton.onClick.AddListener(ShowCredits);
        levelBackButton.onClick.AddListener(ShowMainScreen);
        creditsBackButton.onClick.AddListener(ShowMainScreen);

        exitButton.onClick.AddListener(() => Application.Quit());
        clearProgressButton.onClick.AddListener(() => { PlayerPrefs.DeleteAll(); PlayerPrefs.Save(); });

        for (int i = 0; i < levelList.Length; i++)
        {
            var levelBtn = Instantiate(levelButtonTemplate, levelButtonTemplate.transform.parent);
            levelBtn.text.text = levelList[i].levelName;
            string sceneName = levelList[i].sceneName;
            levelBtn.button.onClick.AddListener(() => SceneManager.LoadScene(sceneName));
            levelButtons.Add(levelBtn);
        }
        levelButtonTemplate.gameObject.SetActive(false);

        for (int i = 0; i < levelButtons.Count; i++)
        {
            var btn = levelButtons[i];

            if (i == 0 || unlockAllLevels || PlayerPrefs.HasKey(levelList[i].sceneName + UNLOCKED_SUFIX))
                btn.button.interactable = true;
            else
                btn.button.interactable = false;
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire2"))
            ShowMainScreen();
    }

    private void ShowCredits()
    {
        mainContainer.SetActive(false);
        levelsContainer.SetActive(false);
        creditsContainer.SetActive(true);

        creditsBackButton.Select();
    }

    private void ShowMainScreen()
    {
        mainContainer.SetActive(true);
        levelsContainer.SetActive(false);
        creditsContainer.SetActive(false);
        playButton.Select();
    }

    private void ShowLevelSelection()
    {
        mainContainer.SetActive(false);
        levelsContainer.SetActive(true);
        creditsContainer.SetActive(false);
        levelButtons[0].button.Select();
    }
}
