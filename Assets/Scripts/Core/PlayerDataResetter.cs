using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility to reset all persistent player data.
/// Call ResetAllData() from a debug menu button or directly from the Inspector
/// using the context menu (right-click the component header).
/// </summary>
public class PlayerDataResetter : MonoBehaviour
{
    [ContextMenu("RESET ALL PLAYER DATA NOW")]
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[PlayerDataResetter] All PlayerPrefs deleted. Level=0, Shards=0, XP=0.");

        // Immediately set level back to 1 (default)
        PlayerPrefs.SetInt("CipherLevel", 1);
        PlayerPrefs.Save();

        Debug.Log("[PlayerDataResetter] Reset complete. Restart the scene to apply.");
    }

    [ContextMenu("Print Current Save Data")]
    public void PrintSaveData()
    {
        Debug.Log(
            $"[SaveData] CipherLevel={PlayerPrefs.GetInt("CipherLevel", 1)}" +
            $"  TotalShards={PlayerPrefs.GetInt("TotalShards", 0)}" +
            $"  CipherXP={PlayerPrefs.GetInt("CipherXP", 0)}" +
            $"  BestDistance={PlayerPrefs.GetFloat("BestDistance", 0f):F0}m"
        );
    }
}