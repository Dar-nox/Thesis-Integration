using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Central database for all processing recipes. 
/// Singleton that helps query available actions based on current game state.
/// </summary>
public class RecipeDatabase : MonoBehaviour
{
    public static RecipeDatabase Instance { get; private set; }
    
    [Header("Recipe Database")]
    [Tooltip("All available processing recipes in the game")]
    public List<ProcessingRecipe> allRecipes = new List<ProcessingRecipe>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Get all available actions for a specific item on a specific tool
    /// </summary>
    public List<ProcessingRecipe> GetAvailableRecipes(ItemData item, ProcessingStates itemStates, ToolState tool)
    {
        List<ProcessingRecipe> availableRecipes = new List<ProcessingRecipe>();
        
        foreach (var recipe in allRecipes)
        {
            // Check if the recipe matches the input item, state, and tool
            if (recipe.inputItem == item && 
                itemStates.HasRequiredStates(recipe.requiredInputStates) &&
                recipe.requiredTool == tool.toolType &&
                recipe.ArePrerequisitesMet(tool))
            {
                availableRecipes.Add(recipe);
            }
        }
        
        return availableRecipes;
    }
    
    /// <summary>
    /// Get available actions for a tool (like adding water to pot)
    /// </summary>
    public List<ProcessingRecipe> GetToolOnlyActions(ToolState tool)
    {
        List<ProcessingRecipe> availableRecipes = new List<ProcessingRecipe>();
        
        foreach (var recipe in allRecipes)
        {
            // Check for tool-only actions (no input item required)
            if (recipe.inputItem == null &&
                recipe.requiredTool == tool.toolType &&
                recipe.ArePrerequisitesMet(tool))
            {
                availableRecipes.Add(recipe);
            }
        }
        
        return availableRecipes;
    }
    
    /// <summary>
    /// Find a specific recipe by action type and tool
    /// </summary>
    public ProcessingRecipe FindRecipe(ActionType action, ToolType tool, ItemData item = null, ProcessingStates itemStates = null)
    {
        return allRecipes.FirstOrDefault(r => 
            r.actionType == action && 
            r.requiredTool == tool &&
            (item == null || r.inputItem == item) &&
            (itemStates == null || itemStates.HasRequiredStates(r.requiredInputStates))
        );
    }
    
    /// <summary>
    /// Check if a specific action is available
    /// </summary>
    public bool IsActionAvailable(ItemData item, ProcessingStates itemStates, ToolState tool, ActionType action)
    {
        var availableRecipes = GetAvailableRecipes(item, itemStates, tool);
        return availableRecipes.Any(r => r.actionType == action);
    }
}
