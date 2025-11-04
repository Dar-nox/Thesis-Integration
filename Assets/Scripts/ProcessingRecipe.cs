using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Herbal Medicine/Processing Recipe")]
public class ProcessingRecipe : ScriptableObject
{
    [Header("Recipe Info")]
    public string recipeName;
    
    [Header("Input")]
    public ItemData inputItem;
    public ProcessingStates requiredInputStates;
    
    [Header("Process")]
    public ToolType requiredTool;
    public ActionType actionType;
    
    [Header("Prerequisites")]
    public List<ToolPrerequisite> prerequisites = new List<ToolPrerequisite>();
    
    [Header("Output")]
    public ItemData outputItem;
    public ProcessingStates outputStateChanges;
    
    [Header("Settings")]
    [Tooltip("Time in seconds for the process to complete (0 for instant)")]
    public float processingTime = 0f;
    
    [TextArea(2, 4)]
    public string processDescription;
    
    /// <summary>
    /// Check if all prerequisites are met for this recipe
    /// </summary>
    public bool ArePrerequisitesMet(ToolState toolState)
    {
        if (toolState.toolType != requiredTool)
            return false;
            
        foreach (var prereq in prerequisites)
        {
            if (!prereq.IsMet(toolState))
                return false;
        }
        
        return true;
    }
}

[System.Serializable]
public class ToolPrerequisite
{
    public PrerequisiteType type;
    public ToolType requiredAttachedTool; // For prerequisites like "must be on stove"
    
    public bool IsMet(ToolState toolState)
    {
        switch (type)
        {
            case PrerequisiteType.MustHaveWater:
                return toolState.hasWater;
                
            case PrerequisiteType.MustBeHeated:
                return toolState.isHeated;
                
            case PrerequisiteType.MustBeOnTool:
                return toolState.IsAttachedToTool(requiredAttachedTool);
                
            case PrerequisiteType.MustBeEmpty:
                return toolState.IsEmpty();
                
            default:
                return true;
        }
    }
}

public enum ToolType
{
    ChoppingBoard,
    Pot,
    Stove,
    Sink,
    Strainer,
    MortarAndPestle,
    TrashCan,
    None
}

public enum ActionType
{
    Peel,
    Cut,
    Crush,
    Boil,
    Strain,
    Wash,
    AddWater,
    Discard,
    Cancel,
    Complete
}

public enum PrerequisiteType
{
    None,
    MustHaveWater,      // Tool must contain water
    MustBeHeated,       // Tool must be heated (on stove)
    MustBeOnTool,       // Tool must be placed on another specific tool
    MustBeEmpty         // Tool must not contain any items
}
