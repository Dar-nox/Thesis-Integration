using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Makes a tool draggable and able to receive items/other tools
/// Similar to DraggableItem but for tools (no snap-back, no count system)
/// </summary>
public class DraggableTool : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Tool References")]
    public ToolState toolState;
    public Image toolImage;
    
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    
    [HideInInspector] public Transform parentAfterDrag;
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        // Get or add CanvasGroup for raycast control
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Get ToolState if not assigned
        if (toolState == null)
        {
            toolState = GetComponent<ToolState>();
        }
    }
    
    #region Drag Handlers (for dragging the tool itself)
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"Started dragging {toolState.toolType}");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        
        // Make tool semi-transparent while dragging
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"Ended dragging {toolState.toolType}");
        
        // Restore appearance
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Return to parent (tools don't snap back like items do)
        transform.SetParent(parentAfterDrag);
    }
    
    #endregion
    
    #region Drop Handler (for receiving items/tools)
    
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        
        GameObject dropped = eventData.pointerDrag;
        
        // Check if a DraggableItem was dropped
        DraggableItem droppedItem = dropped.GetComponent<DraggableItem>();
        if (droppedItem != null)
        {
            HandleItemDrop(droppedItem);
            return;
        }
        
        // Check if another DraggableTool was dropped
        DraggableTool droppedTool = dropped.GetComponent<DraggableTool>();
        if (droppedTool != null)
        {
            HandleToolDrop(droppedTool);
            return;
        }
    }
    
    #endregion
    
    #region Drop Handling Logic
    
    private void HandleItemDrop(DraggableItem item)
    {
        Debug.Log($"Item {item.itemData.itemName} dropped on {toolState.toolType}");
        
        // Add the item to this tool's state
        if (item.itemData != null)
        {
            toolState.AddItem(item.itemData, item.itemData.processingStates);
            
            // Parent the item to this tool visually
            item.parentAfterDrag = transform;
            
            // Query available actions
            ShowActionsForItem(item);
        }
    }
    
    private void HandleToolDrop(DraggableTool tool)
    {
        Debug.Log($"Tool {tool.toolState.toolType} dropped on {toolState.toolType}");
        
        // Handle tool-to-tool interactions (e.g., pot on stove)
        HandleToolInteraction(tool);
    }
    
    private void HandleToolInteraction(DraggableTool otherTool)
    {
        // Example: If pot is dropped on stove, attach it
        if (toolState.toolType == ToolType.Stove && otherTool.toolState.toolType == ToolType.Pot)
        {
            otherTool.toolState.AttachToTool(ToolType.Stove);
            otherTool.parentAfterDrag = transform;
            Debug.Log("Pot placed on stove - now heated!");
        }
        // Add more tool interactions as needed
    }
    
    #endregion
    
    #region Action UI
    
    private void ShowActionsForItem(DraggableItem item)
    {
        if (RecipeDatabase.Instance == null)
        {
            Debug.LogWarning("RecipeDatabase not found! Make sure it exists in the scene.");
            return;
        }
        
        // Get available recipes for this item + tool combination
        List<ProcessingRecipe> availableRecipes = RecipeDatabase.Instance.GetAvailableRecipes(
            item.itemData,
            item.itemData.processingStates,
            toolState
        );
        
        if (availableRecipes.Count > 0)
        {
            Debug.Log($"Found {availableRecipes.Count} available actions:");
            foreach (var recipe in availableRecipes)
            {
                Debug.Log($"  - {recipe.actionType}: {recipe.processDescription}");
            }
            
            // TODO: Show UI buttons for actions
            // For now, we'll create a temporary system
            ActionButtonUI actionUI = FindObjectOfType<ActionButtonUI>();
            if (actionUI != null)
            {
                actionUI.ShowActions(availableRecipes, item, this);
            }
            else
            {
                Debug.LogWarning("ActionButtonUI not found in scene. Actions are available but no UI to display them.");
            }
        }
        else
        {
            Debug.Log($"No actions available for {item.itemData.itemName} on {toolState.toolType}");
        }
    }
    
    #endregion
    
    /// <summary>
    /// Execute a processing recipe on an item
    /// </summary>
    public void ProcessItem(ProcessingRecipe recipe, DraggableItem item)
    {
        if (recipe == null || item == null) return;
        
        Debug.Log($"Processing: {recipe.recipeName}");
        
        // Check if we need to transform to a different item or just update states
        if (recipe.outputItem != null && recipe.outputItem != item.itemData)
        {
            // Transform the item to the output item
            item.TransformToItem(recipe.outputItem);
            
            // Apply state changes to the new item
            item.itemData.processingStates.ApplyStateChanges(recipe.outputStateChanges);
            
            // Update the tool state
            toolState.RemoveItem(0); // Remove old item
            toolState.AddItem(recipe.outputItem, recipe.outputItem.processingStates); // Add new item
            
            Debug.Log($"Item transformed to: {recipe.outputItem.itemName}");
        }
        else
        {
            // Just update the processing states of the current item
            item.itemData.processingStates.ApplyStateChanges(recipe.outputStateChanges);
            
            // Update the tool state
            if (toolState.containedItems.Count > 0)
            {
                toolState.containedItemStates[0].ApplyStateChanges(recipe.outputStateChanges);
            }
            
            Debug.Log($"Item states updated: {item.itemData.processingStates.ToString()}");
        }
    }
}
