using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Central game controller. Manages game state, HUD updates, and Game Over flow.
/// Subscribes to PlayerHealth events to react to damage and death.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ─── Singleton ───────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    // ─── Inspector References ────────────────────────────────────

    [Header("UI Panels")]
    public GameObject hudPanel;
    public GameObject gameOverPanel;

    [Header("HUD Elements")]
    public Image healthBarFill;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI shardText;

    [Header("Game Over Elements")]
    public TextMeshProUGUI goDistanceText;
    public TextMeshProUGUI goScoreText;
    public TextMeshProUGUI goBestText;

    // ─── Private References ──────────────────────────────────────

    private PlayerController playerController;
    private PlayerHealth playerHealth;
    private CollectibleManager collectibleManager;

    private float score = 0f;
    private bool gameOver = false;

    // ─────────────────────────────────────────────────────────────

    void Start()
    {
        // Find player components
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) { Debug.LogError("GameManager: No Player found!"); return; }

        playerController = playerObj.GetComponent<PlayerController>();
        playerHealth = playerObj.GetComponent<PlayerHealth>();

        // Subscribe to events
        playerHealth.OnDeath          += HandleDeath;
        playerHealth.OnHealthChanged  += UpdateHealthBar;

        collectibleManager = CollectibleManager.Instance;
        if (collectibleManager != null)
            collectibleManager.OnShardCollected += UpdateShardUI;

        // Initial UI state
        gameOverPanel.SetActive(false);
        hudPanel.SetActive(true);
        UpdateHealthBar(playerHealth.CurrentHP, playerHealth.maxHP);
    }

    void Update()
    {
        if (gameOver || playerController == null || !playerController.IsRunning) return;

        // Score increases over time (distance × 10)
        score += playerController.runSpeed * Time.deltaTime * 10f;

        // Update HUD
        distanceText.text = $"{playerController.DistanceRun:F0}m";
        scoreText.text    = $"{(int)score:N0}";
    }

    // ─── Event Handlers ──────────────────────────────────────────

    void UpdateHealthBar(int current, int max)
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)current / max;

        // Optional: colour shifts red when HP is low
        if (healthBarFill != null)
        {
            float ratio = (float)current / max;
            healthBarFill.color = Color.Lerp(Color.red, Color.green, ratio);
        }
    }

    void UpdateShardUI(int totalShards)
    {
        if (shardText != null)
            shardText.text = $"◆ {totalShards}";
    }

    void HandleDeath()
    {
        if (gameOver) return;
        gameOver = true;

        playerController.StopRunning();

        float dist = playerController.DistanceRun;
        float best = PlayerPrefs.GetFloat("BestDistance", 0f);
        if (dist > best)
        {
            best = dist;
            PlayerPrefs.SetFloat("BestDistance", best);
            PlayerPrefs.Save();
        }

        // Populate Game Over UI
        goDistanceText.text = $"Distance: {dist:F0}m";
        goScoreText.text    = $"Score: {(int)score:N0}";
        goBestText.text     = $"Best: {best:F0}m";

        // Show Game Over, hide HUD
        gameOverPanel.SetActive(true);
        hudPanel.SetActive(false);

        Debug.Log("[GameManager] Game Over screen shown.");
    }

    // ─── Button Callbacks ─────────────────────────────────────────

    /// <summary>
    /// Called by the Retry button's OnClick event.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Called by the Main Menu button's OnClick event.
    /// (Scene named "MainMenu" must exist in Build Settings — add it in Week 3.)
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        // For now, just restart the game scene (main menu built in Week 3)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}