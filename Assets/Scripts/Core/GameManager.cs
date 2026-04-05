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

    [Header("UI — Game Over")]
    public GameObject      gameOverPanel;
    public TextMeshProUGUI goDistanceText;
    public TextMeshProUGUI goBestText;
    public TextMeshProUGUI goScoreText;

    private PlayerController   playerController;
    private PlayerHealth       playerHealth;
    private CollectibleManager collectibleManager;

    private float score    = 0f;
    private bool  gameOver = false;

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
        float ratio = (float)current / max;
        healthBarFill.fillAmount = ratio;
        healthBarFill.color = Color.Lerp(Color.red, new Color(0f, 0.86f, 0.31f), ratio);
    }

    void UpdateShardUI(int total)
	{
		if (shardText != null) shardText.text = $"SHARDS: {total}";
	}

    void HandleDeath()
	{
		if (gameOver) return;
		gameOver = true;

		playerController.StopRunning();

		// ── Commit XP and check level-up ─────────────────────────
		ProgressionManager.Instance?.CommitXP();

		float dist = playerController.DistanceRun;
		float best = PlayerPrefs.GetFloat("BestDistance", 0f);
		if (dist > best) { best = dist; PlayerPrefs.SetFloat("BestDistance", best); }

		if (collectibleManager != null && SkillTree.Instance != null)
			SkillTree.Instance.AddShards(collectibleManager.TotalShards);

		PlayerPrefs.Save();

		// Show current CIPHER level on Game Over screen
		int cipherLevel = PlayerPrefs.GetInt("CipherLevel", 1);
		int xpEarned    = ProgressionManager.Instance != null
			? ProgressionManager.Instance.GetXPEarnedThisRun() : 0;

		goDistanceText.text = $"Distance: {dist:F0}m";
		goScoreText.text    = $"Score: {(int)score:N0}";
		goBestText.text     = $"Best: {best:F0}m  |  CIPHER LVL {cipherLevel}  |  +{xpEarned} XP";

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