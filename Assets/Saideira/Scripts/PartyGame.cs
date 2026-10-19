using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class PartyLevel
{
    public string name;
    public int target, guestCount, spillCount;
    public float duration, guestSpeedMultiplier;
    public PartyLevel(string title, int bottles, float seconds, int guests, float speed, int spills)
    {
        name = title; target = bottles; duration = seconds;
        guestCount = guests; guestSpeedMultiplier = speed; spillCount = spills;
    }
}

public class PartyGame : MonoBehaviour
{
    public static PartyGame Instance { get; private set; }
    public enum Phase { Menu, Playing, Paused, Result }
    public Phase State { get; private set; }
    public bool IsPlaying => State == Phase.Playing;
    public const int Capacity = 3;
    public int LevelIndex { get; private set; }
    public PartyLevel CurrentLevel => levels[LevelIndex];
    public int Collected { get; private set; }
    public int Carrying { get; private set; }
    public int Delivered { get; private set; }
    public int Lives { get; private set; }
    public float TimeLeft { get; private set; }
    public bool WonRound { get; private set; }
    [Header("Fases")]
    public PartyLevel[] levels = {
        new PartyLevel("ESQUENTA", 6, 30, 2, 1f, 0),
        new PartyLevel("CASA CHEIA", 9, 35, 3, 1.16f, 2),
        new PartyLevel("SAIDEIRA", 12, 40, 4, 1.30f, 4)
    };
    public float roundDuration = 30;
    public PartyPlayer player;
    public PartyGuest[] guests;
    public DrinkCollectible[] drinks;
    public PartyCooler cooler;
    public PartySpill[] spills;
    [Header("Telas")]
    public GameObject menuPanel, howToPanel, hudPanel, pausePanel, resultPanel;
    public Button playButton, howToBackButton, resumeButton, retryButton;
    public Text scoreText, timeText, lifeText, resultTitle, resultMessage, resultStats, toastText;
    public Text phaseText, carryText, objectiveText, retryLabel;
    public Text[] soundLabels;
    public Image timerFill, damageFlash;
    [Header("Áudio")]
    public AudioSource musicSource, effectsSource;
    public AudioClip collectSound, damageSound, victorySound, defeatSound, buttonSound;
    bool muted, musicStarted;
    float invulnerableUntil, toastUntil, flashUntil, completedTime;

    void Awake() { Instance = this; }
    void Start()
    {
        Application.targetFrameRate = 60;
        muted = PlayerPrefs.GetInt("soundMuted", 0) == 1;
        ApplySound(); ShowMenu();
    }
    void Update()
    {
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;
        bool back = (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            || (gamepad != null && gamepad.startButton.wasPressedThisFrame);
        if (keyboard != null && keyboard.mKey.wasPressedThisFrame) ToggleSound();
        if (back)
        {
            if (IsPlaying) PauseGame();
            else if (State == Phase.Paused) ResumeGame();
            else if (howToPanel.activeSelf) CloseHowTo();
        }
        if (IsPlaying)
        {
            TimeLeft = Mathf.Max(0, TimeLeft - Time.deltaTime);
            UpdateHud();
            if (TimeLeft <= 0) Finish(false, "O tempo acabou. Leve todas as garrafas ao isopor para passar.");
        }
        toastText.gameObject.SetActive(IsPlaying && Time.unscaledTime < toastUntil);
        Color flash = damageFlash.color;
        flash.a = Mathf.Clamp01((flashUntil - Time.unscaledTime) / .3f) * .25f;
        damageFlash.color = flash;
    }
    public void StartGame() { LevelIndex = 0; completedTime = 0; RestartLevel(); }
    public void RestartLevel()
    {
        Time.timeScale = 1;
        Collected = Carrying = Delivered = 0;
        Lives = 3; WonRound = false;
        roundDuration = CurrentLevel.duration; TimeLeft = roundDuration;
        invulnerableUntil = Time.time + 1f; toastUntil = flashUntil = 0;
        State = Phase.Playing;
        player.ResetPlayer();
        for (int i = 0; i < guests.Length; i++)
        {
            guests[i].gameObject.SetActive(i < CurrentLevel.guestCount);
            if (guests[i].gameObject.activeSelf) guests[i].ResetGuest(CurrentLevel.guestSpeedMultiplier);
        }
        for (int i = 0; i < drinks.Length; i++)
        {
            drinks[i].ResetDrink();
            drinks[i].gameObject.SetActive(i < CurrentLevel.target);
        }
        for (int i = 0; i < spills.Length; i++) spills[i].gameObject.SetActive(i < CurrentLevel.spillCount);
        Panels(false, false, true, false, false);
        EventSystem.current.SetSelectedGameObject(null);
        StartMusic(); PlaySound(buttonSound);
        Toast("FASE " + (LevelIndex + 1) + " — " + CurrentLevel.target + " GARRAFAS EM " + roundDuration + "s");
        UpdateHud();
    }
    public void AdvanceOrRetry()
    {
        if (State != Phase.Result) return;
        if (WonRound && LevelIndex < levels.Length - 1)
        {
            completedTime += roundDuration - TimeLeft;
            LevelIndex++; RestartLevel();
        }
        else if (WonRound) StartGame();
        else RestartLevel();
    }
    public void ShowMenu()
    {
        Time.timeScale = 1; State = Phase.Menu; StopActors();
        Panels(true, false, false, false, false); Select(playButton);
        if (musicStarted) musicSource.volume = .22f;
    }
    public void ShowHowTo()
    {
        Panels(false, true, false, false, false); Select(howToBackButton);
        StartMusic(); PlaySound(buttonSound);
    }
    public void CloseHowTo() { PlaySound(buttonSound); ShowMenu(); }
    public void PauseGame()
    {
        if (!IsPlaying) return;
        State = Phase.Paused; StopActors(); Time.timeScale = 0;
        pausePanel.SetActive(true); Select(resumeButton); musicSource.volume = .16f;
    }
    public void ResumeGame()
    {
        if (State != Phase.Paused) return;
        Time.timeScale = 1; State = Phase.Playing; pausePanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null); musicSource.volume = .36f;
    }
    public void Collect(DrinkCollectible drink)
    {
        if (!IsPlaying || !drink.gameObject.activeSelf || Carrying >= Capacity) return;
        drink.gameObject.SetActive(false); Collected++; Carrying++;
        PlaySound(collectSound);
        if (Carrying == Capacity) Toast("MÃOS CHEIAS: LEVE AS 3 GARRAFAS AO ISOPOR");
        UpdateHud();
    }
    public bool TryDeposit()
    {
        if (!IsPlaying || Carrying == 0 || cooler == null
            || Vector2.Distance(player.transform.position, cooler.transform.position) > cooler.radius) return false;
        int amount = Carrying; Delivered += amount; Carrying = 0;
        PlaySound(buttonSound);
        Toast("+" + amount + " NO ISOPOR  •  FALTAM " + (CurrentLevel.target - Delivered));
        UpdateHud();
        if (Delivered >= CurrentLevel.target) Finish(true, "");
        return true;
    }
    public void TakeDamage()
    {
        if (!IsPlaying || Time.time < invulnerableUntil) return;
        Lives--; invulnerableUntil = Time.time + 1.6f; flashUntil = Time.unscaledTime + .3f;
        player.BlinkFor(1.6f); PlaySound(damageSound);
        Toast("ESBARRÃO!  −1 VIDA  •  AFASTE-SE DOS CONVIDADOS");
        UpdateHud();
        if (Lives <= 0) Finish(false, "Três esbarrões encerram a fase. Dê a volta nos convidados.");
    }
    void Finish(bool won, string message)
    {
        WonRound = won; State = Phase.Result; StopActors();
        Panels(false, false, false, false, true);
        bool complete = won && LevelIndex == levels.Length - 1;
        resultTitle.text = complete ? "CHURRASCO SALVO!" : won ? "FASE " + (LevelIndex + 1) + " CONCLUÍDA" : "TENTE NOVAMENTE";
        resultTitle.color = won ? new Color32(131, 229, 181, 255) : new Color32(255, 139, 124, 255);
        if (complete) message = "Você completou as três fases e garantiu as 27 garrafas da saideira.";
        else if (won)
        {
            var next = levels[LevelIndex + 1];
            message = "A seguir: " + next.target + " garrafas em " + next.duration + "s.\nMais convidados e poças que deixam você lento.";
        }
        resultMessage.text = message;
        float elapsed = roundDuration - TimeLeft;
        resultStats.text = "ENTREGUES  " + Delivered + " / " + CurrentLevel.target + "     VIDAS  " + Lives + " / 3"
            + "\nTEMPO DA FASE  " + elapsed.ToString("0.0") + " s"
            + (complete ? "     TOTAL  " + (completedTime + elapsed).ToString("0.0") + " s" : "");
        retryLabel.text = complete ? "JOGAR DE NOVO" : won ? "PRÓXIMA FASE" : "REPETIR FASE " + (LevelIndex + 1);
        PlaySound(won ? victorySound : defeatSound); musicSource.volume = .16f; Select(retryButton);
    }
    void UpdateHud()
    {
        phaseText.text = "FASE " + (LevelIndex + 1) + " / " + levels.Length;
        scoreText.text = "NO ISOPOR  " + Delivered + " / " + CurrentLevel.target;
        carryText.text = "MÃOS  " + Carrying + " / " + Capacity;
        timeText.text = Mathf.CeilToInt(TimeLeft).ToString("00") + "s";
        lifeText.text = "VIDAS  " + Lives + " / 3";
        float time = TimeLeft / roundDuration;
        timerFill.rectTransform.localScale = new Vector3(time, 1, 1);
        timerFill.color = TimeLeft <= 10 ? new Color32(255, 139, 124, 255) : new Color32(131, 229, 181, 255);
        carryText.color = Carrying == Capacity ? new Color32(255, 202, 115, 255) : Color.white;
        if (player.SpeedMultiplier < 1)
            objectiveText.text = "POÇA: você fica lento. Saia pela borda e continue a coleta.";
        else if (Carrying == Capacity || Collected == CurrentLevel.target)
            objectiveText.text = "LEVE AS " + Carrying + " GARRAFAS AO ISOPOR DESTACADO. A entrega é automática.";
        else objectiveText.text = "PEGUE ATÉ 3 GARRAFAS E LEVE AO ISOPOR. Desvie dos convidados.";
    }
    void Panels(bool menu, bool howTo, bool hud, bool pause, bool result)
    {
        menuPanel.SetActive(menu); howToPanel.SetActive(howTo); hudPanel.SetActive(hud);
        pausePanel.SetActive(pause); resultPanel.SetActive(result);
    }
    void StopActors()
    {
        if (player != null) player.Stop();
        foreach (var guest in guests) guest.Stop();
    }
    void Select(Button button)
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (button != null) EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
    void Toast(string message) { toastText.text = message; toastUntil = Time.unscaledTime + 1.7f; }
    void StartMusic()
    {
        musicStarted = true;
        musicSource.volume = IsPlaying ? .36f : State == Phase.Menu ? .22f : .16f;
        if (!musicSource.isPlaying) musicSource.Play();
    }
    public void PlaySound(AudioClip clip) { if (!muted && clip != null) effectsSource.PlayOneShot(clip); }
    public void ToggleSound()
    {
        muted = !muted; PlayerPrefs.SetInt("soundMuted", muted ? 1 : 0); ApplySound(); StartMusic();
    }
    void ApplySound()
    {
        musicSource.mute = muted; effectsSource.mute = muted;
        foreach (var label in soundLabels) label.text = muted ? "SOM OFF" : "SOM ON";
    }
    void OnApplicationFocus(bool focused) { if (!focused && IsPlaying) PauseGame(); }
    void OnApplicationPause(bool paused) { if (paused && IsPlaying) PauseGame(); }
    void OnDestroy() { Time.timeScale = 1; if (Instance == this) Instance = null; }
}

