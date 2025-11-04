using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI system that displays action buttons when items are dropped on tools
/// </summary>
public class ActionButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent container for action buttons")]
    public Transform buttonContainer;
    
    [Tooltip("Prefab for action buttons")]
    public GameObject actionButtonPrefab;
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(200, 0, 0);
    
    private DraggableItem currentItem;
    private DraggableTool currentTool;
    private List<GameObject> activeButtons = new List<GameObject>();
    
    private void Start()
    {
        // Hide UI on start
        if (buttonContainer != null)
        {
            buttonContainer.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Show action buttons for available recipes
    /// </summary>
    public void ShowActions(List<ProcessingRecipe> recipes, DraggableItem item, DraggableTool tool)
    {
        // Store references
        currentItem = item;
        currentTool = tool;
        
        // Clear previous buttons
        ClearButtons();
        
        // Show the container
        if (buttonContainer != null)
        {
            buttonContainer.gameObject.SetActive(true);
            
            // Position near the tool
            if (tool != null)
            {
                buttonContainer.position = tool.transform.position + offset;
            }
        }
        
        // Create a button for each available recipe
        foreach (var recipe in recipes)
        {
            CreateActionButton(recipe);
        }
        
        // Always add a Cancel button
        CreateCancelButton();
    }
    
    /// <summary>
    /// Hide the action UI
    /// </summary>
    public void HideActions()
    {
        ClearButtons();
        
        if (buttonContainer != null)
        {
            buttonContainer.gameObject.SetActive(false);
        }
        
        currentItem = null;
        currentTool = null;
    }
    
    private void CreateActionButton(ProcessingRecipe recipe)
    {
        if (actionButtonPrefab == null || buttonContainer == null) return;
        
        // Instantiate button
        GameObject buttonObj = Instantiate(actionButtonPrefab, buttonContainer);
        activeButtons.Add(buttonObj);
        
        // Set button text
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = recipe.actionType.ToString();
        }
        
        // Add click listener
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnActionButtonClicked(recipe));
        }
    }
    
    private void CreateCancelButton()
    {
        if (actionButtonPrefab == null || buttonContainer == null) return;
        
        // Instantiate button
        GameObject buttonObj = Instantiate(actionButtonPrefab, buttonContainer);
        activeButtons.Add(buttonObj);
        
        // Set button text
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = "Cancel";
        }
        
        // Add click listener
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnCancelButtonClicked);
        }
    }
    
    private void OnActionButtonClicked(ProcessingRecipe recipe)
    {
        Debug.Log($"Action clicked: {recipe.actionType}");
        
        if (currentTool != null && currentItem != null)
        {
            // Process the item
            currentTool.ProcessItem(recipe, currentItem);
        }
        
        // Hide the UI
        HideActions();
    }
    
    private void OnCancelButtonClicked()
    {
        Debug.Log("Action cancelled");
        
        // Return item to original position
        if (currentItem != null)
        {
            // Item will return to its parentAfterDrag automatically
        }
        
        // Hide the UI
        HideActions();
    }
    
    private void ClearButtons()
    {
        foreach (var button in activeButtons)
        {
            Destroy(button);
        }
        activeButtons.Clear();
    }
}
