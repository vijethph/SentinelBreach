using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    CollectShards,       // collect N data shards
    SurviveDistance,     // survive N metres without dying
    UseGadget,           // use a gadget N times
    ReachDistance,       // reach a total distance of Nm in one run
    CollectGhostChips,   // collect N ghost chips
    TakeDamage,          // do NOT take damage for N metres (track negatively)
    ReachExfil,          // reach an exfil checkpoint
    UseEMPOnEnemies      // use EMP while 2+ enemies are active
}

[CreateAssetMenu(fileName = "QuestData", menuName = "CipherGame/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questId;
    [TextArea(1,3)]
    public string description;    // shown in HUD card
    public QuestType questType;
    public int targetCount;       // how many to collect/survive/etc.
    public int xpReward;
    public int shardBonus;
}