using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    CollectShards,       
    SurviveDistance,     
    UseGadget,           
    ReachDistance,       
    CollectGhostChips,   
    TakeDamage,          
    ReachExfil,          
    UseEMPOnEnemies      
}

[CreateAssetMenu(fileName = "QuestData", menuName = "CipherGame/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questId;
    [TextArea(1,3)]
    public string description;    
    public QuestType questType;
    public int targetCount;       
    public int xpReward;
    public int shardBonus;
}