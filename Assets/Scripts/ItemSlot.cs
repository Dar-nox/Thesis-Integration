using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    private void Update()
    {
        ConsolidateChildren();
    }

    private void ConsolidateChildren()
    {
        if (transform.childCount > 1)
        {
            // Keep the first child and stack the rest onto it
            DraggableItem firstItem = transform.GetChild(0).GetComponent<DraggableItem>();
            
            if (firstItem != null)
            {
                int childrenToRemove = transform.childCount - 1;
                
                // Add the count of excess children to the first item
                firstItem.count += childrenToRemove;
                firstItem.UpdateCountDisplay();
                
                // Delete all children except the first one
                for (int i = transform.childCount - 1; i > 0; i--)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            GameObject dropped = eventData.pointerDrag;
            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
            
            if (draggableItem == null) return;
            
            if (transform.childCount == 0)
            {
                // Slot is empty, place the item here
                draggableItem.parentAfterDrag = transform;
            }
            else
            {
                // Slot already has an item - check if we can stack
                DraggableItem existingItem = transform.GetChild(0).GetComponent<DraggableItem>();
                
                // Check if items are the same (compare itemData reference for exact match)
                if (existingItem != null && 
                    existingItem.itemData != null && 
                    draggableItem.itemData != null &&
                    existingItem.itemData == draggableItem.itemData)
                {
                    // Same item type AND same processing state, stack them
                    existingItem.count += draggableItem.count;
                    existingItem.UpdateCountDisplay();
                    Destroy(dropped);
                }
                // If different items or different processing states, do nothing (item will return to original position)
            }
        }
    }
}
