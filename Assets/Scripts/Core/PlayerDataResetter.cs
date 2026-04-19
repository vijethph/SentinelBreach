using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class PlayerDataResetter : MonoBehaviour
{
    [ContextMenu("RESET ALL PLAYER DATA NOW")]
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[PlayerDataResetter] All PlayerPrefs deleted. Level=0, Shards=0, XP=0.");

        
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