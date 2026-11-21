using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Главное меню: Продолжить, Новая игра, Настройки, Авторы, Выход.
public class MainMenu : MonoBehaviour
{
    [Tooltip("Имя сцены с игрой (как в Build Settings)")]
    public string gameSceneName = "Cacatonia";

    [Header("Экраны")]
    public GameObject mainPanel;
    public GameObject creditsPanel;
    public GameObject loadingScreen;

    [Header("Кнопки")]
    public Button continueButton;
    public Button newGameButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;
    public Button creditsBackButton;

    [Header("Окна")]
    public SettingsMenu settings;
    public ConfirmDialog confirm;

    void Start()
    {
        // После выхода из игры в меню всё должно «размораживаться»
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        continueButton.onClick.AddListener(ContinueGame);
        newGameButton.onClick.AddListener(NewGame);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(AskQuit);
        creditsBackButton.onClick.AddListener(CloseCredits);
        settings.Closed += OnSettingsClosed;

        mainPanel.SetActive(true);
        creditsPanel.SetActive(false);
        if (loadingScreen != null) loadingScreen.SetActive(false);

        UpdateContinueButton();
    }

    void OnDestroy()
    {
        if (settings != null) settings.Closed -= OnSettingsClosed;
    }

    void Update()
    {
        // Esc закрывает открытое окно
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (confirm.IsOpen) confirm.Hide();
        else if (settings.IsOpen) settings.Close();
        else if (creditsPanel.activeSelf) CloseCredits();
    }

    void UpdateContinueButton()
    {
        bool has = SaveSystem.HasSave;
        continueButton.interactable = has;
        TMP_Text label = continueButton.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = has ? "Продолжить (глава " + SaveSystem.SavedChapter + ")" : "Продолжить";
            label.alpha = has ? 1f : 0.35f;
        }
    }

    void ContinueGame()
    {
        SaveSystem.PrepareContinue();
        LoadGame();
    }

    void NewGame()
    {
        if (SaveSystem.HasSave)
        {
            confirm.Show("Начать новую игру?\nСохранённый прогресс будет удалён.", () =>
            {
                SaveSystem.DeleteSave();
                StartNewGame();
            });
        }
        else
        {
            StartNewGame();
        }
    }

    void StartNewGame()
    {
        SaveSystem.PrepareNewGame();
        LoadGame();
    }

    void LoadGame()
    {
        StartCoroutine(LoadRoutine());
    }

    IEnumerator LoadRoutine()
    {
        mainPanel.SetActive(false);
        if (loadingScreen != null) loadingScreen.SetActive(true);
        yield return null; // кадр, чтобы надпись «Загрузка...» успела появиться
        AsyncOperation op = SceneManager.LoadSceneAsync(gameSceneName);
        while (!op.isDone) yield return null;
    }

    void OpenSettings()
    {
        mainPanel.SetActive(false);
        settings.Open();
    }

    void OnSettingsClosed()
    {
        mainPanel.SetActive(true);
    }

    void OpenCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    void CloseCredits()
    {
        creditsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    void AskQuit()
    {
        confirm.Show("Выйти из игры?", QuitGame);
    }

    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // в редакторе просто останавливаем Play
#else
        Application.Quit();
#endif
    }
}
