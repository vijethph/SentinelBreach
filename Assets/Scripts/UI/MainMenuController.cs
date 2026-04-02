using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelBadgeText;
    public GameObject skillTreePanel;   // assign in Inspector after building it below

    void Start()
    {
        // Show persisted CIPHER level
        int level = PlayerPrefs.GetInt("CipherLevel", 1);
        if (levelBadgeText != null)
            levelBadgeText.text = $"LVL {level}";

        if (skillTreePanel != null)
            skillTreePanel.SetActive(false);
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
