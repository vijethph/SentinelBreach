using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data container for one skill tree node.
/// Create instances via right-click > Create > CipherGame > Skill Data.
/// </summary>
[CreateAssetMenu(fileName = "SkillData", menuName = "CipherGame/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Identity")]
    public string nodeId;           // unique key used for PlayerPrefs — do NOT change after creation
    public string displayName;
    [TextArea(2,4)]
    public string description;

    [Header("Upgrade Costs (3 levels)")]
    public int[] costs = { 100, 250, 500 };

    [Header("Level Labels shown in UI")]
    public string[] levelDescriptions =
    {
        "Level 1 effect description",
        "Level 2 effect description",
        "Level 3 effect description"
    };

    public int MaxLevel => costs.Length;
}