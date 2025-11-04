using System;
using UnityEngine;

[Serializable]
public class Illness
{
    public string name;
    [Tooltip("Base weight used for weighted random selection. Higher = more likely.")]
    public float weight = 1f;
    public Rarity rarity = Rarity.Common;

    public Illness() { }
    public Illness(string name, float weight, Rarity rarity)
    {
        this.name = name;
        this.weight = weight;
        this.rarity = rarity;
    }
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare
}
