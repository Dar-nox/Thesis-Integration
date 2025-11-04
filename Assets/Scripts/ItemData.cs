using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Herbal Medicine/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemID;
    public string itemName;
    public Sprite itemSprite;
    
    [Header("Processing States")]
    public ProcessingStates processingStates = new ProcessingStates();
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description;
}

/// <summary>
/// Holds multiple processing state flags that an item can have simultaneously
/// </summary>
[System.Serializable]
public class ProcessingStates
{
    [Tooltip("Item has been peeled")]
    public int peeled = 0;
    
    [Tooltip("Item has been cut/chopped")]
    public int cut = 0;
    
    [Tooltip("Item has been crushed")]
    public int crushed = 0;
    
    [Tooltip("Item has been boiled")]
    public int boiled = 0;
    
    [Tooltip("Item has been strained")]
    public int strained = 0;
    
    [Tooltip("Item has been washed")]
    public int washed = 0;
    
    [Tooltip("Item has been mixed with other ingredients")]
    public int mixed = 0;
    
    [Tooltip("Item is in final processed state")]
    public int complete = 0;
    
    /// <summary>
    /// Create a copy of this ProcessingStates object
    /// </summary>
    public ProcessingStates Clone()
    {
        ProcessingStates clone = new ProcessingStates();
        clone.peeled = this.peeled;
        clone.cut = this.cut;
        clone.crushed = this.crushed;
        clone.boiled = this.boiled;
        clone.strained = this.strained;
        clone.washed = this.washed;
        clone.mixed = this.mixed;
        clone.complete = this.complete;
        return clone;
    }
    
    /// <summary>
    /// Check if all states match another ProcessingStates object
    /// </summary>
    public bool MatchesExactly(ProcessingStates other)
    {
        if (other == null) return false;
        
        return peeled == other.peeled &&
               cut == other.cut &&
               crushed == other.crushed &&
               boiled == other.boiled &&
               strained == other.strained &&
               washed == other.washed &&
               mixed == other.mixed &&
               complete == other.complete;
    }
    
    /// <summary>
    /// Check if this object has all required states from another ProcessingStates object
    /// </summary>
    public bool HasRequiredStates(ProcessingStates required)
    {
        if (required == null) return true;
        
        return (required.peeled == 0 || peeled >= required.peeled) &&
               (required.cut == 0 || cut >= required.cut) &&
               (required.crushed == 0 || crushed >= required.crushed) &&
               (required.boiled == 0 || boiled >= required.boiled) &&
               (required.strained == 0 || strained >= required.strained) &&
               (required.washed == 0 || washed >= required.washed) &&
               (required.mixed == 0 || mixed >= required.mixed) &&
               (required.complete == 0 || complete >= required.complete);
    }
    
    /// <summary>
    /// Apply state changes from another ProcessingStates object
    /// </summary>
    public void ApplyStateChanges(ProcessingStates changes)
    {
        if (changes == null) return;
        
        if (changes.peeled > 0) peeled = changes.peeled;
        if (changes.cut > 0) cut = changes.cut;
        if (changes.crushed > 0) crushed = changes.crushed;
        if (changes.boiled > 0) boiled = changes.boiled;
        if (changes.strained > 0) strained = changes.strained;
        if (changes.washed > 0) washed = changes.washed;
        if (changes.mixed > 0) mixed = changes.mixed;
        if (changes.complete > 0) complete = changes.complete;
    }
    
    /// <summary>
    /// Get a string representation of all active states
    /// </summary>
    public override string ToString()
    {
        List<string> activeStates = new List<string>();
        
        if (peeled > 0) activeStates.Add("Peeled");
        if (cut > 0) activeStates.Add("Cut");
        if (crushed > 0) activeStates.Add("Crushed");
        if (boiled > 0) activeStates.Add("Boiled");
        if (strained > 0) activeStates.Add("Strained");
        if (washed > 0) activeStates.Add("Washed");
        if (mixed > 0) activeStates.Add("Mixed");
        if (complete > 0) activeStates.Add("Complete");
        
        return activeStates.Count > 0 ? string.Join(", ", activeStates) : "Raw";
    }
}

public enum ProcessingState
{
    Raw,            // Unprocessed
    Peeled,         // Peeled
    Cut,            // Cut/Chopped
    Crushed,        // Crushed with mortar and pestle
    Boiled,         // Boiled in water
    Strained,       // Strained
    Washed,         // Washed in sink
    Mixed,          // Mixed with other ingredients
    Complete        // Final processed state ready to use
}
