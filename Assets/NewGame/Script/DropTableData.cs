using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterInfo
{
    public string MonsterID;
    public string MonsterName;
    public float DropChance;
    public int MinDropCount;
    public int MaxDropCount;
    public string MainRarity;
}

[System.Serializable]
public class ItemTypeDropRate
{
    public string MonsterID;
    public float WeaponDropRate;
    public float ArmorDropRate;
    public float AccessoryDropRate;
}

[System.Serializable]
public class MonsterRarityDropRate
{
    public string MonsterID;
    public float CommonRate;
    public float RareRate;
    public float EpicRate;
    public float LegendaryRate;
    public float PrimordialRate;
}

[System.Serializable]
public class DropTableData
{
    public List<MonsterInfo> MonsterInfos = new List<MonsterInfo>();
    public List<ItemTypeDropRate> ItemTypeDropRates = new List<ItemTypeDropRate>();
    public List<MonsterRarityDropRate> MonsterRarityDropRates = new List<MonsterRarityDropRate>();
}

public static class DropTableDataExtensions
{
    public static MonsterInfo GetMonsterInfo(this DropTableData data, string monsterID)
    {
        return data.MonsterInfos.Find(m => m.MonsterID == monsterID);
    }
    
    public static ItemTypeDropRate GetItemTypeDropRate(this DropTableData data, string monsterID)
    {
        return data.ItemTypeDropRates.Find(r => r.MonsterID == monsterID);
    }
    
    public static MonsterRarityDropRate GetMonsterRarityDropRate(this DropTableData data, string monsterID)
    {
        return data.MonsterRarityDropRates.Find(r => r.MonsterID == monsterID);
    }
    
    public static float GetRarityDropRate(this MonsterRarityDropRate rarityData, string rarity)
    {
        switch (rarity.ToLower())
        {
            case "common": return rarityData.CommonRate;
            case "rare": return rarityData.RareRate;
            case "epic": return rarityData.EpicRate;
            case "legendary": return rarityData.LegendaryRate;
            case "primordial": return rarityData.PrimordialRate;
            default: return 0f;
        }
    }
} 