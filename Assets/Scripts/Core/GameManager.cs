using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;





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
	
	
	private int shardsThisRun   = 0;
	private int gadgetsUsedThisRun = 0;

    void Start()
    {
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { Debug.LogError("GameManager: No Player found!"); return; }

        playerController   = player.GetComponent<PlayerController>();
        playerHealth       = player.GetComponent<PlayerHealth>();
        collectibleManager = CollectibleManager.Instance;

        
        if (playerHealth != null)
        {
            playerHealth.OnDeath         += HandleDeath;
            playerHealth.OnHealthChanged += UpdateHealthBar;
        }
        if (collectibleManager != null)
            collectibleManager.OnShardCollected += UpdateShardUI;

        
        gameOverPanel.SetActive(false);
        hudPanel.SetActive(true);
        Application.targetFrameRate = 60;
		
		
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

    

    void UpdateHealthBar(int current, int max)
	{
		if (healthBarFill == null) return;

		
		float ratio = max > 0 ? (float)current / max : 0f;
		healthBarFill.fillAmount = ratio;

		
		healthBarFill.color = Color.Lerp(Color.red, new Color(0f, 0.86f, 0.31f), ratio);

		
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
		
		
		if (PauseManager.Instance != null)
			PauseManager.Instance.enabled = false;

		playerController.StopRunning();

		float dist = playerController != null ? playerController.DistanceRun : 0f;
		float best = PlayerPrefs.GetFloat("BestDistance", 0f);
		if (dist > best) { best = dist; PlayerPrefs.SetFloat("BestDistance", best); }

		int xpEarned    = ProgressionManager.Instance?.GetXPEarnedThisRun() ?? 0;

		ProgressionManager.Instance?.CommitXP();

		
		
	
		int cipherLevel = PlayerPrefs.GetInt("CipherLevel", 1);
		PlayerPrefs.Save();

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