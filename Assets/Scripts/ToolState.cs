using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks the current state of a tool (water, heat, attached items, etc.)
/// </summary>
public class ToolState : MonoBehaviour
{
    [Header("Tool Configuration")]
    public ToolType toolType;
    
    [Header("Current State")]
    public bool hasWater = false;
    public bool isHeated = false;
    
    [Header("Relationships")]
    [Tooltip("Tools that this tool is currently attached to (e.g., pot on stove)")]
    public List<ToolType> attachedToTools = new List<ToolType>();
    
    [Header("Contents")]
    [Tooltip("Items currently being processed in this tool")]
    public List<ItemData> containedItems = new List<ItemData>();
    
    [Tooltip("Current processing states of contained items")]
    public List<ProcessingStates> containedItemStates = new List<ProcessingStates>();
    
    /// <summary>
    /// Check if this tool is attached to a specific tool type
    /// </summary>
    public bool IsAttachedToTool(ToolType toolType)
    {
        return attachedToTools.Contains(toolType);
    }
    
    /// <summary>
    /// Check if this tool is empty (no items)
    /// </summary>
    public bool IsEmpty()
    {
        return containedItems.Count == 0;
    }
    
    /// <summary>
    /// Add an item to this tool
    /// </summary>
    public void AddItem(ItemData item, ProcessingStates states)
    {
        containedItems.Add(item);
        containedItemStates.Add(states.Clone());
    }
    
    /// <summary>
    /// Remove an item from this tool
    /// </summary>
    public void RemoveItem(int index)
    {
        if (index >= 0 && index < containedItems.Count)
        {
            containedItems.RemoveAt(index);
            containedItemStates.RemoveAt(index);
        }
    }
    
    /// <summary>
    /// Clear all items from this tool
    /// </summary>
    public void ClearItems()
    {
        containedItems.Clear();
        containedItemStates.Clear();
    }
    
    /// <summary>
    /// Attach this tool to another tool (e.g., pot placed on stove)
    /// </summary>
    public void AttachToTool(ToolType toolType)
    {
        if (!attachedToTools.Contains(toolType))
        {
            attachedToTools.Add(toolType);
            
            // Special case: if attached to stove, mark as heated
            if (toolType == ToolType.Stove)
            {
                isHeated = true;
            }
        }
    }
    
    /// <summary>
    /// Detach this tool from another tool
    /// </summary>
    public void DetachFromTool(ToolType toolType)
    {
        if (attachedToTools.Contains(toolType))
        {
            attachedToTools.Remove(toolType);
            
            // Special case: if detached from stove, no longer heated
            if (toolType == ToolType.Stove)
            {
                isHeated = false;
            }
        }
    }
    
    /// <summary>
    /// Add water to this tool (e.g., from sink)
    /// </summary>
    public void AddWater()
    {
        hasWater = true;
        Debug.Log($"{toolType} now has water");
    }
    
    /// <summary>
    /// Remove water from this tool
    /// </summary>
    public void RemoveWater()
    {
        hasWater = false;
        Debug.Log($"{toolType} water removed");
    }
    
    /// <summary>
    /// Get the first item and its state (for single-item tools)
    /// </summary>
    public bool TryGetFirstItem(out ItemData item, out ProcessingStates states)
    {
        if (containedItems.Count > 0)
        {
            item = containedItems[0];
            states = containedItemStates[0];
            return true;
        }
        
        item = null;
        states = new ProcessingStates();
        return false;
    }
}
