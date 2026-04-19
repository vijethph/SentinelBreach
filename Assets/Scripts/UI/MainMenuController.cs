using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelBadgeText;
    public GameObject skillTreePanel;   
	
	[Header("XP Bar")]
	public Image           xpBarFill;
	public TextMeshProUGUI xpProgressText;

    void Start()
    {
        int level   = PlayerPrefs.GetInt("CipherLevel", 1);
		int totalXP = PlayerPrefs.GetInt("CipherXP", 0);

		if (levelBadgeText) levelBadgeText.text = $"LVL {level}";

		
		int[] thresholds = { 0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700 };
		if (level <= thresholds.Length)
		{
			int prevXP = thresholds[level - 1];
			int nextXP = level < thresholds.Length ? thresholds[level] : thresholds[thresholds.Length - 1];
			float fill = nextXP > prevXP ? (float)(totalXP - prevXP) / (nextXP - prevXP) : 1f;
			if (xpBarFill)     xpBarFill.fillAmount = Mathf.Clamp01(fill);
			if (xpProgressText) xpProgressText.text = $"{totalXP - prevXP} / {nextXP - prevXP} XP";
		}

		if (skillTreePanel) skillTreePanel.SetActive(false);
    }

    public void OnPlayPressed()
    {
        SceneManager.LoadScene("Game");
    }

    public void OnUpgradesPressed()
    {
        if (skillTreePanel != null)
            skillTreePanel.SetActive(true);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }

    public void OnCloseUpgradesPressed()
    {
        if (skillTreePanel != null)
            skillTreePanel.SetActive(false);
    }
}
