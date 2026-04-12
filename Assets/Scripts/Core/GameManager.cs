using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Central game controller. Subscribes to PlayerHealth events.
/// Updates HUD and shows Game Over screen on death.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("UI — HUD")]
    public GameObject      hudPanel;
    public Image           healthBarFill;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI shardText;
	public TextMeshProUGUI hpPercentText; 

    [Header("UI — Game Over")]
    public GameObject      gameOverPanel;
    public TextMeshProUGUI goDistanceText;
    public TextMeshProUGUI goBestText;
    public TextMeshProUGUI goScoreText;
	public TextMeshProUGUI goShardsText;     
	public TextMeshProUGUI goGadgetsText;    
	public TextMeshProUGUI goCipherLevelText; 

    private PlayerController   playerController;
    private PlayerHealth       playerHealth;
    private CollectibleManager collectibleManager;

    private float score    = 0f;
    private bool  gameOver = false;
	
	// ─── Run Tracking ─────────────────────────────────────────────
	private int shardsThisRun   = 0;
	private int gadgetsUsedThisRun = 0;

    void Start()
    {
        // Find player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { Debug.LogError("GameManager: No Player found!"); return; }

        playerController   = player.GetComponent<PlayerController>();
        playerHealth       = player.GetComponent<PlayerHealth>();
        collectibleManager = CollectibleManager.Instance;

        // Subscribe to events
        if (playerHealth != null)
        {
            playerHealth.OnDeath         += HandleDeath;
            playerHealth.OnHealthChanged += UpdateHealthBar;
        }
        if (collectibleManager != null)
            collectibleManager.OnShardCollected += UpdateShardUI;

        // Initial state
        gameOverPanel.SetActive(false);
        hudPanel.SetActive(true);
        Application.targetFrameRate = 60;
		
		// After existing subscriptions, add:
		if (collectibleManager != null)
			collectibleManager.OnShardCollected += _ => shardsThisRun = collectibleManager.TotalShards;
    }

    void Update()
    {
        if (gameOver || playerController == null || !playerController.IsRunning) return;

        score += playerController.runSpeed * Time.deltaTime * 10f;
        distanceText.text = $"{playerController.DistanceRun:F0}m";
        scoreText.text    = $"{(int)score:N0}";
    }

    // ─── Event Handlers ──────────────────────────────────────────

    void UpdateHealthBar(int current, int max)
	{
		if (healthBarFill == null) return;

		// Drive the fill amount — this makes the bar shrink
		float ratio = max > 0 ? (float)current / max : 0f;
		healthBarFill.fillAmount = ratio;

		// Colour shift: green at full HP → red at zero HP
		healthBarFill.color = Color.Lerp(Color.red, new Color(0f, 0.86f, 0.31f), ratio);

		// Update percentage text if assigned
		if (hpPercentText != null)
			hpPercentText.text = $"{Mathf.RoundToInt(ratio * 100f)}%";
	}

    void UpdateShardUI(int total)
	{
		if (shardText != null) shardText.text = $"SHARDS: {total}";
	}

    void HandleDeath()
	{
		if (gameOver) return;
		gameOver = true;
		
		// Disable pause input when game is over
		if (PauseManager.Instance != null)
			PauseManager.Instance.enabled = false;

		playerController.StopRunning();

		float dist = playerController != null ? playerController.DistanceRun : 0f;
		float best = PlayerPrefs.GetFloat("BestDistance", 0f);
		if (dist > best) { best = dist; PlayerPrefs.SetFloat("BestDistance", best); }

		ProgressionManager.Instance?.CommitXP();

		// if (collectibleManager != null && SkillTree.Instance != null)
		//	SkillTree.Instance.AddShards(collectibleManager.TotalShards);

		PlayerPrefs.Save();

		int cipherLevel = PlayerPrefs.GetInt("CipherLevel", 1);
		int xpEarned    = ProgressionManager.Instance?.GetXPEarnedThisRun() ?? 0;

		goDistanceText.text = $"Distance: {dist:F0}m";
		goBestText.text     = $"Best: {best:F0}m";
		goScoreText.text    = $"Score: {(int)score:N0}";
		goShardsText.text   = $"Shards: {collectibleManager?.TotalShards ?? 0}";
		goGadgetsText.text  = $"Gadgets used: {GadgetManager.Instance?.TotalGadgetsUsed ?? 0}";
		goCipherLevelText.text = $"CIPHER LVL {cipherLevel}  |  +{xpEarned} XP";

		gameOverPanel.SetActive(true);
		hudPanel.SetActive(false);
		AudioManager.Instance?.PlayDeath();
	}

    // ─── Button Callbacks ─────────────────────────────────────────

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
	{
		Time.timeScale = 1f;
		UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
	}
}